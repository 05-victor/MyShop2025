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

    /// <summary>
    /// Get admin dashboard summary - Platform-wide metrics
    /// Admin only - shows aggregated data from all sales agents
    /// </summary>
    /// <param name="period">Optional period for orders/revenue calculation:
    /// - "day": Today (from 00:00:00 to now)
    /// - "week": This week (from Monday to now)
    /// - "month": This month (from 1st to now)
    /// - "year": This year (from Jan 1 to now)
    /// - Not specified: All-time data
    /// </param>
    /// <returns>Admin dashboard summary data</returns>
    /// <response code="200">Returns admin dashboard summary</response>
    /// <response code="400">Invalid period parameter</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">User is not an admin</response>
    [HttpGet("admin-summary")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<AdminDashboardSummaryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAdminSummary([FromQuery] string? period = null)
    {
        try
        {
            _logger.LogInformation("GET /api/v1/dashboard/admin-summary - Period: {Period}", period ?? "all-time");

            // Validate period parameter if provided
            if (!string.IsNullOrWhiteSpace(period))
            {
                var validPeriods = new[] { "day", "week", "month", "year" };
                if (!validPeriods.Contains(period.ToLower()))
                {
                    _logger.LogWarning("Invalid period parameter: {Period}", period);
                    return BadRequest(ApiResponse<object>.ErrorResponse(
                        $"Invalid period. Valid values are: {string.Join(", ", validPeriods)}"));
                }
            }

            var summary = await _dashboardService.GetAdminSummaryAsync(period);

            return Ok(ApiResponse<AdminDashboardSummaryResponse>.SuccessResponse(
                summary,
                "Admin dashboard summary retrieved successfully"));
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Unauthorized access to admin dashboard summary: {Message}", ex.Message);
            return Unauthorized(ApiResponse<object>.UnauthorizedResponse("User not authenticated"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving admin dashboard summary");
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<object>.ServerErrorResponse("An error occurred while retrieving admin dashboard summary"));
        }
    }

    /// <summary>
    /// Get admin revenue & commission chart data - Platform-wide
    /// Admin only - shows platform-wide revenue and commission trends
    /// </summary>
    /// <param name="period">Chart period:
    /// - "day": Hourly data for today (24 hours)
    /// - "week": Daily data for this week (Mon-Sun)
    /// - "month": Daily data for this month (1st to today)
    /// - "year": Monthly data for this year (Jan to current month)
    /// </param>
    /// <returns>Revenue & commission chart data with labels and values</returns>
    /// <response code="200">Returns admin revenue chart data</response>
    /// <response code="400">Invalid period parameter</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">User is not an admin</response>
    [HttpGet("admin-revenue-chart")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<AdminRevenueChartResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAdminRevenueChart([FromQuery] string period = "week")
    {
        try
        {
            _logger.LogInformation("GET /api/v1/dashboard/admin-revenue-chart - Period: {Period}", period);

            // Validate period parameter
            var validPeriods = new[] { "day", "week", "month", "year" };
            if (!validPeriods.Contains(period.ToLower()))
            {
                _logger.LogWarning("Invalid period parameter: {Period}", period);
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    $"Invalid period. Valid values are: {string.Join(", ", validPeriods)}"));
            }

            var chartData = await _dashboardService.GetAdminRevenueChartAsync(period);

            return Ok(ApiResponse<AdminRevenueChartResponse>.SuccessResponse(
                chartData,
                "Admin revenue chart data retrieved successfully"));
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Unauthorized access to admin revenue chart: {Message}", ex.Message);
            return Unauthorized(ApiResponse<object>.UnauthorizedResponse("User not authenticated"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving admin revenue chart");
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<object>.ServerErrorResponse("An error occurred while retrieving admin revenue chart"));
        }
    }

    /// <summary>
    /// Get consolidated admin reports data - All metrics in one API call
    /// Admin only - Returns revenue trends, category analysis, ratings, salesperson stats, and product summary
    /// Optimized for performance with single database query
    /// </summary>
    /// <param name="from">Start date (inclusive) in ISO 8601 format</param>
    /// <param name="to">End date (inclusive) in ISO 8601 format</param>
    /// <param name="categoryId">Optional category filter for product summary</param>
    /// <param name="pageNumber">Page number for product summary (default: 1)</param>
    /// <param name="pageSize">Page size for product summary (default: 10, max: 100)</param>
    /// <returns>Consolidated admin reports data</returns>
    /// <response code="200">Returns admin reports data</response>
    /// <response code="400">Invalid parameters (date range, pagination, etc.)</response>
    /// <response code="401">User not authenticated</response>
    /// <response code="403">User is not an admin</response>
    [HttpGet("admin-reports")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<AdminReportsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAdminReports(
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        [FromQuery] Guid? categoryId = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            _logger.LogInformation(
                "GET /api/v1/dashboard/admin-reports - From: {From}, To: {To}, CategoryId: {CategoryId}, Page: {Page}, PageSize: {PageSize}",
                from?.ToString("yyyy-MM-dd") ?? "not specified",
                to?.ToString("yyyy-MM-dd") ?? "not specified",
                categoryId,
                pageNumber,
                pageSize);

            // Validate date range - default to last 7 days if not specified
            var startDate = from ?? DateTime.UtcNow.AddDays(-7).Date;
            var endDate = to ?? DateTime.UtcNow;

            if (startDate > endDate)
            {
                _logger.LogWarning("Invalid date range: from={From} > to={To}", startDate, endDate);
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    "Invalid date range. Start date must be before or equal to end date."));
            }

            // Validate pagination
            if (pageNumber < 1)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    "Page number must be at least 1"));
            }

            if (pageSize < 1 || pageSize > 100)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    "Page size must be between 1 and 100"));
            }

            var reports = await _dashboardService.GetAdminReportsAsync(
                startDate,
                endDate,
                categoryId,
                pageNumber,
                pageSize);

            return Ok(ApiResponse<AdminReportsResponse>.SuccessResponse(
                reports,
                "Admin reports retrieved successfully"));
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Unauthorized access to admin reports: {Message}", ex.Message);
            return Unauthorized(ApiResponse<object>.UnauthorizedResponse("User not authenticated"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving admin reports");
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<object>.ServerErrorResponse("An error occurred while retrieving admin reports"));
        }
    }
}
