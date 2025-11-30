namespace MyShop.Shared.DTOs.Responses;

/// <summary>
/// Response DTO for order details
/// </summary>
public class OrderResponse
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public Guid? SalesAgentId { get; set; }
    public string? SalesAgentName { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public ShippingAddressResponse? ShippingAddress { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<OrderItemResponse> Items { get; set; } = new();
}

public class ShippingAddressResponse
{
    public string? Street { get; set; }
    public string? District { get; set; }
    public string? City { get; set; }
    public string? PostalCode { get; set; }
}

public class TrackingResponse
{
    public string? Carrier { get; set; }
    public string? TrackingNumber { get; set; }
}

/// <summary>
/// Order item in response
/// </summary>
public class OrderItemResponse
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ProductImage { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal Subtotal { get; set; }
}
