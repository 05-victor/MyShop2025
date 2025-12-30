using MyShop.Shared.DTOs.Common;
using MyShop.Shared.DTOs.Requests;
using MyShop.Shared.DTOs.Responses;
using Refit;

namespace MyShop.Plugins.API.Forecasts;

/// <summary>
/// API interface for forecast operations (Demand, Price/Revenue prediction)
/// </summary>
public interface IForecastApi
{
    /// <summary>
    /// Predict weekly revenue for current SalesAgent's department
    /// </summary>
    /// <param name="request">Price forecast request with date and optional strategy</param>
    /// <returns>Forecast response with predicted weekly sales value</returns>
    [Post("/api/v1/forecasts/my-revenue")]
    Task<Refit.ApiResponse<MyShop.Shared.DTOs.Common.ApiResponse<PriceForecastResponse>>> PredictMyRevenueAsync([Body] PriceForecastRequest request);

    /// <summary>
    /// Predict product demand for current SalesAgent's store
    /// </summary>
    /// <param name="request">Demand forecast request with product details</param>
    /// <returns>Forecast response with predicted units sold</returns>
    [Post("/api/v1/forecasts/my-demand")]
    Task<Refit.ApiResponse<MyShop.Shared.DTOs.Common.ApiResponse<DemandForecastResponse>>> PredictMyDemandAsync([Body] DemandForecastRequest request);
}
