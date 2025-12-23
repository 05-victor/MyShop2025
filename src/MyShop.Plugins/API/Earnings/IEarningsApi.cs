using Refit;
using MyShop.Shared.DTOs.Commons;
using MyShop.Shared.DTOs.Responses;

namespace MyShop.Plugins.API.Earnings;

/// <summary>
/// API interface for earnings endpoints
/// </summary>
public interface IEarningsApi
{
    /// <summary>
    /// Get earnings summary for the current sales agent
    /// </summary>
    [Get("/api/v1/earnings/summary")]
    Task<Refit.ApiResponse<MyShop.Shared.DTOs.Common.ApiResponse<EarningsSummaryResponse>>> GetSummaryAsync();

    /// <summary>
    /// Get earnings history with pagination
    /// </summary>
    [Get("/api/v1/earnings/history?pageNumber={pageNumber}&pageSize={pageSize}")]
    Task<Refit.ApiResponse<MyShop.Shared.DTOs.Common.ApiResponse<PagedResult<EarningHistoryResponse>>>> GetHistoryAsync(
        int pageNumber = 1,
        int pageSize = 20,
        [Query] DateTime? startDate = null,
        [Query] DateTime? endDate = null,
        [Query] string? status = null,
        [Query] string? paymentStatus = null);
}
