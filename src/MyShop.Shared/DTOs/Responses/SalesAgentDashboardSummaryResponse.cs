namespace MyShop.Shared.DTOs.Responses;

using System.Text.Json.Serialization;

/// <summary>
/// Response DTO for sales agent dashboard summary
/// Contains overview information including products, orders, revenue statistics
/// </summary>
public class SalesAgentDashboardSummaryResponse
{
    /// <summary>
    /// Total number of products published by this sales agent (all time)
    /// </summary>
    [JsonPropertyName("totalProducts")]
    public int TotalProducts { get; set; }

    /// <summary>
    /// Total number of orders for the selected period
    /// If period is not specified, returns all-time total
    /// </summary>
    [JsonPropertyName("totalOrders")]
    public int TotalOrders { get; set; }

    /// <summary>
    /// Total revenue for the selected period
    /// If period is not specified, returns all-time total
    /// </summary>
    [JsonPropertyName("totalRevenue")]
    public decimal TotalRevenue { get; set; }

    /// <summary>
    /// Top 5 low stock products published by this sales agent
    /// </summary>
    [JsonPropertyName("lowStockProducts")]
    public List<LowStockProductDto> LowStockProducts { get; set; } = new();

    /// <summary>
    /// Top 5 best-selling products published by this sales agent
    /// </summary>
    [JsonPropertyName("topSellingProducts")]
    public List<TopSellingProductDto> TopSellingProducts { get; set; } = new();

    /// <summary>
    /// Top 5 recent orders for this sales agent
    /// </summary>
    [JsonPropertyName("recentOrders")]
    public List<RecentOrderDto> RecentOrders { get; set; } = new();
}

/// <summary>
/// DTO for low stock product information
/// </summary>
public class LowStockProductDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("categoryName")]
    public string? CategoryName { get; set; }

    [JsonPropertyName("quantity")]
    public int Quantity { get; set; }

    [JsonPropertyName("imageUrl")]
    public string? ImageUrl { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;
}

/// <summary>
/// DTO for top selling product information
/// </summary>
public class TopSellingProductDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("categoryName")]
    public string? CategoryName { get; set; }

    [JsonPropertyName("soldCount")]
    public int SoldCount { get; set; }

    [JsonPropertyName("revenue")]
    public decimal Revenue { get; set; }

    [JsonPropertyName("imageUrl")]
    public string? ImageUrl { get; set; }
}

/// <summary>
/// DTO for recent order information
/// </summary>
public class RecentOrderDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("customerName")]
    public string CustomerName { get; set; } = string.Empty;

    [JsonPropertyName("orderDate")]
    public DateTime OrderDate { get; set; }

    [JsonPropertyName("totalAmount")]
    public decimal TotalAmount { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;
}
