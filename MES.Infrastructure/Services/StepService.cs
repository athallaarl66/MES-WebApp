using MES.Core.Entities;
using MES.Core.Enums;
using MES.Core.Interfaces;
using MES.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MES.Infrastructure.Services;

public class StepService : IStepService
{
    private readonly MesDbContext _db;

    public StepService(MesDbContext db)
    {
        _db = db;
    }

    public async Task StartStepAsync(int stepExecutionId, string operatorName)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync();

        try
        {
            var stepExecution = await _db.StepExecutions
                .FromSqlRaw(
                    @"SELECT * FROM ""StepExecutions"" 
                      WHERE ""Id"" = {0} 
                      FOR UPDATE", stepExecutionId)
                .Include(se => se.StepDefinition)
                .FirstOrDefaultAsync();

            if (stepExecution == null)
                throw new KeyNotFoundException("Step tidak ditemukan");

            await _db.Entry(stepExecution)
                .Reference(se => se.WorkOrder)
                .LoadAsync();

            await _db.Entry(stepExecution.WorkOrder)
                .Collection(wo => wo.StepExecutions)
                .Query()
                .Include(s => s.StepDefinition)
                .LoadAsync();

            if (stepExecution.WorkOrder.DeletedAt != null)
                throw new InvalidOperationException("Work order sudah dihapus");

            ValidateStepCanStart(stepExecution);

            stepExecution.Status = StepStatus.InProgress;
            stepExecution.StartedAt = DateTime.UtcNow;
            stepExecution.ExecutedBy = operatorName;

            if (stepExecution.WorkOrder.Status == WorkOrderStatus.Pending)
                stepExecution.WorkOrder.Status = WorkOrderStatus.InProgress;

            _db.ActivityLogs.Add(new ActivityLog
            {
                WorkOrderId = stepExecution.WorkOrderId,
                StepExecutionId = stepExecution.Id,
                StepName = stepExecution.StepDefinition.Name,
                Action = "step_started",
                Notes = $"Step {stepExecution.StepDefinition.Name} dimulai",
                CreatedBy = operatorName,
                CreatedAt = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task FinishStepAsync(int stepExecutionId, string operatorName)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync();

        try
        {
            var stepExecution = await GetStepExecutionWithContext(stepExecutionId);

            if (stepExecution.Status != StepStatus.InProgress)
                throw new InvalidOperationException("Hanya step yang sedang berjalan yang bisa diselesaikan");

            stepExecution.Status = StepStatus.Done;
            stepExecution.CompletedAt = DateTime.UtcNow;

            // Kalau semua step selesai, tandai WorkOrder completed
            var semuaStepSelesai = stepExecution.WorkOrder.StepExecutions
                .All(s => s.Id == stepExecution.Id || s.Status == StepStatus.Done);

            if (semuaStepSelesai)
                stepExecution.WorkOrder.Status = WorkOrderStatus.Completed;

            _db.ActivityLogs.Add(new ActivityLog
            {
                WorkOrderId = stepExecution.WorkOrderId,
                StepExecutionId = stepExecution.Id,
                StepName = stepExecution.StepDefinition.Name,  
                Action = "step_completed",
                Notes = $"Step {stepExecution.StepDefinition.Name} selesai",
                CreatedBy = operatorName,
                CreatedAt = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task FailQcAsync(int stepExecutionId, string operatorName, string reason)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync();

        try
        {
            var stepExecution = await GetStepExecutionWithContext(stepExecutionId);

            if (stepExecution.StepDefinition.Name != "Quality Check")
                throw new InvalidOperationException("Hanya step Quality Check yang bisa di-fail");

            if (stepExecution.Status != StepStatus.InProgress)
                throw new InvalidOperationException("Quality Check harus dalam status in_progress untuk bisa gagal");

            stepExecution.Status = StepStatus.Failed;
            stepExecution.CompletedAt = DateTime.UtcNow;

            // Cari step sebelum QC by order, bukan by name — lebih robust
            var stepYangHarusDiulang = stepExecution.WorkOrder.StepExecutions
                .FirstOrDefault(s => s.StepDefinition.Order == stepExecution.StepDefinition.Order - 1)
                ?? throw new Exception("Step sebelum QC tidak ditemukan");

            stepYangHarusDiulang.Status = StepStatus.Pending;
            stepYangHarusDiulang.StartedAt = null;
            stepYangHarusDiulang.CompletedAt = null;
            stepYangHarusDiulang.ExecutedBy = null;

            _db.ActivityLogs.Add(new ActivityLog
            {
                WorkOrderId = stepExecution.WorkOrderId,
                StepExecutionId = stepExecution.Id,
                StepName = stepExecution.StepDefinition.Name,
                Action = "qc_failed",
                Notes = $"QC gagal: {reason}. Dikembalikan ke {stepYangHarusDiulang.StepDefinition.Name}",
                CreatedBy = operatorName,
                CreatedAt = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private async Task<StepExecution> GetStepExecutionWithContext(int stepExecutionId)
    {
        var stepExecution = await _db.StepExecutions
            .Include(se => se.StepDefinition)
            .Include(se => se.WorkOrder)
                .ThenInclude(wo => wo.StepExecutions)
                    .ThenInclude(s => s.StepDefinition)
            .FirstOrDefaultAsync(se => se.Id == stepExecutionId);

        if (stepExecution == null)
            throw new KeyNotFoundException("Step tidak ditemukan");

        if (stepExecution.WorkOrder.DeletedAt != null)
            throw new InvalidOperationException("Work order sudah dihapus");

        return stepExecution;
    }

    private static void ValidateStepCanStart(StepExecution stepExecution)
    {
        if (stepExecution.Status != StepStatus.Pending)
            throw new InvalidOperationException("Step ini tidak bisa dimulai — statusnya bukan pending");

        var adaStepAktif = stepExecution.WorkOrder.StepExecutions
            .Any(s => s.Status == StepStatus.InProgress);

        if (adaStepAktif)
            throw new InvalidOperationException("Ada step lain yang sedang berjalan");

        // Urutan dijaga by Order, bukan by nama step
        var adaStepSebelumnyaYangBelumSelesai = stepExecution.WorkOrder.StepExecutions
            .Any(s => s.StepDefinition.Order < stepExecution.StepDefinition.Order
                   && s.Status != StepStatus.Done);

        if (adaStepSebelumnyaYangBelumSelesai)
            throw new InvalidOperationException("Step sebelumnya belum selesai");
    }
}