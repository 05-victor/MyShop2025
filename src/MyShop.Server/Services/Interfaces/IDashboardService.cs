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
}
