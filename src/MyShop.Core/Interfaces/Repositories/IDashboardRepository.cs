using MyShop.Core.Common;
using MyShop.Shared.Models;

namespace MyShop.Core.Interfaces.Repositories;

/// <summary>
/// Repository interface for dashboard analytics and statistics
/// </summary>
public interface IDashboardRepository
{
    /// <summary>
    /// Get dashboard summary statistics (KPIs, low stock, top selling, recent orders)
    /// </summary>
    Task<Result<DashboardSummary>> GetSummaryAsync();

    /// <summary>
    /// Get revenue chart data for specified period
    /// </summary>
    /// <param name="period">Period type: daily, weekly, monthly, yearly</param>
    Task<Result<RevenueChartData>> GetRevenueChartAsync(string period);
}
