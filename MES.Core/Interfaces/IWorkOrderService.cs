using MES.Core.DTOs;

namespace MES.Core.Interfaces;

public interface IWorkOrderService
{
    Task<WorkOrderResponse> CreateWorkOrderAsync(CreateWorkOrderRequest request, string createdBy);
    Task<WorkOrderResponse?> GetWorkOrderByIdAsync(int id);
    Task<List<WorkOrderResponse>> GetAllWorkOrdersAsync();
    Task<WorkOrderSummaryDto> GetSummaryAsync();

}