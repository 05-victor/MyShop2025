namespace MyShop.Shared.DTOs.Responses;

/// <summary>
/// Response DTO for cart data
/// </summary>
public class CartResponse
{
    public Guid UserId { get; set; }
    public List<CartItemResponse> Items { get; set; } = new();
    public decimal Subtotal { get; set; }
    public decimal Tax { get; set; }
    public decimal ShippingFee { get; set; }
    public decimal Total { get; set; }
    public int ItemCount { get; set; }
}

/// <summary>
/// Cart item in response
/// </summary>
public class CartItemResponse
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ProductImage { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public decimal Subtotal { get; set; }
    public int StockAvailable { get; set; }
    public string? CategoryName { get; set; }
    public DateTime? AddedAt { get; set; }
}
