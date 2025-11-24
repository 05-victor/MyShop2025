using MyShop.Core.Interfaces.Repositories;
using MyShop.Plugins.Mocks.Data;
using MyShop.Shared.Models;

namespace MyShop.Plugins.Repositories.Mocks;

/// <summary>
/// Mock implementation for sales reports and analytics - delegates to MockReportData
/// </summary>
public class MockReportRepository : IReportRepository
{

    public async Task<SalesReport> GetSalesReportAsync(Guid salesAgentId, DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var report = await MockReportData.GetSalesReportAsync(salesAgentId, startDate, endDate);
            System.Diagnostics.Debug.WriteLine($"[MockReportRepository] GetSalesReportAsync success");
            return report;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockReportRepository] GetSalesReportAsync error: {ex.Message}");
            throw;
        }
    }

    public async Task<PerformanceMetrics> GetPerformanceMetricsAsync(Guid salesAgentId)
    {
        try
        {
            var metrics = await MockReportData.GetPerformanceMetricsAsync(salesAgentId);
            System.Diagnostics.Debug.WriteLine($"[MockReportRepository] GetPerformanceMetricsAsync success");
            return metrics;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockReportRepository] GetPerformanceMetricsAsync error: {ex.Message}");
            throw;
        }
    }

    public async Task<IEnumerable<ProductPerformance>> GetTopProductsAsync(Guid salesAgentId, int topCount = 10)
    {
        try
        {
            var products = await MockReportData.GetTopProductsAsync(salesAgentId, topCount);
            System.Diagnostics.Debug.WriteLine($"[MockReportRepository] GetTopProductsAsync returned {products.Count()} products");
            return products;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockReportRepository] GetTopProductsAsync error: {ex.Message}");
            return new List<ProductPerformance>();
        }
    }

    public async Task<SalesTrend> GetSalesTrendAsync(Guid salesAgentId, string period = "monthly")
    {
        try
        {
            var trend = await MockReportData.GetSalesTrendAsync(salesAgentId, period);
            System.Diagnostics.Debug.WriteLine($"[MockReportRepository] GetSalesTrendAsync success for period: {period}");
            return trend;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockReportRepository] GetSalesTrendAsync error: {ex.Message}");
            throw;
        }
    }
}
