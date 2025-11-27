namespace MyShop.Shared.Models;

/// <summary>
/// Dashboard summary with all KPIs and analytics data
/// </summary>
public class DashboardSummary
{
    public DateTime Date { get; set; }
    public int TotalProducts { get; set; }
    public int TodayOrders { get; set; }
    public decimal TodayRevenue { get; set; }
    public decimal WeekRevenue { get; set; }
    public decimal MonthRevenue { get; set; }
    public List<LowStockProduct> LowStockProducts { get; set; } = new();
    public List<TopSellingProduct> TopSellingProducts { get; set; } = new();
    public List<RecentOrder> RecentOrders { get; set; } = new();
    public List<CategorySales> SalesByCategory { get; set; } = new();
}

/// <summary>
/// Product with low stock that needs attention
/// </summary>
public class LowStockProduct
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? CategoryName { get; set; }
    public int Quantity { get; set; }
    public string? ImageUrl { get; set; }
    public string Status { get; set; } = string.Empty;
}

/// <summary>
/// Top selling product with sales statistics
/// </summary>
public class TopSellingProduct
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? CategoryName { get; set; }
    public int SoldCount { get; set; }
    public decimal Revenue { get; set; }
    public string? ImageUrl { get; set; }
}

/// <summary>
/// Recent order summary for dashboard display
/// </summary>
public class RecentOrder
{
    public Guid Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? SalesAgentName { get; set; }
}

/// <summary>
/// Sales statistics grouped by category
/// </summary>
public class CategorySales
{
    public string CategoryName { get; set; } = string.Empty;
    public decimal TotalRevenue { get; set; }
    public int OrderCount { get; set; }
    public double Percentage { get; set; }
}

/// <summary>
/// Revenue chart data with labels and values
/// </summary>
public class RevenueChartData
{
    public List<string> Labels { get; set; } = new();
    public List<decimal> Data { get; set; } = new();
}

/// <summary>
/// Top performing sales agent with performance metrics
/// </summary>
public class TopSalesAgent
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Avatar { get; set; } = string.Empty;
    public decimal GMV { get; set; } // Gross Merchandise Value
    public decimal Commission { get; set; }
    public int OrderCount { get; set; }
    public double Rating { get; set; }
    public string Status { get; set; } = string.Empty;
}
