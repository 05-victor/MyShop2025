using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyShop.Core.Interfaces.Repositories;

/// <summary>
/// Repository interface for sales reports and analytics
/// </summary>
public interface IReportRepository
{
    /// <summary>
    /// Get sales report for a specific sales agent
    /// </summary>
    Task<SalesReport> GetSalesReportAsync(Guid salesAgentId, DateTime? startDate = null, DateTime? endDate = null);

    /// <summary>
    /// Get performance metrics for a sales agent
    /// </summary>
    Task<PerformanceMetrics> GetPerformanceMetricsAsync(Guid salesAgentId);

    /// <summary>
    /// Get top performing products for a sales agent
    /// </summary>
    Task<IEnumerable<ProductPerformance>> GetTopProductsAsync(Guid salesAgentId, int topCount = 10);

    /// <summary>
    /// Get sales trend data (daily, weekly, monthly)
    /// </summary>
    Task<SalesTrend> GetSalesTrendAsync(Guid salesAgentId, string period = "monthly");
}

/// <summary>
/// Sales report with summary statistics
/// </summary>
public class SalesReport
{
    public Guid SalesAgentId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TotalOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal TotalCommission { get; set; }
    public int CompletedOrders { get; set; }
    public int PendingOrders { get; set; }
    public int CancelledOrders { get; set; }
    public decimal AverageOrderValue { get; set; }
    public decimal ConversionRate { get; set; } // Percentage
}

/// <summary>
/// Performance metrics for a sales agent
/// </summary>
public class PerformanceMetrics
{
    public Guid SalesAgentId { get; set; }
    public int TotalProductsShared { get; set; }
    public int TotalClicks { get; set; }
    public int TotalOrders { get; set; }
    public decimal ConversionRate { get; set; } // (Orders / Clicks) * 100
    public decimal TotalRevenue { get; set; }
    public decimal TotalCommission { get; set; }
    public decimal AverageOrderValue { get; set; }
    public string PerformanceRank { get; set; } = "N/A"; // Top 10%, Top 25%, etc.
}

/// <summary>
/// Product performance for a sales agent
/// </summary>
public class ProductPerformance
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public int TotalSold { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal TotalCommission { get; set; }
    public int Clicks { get; set; }
    public decimal ConversionRate { get; set; }
}

/// <summary>
/// Sales trend data for charting
/// </summary>
public class SalesTrend
{
    public string Period { get; set; } = "monthly"; // daily, weekly, monthly
    public List<string> Labels { get; set; } = new();
    public List<decimal> RevenueData { get; set; } = new();
    public List<int> OrdersData { get; set; } = new();
    public List<decimal> CommissionData { get; set; } = new();
}
