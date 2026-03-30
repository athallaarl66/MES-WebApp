using MES.Core.DTOs;
using Microsoft.AspNetCore.Mvc;
using MES.Web.Models;
using MES.Web.Services;

namespace MES.Web.Controllers;

public class WorkOrderController : Controller
{
    private readonly IWorkOrderApiService _workOrderApi;
    private readonly ILogger<WorkOrderController> _logger;

    public WorkOrderController(IWorkOrderApiService workOrderApi, ILogger<WorkOrderController> logger)
    {
        _workOrderApi = workOrderApi;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        var workOrders = await _workOrderApi.GetAllWorkOrdersAsync();
        return View(workOrders);
    }

    public async Task<IActionResult> Detail(int id)
    {
        var workOrder = await _workOrderApi.GetWorkOrderByIdAsync(id);
        if (workOrder == null)
            return NotFound();

        return View(workOrder);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateWorkOrderViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var success = await _workOrderApi.CreateWorkOrderAsync(model);
        if (!success)
        {
            ModelState.AddModelError(string.Empty, "Gagal membuat work order, coba lagi");
            return View(model);
        }

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Logs(int id)
    {
        var workOrder = await _workOrderApi.GetWorkOrderByIdAsync(id);
        if (workOrder == null)
        {
            TempData["Error"] = "Work order tidak ditemukan";
            return RedirectToAction("Index");
        }

        var logs = await _workOrderApi.GetActivityLogsAsync(id) ?? [];
        ViewBag.WorkOrder = workOrder;
        return View(logs);
    }
}