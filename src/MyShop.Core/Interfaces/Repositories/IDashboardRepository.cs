namespace MyShop.Core.Interfaces.Repositories;

/// <summary>
/// Repository interface for dashboard analytics and statistics
/// </summary>
public interface IDashboardRepository
{
    /// <summary>
    /// Get dashboard summary statistics (KPIs, low stock, top selling, recent orders)
    /// </summary>
    Task<DashboardSummary?> GetSummaryAsync();

    /// <summary>
    /// Get revenue chart data for specified period
    /// </summary>
    /// <param name="period">Period type: daily, weekly, monthly, yearly</param>
    Task<RevenueChartData?> GetRevenueChartAsync(string period);
}

// ===== Data Models for Dashboard =====

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

public class LowStockProduct
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? CategoryName { get; set; }
    public int Quantity { get; set; }
    public string? ImageUrl { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class TopSellingProduct
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? CategoryName { get; set; }
    public int SoldCount { get; set; }
    public decimal Revenue { get; set; }
    public string? ImageUrl { get; set; }
}

public class RecentOrder
{
    public Guid Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? SalesAgentName { get; set; }
}

public class CategorySales
{
    public string CategoryName { get; set; } = string.Empty;
    public decimal TotalRevenue { get; set; }
    public int OrderCount { get; set; }
    public double Percentage { get; set; }
}

public class RevenueChartData
{
    public List<string> Labels { get; set; } = new();
    public List<decimal> Data { get; set; } = new();
}
