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
                    var commissions = apiResponse.Result.Select(MapToCommission).ToList();
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
                    var summary = MapToCommissionSummary(apiResponse.Result);
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

    /// <summary>
    /// Map CommissionResponse DTO to Commission domain model
    /// </summary>
    private static Commission MapToCommission(MyShop.Shared.DTOs.Responses.CommissionResponse dto)
    {
        return new Commission
        {
            Id = dto.Id,
            OrderId = dto.OrderId,
            SalesAgentId = dto.SalesAgentId,
            OrderNumber = dto.OrderNumber,
            OrderAmount = dto.OrderAmount,
            CommissionRate = dto.CommissionRate,
            CommissionAmount = dto.CommissionAmount,
            Status = dto.Status,
            CreatedDate = dto.CreatedDate,
            PaidDate = dto.PaidDate
        };
    }

    /// <summary>
    /// Map CommissionSummaryResponse DTO to CommissionSummary domain model
    /// </summary>
    private static CommissionSummary MapToCommissionSummary(MyShop.Shared.DTOs.Responses.CommissionSummaryResponse dto)
    {
        return new CommissionSummary
        {
            TotalEarnings = dto.TotalEarnings,
            PendingCommission = dto.PendingCommission,
            PaidCommission = dto.PaidCommission,
            TotalOrders = dto.TotalOrders,
            AverageCommission = dto.AverageCommission,
            ThisMonthEarnings = dto.ThisMonthEarnings,
            LastMonthEarnings = dto.LastMonthEarnings
        };
    }
}
