namespace MyShop.Shared.DTOs.Responses;

/// <summary>
/// Response DTO for sales agent earnings summary
/// </summary>
public class EarningsSummaryResponse
{
    /// <summary>
    /// Total earnings (sum of all order amounts for this agent)
    /// </summary>
    public decimal TotalEarnings { get; set; }

    /// <summary>
    /// Total platform fees (TotalEarnings * PlatformFee)
    /// </summary>
    public decimal TotalPlatformFees { get; set; }

    /// <summary>
    /// Net earnings after platform fees (TotalEarnings - TotalPlatformFees)
    /// </summary>
    public decimal NetEarnings { get; set; }

    /// <summary>
    /// Pending earnings (orders not yet paid)
    /// </summary>
    public decimal PendingEarnings { get; set; }

    /// <summary>
    /// Paid earnings (orders marked as paid)
    /// </summary>
    public decimal PaidEarnings { get; set; }

    /// <summary>
    /// Total number of orders
    /// </summary>
    public int TotalOrders { get; set; }

    /// <summary>
    /// Average earnings per order
    /// </summary>
    public decimal AverageEarningsPerOrder { get; set; }

    /// <summary>
    /// Earnings from this month (current month)
    /// </summary>
    public decimal ThisMonthEarnings { get; set; }

    /// <summary>
    /// Earnings from last month
    /// </summary>
    public decimal LastMonthEarnings { get; set; }

    /// <summary>
    /// Platform fee rate (e.g., 0.10 for 10%)
    /// </summary>
    public decimal PlatformFeeRate { get; set; }
}
