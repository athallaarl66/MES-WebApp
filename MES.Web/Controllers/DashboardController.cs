using MES.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace MES.Web.Controllers;

public class DashboardController : Controller
{
    private readonly IWorkOrderApiService _workOrderApi;

    public DashboardController(IWorkOrderApiService workOrderApi)
    {
        _workOrderApi = workOrderApi;
    }

    public async Task<IActionResult> Index()
    {
        var summary = await _workOrderApi.GetSummaryAsync();
        return View(summary);
    }
}