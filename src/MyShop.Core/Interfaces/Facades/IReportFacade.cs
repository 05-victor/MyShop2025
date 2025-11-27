using MyShop.Core.Common;
using MyShop.Shared.Models;

namespace MyShop.Core.Interfaces.Facades;

/// <summary>
/// Facade pattern for reports and analytics
/// Aggregates: IReportRepository, IOrderRepository, IProductRepository, ICommissionRepository
/// </summary>
public interface IReportFacade
{
    /// <summary>
    /// Get sales report for period
    /// </summary>
    Task<Result<SalesReport>> GetSalesReportAsync(string period = "current");

    /// <summary>
    /// Get product performance report
    /// </summary>
    Task<Result<List<ProductPerformance>>> GetProductPerformanceAsync(
        DateTime? startDate = null,
        DateTime? endDate = null,
        int top = 20);

    /// <summary>
    /// Get sales agent performance
    /// </summary>
    Task<Result<List<AgentPerformance>>> GetAgentPerformanceAsync(
        DateTime? startDate = null,
        DateTime? endDate = null);

    /// <summary>
    /// Get sales trend data for charts
    /// </summary>
    Task<Result<SalesTrend>> GetSalesTrendAsync(string period = "current");

    /// <summary>
    /// Get performance metrics
    /// </summary>
    Task<Result<PerformanceMetrics>> GetPerformanceMetricsAsync(string period = "current");

    /// <summary>
    /// Export sales report to CSV
    /// </summary>
    Task<Result<string>> ExportSalesReportAsync(string period = "current");

    /// <summary>
    /// Export product performance to CSV
    /// </summary>
    Task<Result<string>> ExportProductPerformanceAsync(DateTime? startDate = null, DateTime? endDate = null);
}

/// <summary>
/// Agent performance metrics
/// </summary>
public class AgentPerformance
{
    public Guid AgentId { get; set; }
    public string AgentName { get; set; } = string.Empty;
    public int TotalOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal TotalCommission { get; set; }
    public decimal ConversionRate { get; set; }
}
