using MyShop.Core.Common;
using MyShop.Shared.Models;

namespace MyShop.Core.Interfaces.Facades;

/// <summary>
/// Facade pattern for dashboard operations
/// Aggregates: IDashboardRepository, IProductRepository, IOrderRepository, IUserRepository, IToastService
/// </summary>
public interface IDashboardFacade
{
    /// <summary>
    /// Load dashboard summary for specific period
    /// </summary>
    /// <param name="period">"current", "last", "3months", "6months", "year"</param>
    Task<Result<DashboardSummary>> LoadDashboardAsync(string period = "current");

    /// <summary>
    /// Get revenue chart data for period
    /// </summary>
    Task<Result<RevenueChartData>> GetRevenueChartDataAsync(string period = "current");

    /// <summary>
    /// Get top selling products
    /// </summary>
    Task<Result<List<TopSellingProduct>>> GetTopSellingProductsAsync(int top = 10);

    /// <summary>
    /// Get low stock products
    /// </summary>
    Task<Result<List<LowStockProduct>>> GetLowStockProductsAsync(int threshold = 10);

    /// <summary>
    /// Get recent orders
    /// </summary>
    Task<Result<List<RecentOrder>>> GetRecentOrdersAsync(int count = 10);

    /// <summary>
    /// Get sales by category
    /// </summary>
    Task<Result<List<CategorySales>>> GetSalesByCategoryAsync(string period = "current");

    /// <summary>
    /// Get top performing sales agents
    /// </summary>
    Task<Result<List<TopSalesAgent>>> GetTopSalesAgentsAsync(string period = "current", int topCount = 5);

    /// <summary>
    /// Get admin dashboard summary (admin-only)
    /// </summary>
    /// <param name="period">Optional period: "day", "week", "month", "year". If null, returns all-time data.</param>
    Task<Result<MyShop.Shared.DTOs.Responses.AdminDashboardSummaryResponse>> GetAdminSummaryAsync(string? period = null);

    /// <summary>
    /// Get admin revenue chart with commission data (admin-only)
    /// </summary>
    /// <param name="period">Period type: "day", "week", "month", "year"</param>
    Task<Result<MyShop.Shared.DTOs.Responses.AdminRevenueChartResponse>> GetAdminRevenueChartAsync(string period = "week");

    /// <summary>
    /// Export dashboard data to CSV
    /// </summary>
    Task<Result<string>> ExportDashboardDataAsync(string period = "current");

    /// <summary>
    /// Navigate to specific dashboard page (Products, Users, Reports, etc.)
    /// </summary>
    Task NavigateToDashboardPageAsync(string pageName);
}
