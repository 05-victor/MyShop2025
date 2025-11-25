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
}
