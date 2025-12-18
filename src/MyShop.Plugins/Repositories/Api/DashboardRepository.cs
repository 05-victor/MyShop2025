using MyShop.Core.Common;
using MyShop.Core.Interfaces.Repositories;
using MyShop.Shared.Models;
using MyShop.Plugins.API.Dashboard;
using Refit;
using System.Diagnostics;

namespace MyShop.Plugins.Repositories.Api;

/// <summary>
/// API-based implementation of IDashboardRepository
/// Uses Refit client to communicate with backend Dashboard API
/// </summary>
public class DashboardRepository : IDashboardRepository
{
    private readonly IDashboardApi _apiClient;

    public DashboardRepository(IDashboardApi apiClient)
    {
        _apiClient = apiClient ?? throw new ArgumentNullException(nameof(apiClient));
    }

    public async Task<Result<DashboardSummary>> GetSummaryAsync(string period = "current")
    {
        try
        {
            // Map client period format to API period format
            var apiPeriod = MapPeriodToApi(period);
            
            Debug.WriteLine($"[DashboardRepository] Fetching dashboard summary from API (period: {apiPeriod})");
            var refitResponse = await _apiClient.GetSalesAgentSummaryAsync(apiPeriod);

            // Check Refit outer wrapper (HTTP status)
            if (refitResponse.IsSuccessStatusCode && refitResponse.Content != null)
            {
                var apiResponse = refitResponse.Content;

                // Check inner ApiResponse (business logic)
                if (apiResponse.Success == true && apiResponse.Result != null)
                {
                    // Map SalesAgentDashboardSummaryResponse to DashboardSummary
                    var dashboardSummary = MapToDashboardSummary(apiResponse.Result);
                    
                    Debug.WriteLine($"[DashboardRepository] Successfully fetched summary: {dashboardSummary.TotalProducts} products, {dashboardSummary.TodayOrders} orders");
                    return Result<DashboardSummary>.Success(dashboardSummary);
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

    /// <summary>
    /// TODO: Temporary implementation - will be replaced with real API call when backend is ready
    /// Gets top performing sales agents by GMV
    /// </summary>
    public async Task<Result<List<TopSalesAgent>>> GetTopSalesAgentsAsync(string period = "current", int topCount = 5)
    {
        try
        {
            Debug.WriteLine($"[DashboardRepository] Temporary: GetTopSalesAgentsAsync not yet implemented in API");
            
            // TODO: Replace with real API call when endpoint is ready
            // For now, return empty list to avoid errors
            await Task.CompletedTask; // Keep async signature
            
            return Result<List<TopSalesAgent>>.Success(new List<TopSalesAgent>());
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[DashboardRepository] Error in GetTopSalesAgentsAsync: {ex.Message}");
            return Result<List<TopSalesAgent>>.Failure("An error occurred while fetching top sales agents.", ex);
        }
    }

    #region Helper Methods

    /// <summary>
    /// Map client period format to API period format
    /// Client: "current", "last", "last3"
    /// API: "day", "week", "month", "year"
    /// </summary>
    private static string MapPeriodToApi(string clientPeriod)
    {
        return clientPeriod.ToLower() switch
        {
            "current" => "month",
            "last" => "month",
            "last3" => "month", // Could be extended to support quarter
            _ => "month"
        };
    }

    /// <summary>
    /// Map SalesAgentDashboardSummaryResponse to DashboardSummary
    /// </summary>
    private static DashboardSummary MapToDashboardSummary(MyShop.Shared.DTOs.Responses.SalesAgentDashboardSummaryResponse response)
    {
        return new DashboardSummary
        {
            Date = DateTime.UtcNow,
            TotalProducts = response.TotalProducts,
            TodayOrders = response.TodayOrders,
            TodayRevenue = response.TodayRevenue,
            WeekRevenue = response.WeekRevenue,
            MonthRevenue = response.MonthRevenue,
            LowStockProducts = response.LowStockProducts.Select(p => new LowStockProduct
            {
                Id = p.Id,
                Name = p.Name,
                CategoryName = p.CategoryName,
                Quantity = p.Quantity,
                ImageUrl = p.ImageUrl,
                Status = p.Status
            }).ToList(),
            TopSellingProducts = response.TopSellingProducts.Select(p => new TopSellingProduct
            {
                Id = p.Id,
                Name = p.Name,
                CategoryName = p.CategoryName,
                SoldCount = p.SoldCount,
                Revenue = p.Revenue,
                ImageUrl = p.ImageUrl
            }).ToList(),
            RecentOrders = response.RecentOrders.Select(o => new RecentOrder
            {
                Id = o.Id,
                CustomerName = o.CustomerName,
                OrderDate = o.OrderDate,
                TotalAmount = o.TotalAmount,
                Status = o.Status
            }).ToList(),
            SalesByCategory = new List<CategorySales>() // Not included in sales agent summary
        };
    }

    #endregion

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
