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
    public string StepName { get; set; }
    public int StepOrder { get; set; }
    public string Status { get; set; }
    public string ExecutedBy { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    
    // Flag untuk UI Control
    public bool CanStart { get; set; }
    public bool CanPassQc { get; set; }
    public bool CanFailQc { get; set; }
}

public class WorkOrderSummaryDto
{
    public int Total { get; set; }
    public int Pending { get; set; }
    public int InProgress { get; set; }
    public int Completed { get; set; }
    public int Cancelled { get; set; }
}

public class ActivityLogDto
{
    public int Id { get; set; }
    public string? StepName { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}