namespace MES.Core.Entities;

public class WorkOrder
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string ProductCode { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string Status { get; set; } = "pending";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DeletedAt { get; set; }

    public ICollection<StepExecution> StepExecutions { get; set; } = [];
    public ICollection<ActivityLog> ActivityLogs { get; set; } = [];
}