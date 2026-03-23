using System.Collections;

namespace MES.Core.Entities;

public class WorkOrder
{
    public int id { get; set; }
    public string OrderNumber { get; set; } = String.Empty;
    public string ProductNumber { get; set; } = String.Empty;
    public int Quantity { get; set; }
    public string Status { get; set; } = "pending";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DeletedAt { get; set;}

    // Navigation properties
    public ICollection<StepExecution> StepExecutions { get; set; } = [];
    public ICollection<ActivityLog> ActivityLogs { get; set; } = [];


}