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

    public async Task<IEnumerable<Commission>> GetBySalesAgentIdAsync(Guid salesAgentId)
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
                    return apiResponse.Result.Select(MapToCommission);
                }
            }

            return Enumerable.Empty<Commission>();
        }
        catch (Exception)
        {
            return Enumerable.Empty<Commission>();
        }
    }

    public async Task<CommissionSummary> GetSummaryAsync(Guid salesAgentId)
    {
        try
        {
            var response = await _api.GetMyEarningsAsync();
            
            if (response.IsSuccessStatusCode && response.Content != null)
            {
                var apiResponse = response.Content;
                if (apiResponse.Success && apiResponse.Result != null)
                {
                    return MapToCommissionSummary(apiResponse.Result);
                }
            }

            return new CommissionSummary();
        }
        catch (Exception)
        {
            return new CommissionSummary();
        }
    }

    public async Task<Commission?> GetByOrderIdAsync(Guid orderId)
    {
        try
        {
            // Note: Backend may need dedicated endpoint for this
            var allCommissions = await GetBySalesAgentIdAsync(Guid.Empty);
            return allCommissions.FirstOrDefault(c => c.OrderId == orderId);
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task<decimal> CalculateCommissionAsync(Guid orderId)
    {
        try
        {
            // Note: Backend calculates commission automatically
            var commission = await GetByOrderIdAsync(orderId);
            return commission?.CommissionAmount ?? 0;
        }
        catch (Exception)
        {
            return 0;
        }
    }

    public async Task<IEnumerable<Commission>> GetByDateRangeAsync(Guid salesAgentId, DateTime startDate, DateTime endDate)
    {
        try
        {
            // Note: Backend may need query parameters for date filtering
            var allCommissions = await GetBySalesAgentIdAsync(salesAgentId);
            return allCommissions.Where(c => c.CreatedDate >= startDate && c.CreatedDate <= endDate);
        }
        catch (Exception)
        {
            return Enumerable.Empty<Commission>();
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
