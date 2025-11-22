namespace MyShop.Shared.DTOs.Requests;

/// <summary>
/// Request DTO for searching products
/// </summary>
public class SearchProductsRequest
{
    /// <summary>
    /// Search query (product name, description, or SKU)
    /// </summary>
    public string? Query { get; set; }

    /// <summary>
    /// Filter by category ID
    /// </summary>
    public Guid? CategoryId { get; set; }

    /// <summary>
    /// Minimum price filter
    /// </summary>
    public decimal? MinPrice { get; set; }

    /// <summary>
    /// Maximum price filter
    /// </summary>
    public decimal? MaxPrice { get; set; }

    /// <summary>
    /// Filter by availability (in stock)
    /// </summary>
    public bool? InStockOnly { get; set; }

    /// <summary>
    /// Sort field (name, price, createdAt)
    /// </summary>
    public string? SortBy { get; set; }

    /// <summary>
    /// Sort direction (asc, desc)
    /// </summary>
    public string? SortOrder { get; set; }

    /// <summary>
    /// Page number (default: 1)
    /// </summary>
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// Page size (default: 20)
    /// </summary>
    public int PageSize { get; set; } = 20;
}
