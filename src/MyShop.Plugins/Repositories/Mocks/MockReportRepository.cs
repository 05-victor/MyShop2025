using MyShop.Core.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyShop.Plugins.Repositories.Mocks;

/// <summary>
/// Mock implementation for sales reports and analytics
/// </summary>
public class MockReportRepository : IReportRepository
{
    private readonly Random _random = new Random(42);

    public Task<SalesReport> GetSalesReportAsync(Guid salesAgentId, DateTime? startDate = null, DateTime? endDate = null)
    {
        var start = startDate ?? DateTime.Now.AddMonths(-3);
        var end = endDate ?? DateTime.Now;

        var totalOrders = _random.Next(50, 200);
        var completedOrders = (int)(totalOrders * 0.75);
        var pendingOrders = (int)(totalOrders * 0.15);
        var cancelledOrders = totalOrders - completedOrders - pendingOrders;
        var totalRevenue = (decimal)(_random.NextDouble() * 50000 + 10000);

        var report = new SalesReport
        {
            SalesAgentId = salesAgentId,
            StartDate = start,
            EndDate = end,
            TotalOrders = totalOrders,
            TotalRevenue = totalRevenue,
            TotalCommission = Math.Round(totalRevenue * 0.10m, 2),
            CompletedOrders = completedOrders,
            PendingOrders = pendingOrders,
            CancelledOrders = cancelledOrders,
            AverageOrderValue = Math.Round(totalRevenue / totalOrders, 2),
            ConversionRate = (decimal)(_random.NextDouble() * 5 + 2) // 2% - 7%
        };

        return Task.FromResult(report);
    }

    public Task<PerformanceMetrics> GetPerformanceMetricsAsync(Guid salesAgentId)
    {
        var totalClicks = _random.Next(1000, 5000);
        var totalOrders = _random.Next(50, 200);
        var totalRevenue = (decimal)(_random.NextDouble() * 50000 + 10000);

        var metrics = new PerformanceMetrics
        {
            SalesAgentId = salesAgentId,
            TotalProductsShared = _random.Next(20, 100),
            TotalClicks = totalClicks,
            TotalOrders = totalOrders,
            ConversionRate = Math.Round((decimal)totalOrders / totalClicks * 100, 2),
            TotalRevenue = totalRevenue,
            TotalCommission = Math.Round(totalRevenue * 0.10m, 2),
            AverageOrderValue = Math.Round(totalRevenue / totalOrders, 2),
            PerformanceRank = "Top 25%"
        };

        return Task.FromResult(metrics);
    }

    public Task<IEnumerable<ProductPerformance>> GetTopProductsAsync(Guid salesAgentId, int topCount = 10)
    {
        var products = new List<ProductPerformance>();
        var categories = new[] { "Electronics", "Fashion", "Home & Garden", "Sports", "Books" };
        var productNames = new[]
        {
            "iPhone 15 Pro Max", "MacBook Pro 16\"", "Samsung Galaxy S24",
            "Nike Air Max", "Adidas Running Shoes", "Sony WH-1000XM5",
            "LG OLED TV 55\"", "Dyson V15 Vacuum", "KitchenAid Mixer",
            "Leather Jacket", "Designer Handbag", "Smart Watch"
        };

        for (int i = 0; i < Math.Min(topCount, productNames.Length); i++)
        {
            var totalSold = _random.Next(50, 500);
            var clicks = _random.Next(totalSold * 10, totalSold * 50);
            var revenue = (decimal)(_random.NextDouble() * 10000 + 1000);

            products.Add(new ProductPerformance
            {
                ProductId = Guid.NewGuid(),
                ProductName = productNames[i],
                CategoryName = categories[_random.Next(categories.Length)],
                TotalSold = totalSold,
                TotalRevenue = revenue,
                TotalCommission = Math.Round(revenue * 0.10m, 2),
                Clicks = clicks,
                ConversionRate = Math.Round((decimal)totalSold / clicks * 100, 2)
            });
        }

        return Task.FromResult<IEnumerable<ProductPerformance>>(products.OrderByDescending(p => p.TotalRevenue));
    }

    public Task<SalesTrend> GetSalesTrendAsync(Guid salesAgentId, string period = "monthly")
    {
        var trend = new SalesTrend { Period = period };

        switch (period.ToLower())
        {
            case "daily":
                // Last 7 days
                for (int i = 6; i >= 0; i--)
                {
                    var date = DateTime.Now.AddDays(-i);
                    trend.Labels.Add(date.ToString("MMM dd"));
                    trend.RevenueData.Add((decimal)(_random.NextDouble() * 2000 + 500));
                    trend.OrdersData.Add(_random.Next(5, 20));
                    trend.CommissionData.Add(Math.Round(trend.RevenueData.Last() * 0.10m, 2));
                }
                break;

            case "weekly":
                // Last 12 weeks
                for (int i = 11; i >= 0; i--)
                {
                    var weekStart = DateTime.Now.AddDays(-i * 7);
                    trend.Labels.Add($"Week {weekStart.ToString("MMM dd")}");
                    trend.RevenueData.Add((decimal)(_random.NextDouble() * 10000 + 2000));
                    trend.OrdersData.Add(_random.Next(20, 80));
                    trend.CommissionData.Add(Math.Round(trend.RevenueData.Last() * 0.10m, 2));
                }
                break;

            case "monthly":
            default:
                // Last 6 months
                for (int i = 5; i >= 0; i--)
                {
                    var month = DateTime.Now.AddMonths(-i);
                    trend.Labels.Add(month.ToString("MMM yyyy"));
                    trend.RevenueData.Add((decimal)(_random.NextDouble() * 20000 + 5000));
                    trend.OrdersData.Add(_random.Next(50, 150));
                    trend.CommissionData.Add(Math.Round(trend.RevenueData.Last() * 0.10m, 2));
                }
                break;
        }

        return Task.FromResult(trend);
    }
}
