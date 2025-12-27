namespace MyShop.Shared.DTOs.Responses;

/// <summary>
/// Consolidated admin reports response containing all report data in a single API call
/// Reduces HTTP overhead and ensures data consistency across all metrics
/// </summary>
public class AdminReportsResponse
{
    /// <summary>
    /// Date range period for the report
    /// </summary>
    public PeriodInfo Period { get; set; } = new();

    /// <summary>
    /// Revenue trend data (daily revenue over the period)
    /// </summary>
    public List<RevenueTrendItem> RevenueTrend { get; set; } = new();

    /// <summary>
    /// Order distribution by category
    /// </summary>
    public List<OrdersByCategoryItem> OrdersByCategory { get; set; } = new();

    /// <summary>
    /// Product rating distribution analysis
    /// </summary>
    public ProductRatingAnalysis ProductRatings { get; set; } = new();

    /// <summary>
    /// Top performing sales agents by revenue
    /// </summary>
    public List<SalespersonContribution> SalespersonContributions { get; set; } = new();

    /// <summary>
    /// Product performance summary with pagination
    /// </summary>
    public PagedProductSummary ProductSummary { get; set; } = new();
}

/// <summary>
/// Date range information for the report period
/// </summary>
public class PeriodInfo
{
    public DateTime From { get; set; }
    public DateTime To { get; set; }
}

/// <summary>
/// Revenue data point for a specific date
/// </summary>
public class RevenueTrendItem
{
    /// <summary>
    /// Date in yyyy-MM-dd format
    /// </summary>
    public string Date { get; set; } = string.Empty;

    /// <summary>
    /// Total revenue for the date (in VND)
    /// </summary>
    public decimal Revenue { get; set; }

    /// <summary>
    /// Number of orders for the date
    /// </summary>
    public int OrderCount { get; set; }

    /// <summary>
    /// Average order value (Revenue / OrderCount)
    /// </summary>
    public decimal AverageOrderValue { get; set; }
}

/// <summary>
/// Order statistics by product category
/// </summary>
public class OrdersByCategoryItem
{
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int OrderCount { get; set; }
    public decimal Revenue { get; set; }
    
    /// <summary>
    /// Percentage of total revenue
    /// </summary>
    public decimal Percentage { get; set; }
}

/// <summary>
/// Product rating distribution analysis
/// </summary>
public class ProductRatingAnalysis
{
    /// <summary>
    /// Number of 5-star ratings
    /// </summary>
    public int Excellent { get; set; }
    
    /// <summary>
    /// Number of 4-star ratings
    /// </summary>
    public int VeryGood { get; set; }
    
    /// <summary>
    /// Number of 3-star ratings
    /// </summary>
    public int Good { get; set; }
    
    /// <summary>
    /// Number of 2-star ratings
    /// </summary>
    public int Fair { get; set; }
    
    /// <summary>
    /// Number of 1-star ratings
    /// </summary>
    public int Poor { get; set; }
    
    /// <summary>
    /// Total number of ratings
    /// </summary>
    public int TotalRatings { get; set; }
    
    /// <summary>
    /// Average rating (weighted average)
    /// </summary>
    public decimal AverageRating { get; set; }
}

/// <summary>
/// Sales agent contribution to platform revenue
/// </summary>
public class SalespersonContribution
{
    public Guid SalespersonId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    
    /// <summary>
    /// Avatar initials (e.g., "JD" for John Doe)
    /// </summary>
    public string Avatar { get; set; } = string.Empty;
    
    /// <summary>
    /// Total number of sales
    /// </summary>
    public int TotalSales { get; set; }
    
    /// <summary>
    /// Total revenue generated (GMV)
    /// </summary>
    public decimal TotalRevenue { get; set; }
    
    /// <summary>
    /// Commission earned (typically 5% of revenue)
    /// </summary>
    public decimal Commission { get; set; }
    
    /// <summary>
    /// Percentage of total platform revenue
    /// </summary>
    public decimal Percentage { get; set; }
}

/// <summary>
/// Paginated product summary with performance metrics
/// </summary>
public class PagedProductSummary
{
    public List<ProductSummaryItem> Data { get; set; } = new();
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
}

/// <summary>
/// Product performance summary item
/// </summary>
public class ProductSummaryItem
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public int TotalOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AverageRating { get; set; }
    
    /// <summary>
    /// Stock status: IN_STOCK, LOW_STOCK, OUT_OF_STOCK, DISCONTINUED
    /// </summary>
    public string Status { get; set; } = "IN_STOCK";
    
    /// <summary>
    /// Status color for UI display (hex code)
    /// </summary>
    public string StatusColor { get; set; } = "#10b981";
    
    public int StockLevel { get; set; }
    public int LowStockThreshold { get; set; } = 10;
}
