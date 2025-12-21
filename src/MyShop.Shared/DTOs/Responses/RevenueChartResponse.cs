namespace MyShop.Shared.DTOs.Responses;

/// <summary>
/// Response DTO for revenue chart data
/// </summary>
public class RevenueChartResponse
{
    /// <summary>
    /// Chart labels (e.g., ["Mon", "Tue", "Wed"] for week, ["Jan", "Feb"] for year)
    /// </summary>
    public List<string> Labels { get; set; } = new();

    /// <summary>
    /// Revenue data corresponding to each label
    /// </summary>
    public List<decimal> Data { get; set; } = new();
}
