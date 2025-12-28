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

    /// <summary>
    /// GET /api/v1/dashboard/admin-summary
    /// Get dashboard summary statistics for admin (requires Admin role)
    /// </summary>
    /// <param name="period">Optional period for orders/revenue calculation: "day", "week", "month", "year". If null/empty, returns all-time data.</param>
    [Get("/api/v1/dashboard/admin-summary")]
    Task<Refit.ApiResponse<MyShop.Shared.DTOs.Common.ApiResponse<AdminDashboardSummaryResponse>>> GetAdminSummaryAsync([Query] string? period = null);

    /// <summary>
    /// GET /api/v1/dashboard/admin-revenue-chart?period={period}
    /// Get admin revenue chart with commission data (requires Admin role)
    /// </summary>
    /// <param name="period">Period type: "day", "week", "month", "year"</param>
    [Get("/api/v1/dashboard/admin-revenue-chart")]
    Task<Refit.ApiResponse<MyShop.Shared.DTOs.Common.ApiResponse<AdminRevenueChartResponse>>> GetAdminRevenueChartAsync([Query] string period);

    /// <summary>
    /// GET /api/v1/dashboard/admin-reports
    /// Get consolidated admin reports with all metrics (revenue trend, orders by category, ratings, salespersons, products)
    /// Supports filtering by date range and category
    /// Requires Admin role
    /// </summary>
    /// <param name="from">Start date (ISO 8601 UTC format, e.g., "2025-12-21T18:04:58.035Z")</param>
    /// <param name="to">End date (ISO 8601 UTC format, e.g., "2025-12-21T18:04:58.035Z")</param>
    /// <param name="categoryId">Optional category ID filter for product summary</param>
    /// <param name="pageNumber">Page number for product summary pagination (default 1)</param>
    /// <param name="pageSize">Page size for product summary (default 10, max 100)</param>
    [Get("/api/v1/dashboard/admin-reports")]
    Task<Refit.ApiResponse<MyShop.Shared.DTOs.Common.ApiResponse<AdminReportsResponse>>> GetAdminReportsAsync(
        [Query] string from,
        [Query] string to,
        [Query] Guid? categoryId = null,
        [Query] int pageNumber = 1,
        [Query] int pageSize = 10);

    /// <summary>
    /// GET /api/v1/dashboard/sales-agent-reports
    /// Get sales agent personal reports (revenue trends, orders by category, top products)
    /// Data is filtered to show only the current sales agent's performance
    /// Requires SalesAgent role
    /// </summary>
    /// <param name="period">Report period: "day", "week", "month", "year" (default: "week")</param>
    /// <param name="categoryId">Optional category ID filter for top products</param>
    [Get("/api/v1/dashboard/sales-agent-reports")]
    Task<Refit.ApiResponse<MyShop.Shared.DTOs.Common.ApiResponse<SalesAgentReportsResponse>>> GetSalesAgentReportsAsync(
        [Query] string period = "week",
        [Query] Guid? categoryId = null);
}