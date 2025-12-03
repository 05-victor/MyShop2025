using MyShop.Shared.Models;
using System.Text.Json;

namespace MyShop.Plugins.Mocks.Data;

/// <summary>
/// Mock data provider for reports - generates mock report data
/// </summary>
public static class MockReportData
{
    private static readonly object _lock = new object();

    public static async Task<SalesReport> GetSalesReportAsync(Guid salesAgentId, DateTime? startDate = null, DateTime? endDate = null)
    {
        // Simulate network delay
        // await Task.Delay(500);

        var start = startDate ?? DateTime.UtcNow.AddMonths(-1);
        var end = endDate ?? DateTime.UtcNow;

        lock (_lock)
        {
            // Generate mock sales report
            var random = new Random();
            var daysDiff = (end - start).Days;

            var dailySales = new List<DailySales>();
            for (int i = 0; i <= daysDiff; i++)
            {
                var date = start.AddDays(i);
                dailySales.Add(new DailySales
                {
                    Date = date,
                    OrderCount = random.Next(0, 15),
                    Revenue = random.Next(1000000, 50000000)
                });
            }

            var totalOrders = dailySales.Sum(d => d.OrderCount);

            return new SalesReport
            {
                SalesAgentId = salesAgentId,
                StartDate = start,
                EndDate = end,
                TotalOrders = totalOrders,
                TotalRevenue = dailySales.Sum(d => d.Revenue),
                AverageOrderValue = totalOrders > 0 ? dailySales.Sum(d => d.Revenue) / totalOrders : 0,
                DailySales = dailySales
            };
        }
    }

    public static async Task<ProductReport> GetProductReportAsync(DateTime startDate, DateTime endDate)
    {
        // Simulate network delay
        // await Task.Delay(450);

        lock (_lock)
        {
            // Generate mock product report
            var topProducts = new List<ProductSalesData>
            {
                new ProductSalesData
                {
                    ProductId = Guid.Parse("20000000-0000-0000-0000-000000000001"),
                    ProductName = "iPhone 15 Pro 256GB Titanium",
                    UnitsSold = 45,
                    Revenue = 134955000
                },
                new ProductSalesData
                {
                    ProductId = Guid.Parse("20000000-0000-0000-0000-000000000002"),
                    ProductName = "Samsung Galaxy S23 Ultra 512GB",
                    UnitsSold = 32,
                    Revenue = 102368000
                },
                new ProductSalesData
                {
                    ProductId = Guid.Parse("20000000-0000-0000-0000-000000000004"),
                    ProductName = "MacBook Air 13 inch M3",
                    UnitsSold = 28,
                    Revenue = 86772000
                }
            };

            return new ProductReport
            {
                StartDate = startDate,
                EndDate = endDate,
                TotalProductsSold = topProducts.Sum(p => p.UnitsSold),
                TopSellingProducts = topProducts
            };
        }
    }

    public static async Task<CommissionReport> GetCommissionReportAsync(Guid salesAgentId, DateTime startDate, DateTime endDate)
    {
        // Simulate network delay
        // await Task.Delay(400);

        lock (_lock)
        {
            var random = new Random();
            
            return new CommissionReport
            {
                SalesAgentId = salesAgentId,
                SalesAgentName = "Sales Agent",
                StartDate = startDate,
                EndDate = endDate,
                TotalOrders = random.Next(10, 50),
                TotalSales = random.Next(50000000, 200000000),
                TotalCommission = random.Next(2000000, 10000000),
                PaidCommission = random.Next(1000000, 5000000),
                PendingCommission = random.Next(1000000, 5000000)
            };
        }
    }

    public static async Task<InventoryReport> GetInventoryReportAsync()
    {
        // Simulate network delay
        // await Task.Delay(350);

        lock (_lock)
        {
            return new InventoryReport
            {
                TotalProducts = 120,
                TotalValue = 2456789000,
                LowStockCount = 8,
                OutOfStockCount = 3,
                OverstockCount = 2,
                GeneratedAt = DateTime.UtcNow
            };
        }
    }

    public static async Task<PerformanceMetrics> GetPerformanceMetricsAsync(Guid salesAgentId)
    {
        // await Task.Delay(400);

        var random = new Random();
        var totalClicks = random.Next(1000, 5000);
        var totalOrders = random.Next(50, 200);
        var totalRevenue = (decimal)(random.NextDouble() * 50000 + 10000);

        return new PerformanceMetrics
        {
            SalesAgentId = salesAgentId,
            TotalProductsShared = random.Next(20, 100),
            TotalClicks = totalClicks,
            TotalOrders = totalOrders,
            ConversionRate = Math.Round((decimal)totalOrders / totalClicks * 100, 2),
            TotalRevenue = totalRevenue,
            TotalCommission = Math.Round(totalRevenue * 0.10m, 2),
            AverageOrderValue = Math.Round(totalRevenue / totalOrders, 2),
            PerformanceRank = "Top 25%"
        };
    }

    public static async Task<IEnumerable<ProductPerformance>> GetTopProductsAsync(Guid salesAgentId, int topCount = 10)
    {
        // await Task.Delay(350);

        var products = new List<ProductPerformance>();
        var random = new Random();
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
            var totalSold = random.Next(50, 500);
            var clicks = random.Next(totalSold * 10, totalSold * 50);
            var revenue = (decimal)(random.NextDouble() * 10000 + 1000);

            products.Add(new ProductPerformance
            {
                ProductId = Guid.NewGuid(),
                ProductName = productNames[i],
                CategoryName = categories[random.Next(categories.Length)],
                TotalSold = totalSold,
                TotalRevenue = revenue,
                TotalCommission = Math.Round(revenue * 0.10m, 2),
                Clicks = clicks,
                ConversionRate = Math.Round((decimal)totalSold / clicks * 100, 2)
            });
        }

        return products.OrderByDescending(p => p.TotalRevenue);
    }

    public static async Task<SalesTrend> GetSalesTrendAsync(Guid salesAgentId, string period = "monthly")
    {
        // await Task.Delay(400);

        var random = new Random();
        var trend = new SalesTrend { Period = period };

        switch (period.ToLower())
        {
            case "daily":
                for (int i = 6; i >= 0; i--)
                {
                    var date = DateTime.Now.AddDays(-i);
                    trend.Labels.Add(date.ToString("MMM dd"));
                    trend.RevenueData.Add((decimal)(random.NextDouble() * 2000 + 500));
                    trend.OrdersData.Add(random.Next(5, 20));
                    trend.CommissionData.Add(Math.Round(trend.RevenueData.Last() * 0.10m, 2));
                }
                break;

            case "weekly":
                for (int i = 11; i >= 0; i--)
                {
                    var weekStart = DateTime.Now.AddDays(-i * 7);
                    trend.Labels.Add($"Week {weekStart:MMM dd}");
                    trend.RevenueData.Add((decimal)(random.NextDouble() * 10000 + 2000));
                    trend.OrdersData.Add(random.Next(20, 80));
                    trend.CommissionData.Add(Math.Round(trend.RevenueData.Last() * 0.10m, 2));
                }
                break;

            case "monthly":
            default:
                for (int i = 5; i >= 0; i--)
                {
                    var month = DateTime.Now.AddMonths(-i);
                    trend.Labels.Add(month.ToString("MMM yyyy"));
                    trend.RevenueData.Add((decimal)(random.NextDouble() * 20000 + 5000));
                    trend.OrdersData.Add(random.Next(50, 150));
                    trend.CommissionData.Add(Math.Round(trend.RevenueData.Last() * 0.10m, 2));
                }
                break;
        }

        return trend;
    }
}
