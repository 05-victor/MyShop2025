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
        // Initialize empty dashboard - data should be loaded from dashboard.json
        _dashboardData = new DashboardDataModel
        {
            Date = DateTime.UtcNow.ToString("yyyy-MM-dd"),
            TotalProducts = 0,
            TodayOrders = 0,
            TodayRevenue = 0,
            WeekRevenue = 0,
            MonthRevenue = 0,
            LowStockProducts = new List<LowStockProductData>(),
            TopSellingProducts = new List<TopSellingProductData>()
        };
        System.Diagnostics.Debug.WriteLine("[MockDashboardData] JSON file not found - initialized with empty dashboard");
    }

    public static async Task<DashboardSummary> GetDashboardSummaryAsync(string period = "current")
    {
        EnsureDataLoaded();

        // Simulate network delay
        // await Task.Delay(400);

        // Calculate date ranges based on period
        var now = DateTime.Now;
        var (startDate, endDate) = period.ToLower() switch
        {
            "last" => (new DateTime(now.Year, now.Month, 1).AddMonths(-1), new DateTime(now.Year, now.Month, 1).AddDays(-1)),
            "last3" => (new DateTime(now.Year, now.Month, 1).AddMonths(-3), new DateTime(now.Year, now.Month, 1).AddDays(-1)),
            _ => (new DateTime(now.Year, now.Month, 1), now) // Current month
        };

        // Get real orders data for calculation
        var allOrders = await MockOrderData.GetAllAsync();
        System.Diagnostics.Debug.WriteLine($"[MockDashboardData] Total orders loaded: {allOrders.Count}");

        var periodOrders = allOrders.Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate).ToList();
        System.Diagnostics.Debug.WriteLine($"[MockDashboardData] Orders in period ({startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}): {periodOrders.Count}");

        var today = DateTime.Today;
        var weekStart = today.AddDays(-(int)today.DayOfWeek);

        // Get all products for counts
        var allProducts = await MockProductData.GetAllAsync();
        System.Diagnostics.Debug.WriteLine($"[MockDashboardData] Total products loaded: {allProducts.Count}");

        // Calculate real statistics
        var todayOrders = allOrders.Count(o => o.OrderDate.Date == today);
        var todayRevenue = allOrders.Where(o => o.OrderDate.Date == today && o.Status == "PAID").Sum(o => o.FinalPrice);
        var weekRevenue = allOrders.Where(o => o.OrderDate >= weekStart && o.Status == "PAID").Sum(o => o.FinalPrice);
        var monthRevenue = periodOrders.Where(o => o.Status == "PAID").Sum(o => o.FinalPrice);

        // Get low stock products (quantity < 10)
        var lowStockProducts = await MockProductData.GetLowStockAsync(10);

        // Calculate top selling products from orders in period
        var topProducts = periodOrders
            .SelectMany(o => o.OrderItems)
            .GroupBy(item => item.ProductId)
            .Select(g => new
            {
                ProductId = g.Key,
                SoldCount = g.Sum(item => item.Quantity),
                Revenue = g.Sum(item => item.TotalPrice)
            })
            .OrderByDescending(p => p.Revenue)
            .Take(5)
            .ToList();

        var topSellingProducts = new List<TopSellingProduct>();
        foreach (var top in topProducts)
        {
            var product = allProducts.FirstOrDefault(p => p.Id == top.ProductId);
            if (product != null)
            {
                topSellingProducts.Add(new TopSellingProduct
                {
                    Id = product.Id,
                    Name = product.Name,
                    CategoryName = product.DeviceType ?? product.CategoryName ?? "Unknown",
                    SoldCount = top.SoldCount,
                    Revenue = top.Revenue,
                    ImageUrl = product.ImageUrl
                });
            }
        }

        // Calculate sales by category for the period
        var totalOrderItems = periodOrders.Sum(o => o.OrderItems?.Count ?? 0);
        System.Diagnostics.Debug.WriteLine($"[MockDashboardData] Period orders: {periodOrders.Count}, Total order items: {totalOrderItems}, All products: {allProducts.Count}");

        var salesByCategory = periodOrders
            .SelectMany(o => o.OrderItems ?? new List<OrderItem>())
            .Join(allProducts, item => item.ProductId, product => product.Id, (item, product) => new
            {
                Category = product.DeviceType ?? product.CategoryName ?? "Unknown",
                Revenue = item.TotalPrice
            })
            .GroupBy(x => x.Category)
            .Select(g => new CategorySales
            {
                CategoryName = g.Key,
                TotalRevenue = g.Sum(x => x.Revenue),
                OrderCount = g.Count(),
                Percentage = 0 // Will calculate after
            })
            .OrderByDescending(c => c.TotalRevenue)
            .ToList();

        System.Diagnostics.Debug.WriteLine($"[MockDashboardData] Sales by category count: {salesByCategory.Count}");

        // Calculate percentages
        var totalRevenue = salesByCategory.Sum(c => c.TotalRevenue);
        if (totalRevenue > 0)
        {
            foreach (var category in salesByCategory)
            {
                category.Percentage = Math.Round((double)(category.TotalRevenue / totalRevenue * 100), 2);
                System.Diagnostics.Debug.WriteLine($"[MockDashboardData] Category: {category.CategoryName} - {category.Percentage}%");
            }
        }
        else
        {
            System.Diagnostics.Debug.WriteLine($"[MockDashboardData] WARNING: Total revenue is 0!");
        }

        return new DashboardSummary
        {
            Date = now,
            TotalProducts = allProducts.Count,
            TodayOrders = todayOrders,
            TodayRevenue = todayRevenue,
            WeekRevenue = weekRevenue,
            MonthRevenue = monthRevenue,
            LowStockProducts = lowStockProducts.Select(p => new LowStockProduct
            {
                Id = p.Id,
                Name = p.Name,
                CategoryName = p.DeviceType ?? p.CategoryName ?? "Unknown",
                Quantity = p.Quantity,
                ImageUrl = p.ImageUrl,
                Status = p.Status
            }).ToList(),
            TopSellingProducts = topSellingProducts,
            SalesByCategory = salesByCategory,
            RecentOrders = periodOrders
                .OrderByDescending(o => o.OrderDate)
                .Take(10)
                .Select(o => new RecentOrder
                {
                    Id = o.Id,
                    CustomerName = o.CustomerName ?? "Unknown",
                    OrderDate = o.OrderDate,
                    TotalAmount = o.FinalPrice,
                    Status = o.Status,
                    SalesAgentName = o.SalesAgentName
                })
                .ToList()
        };
    }

    public static async Task<RevenueChartData> GetRevenueChartAsync(string period = "daily")
    {
        // await Task.Delay(300);

        // Generate mock chart data based on period
        var chartData = new RevenueChartData
        {
            Labels = new List<string>(),
            Data = new List<decimal>()
        };

        var random = new Random(42); // Seed for consistent mock data

        switch (period.ToLower())
        {
            case "daily":
                // Last 7 days
                for (int i = 6; i >= 0; i--)
                {
                    var day = DateTime.Today.AddDays(-i);
                    chartData.Labels.Add(day.ToString("MM/dd"));
                    chartData.Data.Add(15000 + random.Next(-3000, 8000));
                }
                break;

            case "weekly":
                // Last 12 weeks
                for (int i = 11; i >= 0; i--)
                {
                    var week = DateTime.Today.AddDays(-i * 7);
                    chartData.Labels.Add($"W{week.DayOfYear / 7}");
                    chartData.Data.Add(80000 + random.Next(-15000, 30000));
                }
                break;

            case "monthly":
                // Last 12 months
                for (int i = 11; i >= 0; i--)
                {
                    var month = DateTime.Today.AddMonths(-i);
                    chartData.Labels.Add(month.ToString("MMM yyyy"));
                    chartData.Data.Add(250000 + random.Next(-50000, 100000));
                }
                break;

            case "yearly":
                // Last 5 years
                for (int i = 4; i >= 0; i--)
                {
                    var year = DateTime.Today.AddYears(-i);
                    chartData.Labels.Add(year.Year.ToString());
                    chartData.Data.Add(2500000 + random.Next(-500000, 1000000));
                }
                break;

            default:
                chartData.Labels.Add("No Data");
                chartData.Data.Add(0);
                break;
        }

        return chartData;
    }

    public static async Task<List<TopSalesAgent>> GetTopSalesAgentsAsync(string period = "current", int topCount = 5)
    {
        // await Task.Delay(300);

        // Calculate date ranges based on period
        var now = DateTime.Now;
        var (startDate, endDate) = period.ToLower() switch
        {
            "last" => (new DateTime(now.Year, now.Month, 1).AddMonths(-1), new DateTime(now.Year, now.Month, 1).AddDays(-1)),
            "last3" => (new DateTime(now.Year, now.Month, 1).AddMonths(-3), new DateTime(now.Year, now.Month, 1).AddDays(-1)),
            _ => (new DateTime(now.Year, now.Month, 1), now) // Current month
        };

        // Get all orders in period
        var allOrders = await MockOrderData.GetAllAsync();
        var periodOrders = allOrders.Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate && o.Status == "PAID").ToList();

        // Get all users (SalesAgents)
        var allUsers = await MockUserData.GetAllAsync();
        var salesAgentsDict = allUsers
            .Where(u => u.Roles.Contains(Shared.Models.Enums.UserRole.SalesAgent))
            .ToDictionary(u => u.Id);

        // Calculate sales by agent - Group by SalesAgentId instead of Name
        var agentSales = periodOrders
            .Where(o => o.SalesAgentId.HasValue && o.SalesAgentId.Value != Guid.Empty)
            .GroupBy(o => o.SalesAgentId!.Value)
            .Select(g => new
            {
                AgentId = g.Key,
                AgentName = g.First().SalesAgentName, // Keep name for display
                TotalGMV = g.Sum(o => o.FinalPrice),
                OrderCount = g.Count()
            })
            .OrderByDescending(a => a.TotalGMV)
            .Take(topCount)
            .ToList();

        var topAgents = new List<TopSalesAgent>();
        var random = new Random(42);

        foreach (var sale in agentSales)
        {
            // Find matching user by ID
            salesAgentsDict.TryGetValue(sale.AgentId, out var user);
            var commission = sale.TotalGMV * 0.05m; // 5% commission

            topAgents.Add(new TopSalesAgent
            {
                Id = user?.Id.ToString() ?? sale.AgentId.ToString(),
                Name = user?.FullName ?? sale.AgentName ?? "Unknown Agent",
                Email = user?.Email ?? $"agent{sale.AgentId.ToString()[..8]}@example.com",
                Avatar = user?.Avatar ?? "ms-appx:///Assets/Images/user/avatar-placeholder.png",
                GMV = sale.TotalGMV,
                Commission = commission,
                OrderCount = sale.OrderCount,
                Rating = 4.5 + (random.NextDouble() * 0.5), // Random rating between 4.5-5.0
                Status = "Active"
            });
        }

        // If no sales agents found, return mock data
        if (topAgents.Count == 0)
        {
            topAgents = new List<TopSalesAgent>
            {
                new() {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Michael Chen",
                    Email = "michael.chen@example.com",
                    Avatar = "ms-appx:///Assets/Images/user/avatar-placeholder.png",
                    GMV = 127450m,
                    Commission = 6372.50m,
                    OrderCount = 45,
                    Rating = 4.9,
                    Status = "Active"
                },
                new() {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Sarah Johnson",
                    Email = "sarah.johnson@example.com",
                    Avatar = "ms-appx:///Assets/Images/user/avatar-placeholder.png",
                    GMV = 98320m,
                    Commission = 4916.00m,
                    OrderCount = 38,
                    Rating = 4.8,
                    Status = "Active"
                },
                new() {
                    Id = Guid.NewGuid().ToString(),
                    Name = "David Park",
                    Email = "david.park@example.com",
                    Avatar = "ms-appx:///Assets/Images/user/avatar-placeholder.png",
                    GMV = 87650m,
                    Commission = 4382.50m,
                    OrderCount = 32,
                    Rating = 4.7,
                    Status = "Active"
                },
                new() {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Emma Wilson",
                    Email = "emma.wilson@example.com",
                    Avatar = "ms-appx:///Assets/Images/user/avatar-placeholder.png",
                    GMV = 76890m,
                    Commission = 3844.50m,
                    OrderCount = 28,
                    Rating = 4.9,
                    Status = "Active"
                },
                new() {
                    Id = Guid.NewGuid().ToString(),
                    Name = "James Lee",
                    Email = "james.lee@example.com",
                    Avatar = "ms-appx:///Assets/Images/user/avatar-placeholder.png",
                    GMV = 65430m,
                    Commission = 3271.50m,
                    OrderCount = 25,
                    Rating = 4.6,
                    Status = "Active"
                }
            };
        }

        return topAgents;
    }

    /// <summary>
    /// Get flagged/pending review products with mock data
    /// </summary>
    public static async Task<List<(string Name, string Agent, string Category, string State)>> GetFlaggedProductsAsync(string period = "current")
    {
        // await Task.Delay(300);

        // Mock flagged products data
        var flaggedProducts = new List<(string Name, string Agent, string Category, string State)>
        {
            ("iPhone 14 Pro Max", "Michael Chen", "Smartphones", "Pending Review"),
            ("Samsung Galaxy S23 Ultra", "Sarah Johnson", "Smartphones", "Flagged"),
            ("MacBook Pro 16\"", "David Park", "Laptops", "Pending Review"),
            ("Sony WH-1000XM5", "Emma Wilson", "Audio", "Under Review"),
            ("iPad Pro 12.9\"", "James Lee", "Tablets", "Pending Review")
        };

        return flaggedProducts;
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
