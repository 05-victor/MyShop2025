using MyShop.Shared.Adapters;
using MyShop.Core.Common;
using MyShop.Core.Interfaces.Repositories;
using MyShop.Plugins.API.Commission;
using MyShop.Shared.Models;
namespace MyShop.Plugins.Repositories.Api;

/// <summary>
/// API-based Commission Repository implementation
/// </summary>
public class CommissionRepository : ICommissionRepository
{
    private readonly ICommissionApi _api;

    public CommissionRepository(ICommissionApi api)
    {
        _api = api;
    }

    public async Task<Result<IEnumerable<Commission>>> GetBySalesAgentIdAsync(Guid salesAgentId)
    {
        try
        {
            // Note: API uses JWT to identify sales agent
            var response = await _api.GetCommissionHistoryAsync();
            
            if (response.IsSuccessStatusCode && response.Content != null)
            {
                var apiResponse = response.Content;
                if (apiResponse.Success && apiResponse.Result != null)
                {
                    var commissions = CommissionAdapter.ToModelList(apiResponse.Result);
                    return Result<IEnumerable<Commission>>.Success(commissions);
                }
            }

            return Result<IEnumerable<Commission>>.Failure("Failed to retrieve commissions");
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<Commission>>.Failure($"Error retrieving commissions: {ex.Message}");
        }
    }

    public async Task<Result<CommissionSummary>> GetSummaryAsync(Guid salesAgentId)
    {
        try
        {
            var response = await _api.GetMyEarningsAsync();
            
            if (response.IsSuccessStatusCode && response.Content != null)
            {
                var apiResponse = response.Content;
                if (apiResponse.Success && apiResponse.Result != null)
                {
                    var summary = CommissionAdapter.ToModel(apiResponse.Result);
                    return Result<CommissionSummary>.Success(summary);
                }
            }

            return Result<CommissionSummary>.Failure("Failed to retrieve commission summary");
        }
        catch (Exception ex)
        {
            return Result<CommissionSummary>.Failure($"Error retrieving commission summary: {ex.Message}");
        }
    }

    public async Task<Result<Commission>> GetByOrderIdAsync(Guid orderId)
    {
        try
        {
            // Note: Backend may need dedicated endpoint for this
            var allCommissionsResult = await GetBySalesAgentIdAsync(Guid.Empty);
            
            if (!allCommissionsResult.IsSuccess)
            {
                return Result<Commission>.Failure(allCommissionsResult.ErrorMessage ?? "Failed to retrieve commission");
            }

            var commission = allCommissionsResult.Data.FirstOrDefault(c => c.OrderId == orderId);
            if (commission != null)
            {
                return Result<Commission>.Success(commission);
            }

            return Result<Commission>.Failure($"Commission for order {orderId} not found");
        }
        catch (Exception ex)
        {
            return Result<Commission>.Failure($"Error retrieving commission: {ex.Message}");
        }
    }

    public async Task<Result<decimal>> CalculateCommissionAsync(Guid orderId)
    {
        try
        {
            // Note: Backend calculates commission automatically
            var commissionResult = await GetByOrderIdAsync(orderId);
            
            if (!commissionResult.IsSuccess)
            {
                return Result<decimal>.Failure(commissionResult.ErrorMessage ?? "Failed to calculate commission");
            }

            return Result<decimal>.Success(commissionResult.Data.CommissionAmount);
        }
        catch (Exception ex)
        {
            return Result<decimal>.Failure($"Error calculating commission: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<Commission>>> GetByDateRangeAsync(Guid salesAgentId, DateTime startDate, DateTime endDate)
    {
        try
        {
            // Note: Backend may need query parameters for date filtering
            var allCommissionsResult = await GetBySalesAgentIdAsync(salesAgentId);
            
            if (!allCommissionsResult.IsSuccess)
            {
                return Result<IEnumerable<Commission>>.Failure(allCommissionsResult.ErrorMessage ?? "Failed to retrieve commissions");
            }

            var filteredCommissions = allCommissionsResult.Data
                .Where(c => c.CreatedDate >= startDate && c.CreatedDate <= endDate)
                .ToList();
            return Result<IEnumerable<Commission>>.Success(filteredCommissions);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<Commission>>.Failure($"Error retrieving commissions by date range: {ex.Message}");
        }
    }

    public async Task<decimal> GetTotalEarnedAsync(Guid salesAgentId)
    {
        try
        {
            var summaryResult = await GetSummaryAsync(salesAgentId);
            if (summaryResult.IsSuccess && summaryResult.Data != null)
            {
                return summaryResult.Data.TotalEarnings;
            }
            return 0m;
        }
        catch
        {
            return 0m;
        }
    }

    public async Task<Result<PagedList<Commission>>> GetPagedAsync(
        Guid salesAgentId,
        int page = 1,
        int pageSize = 20,
        string? status = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        string sortBy = "createdDate",
        bool sortDescending = true)
    {
        try
        {
            // Note: Backend API doesn't support server-side paging yet
            // Fallback: fetch all commissions and apply client-side paging/filtering
            var allCommissionsResult = await GetBySalesAgentIdAsync(salesAgentId);
            
            if (!allCommissionsResult.IsSuccess || allCommissionsResult.Data == null)
            {
                return Result<PagedList<Commission>>.Failure(allCommissionsResult.ErrorMessage ?? "Failed to retrieve commissions");
            }

            var query = allCommissionsResult.Data.AsEnumerable();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(c => c.Status.Equals(status, StringComparison.OrdinalIgnoreCase));
            }

            if (startDate.HasValue)
            {
                query = query.Where(c => c.CreatedDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(c => c.CreatedDate <= endDate.Value);
            }

            // Apply sorting
            query = sortBy.ToLower() switch
            {
                "createddate" => sortDescending 
                    ? query.OrderByDescending(c => c.CreatedDate) 
                    : query.OrderBy(c => c.CreatedDate),
                "commissionamount" or "amount" => sortDescending 
                    ? query.OrderByDescending(c => c.CommissionAmount) 
                    : query.OrderBy(c => c.CommissionAmount),
                "status" => sortDescending 
                    ? query.OrderByDescending(c => c.Status) 
                    : query.OrderBy(c => c.Status),
                _ => sortDescending 
                    ? query.OrderByDescending(c => c.CreatedDate) 
                    : query.OrderBy(c => c.CreatedDate)
            };

            var totalCount = query.Count();
            var items = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            var pagedList = new PagedList<Commission>(items, totalCount, page, pageSize);
            return Result<PagedList<Commission>>.Success(pagedList);
        }
        catch (Exception ex)
        {
            return Result<PagedList<Commission>>.Failure($"Error retrieving paged commissions: {ex.Message}");
        }
    }
}
