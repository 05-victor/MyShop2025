<<<<<<< HEAD
using System.ComponentModel.DataAnnotations;

=======
>>>>>>> master
namespace MyShop.Shared.DTOs.Requests;

/// <summary>
/// Request DTO for creating a new order
/// </summary>
public class CreateOrderRequest
{
<<<<<<< HEAD
    [Required(ErrorMessage = "Customer ID is required")]
    public required Guid CustomerId { get; set; }

    public string Status { get; set; } = "PENDING";

    public string PaymentStatus { get; set; } = "UNPAID";

    //[Range(0, int.MaxValue, ErrorMessage = "Total amount must be non-negative")]
    //public int TotalAmount { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Discount amount must be non-negative")]
    public int DiscountAmount { get; set; } = 0;

    [Range(0, int.MaxValue, ErrorMessage = "Shipping fee must be non-negative")]
    public int ShippingFee { get; set; } = 0;

    [Range(0, int.MaxValue, ErrorMessage = "Tax amount must be non-negative")]
    public int TaxAmount { get; set; } = 0;

    //[Range(0, int.MaxValue, ErrorMessage = "Grand total must be non-negative")]
    //public int GrandTotal { get; set; }

    [MaxLength(500, ErrorMessage = "Note cannot exceed 500 characters")]
    public string? Note { get; set; }

    /// <summary>
    /// Optional: Sale agent ID. If not provided, will be auto-assigned to current user
    /// </summary>
    public Guid? SaleAgentId { get; set; }

    /// <summary>
    /// List of order items to be created with the order
    /// </summary>
    public List<CreateOrderItemRequest>? OrderItems { get; set; }
}

/// <summary>
/// Request DTO for creating order items within an order
/// </summary>
public class CreateOrderItemRequest
{
    [Required(ErrorMessage = "Product ID is required")]
    public required Guid ProductId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
    public int Quantity { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Unit sale price must be non-negative")]
    public int UnitSalePrice { get; set; }

    //[Range(0, int.MaxValue, ErrorMessage = "Total price must be non-negative")]
    //public int TotalPrice { get; set; }
=======
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
>>>>>>> master
}
