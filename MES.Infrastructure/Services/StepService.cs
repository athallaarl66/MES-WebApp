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
            // Lock row untuk hindari race condition
            var stepExecution = await _db.StepExecutions
                .FromSqlRaw(
                    @"SELECT * FROM ""StepExecutions"" 
                      WHERE ""Id"" = {0} 
                      FOR UPDATE", stepExecutionId)
                .Include(se => se.StepDefinition)
                .FirstOrDefaultAsync();

            if (stepExecution == null)
                throw new KeyNotFoundException("Step tidak ditemukan");

            await LoadWorkOrderContext(stepExecution);

            if (stepExecution.WorkOrder.DeletedAt != null)
                throw new InvalidOperationException("Work order sudah dihapus");

            ValidateStepCanStart(stepExecution);

            stepExecution.Status = StepStatus.InProgress;
            stepExecution.StartedAt = DateTime.UtcNow;
            stepExecution.ExecutedBy = operatorName;

            if (stepExecution.WorkOrder.Status == WorkOrderStatus.Pending)
                stepExecution.WorkOrder.Status = WorkOrderStatus.InProgress;

            AddLog(stepExecution, "step_started", $"Step {stepExecution.StepDefinition.Name} dimulai", operatorName);

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

            // Pastikan tidak lompat step
            var adaStepSebelumnyaBelumDone = stepExecution.WorkOrder.StepExecutions
                .Any(s =>
                    s.StepDefinition.Order < stepExecution.StepDefinition.Order &&
                    s.Status != StepStatus.Done
                );

            if (adaStepSebelumnyaBelumDone)
                throw new InvalidOperationException("Step sebelumnya belum selesai");

            stepExecution.Status = StepStatus.Done;
            stepExecution.CompletedAt = DateTime.UtcNow;

            var semuaSelesai = stepExecution.WorkOrder.StepExecutions
                .All(s => s.Status == StepStatus.Done);

            if (semuaSelesai)
                stepExecution.WorkOrder.Status = WorkOrderStatus.Completed;

            AddLog(stepExecution, "step_completed", $"Step {stepExecution.StepDefinition.Name} selesai", operatorName);

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
                throw new InvalidOperationException("Hanya QC yang bisa di-fail");

            if (stepExecution.Status != StepStatus.InProgress)
                throw new InvalidOperationException("QC harus sedang berjalan");

            // Tandai QC gagal (audit)
            stepExecution.Status = StepStatus.Failed;
            stepExecution.CompletedAt = DateTime.UtcNow;

            var semuaStep = stepExecution.WorkOrder.StepExecutions;

            // Reset semua step ke awal (sesuai flow lu)
            foreach (var step in semuaStep)
            {
                step.Status = StepStatus.Pending;
                step.StartedAt = null;
                step.CompletedAt = null;
                step.ExecutedBy = null;
            }

            stepExecution.WorkOrder.Status = WorkOrderStatus.InProgress;

            AddLog(stepExecution, "qc_failed", $"QC gagal: {reason}. Work order di-reset", operatorName);

            await _db.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task PassQcAsync(int stepExecutionId, string operatorName)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync();

        try
        {
            var stepExecution = await GetStepExecutionWithContext(stepExecutionId);

            if (stepExecution.StepDefinition.Name != "Quality Check")
                throw new InvalidOperationException("Hanya QC yang bisa di-pass");

            if (stepExecution.Status != StepStatus.InProgress)
                throw new InvalidOperationException("QC harus sedang berjalan");

            stepExecution.Status = StepStatus.Done;
            stepExecution.CompletedAt = DateTime.UtcNow;

            AddLog(stepExecution, "qc_pass", "QC berhasil", operatorName);

            await _db.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    // =========================
    // PRIVATE HELPERS
    // =========================

    private async Task LoadWorkOrderContext(StepExecution stepExecution)
    {
        await _db.Entry(stepExecution)
            .Reference(se => se.WorkOrder)
            .LoadAsync();

        await _db.Entry(stepExecution.WorkOrder)
            .Collection(wo => wo.StepExecutions)
            .Query()
            .Include(s => s.StepDefinition)
            .LoadAsync();
    }

    private async Task<StepExecution> GetStepExecutionWithContext(int id)
    {
        var stepExecution = await _db.StepExecutions
            .Include(se => se.StepDefinition)
            .Include(se => se.WorkOrder)
                .ThenInclude(wo => wo.StepExecutions)
                    .ThenInclude(s => s.StepDefinition)
            .FirstOrDefaultAsync(se => se.Id == id);

        if (stepExecution == null)
            throw new KeyNotFoundException("Step tidak ditemukan");

        if (stepExecution.WorkOrder.DeletedAt != null)
            throw new InvalidOperationException("Work order sudah dihapus");

        return stepExecution;
    }

    private static void ValidateStepCanStart(StepExecution stepExecution)
    {
        // QC boleh diulang walaupun FAILED
        if (stepExecution.StepDefinition.Name == "Quality Check")
        {
            if (stepExecution.Status != StepStatus.Pending &&
                stepExecution.Status != StepStatus.Failed)
            {
                throw new InvalidOperationException("QC tidak bisa dimulai");
            }
        }
        else
        {
            if (stepExecution.Status != StepStatus.Pending)
                throw new InvalidOperationException("Step tidak bisa dimulai");
        }

        var adaStepAktif = stepExecution.WorkOrder.StepExecutions
            .Any(s => s.Status == StepStatus.InProgress);

        if (adaStepAktif)
            throw new InvalidOperationException("Ada step lain yang sedang berjalan");

        var adaStepSebelumnyaBelumDone = stepExecution.WorkOrder.StepExecutions
            .Any(s =>
                s.StepDefinition.Order < stepExecution.StepDefinition.Order &&
                s.Status != StepStatus.Done
            );

        if (adaStepSebelumnyaBelumDone)
            throw new InvalidOperationException("Step sebelumnya belum selesai");
    }

    private void AddLog(StepExecution step, string action, string notes, string user)
    {
        _db.ActivityLogs.Add(new ActivityLog
        {
            WorkOrderId = step.WorkOrderId,
            StepExecutionId = step.Id,
            StepName = step.StepDefinition.Name,
            Action = action,
            Notes = notes,
            CreatedBy = user,
            CreatedAt = DateTime.UtcNow
        });
    }
}