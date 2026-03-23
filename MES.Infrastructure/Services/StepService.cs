using MES.Core.Entities;
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
        var stepExecution = await GetStepExecutionWithContext(stepExecutionId);

        ValidateStepCanStart(stepExecution);

        stepExecution.Status = "in_progress";
        stepExecution.StartedAt = DateTime.UtcNow;
        stepExecution.ExecutedBy = operatorName;

        // Update WorkOrder jadi in_progress kalau masih pending
        if (stepExecution.WorkOrder.Status == "pending")
            stepExecution.WorkOrder.Status = "in_progress";

        _db.ActivityLogs.Add(new ActivityLog
        {
            WorkOrderId = stepExecution.WorkOrderId,
            StepExecutionId = stepExecution.Id,
            Action = "step_started",
            Notes = $"Step {stepExecution.StepDefinition.Name} dimulai",
            CreatedBy = operatorName,
            CreatedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
    }

    public async Task FinishStepAsync(int stepExecutionId, string operatorName)
    {
        var stepExecution = await GetStepExecutionWithContext(stepExecutionId);

        if (stepExecution.Status != "in_progress")
            throw new InvalidOperationException("Hanya step yang sedang berjalan yang bisa diselesaikan");

        stepExecution.Status = "done";
        stepExecution.CompletedAt = DateTime.UtcNow;

        // Kalau ini step terakhir, tandai WorkOrder sebagai completed
        var semuaStepSelesai = stepExecution.WorkOrder.StepExecutions
            .All(s => s.Id == stepExecution.Id || s.Status == "done");

        if (semuaStepSelesai)
            stepExecution.WorkOrder.Status = "completed";

        _db.ActivityLogs.Add(new ActivityLog
        {
            WorkOrderId = stepExecution.WorkOrderId,
            StepExecutionId = stepExecution.Id,
            Action = "step_completed",
            Notes = $"Step {stepExecution.StepDefinition.Name} selesai",
            CreatedBy = operatorName,
            CreatedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
    }

    public async Task FailQcAsync(int stepExecutionId, string operatorName, string reason)
    {
        var stepExecution = await GetStepExecutionWithContext(stepExecutionId);

        if (stepExecution.StepDefinition.Name != "Quality Check")
            throw new InvalidOperationException("Hanya step Quality Check yang bisa di-fail");

        if (stepExecution.Status != "in_progress")
            throw new InvalidOperationException("Quality Check harus dalam status in_progress untuk bisa gagal");

        // Tandai QC sebagai failed
        stepExecution.Status = "failed";
        stepExecution.CompletedAt = DateTime.UtcNow;

        // Reset Assembly supaya bisa dikerjain ulang
        var assemblyStep = stepExecution.WorkOrder.StepExecutions
            .FirstOrDefault(s => s.StepDefinition.Name == "Assembly")
            ?? throw new Exception("Step Assembly tidak ditemukan");

        assemblyStep.Status = "pending";
        assemblyStep.StartedAt = null;
        assemblyStep.CompletedAt = null;
        assemblyStep.ExecutedBy = null;

        _db.ActivityLogs.Add(new ActivityLog
        {
            WorkOrderId = stepExecution.WorkOrderId,
            StepExecutionId = stepExecution.Id,
            Action = "qc_failed",
            Notes = $"QC gagal: {reason}. Dikembalikan ke Assembly",
            CreatedBy = operatorName,
            CreatedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
    }

    // Ambil step beserta semua context yang dibutuhkan untuk validasi
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
        if (stepExecution.Status != "pending")
            throw new InvalidOperationException("Step ini tidak bisa dimulai — statusnya bukan pending");

        // Pastiin ga ada step lain yang lagi in_progress
        var adaStepAktif = stepExecution.WorkOrder.StepExecutions
            .Any(s => s.Status == "in_progress");

        if (adaStepAktif)
            throw new InvalidOperationException("Ada step lain yang sedang berjalan");

        // Pastiin step sebelumnya udah selesai
        var stepSebelumnya = stepExecution.WorkOrder.StepExecutions
            .Where(s => s.StepDefinition.Order < stepExecution.StepDefinition.Order)
            .ToList();

        var adaStepSebelumnyaYangBelumSelesai = stepSebelumnya
            .Any(s => s.Status != "done");

        if (adaStepSebelumnyaYangBelumSelesai)
            throw new InvalidOperationException("Step sebelumnya belum selesai");
    }
}