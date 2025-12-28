using System.Text.Json.Serialization;

namespace MyShop.Shared.DTOs.Responses;

/// <summary>
/// Response DTO for demand forecasting prediction
/// </summary>
public class DemandForecastResponse
{
    [JsonPropertyName("predicted_units_sold")]
    public double PredictedUnitsSold { get; set; }

    [JsonPropertyName("strategy_used")]
    public string StrategyUsed { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;
}
