namespace MyShop.Shared.DTOs.Requests;

/// <summary>
/// Request DTO for adding product to cart
/// </summary>
public class AddToCartRequest
{
    /// <summary>
    /// Product ID to add
    /// </summary>
    public Guid ProductId { get; set; }

    /// <summary>
    /// Quantity to add (default: 1)
    /// </summary>
    public int Quantity { get; set; } = 1;
}
