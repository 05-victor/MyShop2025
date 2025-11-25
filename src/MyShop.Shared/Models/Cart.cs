namespace MyShop.Shared.Models;

/// <summary>
/// Represents an item in the shopping cart
/// </summary>
public class CartItem
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ProductImage { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public string? CategoryName { get; set; }
    public int StockAvailable { get; set; }
    public DateTime AddedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Calculated subtotal for this cart item
    /// </summary>
    public decimal Subtotal => Price * Quantity;
}

/// <summary>
/// Summary of cart calculations including totals and fees
/// </summary>
public class CartSummary
{
    public int ItemCount { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Tax { get; set; }
    public decimal ShippingFee { get; set; }
    public decimal Total { get; set; }
}
