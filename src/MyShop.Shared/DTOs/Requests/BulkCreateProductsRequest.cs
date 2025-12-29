namespace MyShop.Shared.DTOs.Requests;

/// <summary>
/// Request DTO for creating multiple products at once
/// </summary>
public class BulkCreateProductsRequest
{
    /// <summary>
    /// List of products to create
    /// </summary>
    public required List<CreateProductRequest> Products { get; set; }

    /// <summary>
    /// Whether to skip products that fail validation
    /// Default: true (continue with valid products)
    /// </summary>
    public bool SkipInvalidProducts { get; set; } = true;

    /// <summary>
    /// Whether to validate all products before inserting any
    /// Default: false (process one by one)
    /// </summary>
    public bool ValidateBeforeInsert { get; set; } = false;
}
