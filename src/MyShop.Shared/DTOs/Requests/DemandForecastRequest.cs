using System.Text.Json.Serialization;

namespace MyShop.Shared.DTOs.Requests;

/// <summary>
/// Request DTO for demand forecasting prediction
/// </summary>
public class DemandForecastRequest
{
    [JsonPropertyName("week")]
    public string Week { get; set; } = string.Empty;

    [JsonPropertyName("store_id")]
    public int StoreId { get; set; }

    [JsonPropertyName("sku_id")]
    public int SkuId { get; set; }

    [JsonPropertyName("base_price")]
    public double BasePrice { get; set; }

    [JsonPropertyName("total_price")]
    public double? TotalPrice { get; set; }

    [JsonPropertyName("is_featured_sku")]
    public int IsFeaturedSku { get; set; }

    [JsonPropertyName("is_display_sku")]
    public int IsDisplaySku { get; set; }
}
