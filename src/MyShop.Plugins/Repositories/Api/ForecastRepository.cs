using MyShop.Core.Common;
using MyShop.Core.Interfaces.Repositories;
using MyShop.Plugins.API.Forecasts;
using MyShop.Shared.DTOs.Requests;
using MyShop.Shared.DTOs.Responses;

namespace MyShop.Plugins.Repositories.Api;

/// <summary>
/// Repository for forecast operations using Refit API client
/// </summary>
public class ForecastRepository : IForecastRepository
{
    private readonly IForecastApi _forecastApi;

    public ForecastRepository(IForecastApi forecastApi)
    {
        _forecastApi = forecastApi ?? throw new ArgumentNullException(nameof(forecastApi));
    }

    /// <summary>
    /// Predict weekly revenue for the current SalesAgent's department
    /// </summary>
    public async Task<Result<PriceForecastResponse>> PredictRevenueAsync(string date)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"[ForecastRepository] Predicting revenue for date: {date}");

            // Create request with date only
            var request = new PriceForecastRequest
            {
                Date = date,
                Strategy = "linear"
            };

            var response = await _forecastApi.PredictMyRevenueAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                System.Diagnostics.Debug.WriteLine($"[ForecastRepository] API error: {response.StatusCode}");
                return Result<PriceForecastResponse>.Failure($"API error: {response.StatusCode}");
            }

            var content = response.Content;
            if (content?.Success != true || content?.Result == null)
            {
                System.Diagnostics.Debug.WriteLine($"[ForecastRepository] Invalid response: {content?.Message}");
                return Result<PriceForecastResponse>.Failure(content?.Message ?? "Invalid response");
            }

            System.Diagnostics.Debug.WriteLine($"[ForecastRepository] Revenue predicted: ${content.Result.PredictedWeeklySales}");
            return Result<PriceForecastResponse>.Success(content.Result);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ForecastRepository] PredictRevenueAsync error: {ex.Message}");
            return Result<PriceForecastResponse>.Failure($"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Predict product demand for the current SalesAgent's store
    /// </summary>
    public async Task<Result<DemandForecastResponse>> PredictDemandAsync(DemandForecastRequest request)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"[ForecastRepository] Predicting demand for SKU: {request.SkuId}");

            var response = await _forecastApi.PredictMyDemandAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                System.Diagnostics.Debug.WriteLine($"[ForecastRepository] API error: {response.StatusCode}");
                return Result<DemandForecastResponse>.Failure($"API error: {response.StatusCode}");
            }

            var content = response.Content;
            if (content?.Success != true || content?.Result == null)
            {
                System.Diagnostics.Debug.WriteLine($"[ForecastRepository] Invalid response: {content?.Message}");
                return Result<DemandForecastResponse>.Failure(content?.Message ?? "Invalid response");
            }

            System.Diagnostics.Debug.WriteLine($"[ForecastRepository] Demand predicted: {content.Result.PredictedUnitsSold} units");
            return Result<DemandForecastResponse>.Success(content.Result);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ForecastRepository] PredictDemandAsync error: {ex.Message}");
            return Result<DemandForecastResponse>.Failure($"Error: {ex.Message}");
        }
    }
}
