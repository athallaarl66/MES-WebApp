using System.Net.Http.Json;
using MES.Web.Models;

namespace MES.Web.Services;

public class WorkOrderApiService : IWorkOrderApiService
{
    private readonly HttpClient _http;
    private readonly ILogger<WorkOrderApiService> _logger;

    public WorkOrderApiService(IHttpClientFactory httpClientFactory, ILogger<WorkOrderApiService> logger)
    {
        _http = httpClientFactory.CreateClient("MesApi");
        _logger = logger;
    }

    public async Task<List<WorkOrderViewModel>> GetAllWorkOrdersAsync()
{
    try
    {
        var result = await _http.GetFromJsonAsync<ApiResponse<List<WorkOrderViewModel>>>("/api/work-orders");
        return result?.Data ?? [];
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Gagal mengambil daftar work order dari API");
        return [];
    }
}

    public async Task<WorkOrderViewModel?> GetWorkOrderByIdAsync(int id)
{
    try
    {
        var result = await _http.GetFromJsonAsync<ApiResponse<WorkOrderViewModel>>($"/api/work-orders/{id}");
        return result?.Data;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Gagal mengambil work order {Id} dari API", id);
        return null;
    }
}

    public async Task<bool> CreateWorkOrderAsync(CreateWorkOrderViewModel model)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("/api/work-orders", model);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Gagal membuat work order baru");
            return false;
        }
    }

    public async Task<bool> StartStepAsync(int stepExecutionId, string operatorName)
    {
        try
        {
            var response = await _http.PostAsJsonAsync(
                $"/api/steps/{stepExecutionId}/start",
                new { operatorName }
            );
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Gagal start step {Id}", stepExecutionId);
            return false;
        }
    }

    public async Task<bool> FinishStepAsync(int stepExecutionId, string operatorName)
    {
        try
        {
            var response = await _http.PostAsJsonAsync(
                $"/api/steps/{stepExecutionId}/finish",
                new { operatorName }
            );
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Gagal finish step {Id}", stepExecutionId);
            return false;
        }
    }

    public async Task<bool> FailQcAsync(int stepExecutionId, string operatorName, string reason)
    {
        try
        {
            var response = await _http.PostAsJsonAsync(
                $"/api/steps/{stepExecutionId}/fail-qc",
                new { operatorName, reason }
            );
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Gagal fail QC step {Id}", stepExecutionId);
            return false;
        }
    }
}