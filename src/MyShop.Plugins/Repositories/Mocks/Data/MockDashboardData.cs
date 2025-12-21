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

    /// <summary>
    /// Extract category from product name for mock data
    /// Since ProductIds don't match between orders.json and products.json
    /// </summary>
    private static string ExtractCategoryFromProductName(string productName)
    {
        if (string.IsNullOrWhiteSpace(productName))
            return "Unknown";

        var name = productName.ToLower();

        // Map product names to categories
        if (name.Contains("iphone") || name.Contains("ipad") || name.Contains("macbook") || name.Contains("airpods") || name.Contains("apple"))
            return "Apple";
        if (name.Contains("samsung") || name.Contains("galaxy"))
            return "Samsung";
        if (name.Contains("xiaomi") || name.Contains("redmi"))
            return "Xiaomi";
        if (name.Contains("oppo"))
            return "OPPO";
        if (name.Contains("vivo"))
            return "Vivo";
        if (name.Contains("realme"))
            return "Realme";
        if (name.Contains("laptop") || name.Contains("dell") || name.Contains("hp") || name.Contains("asus") || name.Contains("lenovo") || name.Contains("acer"))
            return "Laptop";
        if (name.Contains("watch") || name.Contains("smart band"))
            return "Wearables";
        if (name.Contains("tablet"))
            return "Tablet";
        if (name.Contains("charger") || name.Contains("cable") || name.Contains("adapter") || name.Contains("case") || name.Contains("screen protector"))
            return "Accessories";

        return "Other";
    }

    public static async Task<DashboardSummary> GetDashboardSummaryAsync(string period = "month")
    {
        EnsureDataLoaded();

        // Simulate network delay
        // await Task.Delay(400);

        // Calculate date ranges based on period
        // Server API supports: "day", "week", "month", "year"
        var now = DateTime.Now;
        var (startDate, endDate) = period.ToLower() switch
        {
            "day" => (DateTime.Today, now),                                                       // Today
            "week" => (GetStartOfWeek(DateTime.Today), now),                                      // This week (Monday to now)
            "year" => (new DateTime(now.Year, 1, 1), now),                                       // This year (Jan 1 to now)
            _ => (new DateTime(now.Year, now.Month, 1), now)                                     // This month (1st to now)
        };

        // Get real orders data for calculation
        var allOrders = await MockOrderData.GetAllAsync();
        System.Diagnostics.Debug.WriteLine($"[MockDashboardData] Total orders loaded: {allOrders.Count}");

        // Get all products for counts
        var allProducts = await MockProductData.GetAllAsync();
        System.Diagnostics.Debug.WriteLine($"[MockDashboardData] Total products loaded: {allProducts.Count}");

        // IMPORTANT: If no orders exist in current period, adjust recent orders to current period for demo
        // This ensures dashboard always shows data during development/testing
        var periodOrders = allOrders.Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate).ToList();
        System.Diagnostics.Debug.WriteLine($"[MockDashboardData] Orders in period ({startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}): {periodOrders.Count}");

        if (periodOrders.Count == 0 && allOrders.Count > 0)
        {
            System.Diagnostics.Debug.WriteLine($"[MockDashboardData] ⚠️ No orders in period! Adjusting recent orders to current period for demo...");

            // Take the most recent 50 orders and CLONE them to avoid modifying originals
            var recentOriginalOrders = allOrders.OrderByDescending(o => o.OrderDate).Take(50).ToList();
            var random = new Random(42);
            var daysInPeriod = Math.Max(1, (endDate - startDate).Days);

            System.Diagnostics.Debug.WriteLine($"[MockDashboardData] 📋 Cloning {recentOriginalOrders.Count} orders and adjusting dates...");

            // IMPORTANT: Clone orders to avoid modifying original objects in allOrders list
            periodOrders = recentOriginalOrders.Select(originalOrder =>
            {
                var randomDayOffset = random.Next(0, daysInPeriod);
                var newOrderDate = startDate.AddDays(randomDayOffset).AddHours(random.Next(8, 18));

                // Create a new Order with adjusted date
                var clonedOrder = new Order
                {
                    Id = originalOrder.Id,
                    OrderCode = originalOrder.OrderCode,
                    OrderDate = newOrderDate, // Adjusted date
                    CustomerId = originalOrder.CustomerId,
                    CustomerName = originalOrder.CustomerName,
                    CustomerPhone = originalOrder.CustomerPhone,
                    CustomerAddress = originalOrder.CustomerAddress,
                    SalesAgentId = originalOrder.SalesAgentId,
                    SalesAgentName = originalOrder.SalesAgentName,
                    Subtotal = originalOrder.Subtotal,
                    Discount = originalOrder.Discount,
                    FinalPrice = originalOrder.FinalPrice,
                    Status = originalOrder.Status,
                    Notes = originalOrder.Notes,
                    PaidDate = originalOrder.PaidDate,
                    CancelReason = originalOrder.CancelReason,
                    CreatedAt = originalOrder.CreatedAt,
                    UpdatedAt = originalOrder.UpdatedAt,
                    OrderItems = originalOrder.OrderItems,
                    Items = originalOrder.Items
                };

                return clonedOrder;
            }).ToList();

            var adjustedTotalRevenue = periodOrders.Where(o => o.Status == "PAID").Sum(o => o.FinalPrice);
            System.Diagnostics.Debug.WriteLine($"[MockDashboardData] ✅ Adjusted {periodOrders.Count} orders | Total Revenue: {adjustedTotalRevenue:N0} VND");
        }
        // If still no orders (empty database), use fallback mock data
        if (periodOrders.Count == 0)
        {
            System.Diagnostics.Debug.WriteLine($"[MockDashboardData] No orders found! Using fallback mock data.");
            return GenerateFallbackDashboardData(allProducts, lowStockProducts: await MockProductData.GetLowStockAsync(10));
        }

        var today = DateTime.Today;
        var weekStart = today.AddDays(-(int)today.DayOfWeek);

        // Calculate real statistics from period orders (which may have been adjusted to current period)
        var todayOrders = periodOrders.Count(o => o.OrderDate.Date == today);
        var todayRevenue = periodOrders.Where(o => o.OrderDate.Date == today && o.Status == "PAID").Sum(o => o.FinalPrice);
        var weekRevenue = periodOrders.Where(o => o.OrderDate >= weekStart && o.Status == "PAID").Sum(o => o.FinalPrice);
        var monthRevenue = periodOrders.Where(o => o.Status == "PAID").Sum(o => o.FinalPrice);

        System.Diagnostics.Debug.WriteLine($"[MockDashboardData] 📊 Statistics Calculated:");
        System.Diagnostics.Debug.WriteLine($"   - Today: {todayOrders} orders, {todayRevenue:N0} VND");
        System.Diagnostics.Debug.WriteLine($"   - Week: {weekRevenue:N0} VND");
        System.Diagnostics.Debug.WriteLine($"   - Month: {monthRevenue:N0} VND");
        System.Diagnostics.Debug.WriteLine($"   - Period Orders Count: {periodOrders.Count}");

        // Get low stock products (quantity < 10)
        var lowStockProducts = await MockProductData.GetLowStockAsync(10);

        // Calculate top selling products from orders in period
        // Since ProductIds don't match, use ProductName and aggregate by name
        var topSellingProducts = periodOrders
            .SelectMany(o => o.OrderItems ?? new List<OrderItem>())
            .GroupBy(item => item.ProductName) // Group by ProductName instead of ProductId
            .Select(g => new
            {
                ProductName = g.Key,
                SoldCount = g.Sum(item => item.Quantity),
                Revenue = g.Sum(item => item.TotalPrice)
            })
            .OrderByDescending(p => p.Revenue)
            .Take(5)
            .Select(p => new TopSellingProduct
            {
                Id = Guid.NewGuid(), // Generate new ID since we don't have matching product
                Name = p.ProductName,
                CategoryName = ExtractCategoryFromProductName(p.ProductName),
                SoldCount = p.SoldCount,
                Revenue = p.Revenue,
                ImageUrl = null // No image available
            })
            .ToList();

        // Calculate sales by category for the period
        var totalOrderItems = periodOrders.Sum(o => o.OrderItems?.Count ?? 0);
        System.Diagnostics.Debug.WriteLine($"[MockDashboardData] Period orders: {periodOrders.Count}, Total order items: {totalOrderItems}");

        // IMPORTANT: Extract category from ProductName since ProductIds don't match between orders.json and products.json
        // This is a workaround for mock data inconsistency
        var salesByCategory = periodOrders
            .SelectMany(o => o.OrderItems ?? new List<OrderItem>())
            .Select(item => new
            {
                // Extract category from ProductName (e.g., "iPhone 15" -> "iPhone", "Samsung Galaxy" -> "Samsung")
                Category = ExtractCategoryFromProductName(item.ProductName),
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

        System.Diagnostics.Debug.WriteLine($"[MockDashboardData.GetRevenueChartAsync] Period: {period}");

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
            case "day":
                // Last 7 days - values in VND (millions)
                for (int i = 6; i >= 0; i--)
                {
                    var day = DateTime.Today.AddDays(-i);
                    chartData.Labels.Add(day.ToString("MM/dd"));
                    chartData.Data.Add(15_000_000 + random.Next(-3_000_000, 8_000_000));
                }
                System.Diagnostics.Debug.WriteLine($"[MockDashboardData.GetRevenueChartAsync] Generated {chartData.Labels.Count} daily data points");
                break;

            case "weekly":
                // Last 12 weeks - values in VND (millions)
                for (int i = 11; i >= 0; i--)
                {
                    var week = DateTime.Today.AddDays(-i * 7);
                    chartData.Labels.Add($"W{week.DayOfYear / 7}");
                    chartData.Data.Add(80_000_000 + random.Next(-15_000_000, 30_000_000));
                }
                break;

            case "monthly":
                // Last 12 months - values in VND (millions)
                for (int i = 11; i >= 0; i--)
                {
                    var month = DateTime.Today.AddMonths(-i);
                    chartData.Labels.Add(month.ToString("MMM yyyy"));
                    chartData.Data.Add(250_000_000 + random.Next(-50_000_000, 100_000_000));
                }
                break;

            case "yearly":
                // Last 5 years - values in VND (billions)
                for (int i = 4; i >= 0; i--)
                {
                    var year = DateTime.Today.AddYears(-i);
                    chartData.Labels.Add(year.Year.ToString());
                    chartData.Data.Add(2_500_000_000 + random.Next(-500_000_000, 1_000_000_000));
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

        System.Diagnostics.Debug.WriteLine($"[MockDashboardData.GetTopSalesAgentsAsync] Period: {period}, Orders in period: {periodOrders.Count}");

        // If no orders in period, adjust recent orders to current period (same as dashboard summary)
        if (periodOrders.Count == 0 && allOrders.Count > 0)
        {
            System.Diagnostics.Debug.WriteLine($"[MockDashboardData.GetTopSalesAgentsAsync] ⚠️ No orders in period! Cloning & adjusting recent orders...");

            var recentOriginalOrders = allOrders
                .Where(o => o.Status == "PAID")
                .OrderByDescending(o => o.OrderDate)
                .Take(30)
                .ToList();

            var rng = new Random(42);
            var daysInPeriod = Math.Max(1, (endDate - startDate).Days);

            // IMPORTANT: Clone orders to avoid modifying originals
            periodOrders = recentOriginalOrders.Select(originalOrder =>
            {
                var randomDayOffset = rng.Next(0, daysInPeriod);
                var newOrderDate = startDate.AddDays(randomDayOffset).AddHours(rng.Next(8, 18));

                return new Order
                {
                    Id = originalOrder.Id,
                    OrderCode = originalOrder.OrderCode,
                    OrderDate = newOrderDate, // Adjusted date
                    CustomerId = originalOrder.CustomerId,
                    CustomerName = originalOrder.CustomerName,
                    CustomerPhone = originalOrder.CustomerPhone,
                    CustomerAddress = originalOrder.CustomerAddress,
                    SalesAgentId = originalOrder.SalesAgentId,
                    SalesAgentName = originalOrder.SalesAgentName,
                    Subtotal = originalOrder.Subtotal,
                    Discount = originalOrder.Discount,
                    FinalPrice = originalOrder.FinalPrice,
                    Status = originalOrder.Status,
                    Notes = originalOrder.Notes,
                    PaidDate = originalOrder.PaidDate,
                    CancelReason = originalOrder.CancelReason,
                    CreatedAt = originalOrder.CreatedAt,
                    UpdatedAt = originalOrder.UpdatedAt,
                    OrderItems = originalOrder.OrderItems,
                    Items = originalOrder.Items
                };
            }).ToList();

            System.Diagnostics.Debug.WriteLine($"[MockDashboardData.GetTopSalesAgentsAsync] ✅ Adjusted {periodOrders.Count} orders");
        }
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
                    GMV = 127_450_000m,
                    Commission = 6_372_500m,
                    OrderCount = 45,
                    Rating = 4.9,
                    Status = "Active"
                },
                new() {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Sarah Johnson",
                    Email = "sarah.johnson@example.com",
                    Avatar = "ms-appx:///Assets/Images/user/avatar-placeholder.png",
                    GMV = 98_320_000m,
                    Commission = 4_916_000m,
                    OrderCount = 38,
                    Rating = 4.8,
                    Status = "Active"
                },
                new() {
                    Id = Guid.NewGuid().ToString(),
                    Name = "David Park",
                    Email = "david.park@example.com",
                    Avatar = "ms-appx:///Assets/Images/user/avatar-placeholder.png",
                    GMV = 87_650_000m,
                    Commission = 4_382_500m,
                    OrderCount = 32,
                    Rating = 4.7,
                    Status = "Active"
                },
                new() {
                    Id = Guid.NewGuid().ToString(),
                    Name = "Emma Wilson",
                    Email = "emma.wilson@example.com",
                    Avatar = "ms-appx:///Assets/Images/user/avatar-placeholder.png",
                    GMV = 76_890_000m,
                    Commission = 3_844_500m,
                    OrderCount = 28,
                    Rating = 4.9,
                    Status = "Active"
                },
                new() {
                    Id = Guid.NewGuid().ToString(),
                    Name = "James Lee",
                    Email = "james.lee@example.com",
                    Avatar = "ms-appx:///Assets/Images/user/avatar-placeholder.png",
                    GMV = 65_430_000m,
                    Commission = 3_271_500m,
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

    /// <summary>
    /// Generate fallback mock dashboard data when no real orders exist in the period
    /// </summary>
    private static DashboardSummary GenerateFallbackDashboardData(List<Product> allProducts, List<Product> lowStockProducts)
    {
        var random = new Random(42); // Seed for consistent data
        var now = DateTime.Now;

        // Generate mock sales by category
        var categories = new[] { "Smartphones", "Laptops", "Tablets", "Audio", "Accessories" };
        var salesByCategory = categories.Select(cat => new CategorySales
        {
            CategoryName = cat,
            TotalRevenue = random.Next(50_000_000, 500_000_000),
            OrderCount = random.Next(10, 100),
            Percentage = 0
        }).ToList();

        // Calculate percentages
        var totalRevenue = salesByCategory.Sum(c => c.TotalRevenue);
        foreach (var category in salesByCategory)
        {
            category.Percentage = Math.Round((double)(category.TotalRevenue / totalRevenue * 100), 2);
        }

        // Generate mock top selling products
        var topSellingProducts = allProducts
            .Take(5)
            .Select(p => new TopSellingProduct
            {
                Id = p.Id,
                Name = p.Name,
                CategoryName = p.DeviceType ?? p.CategoryName ?? "Unknown",
                SoldCount = random.Next(50, 500),
                Revenue = random.Next(10_000_000, 200_000_000),
                ImageUrl = p.ImageUrl
            }).ToList();

        // Generate mock recent orders
        var recentOrders = Enumerable.Range(0, 10).Select(i => new RecentOrder
        {
            Id = Guid.NewGuid(),
            CustomerName = $"Customer {i + 1}",
            OrderDate = now.AddDays(-i),
            TotalAmount = random.Next(1_000_000, 50_000_000),
            Status = "PAID",
            SalesAgentName = $"Agent {i % 3 + 1}"
        }).ToList();

        return new DashboardSummary
        {
            Date = now,
            TotalProducts = allProducts.Count,
            TodayOrders = random.Next(5, 25),
            TodayRevenue = random.Next(10_000_000, 100_000_000),
            WeekRevenue = random.Next(50_000_000, 500_000_000),
            MonthRevenue = totalRevenue,
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
            RecentOrders = recentOrders
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

    /// <summary>
    /// Get the start of the week (Monday) for a given date
    /// </summary>
    private static DateTime GetStartOfWeek(DateTime date)
    {
        int diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
        return date.AddDays(-1 * diff);
    }
}
