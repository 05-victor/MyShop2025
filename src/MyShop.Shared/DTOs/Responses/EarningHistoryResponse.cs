namespace MyShop.Shared.DTOs.Responses;

/// <summary>
/// Response DTO for individual earning record (order-based)
/// </summary>
public class EarningHistoryResponse
{
    /// <summary>
    /// Order ID
    /// </summary>
    public Guid OrderId { get; set; }

    /// <summary>
    /// Order code/number for display
    /// </summary>
    public string OrderCode { get; set; } = string.Empty;

    /// <summary>
    /// Customer name
    /// </summary>
    public string CustomerName { get; set; } = string.Empty;

    /// <summary>
    /// Order date
    /// </summary>
    public DateTime OrderDate { get; set; }

    /// <summary>
    /// Order status (Pending, Confirmed, Delivered, etc.)
    /// </summary>
    public string OrderStatus { get; set; } = string.Empty;

    /// <summary>
    /// Payment status (Unpaid, Paid, etc.)
    /// </summary>
    public string PaymentStatus { get; set; } = string.Empty;

    /// <summary>
    /// Total order amount (GrandTotal from order)
    /// </summary>
    public decimal OrderAmount { get; set; }

    /// <summary>
    /// Platform fee for this order (OrderAmount * PlatformFeeRate)
    /// </summary>
    public decimal PlatformFee { get; set; }

    /// <summary>
    /// Net earnings for this order (OrderAmount - PlatformFee)
    /// </summary>
    public decimal NetEarnings { get; set; }
}
