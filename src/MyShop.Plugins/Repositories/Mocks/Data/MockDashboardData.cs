using MyShop.Shared.Models;
using System.Text.Json;

namespace MyShop.Plugins.Mocks.Data;

/// <summary>
/// Mock data provider for dashboard - loads from JSON file
/// </summary>
public static class MockDashboardData
{
    private static DashboardDataModel? _dashboardData;
    private static readonly object _lock = new object();
    private static readonly string _jsonFilePath = Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory, "Mocks", "Data", "Json", "dashboard.json");

    private static void EnsureDataLoaded()
    {
        if (_dashboardData != null) return;

        lock (_lock)
        {
            if (_dashboardData != null) return;

            try
            {
                if (!File.Exists(_jsonFilePath))
                {
                    System.Diagnostics.Debug.WriteLine($"Dashboard JSON file not found at: {_jsonFilePath}");
                    InitializeDefaultData();
                    return;
                }

                var jsonString = File.ReadAllText(_jsonFilePath);
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var data = JsonSerializer.Deserialize<DashboardDataContainer>(jsonString, options);

                if (data?.DashboardSummary != null)
                {
                    _dashboardData = data.DashboardSummary;
                    System.Diagnostics.Debug.WriteLine($"Loaded dashboard data from dashboard.json");
                }
                else
                {
                    InitializeDefaultData();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading dashboard.json: {ex.Message}");
                InitializeDefaultData();
            }
        }
    }

    private static void InitializeDefaultData()
    {
        _dashboardData = new DashboardDataModel
        {
            Date = "2025-11-10",
            TotalProducts = 0,
            TodayOrders = 0,
            TodayRevenue = 0,
            WeekRevenue = 0,
            MonthRevenue = 0,
            LowStockProducts = new List<LowStockProductData>(),
            TopSellingProducts = new List<TopSellingProductData>()
        };
    }

    public static async Task<DashboardSummary> GetDashboardSummaryAsync()
    {
        EnsureDataLoaded();

        // Simulate network delay
        await Task.Delay(400);

        return new DashboardSummary
        {
            Date = DateTime.TryParse(_dashboardData!.Date, out var date) ? date : DateTime.Today,
            TotalProducts = _dashboardData.TotalProducts,
            TodayOrders = _dashboardData.TodayOrders,
            TodayRevenue = _dashboardData.TodayRevenue,
            WeekRevenue = _dashboardData.WeekRevenue,
            MonthRevenue = _dashboardData.MonthRevenue,
            LowStockProducts = _dashboardData.LowStockProducts?.Select(p => new LowStockProduct
            {
                Id = Guid.Parse(p.Id),
                Name = p.Name,
                CategoryName = p.CategoryName,
                Quantity = p.Quantity,
                ImageUrl = p.ImageUrl,
                Status = p.Status
            }).ToList() ?? new List<LowStockProduct>(),
            TopSellingProducts = _dashboardData.TopSellingProducts?.Select(p => new TopSellingProduct
            {
                Id = Guid.Parse(p.Id),
                Name = p.Name,
                CategoryName = p.CategoryName,
                SoldCount = p.SoldCount,
                Revenue = p.Revenue,
                ImageUrl = p.ImageUrl
            }).ToList() ?? new List<TopSellingProduct>()
        };
    }

    // Data container classes for JSON deserialization
    private class DashboardDataContainer
    {
        public DashboardDataModel DashboardSummary { get; set; } = new();
    }

    private class DashboardDataModel
    {
        public string Date { get; set; } = string.Empty;
        public int TotalProducts { get; set; }
        public int TodayOrders { get; set; }
        public decimal TodayRevenue { get; set; }
        public decimal WeekRevenue { get; set; }
        public decimal MonthRevenue { get; set; }
        public List<LowStockProductData>? LowStockProducts { get; set; }
        public List<TopSellingProductData>? TopSellingProducts { get; set; }
    }

    private class LowStockProductData
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? CategoryName { get; set; }
        public int Quantity { get; set; }
        public string? ImageUrl { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    private class TopSellingProductData
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? CategoryName { get; set; }
        public int SoldCount { get; set; }
        public decimal Revenue { get; set; }
        public string? ImageUrl { get; set; }
    }
}
