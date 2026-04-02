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
            return BuildSuccess("Step berhasil dimulai");
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    [HttpPost("{id}/finish")]
    public async Task<IActionResult> Finish(int id)
    {
        try
        {
            await _stepService.FinishStepAsync(id, "operator");
            return BuildSuccess("Step berhasil diselesaikan");
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    [HttpPost("{id}/fail-qc")]
    public async Task<IActionResult> FailQc(int id, [FromBody] FailQcRequest request)
    {
        // Validasi payload (sesuai rule: Inline validation, tapi di BE kita pastikan data tidak kosong)
        if (string.IsNullOrWhiteSpace(request?.Reason))
        {
            return BuildError(400, "Alasan QC gagal wajib diisi.", "VALIDATION_ERROR");
        }

        try
        {
            await _stepService.FailQcAsync(id, "operator", request.Reason);
            return BuildSuccess("QC gagal, step dikembalikan ke Assembly.");
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    [HttpPost("{id}/pass-qc")]
    public async Task<IActionResult> PassQc(int id)
    {
    try
        {
            await _stepService.PassQcAsync(id, "operator"); 
            return BuildSuccess("QC berhasil divalidasi.");
        }
            catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    // ========================================================================
    // HELPER METHODS: Agar API Response selalu seragam sesuai standard rules.
    // ========================================================================

    private IActionResult BuildSuccess(string message, object data = null)
    {
        return Ok(new
        {
            success = true,
            message = message,
            data = data
        });
    }

    private IActionResult BuildError(int statusCode, string message, string errorCode)
    {
        return StatusCode(statusCode, new
        {
            success = false,
            message = message,
            errorCode = errorCode,
            data = (object)null
        });
    }

    private IActionResult HandleException(Exception ex)
    {
        // Pengecekan tipe error untuk sanitasi pesan (Safe for user)
        return ex switch
        {
            KeyNotFoundException => BuildError(404, "Data step tidak ditemukan.", "NOT_FOUND"),
            
            // InvalidOperationException biasanya berisi pesan validasi dari Service yang aman dibaca user
            InvalidOperationException invalidEx => BuildError(400, invalidEx.Message, "INVALID_STATE"),
            
            // Catch-all untuk error yang tidak terduga, jangan bocorin internal server error
            _ => BuildError(500, "Terjadi kesalahan pada sistem saat memproses request.", "SERVER_ERROR")
        };
    }
}

public record FailQcRequest(string Reason);