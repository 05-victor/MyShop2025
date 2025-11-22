namespace MyShop.Shared.DTOs.Requests;

/// <summary>
/// Request DTO for updating order status
/// </summary>
public class UpdateOrderStatusRequest
{
    /// <summary>
    /// New order status (Pending, Processing, Shipped, Delivered, Cancelled)
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Optional status change notes
    /// </summary>
    public string? Notes { get; set; }
}
