namespace MES.Core.Entities;

public class StepExecution
{
    public int Id { get; set; }
    public int WorkOrderId { get; set; }
    public int StepDefinitionId { get; set; }
    public string Status { get; set; } = "pending";
    public string? ExecutedBy { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    // Navigation properties
    public WorkOrder WorkOrder { get; set; } = null!;
    public StepDefinition StepDefinition { get; set; } = null!;
}