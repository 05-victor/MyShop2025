using MyShop.Core.Interfaces.Repositories;
using MyShop.Plugins.Mocks.Data;
using MyShop.Shared.Models;

namespace MyShop.Plugins.Repositories.Mocks;

/// <summary>
/// Mock implementation for Commission management - delegates to MockCommissionData
/// </summary>
public class MockCommissionRepository : ICommissionRepository
{

    public async Task<IEnumerable<Commission>> GetBySalesAgentIdAsync(Guid salesAgentId)
    {
        try
        {
            var commissions = await MockCommissionData.GetBySalesAgentIdAsync(salesAgentId);
            System.Diagnostics.Debug.WriteLine($"[MockCommissionRepository] Got {commissions.Count} commissions for agent {salesAgentId}");
            return commissions;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockCommissionRepository] GetBySalesAgentIdAsync error: {ex.Message}");
            return new List<Commission>();
        }
    }

    public async Task<CommissionSummary> GetSummaryAsync(Guid salesAgentId)
    {
        try
        {
            var summary = await MockCommissionData.GetSummaryAsync(salesAgentId);
            System.Diagnostics.Debug.WriteLine($"[MockCommissionRepository] GetSummaryAsync success");
            return summary;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockCommissionRepository] GetSummaryAsync error: {ex.Message}");
            return new CommissionSummary();
        }
    }

    public async Task<Commission?> GetByOrderIdAsync(Guid orderId)
    {
        try
        {
            var commission = await MockCommissionData.GetByOrderIdAsync(orderId);
            System.Diagnostics.Debug.WriteLine($"[MockCommissionRepository] GetByOrderIdAsync - Found: {commission != null}");
            return commission;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockCommissionRepository] GetByOrderIdAsync error: {ex.Message}");
            return null;
        }
    }

    public async Task<decimal> CalculateCommissionAsync(Guid orderId)
    {
        try
        {
            var amount = await MockCommissionData.CalculateCommissionAsync(orderId);
            System.Diagnostics.Debug.WriteLine($"[MockCommissionRepository] CalculateCommissionAsync: {amount}");
            return amount;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockCommissionRepository] CalculateCommissionAsync error: {ex.Message}");
            return 0m;
        }
    }

    public async Task<IEnumerable<Commission>> GetByDateRangeAsync(Guid salesAgentId, DateTime startDate, DateTime endDate)
    {
        try
        {
            var commissions = await MockCommissionData.GetByDateRangeAsync(salesAgentId, startDate, endDate);
            System.Diagnostics.Debug.WriteLine($"[MockCommissionRepository] GetByDateRangeAsync returned {commissions.Count} commissions");
            return commissions;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockCommissionRepository] GetByDateRangeAsync error: {ex.Message}");
            return new List<Commission>();
        }
    }

    public async Task<decimal> GetTotalEarnedAsync(Guid salesAgentId)
    {
        try
        {
            var total = await MockCommissionData.GetTotalEarnedAsync(salesAgentId);
            System.Diagnostics.Debug.WriteLine($"[MockCommissionRepository] GetTotalEarnedAsync: {total}");
            return total;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockCommissionRepository] GetTotalEarnedAsync error: {ex.Message}");
            return 0m;
        }
    }
}
