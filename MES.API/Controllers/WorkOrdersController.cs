using MES.Core.DTOs;
using MES.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MES.API.Controllers;

[ApiController]
[Route("api/work-orders")]
public class WorkOrdersController : ControllerBase
{
    private readonly IWorkOrderService _workOrderService;

    public WorkOrdersController(IWorkOrderService workOrderService)
    {
        _workOrderService = workOrderService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var workOrders = await _workOrderService.GetAllWorkOrdersAsync();
            return Ok(new { success = true, data = workOrders });
        }
        catch (Exception)
        {
            return StatusCode(500, new { success = false, message = "Gagal mengambil data work order" });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var workOrder = await _workOrderService.GetWorkOrderByIdAsync(id);
            if (workOrder == null)
                return NotFound(new { success = false, message = "Work order tidak ditemukan" });

            return Ok(new { success = true, data = workOrder });
        }
        catch (Exception)
        {
            return StatusCode(500, new { success = false, message = "Gagal mengambil data work order" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateWorkOrderRequest request)
    {
        try
        {
            // Sementara hardcode operator, nanti diganti setelah auth ditambah
            var workOrder = await _workOrderService.CreateWorkOrderAsync(request, "operator");
            return CreatedAtAction(nameof(GetById), new { id = workOrder.Id }, new { success = true, data = workOrder });
        }
        catch (Exception)
        {
            return StatusCode(500, new { success = false, message = "Gagal membuat work order" });
        }
    }
}