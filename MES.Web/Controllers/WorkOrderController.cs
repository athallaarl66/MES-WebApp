using Microsoft.AspNetCore.Mvc;
using MES.Web.Models;
using MES.Web.Services;

namespace MES.Web.Controllers;

public class WorkOrderController : Controller
{
    private readonly IWorkOrderApiService _workOrderService;
    private readonly ILogger<WorkOrderController> _logger;

    public WorkOrderController(IWorkOrderApiService workOrderService, ILogger<WorkOrderController> logger)
    {
        _workOrderService = workOrderService;
        _logger = logger;
    }

    // GET /work-order
    public async Task<IActionResult> Index()
    {
        var workOrders = await _workOrderService.GetAllWorkOrdersAsync();
        return View(workOrders);
    }

    // GET /work-order/{id}
    public async Task<IActionResult> Detail(int id)
    {
        var workOrder = await _workOrderService.GetWorkOrderByIdAsync(id);

        if (workOrder == null)
            return NotFound();

        return View(workOrder);
    }

    // GET /work-order/create
    public IActionResult Create()
    {
        return View();
    }

    // POST /work-order/create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateWorkOrderViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var success = await _workOrderService.CreateWorkOrderAsync(model);

        if (!success)
        {
            ModelState.AddModelError(string.Empty, "Gagal membuat work order, coba lagi");
            return View(model);
        }

        return RedirectToAction(nameof(Index));
    }
}