using MyShop.Shared.DTOs.Commons;
using MyShop.Shared.DTOs.Requests;
using MyShop.Shared.DTOs.Responses;

namespace MyShop.Server.Services.Interfaces;

/// <summary>
/// Service interface for sales agent earnings management
/// </summary>
public interface IEarningsService
{
    /// <summary>
    /// Get earnings summary for the currently logged-in sales agent
    /// </summary>
    Task<EarningsSummaryResponse> GetMySummaryAsync();

    /// <summary>
    /// Get earnings history with pagination for the currently logged-in sales agent
    /// </summary>
    /// <param name="request">Pagination parameters</param>
    /// <param name="startDate">Optional start date filter</param>
    /// <param name="endDate">Optional end date filter</param>
    /// <param name="status">Optional order status filter</param>
    /// <param name="paymentStatus">Optional payment status filter</param>
    Task<PagedResult<EarningHistoryResponse>> GetMyHistoryAsync(
        PaginationRequest request,
        DateTime? startDate = null,
        DateTime? endDate = null,
        string? status = null,
        string? paymentStatus = null);
}
