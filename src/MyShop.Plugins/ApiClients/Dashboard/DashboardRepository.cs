using MyShop.Core.Common;
using MyShop.Core.Interfaces.Repositories;
using MyShop.Shared.Models;
using MyShop.Plugins.ApiClients.Dashboard;
using Refit;
using System.Diagnostics;

namespace MyShop.Plugins.ApiClients.Dashboard;

/// <summary>
/// API-based implementation of IDashboardRepository
/// Uses Refit client to communicate with backend Dashboard API
/// </summary>
public class DashboardRepository : IDashboardRepository
{
    private readonly IDashboardApiClient _apiClient;

    public DashboardRepository(IDashboardApiClient apiClient)
    {
        _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
    }

    public async Task<Result<DashboardSummary>> GetSummaryAsync()
    {
        try
        {
            Debug.WriteLine("[DashboardRepository] Fetching dashboard summary from API");
            var refitResponse = await _apiClient.GetSummaryAsync();

            // Check Refit outer wrapper (HTTP status)
            if (refitResponse.IsSuccessStatusCode && refitResponse.Content != null)
            {
                var apiResponse = refitResponse.Content;

                // Check inner ApiResponse (business logic)
                if (apiResponse.Success == true && apiResponse.Result != null)
                {
                    Debug.WriteLine($"[DashboardRepository] Successfully fetched summary: {apiResponse.Result.TotalProducts} products, {apiResponse.Result.TodayOrders} orders");
                    return Result<DashboardSummary>.Success(apiResponse.Result);
                }

                return Result<DashboardSummary>.Failure(apiResponse.Message ?? "Failed to load dashboard summary");
            }

            // HTTP error
            return Result<DashboardSummary>.Failure($"HTTP Error: {refitResponse.StatusCode}");
        }
        catch (ApiException apiEx)
        {
            var errorMessage = MapApiError(apiEx);
            Debug.WriteLine($"[DashboardRepository] API Error: {errorMessage}");
            return Result<DashboardSummary>.Failure(errorMessage, apiEx);
        }
        catch (HttpRequestException httpEx)
        {
            Debug.WriteLine($"[DashboardRepository] Network Error: {httpEx.Message}");
            return Result<DashboardSummary>.Failure("Cannot connect to server. Please check your network connection and ensure the server is running.", httpEx);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[DashboardRepository] Unexpected Error: {ex.Message}");
            return Result<DashboardSummary>.Failure("An unexpected error occurred while loading dashboard data. Please try again.", ex);
        }
    }

    public async Task<Result<RevenueChartData>> GetRevenueChartAsync(string period)
    {
        try
        {
            Debug.WriteLine($"[DashboardRepository] Fetching revenue chart for period: {period}");
            var refitResponse = await _apiClient.GetRevenueChartAsync(period);

            // Check Refit outer wrapper (HTTP status)
            if (refitResponse.IsSuccessStatusCode && refitResponse.Content != null)
            {
                var apiResponse = refitResponse.Content;

                // Check inner ApiResponse (business logic)
                if (apiResponse.Success == true && apiResponse.Result != null)
                {
                    Debug.WriteLine($"[DashboardRepository] Successfully fetched chart data with {apiResponse.Result.Labels.Count} data points");
                    return Result<RevenueChartData>.Success(apiResponse.Result);
                }

                return Result<RevenueChartData>.Failure(apiResponse.Message ?? "Failed to load revenue chart data");
            }

            // HTTP error
            return Result<RevenueChartData>.Failure($"HTTP Error: {refitResponse.StatusCode}");
        }
        catch (ApiException apiEx)
        {
            var errorMessage = MapApiError(apiEx);
            Debug.WriteLine($"[DashboardRepository] API Error: {errorMessage}");
            return Result<RevenueChartData>.Failure(errorMessage, apiEx);
        }
        catch (HttpRequestException httpEx)
        {
            Debug.WriteLine($"[DashboardRepository] Network Error: {httpEx.Message}");
            return Result<RevenueChartData>.Failure("Cannot connect to server. Please check your network connection and ensure the server is running.", httpEx);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[DashboardRepository] Unexpected Error: {ex.Message}");
            return Result<RevenueChartData>.Failure("An unexpected error occurred while loading chart data. Please try again.", ex);
        }
    }

    #region Error Handling

    /// <summary>
    /// Map API errors to user-friendly messages
    /// </summary>
    private string MapApiError(ApiException apiEx)
    {
        Debug.WriteLine($"[DashboardRepository] API Error: {apiEx.StatusCode} - {apiEx.Content}");

        return apiEx.StatusCode switch
        {
            System.Net.HttpStatusCode.Unauthorized => "You are not authorized to view dashboard data. Please login again.",
            System.Net.HttpStatusCode.Forbidden => "You don't have permission to access dashboard data.",
            System.Net.HttpStatusCode.NotFound => "Dashboard data not found.",
            System.Net.HttpStatusCode.BadRequest => "Invalid request. Please try again.",
            System.Net.HttpStatusCode.InternalServerError => "Server error occurred. Please try again later.",
            _ => "Network error. Please check your connection and try again."
        };
    }

    #endregion
}
