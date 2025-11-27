using MyShop.Core.Common;
using MyShop.Shared.Models;

namespace MyShop.Core.Interfaces.Repositories;

/// <summary>
/// Repository interface for dashboard analytics and statistics
/// </summary>
public interface IDashboardRepository
{
    /// <summary>
    /// Get dashboard summary statistics with period filter
    /// </summary>
    /// <param name="period">Period: "current", "last", "last3" (months)</param>
    Task<Result<DashboardSummary>> GetSummaryAsync(string period = "current");

    /// <summary>
    /// Get revenue chart data for specified period
    /// </summary>
    /// <param name="period">Period type: daily, weekly, monthly, yearly</param>
    Task<Result<RevenueChartData>> GetRevenueChartAsync(string period);

    /// <summary>
    /// Get top performing sales agents with their metrics
    /// </summary>
    /// <param name="period">Period: "current", "last", "last3" (months)</param>
    /// <param name="topCount">Number of top agents to return</param>
    Task<Result<List<TopSalesAgent>>> GetTopSalesAgentsAsync(string period = "current", int topCount = 5);
}
