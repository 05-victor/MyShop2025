using System.Text.Json;
using MyShop.Core.Common;
using MyShop.Core.Interfaces.Repositories;
using MyShop.Shared.Models;

namespace MyShop.Plugins.Repositories.Mocks;

/// <summary>
/// Mock repository for Dashboard data using JSON
/// </summary>
public class MockDashboardRepository : IDashboardRepository
{
    private readonly string _jsonFilePath;

    public MockDashboardRepository()
    {
        _jsonFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Mocks", "Data", "Json", "dashboard.json");
    }

    public async Task<Result<DashboardSummary>> GetSummaryAsync()
    {
        await Task.Delay(400);

        try
        {
            if (!File.Exists(_jsonFilePath))
            {
                System.Diagnostics.Debug.WriteLine($"[MockDashboardRepository] JSON file not found: {_jsonFilePath}");
                return Result<DashboardSummary>.Failure("Dashboard data file not found");
            }

            var json = File.ReadAllText(_jsonFilePath);
            var jsonDoc = JsonDocument.Parse(json);
            var summaryElement = jsonDoc.RootElement.GetProperty("dashboardSummary");

            var summary = new DashboardSummary
            {
                Date = DateTime.Parse(summaryElement.GetProperty("date").GetString()!),
                TotalProducts = summaryElement.GetProperty("totalProducts").GetInt32(),
                TodayOrders = summaryElement.GetProperty("todayOrders").GetInt32(),
                TodayRevenue = summaryElement.GetProperty("todayRevenue").GetDecimal(),
                WeekRevenue = summaryElement.GetProperty("weekRevenue").GetDecimal(),
                MonthRevenue = summaryElement.GetProperty("monthRevenue").GetDecimal(),
                LowStockProducts = new List<LowStockProduct>(),
                TopSellingProducts = new List<TopSellingProduct>(),
                RecentOrders = new List<RecentOrder>(),
                SalesByCategory = new List<CategorySales>()
            };

            // Load low stock products
            if (summaryElement.TryGetProperty("lowStockProducts", out var lowStockArray))
            {
                foreach (var item in lowStockArray.EnumerateArray())
                {
                    summary.LowStockProducts.Add(new LowStockProduct
                    {
                        Id = Guid.Parse(item.GetProperty("id").GetString()!),
                        Name = item.GetProperty("name").GetString()!,
                        Quantity = item.GetProperty("quantity").GetInt32(),
                        ImageUrl = item.GetProperty("imageUrl").GetString(),
                        Status = item.GetProperty("status").GetString()!,
                        CategoryName = item.GetProperty("categoryName").GetString()
                    });
                }
            }

            // Load top selling products
            if (summaryElement.TryGetProperty("topSellingProducts", out var topSellingArray))
            {
                foreach (var item in topSellingArray.EnumerateArray())
                {
                    summary.TopSellingProducts.Add(new TopSellingProduct
                    {
                        Id = Guid.Parse(item.GetProperty("id").GetString()!),
                        Name = item.GetProperty("name").GetString()!,
                        SoldCount = item.GetProperty("soldCount").GetInt32(),
                        Revenue = item.GetProperty("revenue").GetDecimal(),
                        ImageUrl = item.GetProperty("imageUrl").GetString(),
                        CategoryName = item.GetProperty("categoryName").GetString()
                    });
                }
            }

            // Load recent orders
            if (summaryElement.TryGetProperty("recentOrders", out var recentOrdersArray))
            {
                foreach (var item in recentOrdersArray.EnumerateArray())
                {
                    summary.RecentOrders.Add(new RecentOrder
                    {
                        Id = Guid.Parse(item.GetProperty("id").GetString()!),
                        OrderDate = DateTime.Parse(item.GetProperty("orderDate").GetString()!),
                        CustomerName = item.GetProperty("customerName").GetString()!,
                        TotalAmount = item.GetProperty("totalAmount").GetDecimal(),
                        Status = item.GetProperty("status").GetString()!,
                        SalesAgentName = item.GetProperty("salesAgentName").GetString()
                    });
                }
            }

            // Load sales by category
            if (summaryElement.TryGetProperty("salesByCategory", out var categoryArray))
            {
                foreach (var item in categoryArray.EnumerateArray())
                {
                    summary.SalesByCategory.Add(new CategorySales
                    {
                        CategoryName = item.GetProperty("categoryName").GetString()!,
                        TotalRevenue = item.GetProperty("totalRevenue").GetDecimal(),
                        OrderCount = item.GetProperty("orderCount").GetInt32(),
                        Percentage = item.GetProperty("percentage").GetDouble()
                    });
                }
            }

            System.Diagnostics.Debug.WriteLine($"[MockDashboardRepository] Loaded dashboard summary for {summary.Date:yyyy-MM-dd}");
            return Result<DashboardSummary>.Success(summary);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockDashboardRepository] Error loading dashboard: {ex.Message}");
            return Result<DashboardSummary>.Failure($"Failed to load dashboard data: {ex.Message}", ex);
        }
    }

    public async Task<Result<RevenueChartData>> GetRevenueChartAsync(string period = "daily")
    {
        await Task.Delay(350);

        try
        {
            if (!File.Exists(_jsonFilePath))
            {
                return Result<RevenueChartData>.Failure("Revenue chart data file not found");
            }

            var json = File.ReadAllText(_jsonFilePath);
            var jsonDoc = JsonDocument.Parse(json);
            var chartElement = jsonDoc.RootElement.GetProperty("revenueChart");

            JsonElement periodData = period.ToLower() switch
            {
                "daily" => chartElement.GetProperty("daily"),
                "weekly" => chartElement.GetProperty("weekly"),
                "monthly" => chartElement.GetProperty("monthly"),
                "yearly" => chartElement.GetProperty("yearly"),
                _ => chartElement.GetProperty("daily")
            };

            var chartData = new RevenueChartData
            {
                Labels = new List<string>(),
                Data = new List<decimal>()
            };

            foreach (var label in periodData.GetProperty("labels").EnumerateArray())
            {
                chartData.Labels.Add(label.GetString()!);
            }

            foreach (var value in periodData.GetProperty("data").EnumerateArray())
            {
                chartData.Data.Add(value.GetDecimal());
            }

            System.Diagnostics.Debug.WriteLine($"[MockDashboardRepository] Loaded {period} revenue chart with {chartData.Labels.Count} data points");
            return Result<RevenueChartData>.Success(chartData);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockDashboardRepository] Error loading revenue chart: {ex.Message}");
            return Result<RevenueChartData>.Failure($"Failed to load revenue chart: {ex.Message}", ex);
        }
    }
}
