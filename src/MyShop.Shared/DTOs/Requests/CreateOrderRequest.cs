namespace MyShop.Shared.DTOs.Requests;

/// <summary>
/// Request DTO for creating a new order
/// </summary>
public class CreateOrderRequest
{
    /// <summary>
    /// List of order items (product ID + quantity)
    /// </summary>
    public List<OrderItemRequest> Items { get; set; } = new();

    /// <summary>
    /// Shipping address
    /// </summary>
    public string ShippingAddress { get; set; } = string.Empty;

    /// <summary>
    /// Optional order notes
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Payment method (COD, CreditCard, etc.)
    /// </summary>
    public string PaymentMethod { get; set; } = "COD";
}

/// <summary>
/// Order item (product + quantity)
/// </summary>
public class OrderItemRequest
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}
