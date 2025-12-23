namespace MyShop.Shared.DTOs.Responses;

/// <summary>
/// Response DTO for Admin Revenue & Commission chart data
/// Contains dual-series data for revenue and commission visualization
/// </summary>
public class AdminRevenueChartResponse
{
    /// <summary>
    /// Chart labels (e.g., ["0", "1", "2"...] for hours, ["Mon", "Tue"] for week)
    /// Granularity depends on the period:
    /// - day: Hour numbers 0-23
    /// - week: Day names (Mon-Sun)
    /// - month: Day numbers 1-31
    /// - year: Month names (Jan-Dec)
    /// </summary>
    public List<string> Labels { get; set; } = new();

    /// <summary>
    /// Total revenue for each period (across all sales agents)
    /// </summary>
    public List<decimal> RevenueData { get; set; } = new();

    /// <summary>
    /// Admin commission for each period (typically 5% of revenue)
    /// </summary>
    public List<decimal> CommissionData { get; set; } = new();
}
