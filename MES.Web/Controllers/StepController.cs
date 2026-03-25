using Microsoft.AspNetCore.Mvc;
using MES.Web.Services;

namespace MES.Web.Controllers;

public class StepController : Controller
{
    private readonly IWorkOrderApiService _workOrderService;
    private readonly ILogger<StepController> _logger;

    // operator sementara hardcode, nanti diganti setelah auth ada
    private const string DefaultOperator = "operator";

    public StepController(IWorkOrderApiService workOrderService, ILogger<StepController> logger)
    {
        _workOrderService = workOrderService;
        _logger = logger;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Start(int stepExecutionId, int workOrderId)
    {
        var success = await _workOrderService.StartStepAsync(stepExecutionId, DefaultOperator);

        if (!success)
            TempData["Error"] = "Gagal memulai step, pastikan urutan step sudah benar";

        return RedirectToAction("Detail", "WorkOrder", new { id = workOrderId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Finish(int stepExecutionId, int workOrderId)
    {
        var success = await _workOrderService.FinishStepAsync(stepExecutionId, DefaultOperator);

        if (!success)
            TempData["Error"] = "Gagal menyelesaikan step";

        return RedirectToAction("Detail", "WorkOrder", new { id = workOrderId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> FailQc(int stepExecutionId, int workOrderId, string reason)
    {
        var success = await _workOrderService.FailQcAsync(stepExecutionId, DefaultOperator, reason);

        if (!success)
            TempData["Error"] = "Gagal melakukan QC fail";

        return RedirectToAction("Detail", "WorkOrder", new { id = workOrderId });
    }
}