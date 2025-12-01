using MyShop.Core.Common;
using MyShop.Shared.Models;

namespace MyShop.Core.Interfaces.Facades;

/// <summary>
/// Facade pattern for commission/earnings management.
/// Aggregates: ICommissionRepository, IOrderRepository, IUserRepository, IToastService.
/// Handles commission tracking, payment status, and export functionality for sales agents.
/// </summary>
public interface ICommissionFacade
{
    /// <summary>
    /// Load commissions for sales agent with paging.
    /// </summary>
    Task<Result<PagedList<Commission>>> LoadCommissionsAsync(
        Guid? agentId = null,
        string? status = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int page = 1,
        int pageSize = Common.PaginationConstants.DefaultPageSize);

    /// <summary>
    /// Get commission summary for agent
    /// </summary>
    Task<Result<CommissionSummary>> GetCommissionSummaryAsync(Guid agentId, string period = "current");

    /// <summary>
    /// Get pending commissions total
    /// </summary>
    Task<Result<decimal>> GetPendingCommissionsAsync(Guid agentId);

    /// <summary>
    /// Get paid commissions total
    /// </summary>
    Task<Result<decimal>> GetPaidCommissionsAsync(Guid agentId);

    /// <summary>
    /// Mark commission as paid
    /// </summary>
    Task<Result<Unit>> MarkCommissionAsPaidAsync(Guid commissionId);

    /// <summary>
    /// Export commissions to CSV
    /// </summary>
    Task<Result<string>> ExportCommissionsAsync(
        Guid? agentId = null,
        DateTime? startDate = null,
        DateTime? endDate = null);
}
