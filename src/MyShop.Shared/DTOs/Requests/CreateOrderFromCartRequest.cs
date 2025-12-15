using System.ComponentModel.DataAnnotations;

namespace MyShop.Shared.DTOs.Requests;

/// <summary>
/// Request DTO for creating an order from cart items
/// </summary>
public class CreateOrderFromCartRequest
{
    [Required(ErrorMessage = "Customer ID is required")]
    public required Guid CustomerId { get; set; }

    [Required(ErrorMessage = "Sales agent ID is required")]
    public required Guid SalesAgentId { get; set; }

    [Required(ErrorMessage = "Shipping address is required")]
    [MaxLength(500, ErrorMessage = "Shipping address cannot exceed 500 characters")]
    public required string ShippingAddress { get; set; }

    [MaxLength(500, ErrorMessage = "Note cannot exceed 500 characters")]
    public string? Note { get; set; }

    [Required(ErrorMessage = "Payment method is required")]
    public string PaymentMethod { get; set; } = "COD";

    [Range(0, int.MaxValue, ErrorMessage = "Discount amount must be non-negative")]
    public int DiscountAmount { get; set; } = 0;

    /// <summary>
    /// List of cart items to be converted to order items
    /// </summary>
    [Required(ErrorMessage = "Cart items are required")]
    [MinLength(1, ErrorMessage = "At least one cart item is required")]
    public required List<CartItemForOrder> CartItems { get; set; }
}

/// <summary>
/// Cart item information for order creation
/// </summary>
public class CartItemForOrder
{
    [Required(ErrorMessage = "Product ID is required")]
    public required Guid ProductId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
    public int Quantity { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Unit price must be non-negative")]
    public int UnitPrice { get; set; }
}
