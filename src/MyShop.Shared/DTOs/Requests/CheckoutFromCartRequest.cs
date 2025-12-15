using System.ComponentModel.DataAnnotations;

namespace MyShop.Shared.DTOs.Requests;

/// <summary>
/// Request DTO for checkout from cart
/// </summary>
public class CheckoutFromCartRequest
{
    [Required(ErrorMessage = "Shipping address is required")]
    [MaxLength(500, ErrorMessage = "Shipping address cannot exceed 500 characters")]
    public required string ShippingAddress { get; set; }

    [MaxLength(500, ErrorMessage = "Note cannot exceed 500 characters")]
    public string? Note { get; set; }

    [Required(ErrorMessage = "Payment method is required")]
    public string PaymentMethod { get; set; } = "COD";

    [Range(0, int.MaxValue, ErrorMessage = "Discount amount must be non-negative")]
    public int DiscountAmount { get; set; } = 0;
}
