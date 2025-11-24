using MyShop.Shared.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyShop.Core.Interfaces.Repositories;

/// <summary>
/// Repository interface for sales reports and analytics
/// </summary>
public interface IReportRepository
{
    /// <summary>
    /// Get sales report for a specific sales agent
    /// </summary>
    Task<SalesReport> GetSalesReportAsync(Guid salesAgentId, DateTime? startDate = null, DateTime? endDate = null);

    /// <summary>
    /// Get performance metrics for a sales agent
    /// </summary>
    Task<PerformanceMetrics> GetPerformanceMetricsAsync(Guid salesAgentId);

    /// <summary>
    /// Get top performing products for a sales agent
    /// </summary>
    Task<IEnumerable<ProductPerformance>> GetTopProductsAsync(Guid salesAgentId, int topCount = 10);

    /// <summary>
    /// Get sales trend data (daily, weekly, monthly)
    /// </summary>
    Task<SalesTrend> GetSalesTrendAsync(Guid salesAgentId, string period = "monthly");
}
