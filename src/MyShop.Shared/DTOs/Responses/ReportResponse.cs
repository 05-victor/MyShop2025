namespace MyShop.Shared.DTOs.Responses;

/// <summary>
/// Response DTO for sales report data
/// </summary>
public class SalesReportResponse
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
}

/// <summary>
/// Response DTO for performance metrics
/// </summary>
public class PerformanceMetricsResponse
{
    public Guid SalesAgentId { get; set; }
    public int TotalProductsShared { get; set; }
    public int TotalClicks { get; set; }
    public int TotalOrders { get; set; }
    public decimal ConversionRate { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal TotalCommission { get; set; }
    public decimal AverageOrderValue { get; set; }
    public string PerformanceRank { get; set; } = "N/A";
}

/// <summary>
/// Response DTO for product performance
/// </summary>
public class ProductPerformanceResponse
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
/// Response DTO for sales trend data
/// </summary>
public class SalesTrendResponse
{
    public string Period { get; set; } = "monthly";
    public List<string> Labels { get; set; } = new();
    public List<decimal> RevenueData { get; set; } = new();
    public List<int> OrdersData { get; set; } = new();
    public List<decimal> CommissionData { get; set; } = new();
}
