namespace MyShop.Shared.Models;

/// <summary>
/// Order entity model
/// </summary>
public class Order
{
    public Guid Id { get; set; }
    public string OrderCode { get; set; } = string.Empty;
    public Guid? SalesAgentId { get; set; }
    public string? SalesAgentName { get; set; }
    public Guid? CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string? CustomerPhone { get; set; }
    public string? CustomerAddress { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Discount { get; set; }
    public decimal FinalPrice { get; set; }
    public string Status { get; set; } = "CREATED";
    public string? PaymentStatus { get; set; }
    public string? CancelReason { get; set; }
    public string? Notes { get; set; }
    public DateTime OrderDate { get; set; }
    public DateTime? PaidDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<OrderItem> Items { get; set; } = new();
    public List<OrderItem> OrderItems { get; set; } = new();  // Alias for Items
}

/// <summary>
/// Order item (line item in an order)
/// </summary>
public class OrderItem
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ProductSKU { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Total { get; set; }
    public decimal TotalPrice { get; set; }  // Alias for Total
}
