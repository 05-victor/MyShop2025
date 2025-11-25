namespace MyShop.Shared.Models;

/// <summary>
/// Represents a commission earned by a sales agent
/// </summary>
public class Commission
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Guid SalesAgentId { get; set; }
    public string SalesAgentName { get; set; } = string.Empty;
    public string OrderNumber { get; set; } = string.Empty;
    public decimal OrderAmount { get; set; }
    public decimal CommissionRate { get; set; }
    public decimal CommissionAmount { get; set; }
    public decimal Amount { get; set; } // Alias for CommissionAmount
    public string Status { get; set; } = "Pending"; // Pending, Approved, Paid
    public DateTime CreatedDate { get; set; }
    public DateTime CreatedAt { get; set; } // Alias for CreatedDate
    public DateTime? PaidDate { get; set; }
    public DateTime? PaidAt { get; set; } // Alias for PaidDate
}

/// <summary>
/// Summary of commission earnings for a sales agent
/// </summary>
public class CommissionSummary
{
    public decimal TotalEarnings { get; set; }
    public decimal PendingCommission { get; set; }
    public decimal PaidCommission { get; set; }
    public int TotalOrders { get; set; }
    public decimal AverageCommission { get; set; }
    public decimal ThisMonthEarnings { get; set; }
    public decimal LastMonthEarnings { get; set; }
}
