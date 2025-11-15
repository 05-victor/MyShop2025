using Refit;
using MyShop.Shared.Models;
using MyShop.Shared.DTOs.Common;

namespace MyShop.Plugins.ApiClients.Dashboard;

/// <summary>
/// Refit API Client for Dashboard endpoints
/// </summary>
public interface IDashboardApiClient
{
    /// <summary>
    /// GET /api/dashboard/summary
    /// Get dashboard summary statistics
    /// </summary>
    [Get("/api/v1/dashboard/summary")]
    Task<Refit.ApiResponse<MyShop.Shared.DTOs.Common.ApiResponse<DashboardSummary>>> GetSummaryAsync();

    /// <summary>
    /// GET /api/dashboard/revenue-chart?period={period}
    /// Get revenue chart data for specified period
    /// </summary>
    /// <param name="period">Period type: daily, weekly, monthly, yearly</param>
    [Get("/api/v1/dashboard/revenue-chart")]
    Task<Refit.ApiResponse<MyShop.Shared.DTOs.Common.ApiResponse<RevenueChartData>>> GetRevenueChartAsync([Query] string period);
}