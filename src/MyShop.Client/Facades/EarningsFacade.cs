using MyShop.Core.Common;
using MyShop.Core.Interfaces.Repositories;
using MyShop.Core.Interfaces.Facades;
using MyShop.Core.Interfaces.Services;
using MyShop.Shared.DTOs.Commons;
using MyShop.Shared.DTOs.Responses;
using System.Diagnostics;

namespace MyShop.Client.Facades;

/// <summary>
/// Facade for earnings operations
/// </summary>
public class EarningsFacade : IEarningsFacade
{
    private readonly IEarningsRepository _earningsRepository;
    private readonly IToastService _toastService;

    public EarningsFacade(IEarningsRepository earningsRepository, IToastService toastService)
    {
        _earningsRepository = earningsRepository;
        _toastService = toastService;
    }

    public async Task<Result<EarningsSummaryResponse>> GetSummaryAsync()
    {
        try
        {
            Debug.WriteLine("[EarningsFacade] Loading earnings summary");
            var result = await _earningsRepository.GetSummaryAsync();

            if (!result.IsSuccess)
            {
                Debug.WriteLine($"[EarningsFacade] Failed to load earnings summary: {result.ErrorMessage}");
                await _toastService.ShowError($"Failed to load earnings: {result.ErrorMessage}");
                return result;
            }

            Debug.WriteLine($"[EarningsFacade] Earnings summary loaded successfully: " +
                $"Total={result.Data?.TotalEarnings}, Orders={result.Data?.TotalOrders}");
            return result;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[EarningsFacade] Error loading earnings summary: {ex.Message}");
            await _toastService.ShowError($"Error: {ex.Message}");
            return Result<EarningsSummaryResponse>.Failure(ex.Message);
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
            Debug.WriteLine($"[EarningsFacade] Loading earnings history (page {pageNumber})");
            var result = await _earningsRepository.GetHistoryAsync(pageNumber, pageSize, startDate, endDate, status, paymentStatus);

            if (!result.IsSuccess)
            {
                Debug.WriteLine($"[EarningsFacade] Failed to load earnings history: {result.ErrorMessage}");
                return result;
            }

            Debug.WriteLine($"[EarningsFacade] Earnings history loaded: {result.Data?.Items.Count()} items");
            return result;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[EarningsFacade] Error loading earnings history: {ex.Message}");
            return Result<PagedResult<EarningHistoryResponse>>.Failure(ex.Message);
        }
    }
}
