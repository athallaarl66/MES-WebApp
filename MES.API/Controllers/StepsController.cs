using MES.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MES.API.Controllers;

[ApiController]
[Route("api/steps")]
public class StepsController : ControllerBase
{
    private readonly IStepService _stepService;

    public StepsController(IStepService stepService)
    {
        _stepService = stepService;
    }

    [HttpPost("{id}/start")]
    public async Task<IActionResult> Start(int id)
    {
        try
        {
            await _stepService.StartStepAsync(id, "operator");
            return Ok(new { success = true, message = "Step berhasil dimulai" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch
        {
            return StatusCode(500, new { success = false, message = "Gagal memulai step" });
        }
    }

    [HttpPost("{id}/finish")]
    public async Task<IActionResult> Finish(int id)
    {
        try
        {
            await _stepService.FinishStepAsync(id, "operator");
            return Ok(new { success = true, message = "Step berhasil diselesaikan" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch
        {
            return StatusCode(500, new { success = false, message = "Gagal menyelesaikan step" });
        }
    }

    [HttpPost("{id}/fail-qc")]
    public async Task<IActionResult> FailQc(int id, [FromBody] FailQcRequest request)
    {
        try
        {
            await _stepService.FailQcAsync(id, "operator", request.Reason);
            return Ok(new { success = true, message = "QC gagal, ulang dari awal" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch
        {
            return StatusCode(500, new { success = false, message = "Gagal memproses QC" });
        }
    }

    [HttpPost("{id}/pass-qc")]
    public async Task<IActionResult> PassQc(int id)
    {
        try
        {
            await _stepService.PassQcAsync(id, "operator");
            return Ok(new { success = true, message = "QC berhasil" });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch
        {
            return StatusCode(500, new { success = false, message = "Gagal memproses QC" });
        }
    }
}

public record FailQcRequest(string Reason);