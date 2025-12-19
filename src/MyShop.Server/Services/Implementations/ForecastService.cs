using System.Net.Http.Json;
using System.Text.Json;
using MyShop.Server.Exceptions;
using MyShop.Server.Services.Interfaces;
using MyShop.Shared.DTOs.Requests;
using MyShop.Shared.DTOs.Responses;

namespace MyShop.Server.Services.Implementations;

/// <summary>
/// Implementation of forecast service that calls external Python ML API
/// </summary>
public class ForecastService : IForecastService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ForecastService> _logger;
    private readonly string _baseUrl;

    public ForecastService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<ForecastService> logger)
    {
        _httpClient = httpClientFactory.CreateClient();
        _logger = logger;
        _baseUrl = configuration["ForecastSettings:BaseUrl"] ?? "http://localhost:8000";
        
        _logger.LogInformation("ForecastService initialized with base URL: {BaseUrl}", _baseUrl);
    }

    public async Task<DemandForecastResponse> PredictDemandAsync(DemandForecastRequest request)
    {
        var endpoint = $"{_baseUrl}/api/demand-forecast/predict";
        
        _logger.LogInformation(
            "Calling demand forecast API: StoreId={StoreId}, SkuId={SkuId}, Week={Week}",
            request.StoreId, request.SkuId, request.Week);

        try
        {
            var response = await _httpClient.PostAsJsonAsync(endpoint, request);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError(
                    "Demand forecast API returned error: StatusCode={StatusCode}, Content={Content}",
                    response.StatusCode, errorContent);
                
                throw InfrastructureException.ExternalApiError(
                    "Forecast Service (Demand)",
                    new Exception($"API returned {response.StatusCode}: {errorContent}"));
            }

            var result = await response.Content.ReadFromJsonAsync<DemandForecastResponse>();
            
            if (result == null)
            {
                throw InfrastructureException.ExternalApiError(
                    "Forecast Service (Demand)",
                    new Exception("Failed to deserialize response"));
            }

            _logger.LogInformation(
                "Demand forecast successful: PredictedUnitsSold={PredictedUnitsSold}, Strategy={Strategy}",
                result.PredictedUnitsSold, result.StrategyUsed);

            return result;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to connect to demand forecast API at {Endpoint}", endpoint);
            throw InfrastructureException.ServiceUnavailable("Forecast Service (Demand)");
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse demand forecast response");
            throw InfrastructureException.ExternalApiError("Forecast Service (Demand)", ex);
        }
    }

    public async Task<PriceForecastResponse> PredictPriceAsync(PriceForecastRequest request)
    {
        var endpoint = $"{_baseUrl}/api/price-forecast/predict";
        
        _logger.LogInformation(
            "Calling price forecast API: Store={Store}, Dept={Dept}, Date={Date}, Strategy={Strategy}",
            request.Store, request.Dept, request.Date, request.Strategy);

        try
        {
            var response = await _httpClient.PostAsJsonAsync(endpoint, request);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError(
                    "Price forecast API returned error: StatusCode={StatusCode}, Content={Content}",
                    response.StatusCode, errorContent);
                
                throw InfrastructureException.ExternalApiError(
                    "Forecast Service (Price)",
                    new Exception($"API returned {response.StatusCode}: {errorContent}"));
            }

            var result = await response.Content.ReadFromJsonAsync<PriceForecastResponse>();
            
            if (result == null)
            {
                throw InfrastructureException.ExternalApiError(
                    "Forecast Service (Price)",
                    new Exception("Failed to deserialize response"));
            }

            _logger.LogInformation(
                "Price forecast successful: PredictedWeeklySales={PredictedWeeklySales}, Strategy={Strategy}",
                result.PredictedWeeklySales, result.StrategyUsed);

            return result;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to connect to price forecast API at {Endpoint}", endpoint);
            throw InfrastructureException.ServiceUnavailable("Forecast Service (Price)");
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to parse price forecast response");
            throw InfrastructureException.ExternalApiError("Forecast Service (Price)", ex);
        }
    }

    public async Task<bool> CheckHealthAsync()
    {
        var endpoint = $"{_baseUrl}/health";
        
        try
        {
            var response = await _httpClient.GetAsync(endpoint);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Forecast service health check failed");
            return false;
        }
    }
}
