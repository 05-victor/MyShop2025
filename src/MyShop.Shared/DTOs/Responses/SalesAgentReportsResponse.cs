namespace MyShop.Shared.DTOs.Responses;

/// <summary>
/// Response DTO for Sales Agent personal reports
/// Contains revenue trends, orders by category, and top products for the agent
/// </summary>
public class SalesAgentReportsResponse
{
    /// <summary>
    /// Revenue trend data over the selected period
    /// </summary>
    public List<SalesAgentRevenueTrendItem> RevenueTrend { get; set; } = new();

    /// <summary>
    /// Order distribution by product category
    /// </summary>
    public List<SalesAgentOrdersByCategoryItem> OrdersByCategory { get; set; } = new();

    /// <summary>
    /// Top performing products for this sales agent
    /// </summary>
    public List<SalesAgentTopProduct> TopProducts { get; set; } = new();
}

/// <summary>
/// Revenue data point for a specific time period (hour/day/week/month)
/// </summary>
public class SalesAgentRevenueTrendItem
{
    /// <summary>
    /// Date/time label (format depends on period)
    /// - day: "00:00-03:00", "03:00-06:00", etc.
    /// - week: "Mon", "Tue", etc.
    /// - month: "Week 1", "Week 2", etc.
    /// - year: "January", "February", etc.
    /// </summary>
    public string Date { get; set; } = string.Empty;

    /// <summary>
    /// Total revenue for this period (VND)
    /// </summary>
    public decimal Revenue { get; set; }

    /// <summary>
    /// Number of orders in this period
    /// </summary>
    public int OrderCount { get; set; }

    /// <summary>
    /// Average order value (Revenue / OrderCount)
    /// </summary>
    public decimal AverageOrderValue { get; set; }
}

/// <summary>
/// Order statistics by product category for sales agent
/// </summary>
public class SalesAgentOrdersByCategoryItem
{
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int OrderCount { get; set; }
    public decimal Revenue { get; set; }
    
    /// <summary>
    /// Percentage of total revenue
    /// </summary>
    public decimal Percentage { get; set; }
    
    /// <summary>
    /// Commission earned from this category (agent's share: 95% of revenue)
    /// </summary>
    public decimal Commission { get; set; }
}

/// <summary>
/// Top performing product for sales agent
/// </summary>
public class SalesAgentTopProduct
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    
    /// <summary>
    /// Number of units sold
    /// </summary>
    public int UnitsSold { get; set; }
    
    /// <summary>
    /// Total revenue from this product
    /// </summary>
    public decimal Revenue { get; set; }
    
    /// <summary>
    /// Average rating (placeholder - will be 0 until rating system is implemented)
    /// </summary>
    public decimal AverageRating { get; set; }
    
    /// <summary>
    /// Percentage of total revenue
    /// </summary>
    public decimal Percentage { get; set; }
}
