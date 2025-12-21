namespace MyShop.Shared.DTOs.Responses;

/// <summary>
/// Response DTO for card payment processing
/// </summary>
public class ProcessCardPaymentResponse
{
    public Guid OrderId { get; set; }
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? PaymentStatus { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public string? TransactionId { get; set; }
}
