using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyShop.Data.Repositories.Interfaces;
using MyShop.Server.Services.Implementations;
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
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<ForecastController> _logger;

    public ForecastController(
        IForecastService forecastService,
        ICurrentUserService currentUserService,
        IUserRepository userRepository,
        ILogger<ForecastController> logger)
    {
        _forecastService = forecastService;
        _currentUserService = currentUserService;
        _userRepository = userRepository;
        _logger = logger;
    }

    /// <summary>
    /// Predict product demand for current SalesAgent's store
    /// </summary>
    /// <remarks>
    /// Automatically uses the authenticated SalesAgent's store_id.
    /// The store_id is assigned when a user becomes a SalesAgent (0-99).
    /// 
    /// Sample request:
    /// 
    ///     POST /api/v1/forecasts/my-demand
    ///     {
    ///       "week": "17/01/11",
    ///       "sku_id": 216418,
    ///       "base_price": 111.86,
    ///       "total_price": 99.04,
    ///       "is_featured_sku": 0,
    ///       "is_display_sku": 0
    ///     }
    /// 
    /// </remarks>
    [HttpPost("my-demand")]
    [Authorize(Roles = "SalesAgent,Admin")]
    [ProducesResponseType(typeof(ApiResponse<DemandForecastResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    [ProducesResponseType(typeof(ApiResponse<object>), 503)]
    public async Task<ActionResult<ApiResponse<DemandForecastResponse>>> PredictMyDemandAsync(
        [FromBody] DemandForecastRequest request)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
        {
            _logger.LogWarning("Demand forecast requested without valid user ID");
            return Unauthorized(ApiResponse<object>.ErrorResponse("User not authenticated", 401));
        }

        // Get user's store_id
        var user = await _userRepository.GetByIdAsync(userId.Value);
        if (user == null)
        {
            _logger.LogWarning("User {UserId} not found", userId);
            return NotFound(ApiResponse<object>.ErrorResponse("User not found", 404));
        }

        if (!user.StoreId.HasValue)
        {
            _logger.LogWarning("User {UserId} ({Username}) does not have a store_id assigned", 
                userId, user.Username);
            return BadRequest(ApiResponse<object>.ErrorResponse(
                "Store ID not assigned. Only SalesAgents with assigned stores can use this endpoint.", 400));
        }

        _logger.LogInformation(
            "Demand forecast requested by SalesAgent {Username} (StoreId={StoreId}): SkuId={SkuId}, Week={Week}",
            user.Username, user.StoreId.Value, request.SkuId, request.Week);

        // Override store_id with user's store_id
        request.StoreId = user.StoreId.Value;

        var result = await _forecastService.PredictDemandAsync(request);

        return Ok(ApiResponse<DemandForecastResponse>.SuccessResponse(
            result,
            $"Demand forecast for store {user.StoreId}: {result.PredictedUnitsSold:F2} units predicted"));
    }

    /// <summary>
    /// Predict weekly revenue for current SalesAgent's department
    /// </summary>
    /// <remarks>
    /// Automatically uses the authenticated SalesAgent's store_id as the department ID.
    /// Store is always set to 1 (Walmart store convention).
    /// 
    /// Sample request:
    /// 
    ///     POST /api/v1/forecasts/my-revenue
    ///     {
    ///       "date": "2012-11-02",
    ///       "strategy": "linear"
    ///     }
    /// 
    /// </remarks>
    [HttpPost("my-revenue")]
    [Authorize(Roles = "SalesAgent,Admin")]
    [ProducesResponseType(typeof(ApiResponse<PriceForecastResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    [ProducesResponseType(typeof(ApiResponse<object>), 503)]
    public async Task<ActionResult<ApiResponse<PriceForecastResponse>>> PredictMyRevenueAsync(
        [FromBody] PriceForecastRequest request)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
        {
            _logger.LogWarning("Revenue forecast requested without valid user ID");
            return Unauthorized(ApiResponse<object>.ErrorResponse("User not authenticated", 401));
        }

        // Get user's store_id
        var user = await _userRepository.GetByIdAsync(userId.Value);
        if (user == null)
        {
            _logger.LogWarning("User {UserId} not found", userId);
            return NotFound(ApiResponse<object>.ErrorResponse("User not found", 404));
        }

        if (!user.StoreId.HasValue)
        {
            _logger.LogWarning("User {UserId} ({Username}) does not have a store_id assigned", 
                userId, user.Username);
            return BadRequest(ApiResponse<object>.ErrorResponse(
                "Store ID not assigned. Only SalesAgents with assigned stores can use this endpoint.", 400));
        }

        _logger.LogInformation(
            "Revenue forecast requested by SalesAgent {Username} (StoreId={StoreId}): Date={Date}",
            user.Username, user.StoreId.Value, request.Date);

        // Override fields: Store=1 (Walmart convention), Dept=user's store_id
        request.Store = 1;
        request.Dept = user.StoreId.Value;

        var result = await _forecastService.PredictPriceAsync(request);

        return Ok(ApiResponse<PriceForecastResponse>.SuccessResponse(
            result,
            $"Revenue forecast for department {user.StoreId}: ${result.PredictedWeeklySales:F2} weekly sales predicted"));
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
