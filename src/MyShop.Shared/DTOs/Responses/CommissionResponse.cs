namespace MyShop.Shared.DTOs.Responses;

/// <summary>
/// Response DTO for commission data
/// </summary>
public class CommissionResponse
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Guid SalesAgentId { get; set; }
    public string? SalesAgentName { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public decimal OrderAmount { get; set; }
    public decimal CommissionRate { get; set; }
    public decimal CommissionAmount { get; set; }
    public string Status { get; set; } = "Pending";
    public DateTime CreatedDate { get; set; }
    public DateTime? PaidDate { get; set; }
}

/// <summary>
/// Response DTO for commission summary
/// </summary>
public class CommissionSummaryResponse
{
    public decimal TotalEarnings { get; set; }
    public decimal PendingCommission { get; set; }
    public decimal PaidCommission { get; set; }
    public int TotalOrders { get; set; }
    public decimal AverageCommission { get; set; }
    public decimal ThisMonthEarnings { get; set; }
    public decimal LastMonthEarnings { get; set; }
}
