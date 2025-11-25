using MyShop.Core.Interfaces.Repositories;
using MyShop.Plugins.Mocks.Data;
using MyShop.Shared.Models;
using MyShop.Core.Common;

namespace MyShop.Plugins.Repositories.Mocks;

/// <summary>
/// Mock implementation for sales reports and analytics - delegates to MockReportData
/// </summary>
public class MockReportRepository : IReportRepository
{

    public async Task<Result<SalesReport>> GetSalesReportAsync(Guid salesAgentId, DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var report = await MockReportData.GetSalesReportAsync(salesAgentId, startDate, endDate);
            System.Diagnostics.Debug.WriteLine($"[MockReportRepository] GetSalesReportAsync success");
            return Result<SalesReport>.Success(report);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockReportRepository] GetSalesReportAsync error: {ex.Message}");
            return Result<SalesReport>.Failure($"Failed to get sales report: {ex.Message}");
        }
    }

    public async Task<Result<PerformanceMetrics>> GetPerformanceMetricsAsync(Guid salesAgentId)
    {
        try
        {
            var metrics = await MockReportData.GetPerformanceMetricsAsync(salesAgentId);
            System.Diagnostics.Debug.WriteLine($"[MockReportRepository] GetPerformanceMetricsAsync success");
            return Result<PerformanceMetrics>.Success(metrics);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockReportRepository] GetPerformanceMetricsAsync error: {ex.Message}");
            return Result<PerformanceMetrics>.Failure($"Failed to get performance metrics: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<ProductPerformance>>> GetTopProductsAsync(Guid salesAgentId, int topCount = 10)
    {
        try
        {
            var products = await MockReportData.GetTopProductsAsync(salesAgentId, topCount);
            System.Diagnostics.Debug.WriteLine($"[MockReportRepository] GetTopProductsAsync returned {products.Count()} products");
            return Result<IEnumerable<ProductPerformance>>.Success(products);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockReportRepository] GetTopProductsAsync error: {ex.Message}");
            return Result<IEnumerable<ProductPerformance>>.Failure($"Failed to get top products: {ex.Message}");
        }
    }

    public async Task<Result<SalesTrend>> GetSalesTrendAsync(Guid salesAgentId, string period = "monthly")
    {
        try
        {
            var trend = await MockReportData.GetSalesTrendAsync(salesAgentId, period);
            System.Diagnostics.Debug.WriteLine($"[MockReportRepository] GetSalesTrendAsync success for period: {period}");
            return Result<SalesTrend>.Success(trend);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockReportRepository] GetSalesTrendAsync error: {ex.Message}");
            return Result<SalesTrend>.Failure($"Failed to get sales trend: {ex.Message}");
        }
    }
}
