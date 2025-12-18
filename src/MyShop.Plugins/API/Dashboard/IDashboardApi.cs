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
    /// <param name="period">Optional period for orders/revenue calculation: "day", "week", "month", "year". If null/empty, returns all-time data.</param>
    [Get("/api/v1/dashboard/summary")]
    Task<Refit.ApiResponse<MyShop.Shared.DTOs.Common.ApiResponse<SalesAgentDashboardSummaryResponse>>> GetSalesAgentSummaryAsync([Query] string? period = null);

    /// <summary>
    /// GET /api/v1/dashboard/revenue-chart?period={period}
    /// Get revenue chart data for specified period
    /// </summary>
    /// <param name="period">Period type: "day", "week", "month", "year"</param>
    [Get("/api/v1/dashboard/revenue-chart")]
    Task<Refit.ApiResponse<MyShop.Shared.DTOs.Common.ApiResponse<RevenueChartResponse>>> GetRevenueChartAsync([Query] string period);
}