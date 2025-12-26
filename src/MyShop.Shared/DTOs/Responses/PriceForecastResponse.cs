using System.Text.Json.Serialization;

namespace MyShop.Shared.DTOs.Responses;

/// <summary>
/// Response DTO for price (weekly sales) forecasting prediction
/// </summary>
public class PriceForecastResponse
{
    [JsonPropertyName("predicted_weekly_sales")]
    public double PredictedWeeklySales { get; set; }

    [JsonPropertyName("store")]
    public int Store { get; set; }

    [JsonPropertyName("dept")]
    public int Dept { get; set; }

    [JsonPropertyName("date")]
    public string Date { get; set; } = string.Empty;

    [JsonPropertyName("strategy_used")]
    public string StrategyUsed { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;
}
