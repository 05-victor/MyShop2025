using MyShop.Shared.DTOs.Responses;

namespace MyShop.Server.Services.Interfaces;

/// <summary>
/// Service interface for dashboard operations
/// </summary>
public interface IDashboardService
{
    /// <summary>
    /// Get dashboard summary for the currently logged-in sales agent
    /// </summary>
    /// <param name="period">Optional period for orders/revenue calculation: "day", "week", "month", "year". If null/empty, returns all-time data.</param>
    /// <returns>Dashboard summary including products, orders, revenue, and analytics</returns>
    Task<SalesAgentDashboardSummaryResponse> GetSalesAgentSummaryAsync(string? period = null);

    /// <summary>
    /// Get revenue chart data for the currently logged-in sales agent
    /// </summary>
    /// <param name="period">Chart period: "day" (hourly), "week" (daily), "month" (daily), "year" (monthly)</param>
    /// <returns>Chart data with labels and revenue values</returns>
    Task<RevenueChartResponse> GetRevenueChartAsync(string period = "week");

    /// <summary>
    /// Get admin dashboard summary with platform-wide metrics
    /// Admin only - shows aggregated data from all sales agents
    /// </summary>
    /// <param name="period">Optional period filter: "day", "week", "month", "year". If null/empty, returns all-time data.</param>
    /// <returns>Admin dashboard summary with platform metrics</returns>
    Task<AdminDashboardSummaryResponse> GetAdminSummaryAsync(string? period = null);

    /// <summary>
    /// Get admin revenue & commission chart data
    /// Admin only - shows platform-wide revenue and commission trends
    /// </summary>
    /// <param name="period">Chart period: "day" (hourly), "week" (daily), "month" (daily), "year" (monthly)</param>
    /// <returns>Chart data with labels, revenue data, and commission data</returns>
    Task<AdminRevenueChartResponse> GetAdminRevenueChartAsync(string period = "week");

    /// <summary>
    /// Get consolidated admin reports data (Admin only)
    /// Returns all report metrics in a single API call for optimal performance
    /// </summary>
    /// <param name="from">Start date (inclusive)</param>
    /// <param name="to">End date (inclusive)</param>
    /// <param name="categoryId">Optional category filter for product summary</param>
    /// <param name="pageNumber">Page number for product summary (default: 1)</param>
    /// <param name="pageSize">Page size for product summary (default: 10)</param>
    /// <returns>Consolidated admin reports data</returns>
    Task<AdminReportsResponse> GetAdminReportsAsync(
        DateTime from,
        DateTime to,
        Guid? categoryId = null,
        int pageNumber = 1,
        int pageSize = 10);

    /// <summary>
    /// Get sales agent personal reports (Sales Agent only)
    /// Returns revenue trends, orders by category, and top products for the current sales agent
    /// </summary>
    /// <param name="period">Report period: "day", "week", "month", "year"</param>
    /// <param name="categoryId">Optional category filter</param>
    /// <returns>Sales agent reports data</returns>
    Task<SalesAgentReportsResponse> GetSalesAgentReportsAsync(string period, Guid? categoryId = null);
}
