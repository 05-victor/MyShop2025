using System.Text.Json.Serialization;

namespace MyShop.Shared.DTOs.Requests;

/// <summary>
/// Request DTO for price (weekly sales) forecasting prediction
/// </summary>
public class PriceForecastRequest
{
    [JsonPropertyName("Store")]
    public int Store { get; set; }

    [JsonPropertyName("Dept")]
    public int Dept { get; set; }

    [JsonPropertyName("Date")]
    public string Date { get; set; } = string.Empty;

    [JsonPropertyName("strategy")]
    public string? Strategy { get; set; } = "linear";
}
