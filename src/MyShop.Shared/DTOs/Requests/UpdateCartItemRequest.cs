namespace MyShop.Shared.DTOs.Requests;

/// <summary>
/// Request DTO for updating cart item quantity
/// </summary>
public class UpdateCartItemRequest
{
    /// <summary>
    /// New quantity (set to 0 to remove item)
    /// </summary>
    public int Quantity { get; set; }
}
