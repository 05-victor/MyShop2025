namespace MyShop.Shared.DTOs.Responses;

/// <summary>
/// Response DTO for sales agent dashboard summary
/// Contains overview information including products, orders, revenue statistics
/// </summary>
public class SalesAgentDashboardSummaryResponse
{
    /// <summary>
    /// Total number of products published by this sales agent
    /// </summary>
    public int TotalProducts { get; set; }

    /// <summary>
    /// Total number of orders for this sales agent
    /// </summary>
    public int TotalOrders { get; set; }

    /// <summary>
    /// Total revenue for this sales agent (based on selected period)
    /// </summary>
    public decimal TotalRevenue { get; set; }

    /// <summary>
    /// Today's orders count
    /// </summary>
    public int TodayOrders { get; set; }

    /// <summary>
    /// Today's revenue
    /// </summary>
    public decimal TodayRevenue { get; set; }

    /// <summary>
    /// This week's revenue
    /// </summary>
    public decimal WeekRevenue { get; set; }

    /// <summary>
    /// This month's revenue
    /// </summary>
    public decimal MonthRevenue { get; set; }

    /// <summary>
    /// This year's revenue
    /// </summary>
    public decimal YearRevenue { get; set; }

    /// <summary>
    /// Top 5 low stock products published by this sales agent
    /// </summary>
    public List<LowStockProductDto> LowStockProducts { get; set; } = new();

    /// <summary>
    /// Top 5 best-selling products published by this sales agent
    /// </summary>
    public List<TopSellingProductDto> TopSellingProducts { get; set; } = new();

    /// <summary>
    /// Top 5 recent orders for this sales agent
    /// </summary>
    public List<RecentOrderDto> RecentOrders { get; set; } = new();
}

/// <summary>
/// DTO for low stock product information
/// </summary>
public class LowStockProductDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? CategoryName { get; set; }
    public int Quantity { get; set; }
    public string? ImageUrl { get; set; }
    public string Status { get; set; } = string.Empty;
}

/// <summary>
/// DTO for top selling product information
/// </summary>
public class TopSellingProductDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? CategoryName { get; set; }
    public int SoldCount { get; set; }
    public decimal Revenue { get; set; }
    public string? ImageUrl { get; set; }
}

/// <summary>
/// DTO for recent order information
/// </summary>
public class RecentOrderDto
{
    public Guid Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
}
