using MyShop.Shared.DTOs.Responses;
using MyShop.Shared.Models;

namespace MyShop.Plugins.Adapters;

/// <summary>
/// Adapter for mapping Report DTOs to domain models
/// </summary>
public static class ReportAdapter
{
    /// <summary>
    /// Maps SalesReportResponse DTO to SalesReport model
    /// </summary>
    public static SalesReport ToModel(SalesReportResponse dto)
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
            // Note: DailySales is not in SalesReportResponse, will be empty
        };
    }

    /// <summary>
    /// Maps ProductPerformanceResponse DTO to ProductPerformance model
    /// </summary>
    public static ProductPerformance ToModel(ProductPerformanceResponse dto)
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

    /// <summary>
    /// Maps PerformanceMetricsResponse DTO to PerformanceMetrics model
    /// </summary>
    public static PerformanceMetrics ToModel(PerformanceMetricsResponse dto)
    {
        return new PerformanceMetrics
        {
            SalesAgentId = dto.SalesAgentId,
            TotalProductsShared = dto.TotalProductsShared,
            TotalClicks = dto.TotalClicks,
            TotalOrders = dto.TotalOrders,
            ConversionRate = dto.ConversionRate,
            TotalRevenue = dto.TotalRevenue,
            TotalCommission = dto.TotalCommission,
            AverageOrderValue = dto.AverageOrderValue,
            PerformanceRank = dto.PerformanceRank
        };
    }

    /// <summary>
    /// Maps SalesTrendResponse DTO to SalesTrend model
    /// </summary>
    public static SalesTrend ToModel(SalesTrendResponse dto)
    {
        return new SalesTrend
        {
            Period = dto.Period,
            Labels = dto.Labels,
            RevenueData = dto.RevenueData,
            OrdersData = dto.OrdersData,
            CommissionData = dto.CommissionData
        };
    }

    /// <summary>
    /// Maps list of ProductPerformanceResponse DTOs to list of ProductPerformance models
    /// </summary>
    public static List<ProductPerformance> ToProductPerformanceList(IEnumerable<ProductPerformanceResponse> dtos)
    {
        return dtos.Select(ToModel).ToList();
    }
}
