namespace MyShop.Shared.DTOs.Responses;

/// <summary>
/// Response DTO for order information
/// </summary>
public class OrderResponse
{
    public Guid Id { get; set; }
    public DateTime OrderDate { get; set; }
    public string? Status { get; set; }
    public string? PaymentStatus { get; set; }
    public string? PaymentMethod { get; set; }
    public int TotalAmount { get; set; }
    public int DiscountAmount { get; set; }
    public int ShippingFee { get; set; }
    public int TaxAmount { get; set; }
    public int GrandTotal { get; set; }
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Customer information
    public Guid CustomerId { get; set; }
    public string? CustomerUsername { get; set; }
    public string? CustomerFullName { get; set; }
    public string? CustomerEmail { get; set; }

    // Sale Agent information
    public Guid SaleAgentId { get; set; }
    public string? SaleAgentUsername { get; set; }
    public string? SaleAgentFullName { get; set; }

    // Order items
    public List<OrderItemResponse>? OrderItems { get; set; }
}

/// <summary>
/// Response DTO for order item information
/// </summary>
public class OrderItemResponse
{
    public Guid Id { get; set; }
    public int Quantity { get; set; }
    public int UnitSalePrice { get; set; }
    public int TotalPrice { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Product information
    public Guid ProductId { get; set; }
    public string? ProductName { get; set; }
    public string? ProductSKU { get; set; }
}