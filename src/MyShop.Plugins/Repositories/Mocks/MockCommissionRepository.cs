using MyShop.Core.Interfaces.Repositories;
using MyShop.Plugins.Mocks.Data;
using MyShop.Shared.Models;
using MyShop.Core.Common;

namespace MyShop.Plugins.Repositories.Mocks;

/// <summary>
/// Mock implementation for Commission management - delegates to MockCommissionData
/// </summary>
public class MockCommissionRepository : ICommissionRepository
{

    public async Task<Result<IEnumerable<Commission>>> GetBySalesAgentIdAsync(Guid salesAgentId)
    {
        try
        {
            var commissions = await MockCommissionData.GetBySalesAgentIdAsync(salesAgentId);
            System.Diagnostics.Debug.WriteLine($"[MockCommissionRepository] Got {commissions.Count} commissions for agent {salesAgentId}");
            return Result<IEnumerable<Commission>>.Success(commissions);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockCommissionRepository] GetBySalesAgentIdAsync error: {ex.Message}");
            return Result<IEnumerable<Commission>>.Failure($"Failed to get commissions: {ex.Message}");
        }
    }

    public async Task<Result<CommissionSummary>> GetSummaryAsync(Guid salesAgentId)
    {
        try
        {
            var summary = await MockCommissionData.GetSummaryAsync(salesAgentId);
            System.Diagnostics.Debug.WriteLine($"[MockCommissionRepository] GetSummaryAsync success");
            return Result<CommissionSummary>.Success(summary);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockCommissionRepository] GetSummaryAsync error: {ex.Message}");
            return Result<CommissionSummary>.Failure($"Failed to get commission summary: {ex.Message}");
        }
    }

    public async Task<Result<Commission>> GetByOrderIdAsync(Guid orderId)
    {
        try
        {
            var commission = await MockCommissionData.GetByOrderIdAsync(orderId);
            System.Diagnostics.Debug.WriteLine($"[MockCommissionRepository] GetByOrderIdAsync - Found: {commission != null}");
            return commission != null
                ? Result<Commission>.Success(commission)
                : Result<Commission>.Failure($"Commission not found for order {orderId}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockCommissionRepository] GetByOrderIdAsync error: {ex.Message}");
            return Result<Commission>.Failure($"Failed to get commission: {ex.Message}");
        }
    }

    public async Task<Result<decimal>> CalculateCommissionAsync(Guid orderId)
    {
        try
        {
            var amount = await MockCommissionData.CalculateCommissionAsync(orderId);
            System.Diagnostics.Debug.WriteLine($"[MockCommissionRepository] CalculateCommissionAsync: {amount}");
            return Result<decimal>.Success(amount);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockCommissionRepository] CalculateCommissionAsync error: {ex.Message}");
            return Result<decimal>.Failure($"Failed to calculate commission: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<Commission>>> GetByDateRangeAsync(Guid salesAgentId, DateTime startDate, DateTime endDate)
    {
        try
        {
            var commissions = await MockCommissionData.GetByDateRangeAsync(salesAgentId, startDate, endDate);
            System.Diagnostics.Debug.WriteLine($"[MockCommissionRepository] GetByDateRangeAsync returned {commissions.Count} commissions");
            return Result<IEnumerable<Commission>>.Success(commissions);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockCommissionRepository] GetByDateRangeAsync error: {ex.Message}");
            return Result<IEnumerable<Commission>>.Failure($"Failed to get commissions by date range: {ex.Message}");
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
            var (items, totalCount) = await MockCommissionData.GetPagedAsync(
                salesAgentId, page, pageSize, status, startDate, endDate, sortBy, sortDescending);

            var pagedList = new PagedList<Commission>(items, totalCount, page, pageSize);

            System.Diagnostics.Debug.WriteLine($"[MockCommissionRepository] GetPagedAsync: Page {page}/{pagedList.TotalPages}, Total {totalCount}");
            return Result<PagedList<Commission>>.Success(pagedList);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockCommissionRepository] GetPagedAsync error: {ex.Message}");
            return Result<PagedList<Commission>>.Failure($"Failed to get paged commissions: {ex.Message}");
        }
    }
}
