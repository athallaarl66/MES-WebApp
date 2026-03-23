namespace MES.Core.Entities;

public class ActivityLog
{
    public int Id { get; set; }
    public int WorkOrderId { get; set; }
    public int? StepExecutionId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public WorkOrder WorkOrder { get; set; } = null!;
}