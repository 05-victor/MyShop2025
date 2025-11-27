using MyShop.Shared.Models;
using MyShop.Core.Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyShop.Core.Interfaces.Repositories;

/// <summary>
/// Repository interface for commission and earnings management
/// </summary>
public interface ICommissionRepository
{
    /// <summary>
    /// Get all commissions for a specific sales agent
    /// </summary>
    Task<Result<IEnumerable<Commission>>> GetBySalesAgentIdAsync(Guid salesAgentId);

    /// <summary>
    /// Get commission summary/statistics for a sales agent
    /// </summary>
    Task<Result<CommissionSummary>> GetSummaryAsync(Guid salesAgentId);

    /// <summary>
    /// Get commission by order ID
    /// </summary>
    Task<Result<Commission>> GetByOrderIdAsync(Guid orderId);

    /// <summary>
    /// Calculate commission for an order
    /// </summary>
    Task<Result<decimal>> CalculateCommissionAsync(Guid orderId);

    /// <summary>
    /// Get commission history with date range filter
    /// </summary>
    Task<Result<IEnumerable<Commission>>> GetByDateRangeAsync(Guid salesAgentId, DateTime startDate, DateTime endDate);
    
    /// <summary>
    /// Get total earned commission for a sales agent
    /// </summary>
    Task<decimal> GetTotalEarnedAsync(Guid salesAgentId);
    
    /// <summary>
    /// Get paginated commissions for a sales agent with filtering
    /// </summary>
    Task<Result<PagedList<Commission>>> GetPagedAsync(
        Guid salesAgentId,
        int page = 1,
        int pageSize = Common.PaginationConstants.DefaultPageSize,
        string? status = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        string sortBy = "createdDate",
        bool sortDescending = true);
}
