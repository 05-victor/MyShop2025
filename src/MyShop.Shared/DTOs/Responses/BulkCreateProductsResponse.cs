namespace MyShop.Shared.DTOs.Responses;

/// <summary>
/// Response DTO for bulk product creation
/// </summary>
public class BulkCreateProductsResponse
{
    /// <summary>
    /// Total number of products submitted
    /// </summary>
    public int TotalSubmitted { get; set; }

    /// <summary>
    /// Number of products successfully created
    /// </summary>
    public int SuccessCount { get; set; }

    /// <summary>
    /// Number of products that failed
    /// </summary>
    public int FailureCount { get; set; }

    /// <summary>
    /// List of successfully created products
    /// </summary>
    public List<ProductResponse> CreatedProducts { get; set; } = new();

    /// <summary>
    /// List of errors for failed products
    /// </summary>
    public List<BulkCreateError> Errors { get; set; } = new();
}

/// <summary>
/// Error details for a failed product creation
/// </summary>
public class BulkCreateError
{
    /// <summary>
    /// Index of the product in the request list (0-based)
    /// </summary>
    public int Index { get; set; }

    /// <summary>
    /// Product name or SKU for identification
    /// </summary>
    public string? ProductIdentifier { get; set; }

    /// <summary>
    /// Error message
    /// </summary>
    public string ErrorMessage { get; set; } = string.Empty;
}
