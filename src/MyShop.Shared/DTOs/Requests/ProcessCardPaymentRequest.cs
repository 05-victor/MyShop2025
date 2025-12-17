using System.ComponentModel.DataAnnotations;

namespace MyShop.Shared.DTOs.Requests;

/// <summary>
/// Request DTO for processing card payment
/// </summary>
public class ProcessCardPaymentRequest
{
    [Required(ErrorMessage = "Order ID is required")]
    public required Guid OrderId { get; set; }

    [Required(ErrorMessage = "Card number is required")]
    [RegularExpression(@"^\d{4}\s\d{4}\s\d{4}\s\d{4}$", ErrorMessage = "Card number must be in format: #### #### #### ####")]
    public required string CardNumber { get; set; }

    [Required(ErrorMessage = "Card holder name is required")]
    [MaxLength(100, ErrorMessage = "Card holder name cannot exceed 100 characters")]
    public required string CardHolderName { get; set; }

    [Required(ErrorMessage = "Expiry date is required")]
    [RegularExpression(@"^(0[1-9]|1[0-2])\/\d{2}$", ErrorMessage = "Expiry date must be in format: MM/YY")]
    public required string ExpiryDate { get; set; }

    [Required(ErrorMessage = "CVV is required")]
    [RegularExpression(@"^\d{3,4}$", ErrorMessage = "CVV must be 3 or 4 digits")]
    public required string Cvv { get; set; }
}
