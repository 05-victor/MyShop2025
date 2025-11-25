namespace MyShop.Shared.Models;

/// <summary>
/// Sales report for a sales agent
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
    public decimal ConversionRate { get; set; }
    public List<DailySales> DailySales { get; set; } = new();
}

/// <summary>
/// Daily sales data for reports
/// </summary>
public class DailySales
{
    public DateTime Date { get; set; }
    public int OrderCount { get; set; }
    public decimal Revenue { get; set; }
}

/// <summary>
/// Product report with top selling products
/// </summary>
public class ProductReport
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TotalProductsSold { get; set; }
    public List<ProductSalesData> TopSellingProducts { get; set; } = new();
}

/// <summary>
/// Product sales data for reports
/// </summary>
public class ProductSalesData
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int UnitsSold { get; set; }
    public decimal Revenue { get; set; }
}

/// <summary>
/// Commission report for sales agents
/// </summary>
public class CommissionReport
{
    public Guid SalesAgentId { get; set; }
    public string SalesAgentName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TotalOrders { get; set; }
    public decimal TotalSales { get; set; }
    public decimal TotalCommission { get; set; }
    public decimal PaidCommission { get; set; }
    public decimal PendingCommission { get; set; }
}

/// <summary>
/// Inventory report
/// </summary>
public class InventoryReport
{
    public int TotalProducts { get; set; }
    public decimal TotalValue { get; set; }
    public int LowStockCount { get; set; }
    public int OutOfStockCount { get; set; }
    public int OverstockCount { get; set; }
    public DateTime GeneratedAt { get; set; }
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
    public decimal ConversionRate { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal TotalCommission { get; set; }
    public decimal AverageOrderValue { get; set; }
    public string PerformanceRank { get; set; } = string.Empty;
}

/// <summary>
/// Product performance data for reports
/// </summary>
public class ProductPerformance
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? CategoryName { get; set; }
    public int TotalSold { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal TotalCommission { get; set; }
    public int Clicks { get; set; }
    public decimal ConversionRate { get; set; }
}

/// <summary>
/// Sales trend data for charts
/// </summary>
public class SalesTrend
{
    public string Period { get; set; } = string.Empty; // daily, weekly, monthly
    public List<string> Labels { get; set; } = new();
    public List<decimal> RevenueData { get; set; } = new();
    public List<int> OrdersData { get; set; } = new();
    public List<decimal> CommissionData { get; set; } = new();
}
