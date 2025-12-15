namespace MyShop.Shared.DTOs.Responses;

/// <summary>
/// Response DTO for cart grouped by sales agents
/// </summary>
public class GroupedCartResponse
{
    public Guid UserId { get; set; }
    public List<SalesAgentCartGroup> SalesAgentGroups { get; set; } = new();
    public decimal GrandTotal { get; set; }
    public int TotalItemCount { get; set; }
    public int TotalSalesAgents { get; set; }
}

/// <summary>
/// Cart items grouped by a single sales agent
/// </summary>
public class SalesAgentCartGroup
{
    public Guid SalesAgentId { get; set; }
    public string? SalesAgentUsername { get; set; }
    public string? SalesAgentFullName { get; set; }
    public List<CartItemResponse> Items { get; set; } = new();
    public decimal Subtotal { get; set; }
    public decimal Tax { get; set; }
    public decimal ShippingFee { get; set; }
    public decimal Total { get; set; }
    public int ItemCount { get; set; }
}
