using System.ComponentModel.DataAnnotations;

namespace MyShop.Shared.DTOs.Requests;

/// <summary>
/// Request DTO for updating an existing order
/// </summary>
public class UpdateOrderRequest
{
    public string? Status { get; set; }
    public string? PaymentStatus { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Total amount must be non-negative")]
    public int? TotalAmount { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Discount amount must be non-negative")]
    public int? DiscountAmount { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Shipping fee must be non-negative")]
    public int? ShippingFee { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Tax amount must be non-negative")]
    public int? TaxAmount { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Grand total must be non-negative")]
    public int? GrandTotal { get; set; }

    [MaxLength(500, ErrorMessage = "Note cannot exceed 500 characters")]
    public string? Note { get; set; }

    /// <summary>
    /// Optional: Update the sale agent ID
    /// </summary>
    public Guid? SaleAgentId { get; set; }
}
