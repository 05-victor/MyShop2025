using Refit;
using MyShop.Shared.Models;
using MyShop.Shared.DTOs.Common;
using MyShop.Shared.DTOs.Responses;

namespace MyShop.Plugins.API.Dashboard;

/// <summary>
/// Refit API Client for Dashboard endpoints
/// </summary>
public interface IDashboardApi
{
    /// <summary>
    /// GET /api/v1/dashboard/summary
    /// Get dashboard summary statistics for sales agent
    /// </summary>
    /// <param name="period">Period for revenue calculation: "day", "week", "month", "year"</param>
    [Get("/api/v1/dashboard/summary")]
    Task<Refit.ApiResponse<MyShop.Shared.DTOs.Common.ApiResponse<SalesAgentDashboardSummaryResponse>>> GetSalesAgentSummaryAsync([Query] string period = "month");

    /// <summary>
    /// GET /api/dashboard/revenue-chart?period={period}
    /// Get revenue chart data for specified period
    /// </summary>
    /// <param name="period">Period type: daily, weekly, monthly, yearly</param>
    [Get("/api/v1/dashboard/revenue-chart")]
    Task<Refit.ApiResponse<MyShop.Shared.DTOs.Common.ApiResponse<RevenueChartData>>> GetRevenueChartAsync([Query] string period);
}