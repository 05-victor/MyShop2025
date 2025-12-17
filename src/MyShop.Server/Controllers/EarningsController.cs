using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyShop.Server.Services.Interfaces;
using MyShop.Shared.DTOs.Common;
using MyShop.Shared.DTOs.Commons;
using MyShop.Shared.DTOs.Requests;
using MyShop.Shared.DTOs.Responses;

namespace MyShop.Server.Controllers;

/// <summary>
/// Controller for sales agent earnings management
/// Provides endpoints for viewing earnings summary and history
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize] // Require authentication
public class EarningsController : ControllerBase
{
    private readonly IEarningsService _earningsService;
    private readonly ILogger<EarningsController> _logger;

    public EarningsController(
        IEarningsService earningsService,
        ILogger<EarningsController> logger)
    {
        _earningsService = earningsService;
        _logger = logger;
    }

    /// <summary>
    /// Get earnings summary for the currently logged-in sales agent
    /// </summary>
    /// <returns>Earnings summary including total earnings, platform fees, net earnings, etc.</returns>
    /// <response code="200">Returns earnings summary</response>
    /// <response code="401">User not authenticated</response>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(ApiResponse<EarningsSummaryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetSummary()
    {
        try
        {
            _logger.LogInformation("GET /api/v1/earnings/summary - Getting earnings summary for current user");

            var summary = await _earningsService.GetMySummaryAsync();

            return Ok(ApiResponse<EarningsSummaryResponse>.SuccessResponse(
                summary,
                "Earnings summary retrieved successfully"));
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Unauthorized access to earnings summary: {Message}", ex.Message);
            return Unauthorized(ApiResponse<object>.UnauthorizedResponse("User not authenticated"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving earnings summary");
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<object>.ServerErrorResponse("An error occurred while retrieving earnings summary"));
        }
    }

    /// <summary>
    /// Get earnings history with pagination for the currently logged-in sales agent
    /// </summary>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10, max: 100)</param>
    /// <param name="startDate">Optional start date filter (format: yyyy-MM-dd)</param>
    /// <param name="endDate">Optional end date filter (format: yyyy-MM-dd)</param>
    /// <param name="status">Optional order status filter (e.g., PENDING, CONFIRMED, DELIVERED, CANCELLED)</param>
    /// <param name="paymentStatus">Optional payment status filter (e.g., UNPAID, PAID)</param>
    /// <returns>Paginated earnings history</returns>
    /// <response code="200">Returns earnings history</response>
    /// <response code="400">Invalid request parameters</response>
    /// <response code="401">User not authenticated</response>
    [HttpGet("history")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<EarningHistoryResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetHistory(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] string? status = null,
        [FromQuery] string? paymentStatus = null)
    {
        try
        {
            _logger.LogInformation(
                "GET /api/v1/earnings/history - Page={Page}, PageSize={PageSize}, StartDate={StartDate}, EndDate={EndDate}, Status={Status}, PaymentStatus={PaymentStatus}",
                pageNumber, pageSize, startDate, endDate, status, paymentStatus);

            // Validate pagination parameters
            if (pageNumber < 1)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("Page number must be at least 1"));
            }

            if (pageSize < 1 || pageSize > 100)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("Page size must be between 1 and 100"));
            }

            // Validate date range
            if (startDate.HasValue && endDate.HasValue && startDate.Value > endDate.Value)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("Start date cannot be after end date"));
            }

            var request = new PaginationRequest
            {
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var history = await _earningsService.GetMyHistoryAsync(
                request,
                startDate,
                endDate,
                status,
                paymentStatus);

            return Ok(ApiResponse<PagedResult<EarningHistoryResponse>>.SuccessResponse(
                history,
                "Earnings history retrieved successfully"));
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Unauthorized access to earnings history: {Message}", ex.Message);
            return Unauthorized(ApiResponse<object>.UnauthorizedResponse("User not authenticated"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving earnings history");
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<object>.ServerErrorResponse("An error occurred while retrieving earnings history"));
        }
    }
}
