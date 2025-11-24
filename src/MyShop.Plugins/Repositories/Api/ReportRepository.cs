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

    public async Task<SalesReport> GetSalesReportAsync(Guid salesAgentId, DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var response = await _api.GetSalesReportAsync(startDate, endDate);
            
            if (response.IsSuccessStatusCode && response.Content != null)
            {
                var apiResponse = response.Content;
                if (apiResponse.Success && apiResponse.Result != null)
                {
                    return MapToSalesReport(apiResponse.Result);
                }
            }

            return new SalesReport();
        }
        catch (Exception)
        {
            return new SalesReport();
        }
    }

    public async Task<PerformanceMetrics> GetPerformanceMetricsAsync(Guid salesAgentId)
    {
        try
        {
            // Note: Backend may need dedicated endpoint for performance metrics
            // For now, derive from sales report
            var report = await GetSalesReportAsync(salesAgentId);
            
            return new PerformanceMetrics
            {
                SalesAgentId = salesAgentId,
                TotalOrders = report.TotalOrders,
                TotalRevenue = report.TotalRevenue,
                TotalCommission = report.TotalCommission,
                AverageOrderValue = report.AverageOrderValue,
                ConversionRate = report.ConversionRate
            };
        }
        catch (Exception)
        {
            return new PerformanceMetrics();
        }
    }

    public async Task<IEnumerable<ProductPerformance>> GetTopProductsAsync(Guid salesAgentId, int topCount = 10)
    {
        try
        {
            var response = await _api.GetProductReportAsync();
            
            if (response.IsSuccessStatusCode && response.Content != null)
            {
                var apiResponse = response.Content;
                if (apiResponse.Success && apiResponse.Result != null)
                {
                    return apiResponse.Result
                        .Select(MapToProductPerformance)
                        .OrderByDescending(p => p.TotalRevenue)
                        .Take(topCount);
                }
            }

            return Enumerable.Empty<ProductPerformance>();
        }
        catch (Exception)
        {
            return Enumerable.Empty<ProductPerformance>();
        }
    }

    public async Task<SalesTrend> GetSalesTrendAsync(Guid salesAgentId, string period = "monthly")
    {
        try
        {
            // Note: Backend may need dedicated endpoint for trend data
            // This is a placeholder implementation
            return new SalesTrend
            {
                Period = period,
                Labels = new List<string>(),
                RevenueData = new List<decimal>(),
                OrdersData = new List<int>(),
                CommissionData = new List<decimal>()
            };
        }
        catch (Exception)
        {
            return new SalesTrend();
        }
    }

    /// <summary>
    /// Map SalesReportResponse DTO to SalesReport domain model
    /// </summary>
    private static SalesReport MapToSalesReport(MyShop.Shared.DTOs.Responses.SalesReportResponse dto)
    {
        return new SalesReport
        {
            SalesAgentId = dto.SalesAgentId,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            TotalOrders = dto.TotalOrders,
            TotalRevenue = dto.TotalRevenue,
            TotalCommission = dto.TotalCommission,
            CompletedOrders = dto.CompletedOrders,
            PendingOrders = dto.PendingOrders,
            CancelledOrders = dto.CancelledOrders,
            AverageOrderValue = dto.AverageOrderValue,
            ConversionRate = dto.ConversionRate
        };
    }

    /// <summary>
    /// Map ProductPerformanceResponse DTO to ProductPerformance domain model
    /// </summary>
    private static ProductPerformance MapToProductPerformance(MyShop.Shared.DTOs.Responses.ProductPerformanceResponse dto)
    {
        return new ProductPerformance
        {
            ProductId = dto.ProductId,
            ProductName = dto.ProductName,
            CategoryName = dto.CategoryName,
            TotalSold = dto.TotalSold,
            TotalRevenue = dto.TotalRevenue,
            TotalCommission = dto.TotalCommission,
            Clicks = dto.Clicks,
            ConversionRate = dto.ConversionRate
        };
    }
}
