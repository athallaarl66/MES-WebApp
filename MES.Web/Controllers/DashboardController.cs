using MES.Core.DTOs;
using MES.Web.Models;
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
        var allWorkOrders = await _workOrderApi.GetAllWorkOrdersAsync();

        var recentWorkOrders = allWorkOrders
            .OrderByDescending(wo => wo.CreatedAt)
            .Take(5)
            .ToList();

        ViewBag.Summary = summary;
        ViewBag.RecentWorkOrders = recentWorkOrders;

        return View();
    }
}