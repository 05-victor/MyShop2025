namespace MyShop.Shared.DTOs.Responses;

/// <summary>
/// Response DTO for order information
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
