using MyShop.Shared.Models;
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
    Task<IEnumerable<Commission>> GetBySalesAgentIdAsync(Guid salesAgentId);

    /// <summary>
    /// Get commission summary/statistics for a sales agent
    /// </summary>
    Task<CommissionSummary> GetSummaryAsync(Guid salesAgentId);

    /// <summary>
    /// Get commission by order ID
    /// </summary>
    Task<Commission?> GetByOrderIdAsync(Guid orderId);

    /// <summary>
    /// Calculate commission for an order
    /// </summary>
    Task<decimal> CalculateCommissionAsync(Guid orderId);

    /// <summary>
    /// Get commission history with date range filter
    /// </summary>
    Task<IEnumerable<Commission>> GetByDateRangeAsync(Guid salesAgentId, DateTime startDate, DateTime endDate);
}

/// <summary>
/// Commission record
/// </summary>
public class Commission
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Guid SalesAgentId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public decimal OrderAmount { get; set; }
    public decimal CommissionRate { get; set; } // Percentage (e.g., 10 = 10%)
    public decimal CommissionAmount { get; set; }
    public string Status { get; set; } = "Pending"; // Pending, Approved, Paid
    public DateTime CreatedDate { get; set; }
    public DateTime? PaidDate { get; set; }
}

/// <summary>
/// Commission summary statistics
/// </summary>
public class CommissionSummary
{
    public decimal TotalEarnings { get; set; }
    public decimal PendingCommission { get; set; }
    public decimal PaidCommission { get; set; }
    public int TotalOrders { get; set; }
    public decimal AverageCommission { get; set; }
    public decimal ThisMonthEarnings { get; set; }
    public decimal LastMonthEarnings { get; set; }
}
