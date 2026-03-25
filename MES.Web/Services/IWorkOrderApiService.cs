using MES.Web.Models;

namespace MES.Web.Services;

public interface IWorkOrderApiService
{
    Task<List<WorkOrderViewModel>> GetAllWorkOrdersAsync();
    Task<WorkOrderViewModel?> GetWorkOrderByIdAsync(int id);
    Task<bool> CreateWorkOrderAsync(CreateWorkOrderViewModel model);
    Task<bool> StartStepAsync(int stepExecutionId, string operatorName);
    Task<bool> FinishStepAsync(int stepExecutionId, string operatorName);
    Task<bool> FailQcAsync(int stepExecutionId, string operatorName, string reason);
}