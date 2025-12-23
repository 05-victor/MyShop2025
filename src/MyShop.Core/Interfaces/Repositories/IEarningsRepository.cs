using MyShop.Core.Common;
using MyShop.Shared.DTOs.Commons;
using MyShop.Shared.DTOs.Responses;

namespace MyShop.Core.Interfaces.Repositories;

/// <summary>
/// Interface for earnings repository
/// </summary>
public interface IEarningsRepository
{
    /// <summary>
    /// Get earnings summary for the current sales agent
    /// </summary>
    Task<Result<EarningsSummaryResponse>> GetSummaryAsync();

    /// <summary>
    /// Get earnings history with pagination
    /// </summary>
    Task<Result<PagedResult<EarningHistoryResponse>>> GetHistoryAsync(
        int pageNumber = 1,
        int pageSize = 20,
        DateTime? startDate = null,
        DateTime? endDate = null,
        string? status = null,
        string? paymentStatus = null);
}
