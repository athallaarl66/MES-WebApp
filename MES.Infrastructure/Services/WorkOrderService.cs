using MES.Core.DTOs;
using MES.Core.Entities;
using MES.Core.Enums;
using MES.Core.Interfaces;
using MES.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MES.Infrastructure.Services;

public class WorkOrderService : IWorkOrderService
{
    private readonly MesDbContext _db;

    public WorkOrderService(MesDbContext db)
    {
        _db = db;
    }

    public async Task<WorkOrderResponse> CreateWorkOrderAsync(CreateWorkOrderRequest request, string createdBy)
    {
        var stepDefinitions = await _db.StepDefinitions
            .OrderBy(s => s.Order)
            .ToListAsync();

        var workOrder = new WorkOrder
        {
            OrderNumber = request.OrderNumber,
            ProductName = request.ProductName,
            ProductCode = request.ProductCode,
            Quantity = request.Quantity,
            Status = WorkOrderStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        workOrder.StepExecutions = stepDefinitions.Select(step => new StepExecution
        {
            StepDefinitionId = step.Id,
            Status = StepStatus.Pending
        }).ToList();

        _db.WorkOrders.Add(workOrder);

        _db.ActivityLogs.Add(new ActivityLog
        {
            WorkOrder = workOrder,
            Action = "work_order_created",
            Notes = $"Work order {request.OrderNumber} dibuat",
            CreatedBy = createdBy,
            CreatedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();

        return await GetWorkOrderByIdAsync(workOrder.Id)
            ?? throw new Exception("Gagal mengambil data work order setelah dibuat");
    }

    public async Task<WorkOrderResponse?> GetWorkOrderByIdAsync(int id)
    {
        var workOrder = await _db.WorkOrders
            .Include(wo => wo.StepExecutions)
                .ThenInclude(se => se.StepDefinition)
            .FirstOrDefaultAsync(wo => wo.Id == id);

        if (workOrder == null) return null;

        return MapToResponse(workOrder);
    }

    public async Task<List<WorkOrderResponse>> GetAllWorkOrdersAsync()
    {
        var workOrders = await _db.WorkOrders
            .Include(wo => wo.StepExecutions)
                .ThenInclude(se => se.StepDefinition)
            .OrderByDescending(wo => wo.CreatedAt)
            .ToListAsync();

        return workOrders.Select(MapToResponse).ToList();
    }

public async Task<WorkOrderSummaryDto> GetSummaryAsync()
{
    // Satu query, group by status — ga mau N query
    var counts = await _db.WorkOrders
        .Where(wo => wo.DeletedAt == null)
        .GroupBy(wo => wo.Status)
        .Select(g => new { Status = g.Key, Count = g.Count() })
        .ToListAsync();

    // Default 0 kalau status itu ga ada di DB
    int Get(string status) =>
        counts.FirstOrDefault(c => c.Status == status)?.Count ?? 0;

    return new WorkOrderSummaryDto
    {
        Total      = counts.Sum(c => c.Count),
        Pending    = Get(WorkOrderStatus.Pending),
        InProgress = Get(WorkOrderStatus.InProgress),
        Completed  = Get(WorkOrderStatus.Completed),
        Cancelled  = Get(WorkOrderStatus.Cancelled)
    };
}

public async Task<List<ActivityLogDto>> GetActivityLogsAsync(int workOrderId)
{
    var logs = await _db.ActivityLogs
        .Where(l => l.WorkOrderId == workOrderId)
        .OrderByDescending(l => l.CreatedAt)
        .Select(l => new ActivityLogDto
        {
            Id = l.Id,
            StepName = l.StepName,
            Action = l.Action,
            Notes = l.Notes,
            CreatedBy = l.CreatedBy,
            CreatedAt = l.CreatedAt
        })
        .ToListAsync();

    return logs;
}

    private static WorkOrderResponse MapToResponse(WorkOrder workOrder)
{
    var steps = workOrder.StepExecutions
        .OrderBy(se => se.StepDefinition.Order)
        .Select(se => new StepExecutionResponse
        {
            Id = se.Id,
            StepName = se.StepDefinition.Name,
            StepOrder = se.StepDefinition.Order,
            Status = se.Status,
            ExecutedBy = se.ExecutedBy,
            StartedAt = se.StartedAt,
            CompletedAt = se.CompletedAt
        }).ToList();

    // tentuin step mana yang valid untuk distart
    var stepBerikutnya = steps
        .Where(s => s.Status == StepStatus.Pending)
        .FirstOrDefault(s => steps
            .Where(prev => prev.StepOrder < s.StepOrder)
            .All(prev => prev.Status == StepStatus.Done || prev.Status == StepStatus.Failed));

    if (stepBerikutnya != null)
        stepBerikutnya.CanStart = true;

    var currentStep = steps.FirstOrDefault(s => s.Status == StepStatus.InProgress)
        ?? steps.FirstOrDefault(s => s.Status == StepStatus.Pending);

    return new WorkOrderResponse
    {
        Id = workOrder.Id,
        OrderNumber = workOrder.OrderNumber,
        ProductName = workOrder.ProductName,
        ProductCode = workOrder.ProductCode,
        Quantity = workOrder.Quantity,
        Status = workOrder.Status,
        CreatedAt = workOrder.CreatedAt,
        CurrentStep = currentStep?.StepName ?? "Completed",
        Steps = steps
    };
}
}