using MyShop.Shared.DTOs.Requests;
using MyShop.Shared.DTOs.Responses;

namespace MyShop.Server.Services.Interfaces;

/// <summary>
/// Service interface for forecasting operations
/// </summary>
public interface IForecastService
{
    Task<DemandForecastResponse> PredictDemandAsync(DemandForecastRequest request);
    Task<PriceForecastResponse> PredictPriceAsync(PriceForecastRequest request);
    Task<bool> CheckHealthAsync();
}
