using MyShop.Core.Common;
using MyShop.Core.Interfaces.Repositories;
using MyShop.Plugins.API.Earnings;
using MyShop.Shared.DTOs.Commons;
using MyShop.Shared.DTOs.Responses;
using MyShop.Core.Interfaces.Services;
using System.Diagnostics;

namespace MyShop.Plugins.Repositories.Api;

/// <summary>
/// Repository for earnings endpoints
/// </summary>
public class EarningsRepository : IEarningsRepository
{
    private readonly IEarningsApi _api;
    private readonly IToastService _toastService;

    public EarningsRepository(IEarningsApi api, IToastService toastService)
    {
        _api = api;
        _toastService = toastService;
    }

    public async Task<Result<EarningsSummaryResponse>> GetSummaryAsync()
    {
        try
        {
            var response = await _api.GetSummaryAsync();

            if (response.IsSuccessStatusCode && response.Content != null)
            {
                var apiResponse = response.Content;
                if (apiResponse.Success && apiResponse.Result != null)
                {
                    Debug.WriteLine($"[EarningsRepository] GetSummaryAsync success: " +
                        $"Total={apiResponse.Result.TotalEarnings}, " +
                        $"Orders={apiResponse.Result.TotalOrders}");
                    return Result<EarningsSummaryResponse>.Success(apiResponse.Result);
                }
            }

            return Result<EarningsSummaryResponse>.Failure("Failed to retrieve earnings summary");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[EarningsRepository] GetSummaryAsync error: {ex.Message}");
            return Result<EarningsSummaryResponse>.Failure($"Error retrieving earnings summary: {ex.Message}");
        }
    }

    public async Task<Result<PagedResult<EarningHistoryResponse>>> GetHistoryAsync(
        int pageNumber = 1,
        int pageSize = 20,
        DateTime? startDate = null,
        DateTime? endDate = null,
        string? status = null,
        string? paymentStatus = null)
    {
        try
        {
            var response = await _api.GetHistoryAsync(pageNumber, pageSize, startDate, endDate, status, paymentStatus);

            if (response.IsSuccessStatusCode && response.Content != null)
            {
                var apiResponse = response.Content;
                if (apiResponse.Success && apiResponse.Result != null)
                {
                    Debug.WriteLine($"[EarningsRepository] GetHistoryAsync success: " +
                        $"Page={pageNumber}, Count={apiResponse.Result.Items.Count()}");
                    return Result<PagedResult<EarningHistoryResponse>>.Success(apiResponse.Result);
                }
            }

            return Result<PagedResult<EarningHistoryResponse>>.Failure("Failed to retrieve earnings history");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[EarningsRepository] GetHistoryAsync error: {ex.Message}");
            return Result<PagedResult<EarningHistoryResponse>>.Failure($"Error retrieving earnings history: {ex.Message}");
        }
    }
}
