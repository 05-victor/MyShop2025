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

            System.Diagnostics.Debug.WriteLine($"[DashboardRepository.GetSummaryAsync] START - Period: {period} (mapped to: {apiPeriod})");
            System.Diagnostics.Debug.WriteLine($"[DashboardRepository.GetSummaryAsync] Calling API: GET /api/v1/dashboard/summary?period={apiPeriod}");

            var sw = System.Diagnostics.Stopwatch.StartNew();
            var refitResponse = await _apiClient.GetSalesAgentSummaryAsync(apiPeriod);
            sw.Stop();

            System.Diagnostics.Debug.WriteLine($"[DashboardRepository.GetSummaryAsync] API Response - StatusCode: {refitResponse.StatusCode}, ElapsedMs: {sw.ElapsedMilliseconds}");

            // Check Refit outer wrapper (HTTP status)
            if (refitResponse.IsSuccessStatusCode && refitResponse.Content != null)
            {
                var apiResponse = refitResponse.Content;
                System.Diagnostics.Debug.WriteLine($"[DashboardRepository.GetSummaryAsync] ApiResponse.Success: {apiResponse.Success}");

                // Check inner ApiResponse (business logic)
                if (apiResponse.Success == true && apiResponse.Result != null)
                {
                    var response = apiResponse.Result;
                    System.Diagnostics.Debug.WriteLine($"[DashboardRepository.GetSummaryAsync] Response Data: TotalProducts={response.TotalProducts}, TotalOrders={response.TotalOrders}, TotalRevenue={response.TotalRevenue}");
                    System.Diagnostics.Debug.WriteLine($"[DashboardRepository.GetSummaryAsync] LowStockProducts={response.LowStockProducts?.Count ?? 0}, TopSellingProducts={response.TopSellingProducts?.Count ?? 0}, RecentOrders={response.RecentOrders?.Count ?? 0}");

                    // Map SalesAgentDashboardSummaryResponse to DashboardSummary
                    var dashboardSummary = MapToDashboardSummary(apiResponse.Result);

                    System.Diagnostics.Debug.WriteLine($"[DashboardRepository.GetSummaryAsync] COMPLETED - Summary mapped successfully");
                    return Result<DashboardSummary>.Success(dashboardSummary);
                }

                var errorMsg = apiResponse.Message ?? "Failed to load dashboard summary";
                System.Diagnostics.Debug.WriteLine($"[DashboardRepository.GetSummaryAsync] ERROR - {errorMsg}");
                return Result<DashboardSummary>.Failure(errorMsg);
            }

            // HTTP error
            var httpError = $"HTTP Error: {refitResponse.StatusCode}";
            System.Diagnostics.Debug.WriteLine($"[DashboardRepository.GetSummaryAsync] ERROR - {httpError}");
            return Result<DashboardSummary>.Failure(httpError);
        }
        catch (ApiException apiEx)
        {
            var errorMessage = MapApiError(apiEx);
            System.Diagnostics.Debug.WriteLine($"[DashboardRepository.GetSummaryAsync] ApiException - StatusCode: {apiEx.StatusCode}, Message: {errorMessage}");
            System.Diagnostics.Debug.WriteLine($"[DashboardRepository.GetSummaryAsync] Content: {apiEx.Content}");
            return Result<DashboardSummary>.Failure(errorMessage, apiEx);
        }
        catch (HttpRequestException httpEx)
        {
            System.Diagnostics.Debug.WriteLine($"[DashboardRepository.GetSummaryAsync] HttpRequestException - {httpEx.Message}");
            return Result<DashboardSummary>.Failure("Cannot connect to server. Please check your network connection and ensure the server is running.", httpEx);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[DashboardRepository.GetSummaryAsync] Unexpected Exception - Type: {ex.GetType().Name}, Message: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"[DashboardRepository.GetSummaryAsync] StackTrace: {ex.StackTrace}");
            return Result<DashboardSummary>.Failure("An unexpected error occurred while loading dashboard data. Please try again.", ex);
        }
    }

    public async Task<Result<RevenueChartData>> GetRevenueChartAsync(string period)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"[DashboardRepository.GetRevenueChartAsync] START - Period: {period}");
            System.Diagnostics.Debug.WriteLine($"[DashboardRepository.GetRevenueChartAsync] Calling API: GET /api/v1/dashboard/revenue-chart?period={period}");

            var sw = System.Diagnostics.Stopwatch.StartNew();
            var refitResponse = await _apiClient.GetRevenueChartAsync(period);
            sw.Stop();

            System.Diagnostics.Debug.WriteLine($"[DashboardRepository.GetRevenueChartAsync] API Response - StatusCode: {refitResponse.StatusCode}, ElapsedMs: {sw.ElapsedMilliseconds}");

            // Check Refit outer wrapper (HTTP status)
            if (refitResponse.IsSuccessStatusCode && refitResponse.Content != null)
            {
                var apiResponse = refitResponse.Content;
                System.Diagnostics.Debug.WriteLine($"[DashboardRepository.GetRevenueChartAsync] ApiResponse.Success: {apiResponse.Success}");

                // Check inner ApiResponse (business logic)
                if (apiResponse.Success == true && apiResponse.Result != null)
                {
                    var response = apiResponse.Result;
                    System.Diagnostics.Debug.WriteLine($"[DashboardRepository.GetRevenueChartAsync] Chart Data: Labels={response.Labels?.Count ?? 0}, DataPoints={response.Data?.Count ?? 0}");
                    if (response.Labels?.Count > 0)
                    {
                        System.Diagnostics.Debug.WriteLine($"[DashboardRepository.GetRevenueChartAsync] Labels: {string.Join(", ", response.Labels.Take(5))}{(response.Labels.Count > 5 ? "..." : "")}");
                        System.Diagnostics.Debug.WriteLine($"[DashboardRepository.GetRevenueChartAsync] Data: {string.Join(", ", response.Data?.Take(5) ?? new List<decimal>())}{(response.Data?.Count > 5 ? "..." : "")}");
                    }

                    // Map RevenueChartResponse to RevenueChartData
                    var chartData = new RevenueChartData
                    {
                        Labels = apiResponse.Result.Labels,
                        Data = apiResponse.Result.Data
                    };

                    System.Diagnostics.Debug.WriteLine($"[DashboardRepository.GetRevenueChartAsync] COMPLETED - Chart data mapped with {chartData.Labels.Count} data points");
                    return Result<RevenueChartData>.Success(chartData);
                }

                var errorMsg = apiResponse.Message ?? "Failed to load revenue chart data";
                System.Diagnostics.Debug.WriteLine($"[DashboardRepository.GetRevenueChartAsync] ERROR - {errorMsg}");
                return Result<RevenueChartData>.Failure(errorMsg);
            }

            // HTTP error
            var httpError = $"HTTP Error: {refitResponse.StatusCode}";
            System.Diagnostics.Debug.WriteLine($"[DashboardRepository.GetRevenueChartAsync] ERROR - {httpError}");
            return Result<RevenueChartData>.Failure(httpError);
        }
        catch (ApiException apiEx)
        {
            var errorMessage = MapApiError(apiEx);
            System.Diagnostics.Debug.WriteLine($"[DashboardRepository.GetRevenueChartAsync] ApiException - StatusCode: {apiEx.StatusCode}, Message: {errorMessage}");
            System.Diagnostics.Debug.WriteLine($"[DashboardRepository.GetRevenueChartAsync] Content: {apiEx.Content}");
            return Result<RevenueChartData>.Failure(errorMessage, apiEx);
        }
        catch (HttpRequestException httpEx)
        {
            System.Diagnostics.Debug.WriteLine($"[DashboardRepository.GetRevenueChartAsync] HttpRequestException - {httpEx.Message}");
            return Result<RevenueChartData>.Failure("Cannot connect to server. Please check your network connection and ensure the server is running.", httpEx);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[DashboardRepository.GetRevenueChartAsync] Unexpected Exception - Type: {ex.GetType().Name}, Message: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"[DashboardRepository.GetRevenueChartAsync] StackTrace: {ex.StackTrace}");
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
            TodayOrders = response.TotalOrders, // Map period orders to TodayOrders for compatibility
            TodayRevenue = response.TotalRevenue, // Map period revenue to TodayRevenue for compatibility
            WeekRevenue = response.TotalRevenue,
            MonthRevenue = response.TotalRevenue,
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
