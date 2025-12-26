using Microsoft.AspNetCore.Mvc;
using MyShop.Server.Services.Interfaces;
using MyShop.Shared.DTOs.Common;
using MyShop.Shared.DTOs.Requests;
using MyShop.Shared.DTOs.Responses;

namespace MyShop.Server.Controllers;

/// <summary>
/// Controller for forecasting operations
/// </summary>
[ApiController]
[Route("api/v1/forecasts")]
public class ForecastController : ControllerBase
{
    private readonly IForecastService _forecastService;
    private readonly ILogger<ForecastController> _logger;

    public ForecastController(
        IForecastService forecastService,
        ILogger<ForecastController> logger)
    {
        _forecastService = forecastService;
        _logger = logger;
    }

    [HttpPost("demand")]
    [ProducesResponseType(typeof(ApiResponse<DemandForecastResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 503)]
    public async Task<ActionResult<ApiResponse<DemandForecastResponse>>> PredictDemandAsync(
        [FromBody] DemandForecastRequest request)
    {
        _logger.LogInformation(
            "Demand forecast requested: StoreId={StoreId}, SkuId={SkuId}, Week={Week}",
            request.StoreId, request.SkuId, request.Week);

        var result = await _forecastService.PredictDemandAsync(request);

        return Ok(ApiResponse<DemandForecastResponse>.SuccessResponse(
            result,
            $"Demand forecast successful. Predicted units sold: {result.PredictedUnitsSold:F2}"));
    }

    [HttpPost("price")]
    [ProducesResponseType(typeof(ApiResponse<PriceForecastResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 503)]
    public async Task<ActionResult<ApiResponse<PriceForecastResponse>>> PredictPriceAsync(
        [FromBody] PriceForecastRequest request)
    {
        _logger.LogInformation(
            "Price forecast requested: Store={Store}, Dept={Dept}, Date={Date}",
            request.Store, request.Dept, request.Date);

        var result = await _forecastService.PredictPriceAsync(request);

        return Ok(ApiResponse<PriceForecastResponse>.SuccessResponse(
            result,
            $"Price forecast successful. Predicted weekly sales: {result.PredictedWeeklySales:F2}"));
    }

    [HttpGet("health")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 503)]
    public async Task<ActionResult<ApiResponse<object>>> CheckHealthAsync()
    {
        var isHealthy = await _forecastService.CheckHealthAsync();

        if (isHealthy)
        {
            return Ok(ApiResponse<object>.SuccessResponse(
                new { status = "healthy", service = "forecast" },
                "Forecast service is healthy"));
        }

        return StatusCode(503, ApiResponse<object>.ErrorResponse(
            "Forecast service is unavailable", 503));
    }
}
