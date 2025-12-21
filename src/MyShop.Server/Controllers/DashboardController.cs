using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyShop.Server.Services.Interfaces;
using MyShop.Shared.DTOs.Common;
using MyShop.Shared.DTOs.Responses;

namespace MyShop.Server.Controllers;

/// <summary>
/// Controller for dashboard operations
/// Provides dashboard summary statistics for sales agents
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize] // Require authentication
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(
        IDashboardService dashboardService,
        ILogger<DashboardController> logger)
    {
        _dashboardService = dashboardService;
        _logger = logger;
    }

    /// <summary>
    /// Get dashboard summary for the currently logged-in sales agent
    /// Returns overview information including total products, orders (for period), revenue (for period),
    /// low stock products, top-selling products, and recent orders
    /// </summary>
    /// <param name="period">Optional period for orders/revenue calculation:
    /// - "day": Today (from 00:00:00 to now)
    /// - "week": This week (from Monday to now)
    /// - "month": This month (from 1st to now)
    /// - "year": This year (from Jan 1 to now)
    /// - Not specified: All-time data
    /// </param>
    /// <returns>Dashboard summary data</returns>
    /// <response code="200">Returns dashboard summary</response>
    /// <response code="400">Invalid period parameter</response>
    /// <response code="401">User not authenticated</response>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(ApiResponse<SalesAgentDashboardSummaryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetSummary([FromQuery] string? period = null)
    {
        try
        {
            _logger.LogInformation("GET /api/v1/dashboard/summary - Period: {Period}", period ?? "all-time");

            // Validate period parameter if provided
            if (!string.IsNullOrWhiteSpace(period))
            {
                var validPeriods = new[] { "day", "week", "month", "year" };
                if (!validPeriods.Contains(period.ToLower()))
                {
                    _logger.LogWarning("Invalid period parameter: {Period}", period);
                    return BadRequest(ApiResponse<object>.ErrorResponse(
                        $"Invalid period. Valid values are: {string.Join(", ", validPeriods)}, or omit for all-time data"));
                }
            }

            var summary = await _dashboardService.GetSalesAgentSummaryAsync(period);

            return Ok(ApiResponse<SalesAgentDashboardSummaryResponse>.SuccessResponse(
                summary,
                "Dashboard summary retrieved successfully"));
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Unauthorized access to dashboard summary: {Message}", ex.Message);
            return Unauthorized(ApiResponse<object>.UnauthorizedResponse("User not authenticated"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving dashboard summary");
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<object>.ServerErrorResponse("An error occurred while retrieving dashboard summary"));
        }
    }

    /// <summary>
    /// Get revenue chart data for the currently logged-in sales agent
    /// Returns time-series data for revenue visualization
    /// </summary>
    /// <param name="period">Chart period:
    /// - "day": Hourly data for today (24 hours)
    /// - "week": Daily data for this week (Mon-Sun)
    /// - "month": Daily data for this month (1st to today)
    /// - "year": Monthly data for this year (Jan to current month)
    /// </param>
    /// <returns>Revenue chart data with labels and values</returns>
    /// <response code="200">Returns revenue chart data</response>
    /// <response code="400">Invalid period parameter</response>
    /// <response code="401">User not authenticated</response>
    [HttpGet("revenue-chart")]
    [ProducesResponseType(typeof(ApiResponse<RevenueChartResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetRevenueChart([FromQuery] string period = "week")
    {
        try
        {
            _logger.LogInformation("GET /api/v1/dashboard/revenue-chart - Period: {Period}", period);

            // Validate period parameter
            var validPeriods = new[] { "day", "week", "month", "year" };
            if (!validPeriods.Contains(period.ToLower()))
            {
                _logger.LogWarning("Invalid period parameter: {Period}", period);
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    $"Invalid period. Valid values are: {string.Join(", ", validPeriods)}"));
            }

            var chartData = await _dashboardService.GetRevenueChartAsync(period);

            return Ok(ApiResponse<RevenueChartResponse>.SuccessResponse(
                chartData,
                "Revenue chart data retrieved successfully"));
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Unauthorized access to revenue chart: {Message}", ex.Message);
            return Unauthorized(ApiResponse<object>.UnauthorizedResponse("User not authenticated"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving revenue chart");
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<object>.ServerErrorResponse("An error occurred while retrieving revenue chart"));
        }
    }
}
