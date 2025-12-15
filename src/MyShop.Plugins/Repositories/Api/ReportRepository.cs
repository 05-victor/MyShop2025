using MyShop.Plugins.Adapters;
using MyShop.Core.Common;
using MyShop.Core.Interfaces.Repositories;
using MyShop.Plugins.API.Reports;
using MyShop.Shared.Models;
namespace MyShop.Plugins.Repositories.Api;

/// <summary>
/// API-based Report Repository implementation
/// </summary>
public class ReportRepository : IReportRepository
{
    private readonly IReportsApi _api;

    public ReportRepository(IReportsApi api)
    {
        _api = api;
    }

    public async Task<Result<SalesReport>> GetSalesReportAsync(Guid salesAgentId, DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var response = await _api.GetSalesReportAsync(startDate, endDate);

            if (response.IsSuccessStatusCode && response.Content != null)
            {
                var apiResponse = response.Content;
                if (apiResponse.Success && apiResponse.Result != null)
                {
                    var report = ReportAdapter.ToModel(apiResponse.Result);
                    return Result<SalesReport>.Success(report);
                }
            }

            return Result<SalesReport>.Failure("Failed to retrieve sales report");
        }
        catch (Exception ex)
        {
            return Result<SalesReport>.Failure($"Error retrieving sales report: {ex.Message}");
        }
    }

    public async Task<Result<PerformanceMetrics>> GetPerformanceMetricsAsync(Guid salesAgentId)
    {
        try
        {
            // Note: Backend may need dedicated endpoint for performance metrics
            // For now, derive from sales report
            var reportResult = await GetSalesReportAsync(salesAgentId);

            if (!reportResult.IsSuccess)
            {
                return Result<PerformanceMetrics>.Failure(reportResult.ErrorMessage ?? "Failed to get performance metrics");
            }

            var report = reportResult.Data;
            var metrics = new PerformanceMetrics
            {
                SalesAgentId = salesAgentId,
                TotalOrders = report.TotalOrders,
                TotalRevenue = report.TotalRevenue,
                TotalCommission = report.TotalCommission,
                AverageOrderValue = report.AverageOrderValue,
                ConversionRate = report.ConversionRate
            };
            return Result<PerformanceMetrics>.Success(metrics);
        }
        catch (Exception ex)
        {
            return Result<PerformanceMetrics>.Failure($"Error retrieving performance metrics: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<ProductPerformance>>> GetTopProductsAsync(Guid salesAgentId, int topCount = 10)
    {
        try
        {
            var response = await _api.GetProductReportAsync();

            if (response.IsSuccessStatusCode && response.Content != null)
            {
                var apiResponse = response.Content;
                if (apiResponse.Success && apiResponse.Result != null)
                {
                    var products = ReportAdapter.ToProductPerformanceList(apiResponse.Result)
                        .OrderByDescending(p => p.TotalRevenue)
                        .Take(topCount)
                        .ToList();
                    return Result<IEnumerable<ProductPerformance>>.Success(products);
                }
            }

            return Result<IEnumerable<ProductPerformance>>.Failure("Failed to retrieve top products");
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<ProductPerformance>>.Failure($"Error retrieving top products: {ex.Message}");
        }
    }

    public async Task<Result<SalesTrend>> GetSalesTrendAsync(Guid salesAgentId, string period = "monthly")
    {
        try
        {
            // Note: Backend may need dedicated endpoint for trend data
            // This is a placeholder implementation
            var trend = new SalesTrend
            {
                Period = period,
                Labels = new List<string>(),
                RevenueData = new List<decimal>(),
                OrdersData = new List<int>(),
                CommissionData = new List<decimal>()
            };
            return Result<SalesTrend>.Success(trend);
        }
        catch (Exception ex)
        {
            return Result<SalesTrend>.Failure($"Error retrieving sales trend: {ex.Message}");
        }
    }
}
