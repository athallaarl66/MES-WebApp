using MES.Web.Models;

namespace MES.Web.Services;

public interface IWorkOrderApiService
{
    Task<List<WorkOrderViewModel>> GetAllWorkOrdersAsync();
    Task<WorkOrderViewModel?> GetWorkOrderByIdAsync(int id);
    Task<bool> CreateWorkOrderAsync(CreateWorkOrderViewModel model);
}