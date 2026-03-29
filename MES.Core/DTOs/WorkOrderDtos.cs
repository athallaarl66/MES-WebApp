namespace MES.Core.DTOs;

public class CreateWorkOrderRequest
{
    public string OrderNumber { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string ProductCode { get; set; } = string.Empty;
    public int Quantity { get; set; }
}
 

public class WorkOrderResponse
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string ProductCode { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string CurrentStep { get; set; } = string.Empty;
    public List<StepExecutionResponse> Steps { get; set; } = [];
}
 public class StepExecutionResponse
{
    public int Id { get; set; }
    public string StepName { get; set; } = string.Empty;
    public int StepOrder { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ExecutedBy { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public bool CanStart { get; set; }  // backend yang tentuin, bukan View
}

public class WorkOrderSummaryDto
{
    public int Total { get; set; }
    public int Pending { get; set; }
    public int InProgress { get; set; }
    public int Completed { get; set; }
    public int Cancelled { get; set; }
}