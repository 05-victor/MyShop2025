using MyShop.Shared.DTOs.Responses;
using Refit;

namespace MyShop.Plugins.API.Commission;

/// <summary>
/// Refit interface for Commission API endpoints
/// </summary>
[Headers("User-Agent: MyShop-Client/1.0")]
public interface ICommissionApi
{
    [Get("/api/v1/commission/my-earnings")]
    Task<Refit.ApiResponse<MyShop.Shared.DTOs.Common.ApiResponse<CommissionSummaryResponse>>> GetMyEarningsAsync();

    [Get("/api/v1/commission/history")]
    Task<Refit.ApiResponse<MyShop.Shared.DTOs.Common.ApiResponse<List<CommissionResponse>>>> GetCommissionHistoryAsync();
}
