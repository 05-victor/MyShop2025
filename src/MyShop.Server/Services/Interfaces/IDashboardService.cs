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
    /// <param name="period">Period for revenue calculation: "day", "week", "month", "year"</param>
    /// <returns>Dashboard summary including products, orders, revenue, and analytics</returns>
    Task<SalesAgentDashboardSummaryResponse> GetSalesAgentSummaryAsync(string period = "month");
}
