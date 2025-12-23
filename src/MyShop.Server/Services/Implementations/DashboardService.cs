using Microsoft.EntityFrameworkCore;
using MyShop.Data;
using MyShop.Data.Repositories.Interfaces;
using MyShop.Server.Services.Interfaces;
using MyShop.Shared.DTOs.Responses;
using MyShop.Shared.Enums;
using MyShop.Shared.Extensions;

namespace MyShop.Server.Services.Implementations;

/// <summary>
/// Service for dashboard operations
/// Provides summary statistics for sales agents
/// </summary>
public class DashboardService : IDashboardService
{
    private readonly IProductRepository _productRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ShopContext _context;
    private readonly ILogger<DashboardService> _logger;
    private readonly IConfiguration _configuration;

    public DashboardService(
        IProductRepository productRepository,
        IOrderRepository orderRepository,
        ICurrentUserService currentUserService,
        ShopContext context,
        ILogger<DashboardService> logger,
        IConfiguration configuration)
    {
        _productRepository = productRepository;
        _orderRepository = orderRepository;
        _currentUserService = currentUserService;
        _context = context;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<SalesAgentDashboardSummaryResponse> GetSalesAgentSummaryAsync(string? period = null)
    {
        try
        {
            var currentUserId = _currentUserService.UserId;
            if (!currentUserId.HasValue)
            {
                throw new UnauthorizedAccessException("User not authenticated");
            }

            var salesAgentId = currentUserId.Value;
            _logger.LogInformation("Calculating dashboard summary for sales agent {SalesAgentId}, period: {Period}", 
                salesAgentId, period ?? "all-time");

            // Get all products published by this sales agent
            var allProducts = await _context.Products
                .Where(p => p.SaleAgentId == salesAgentId)
                .ToListAsync();

            var totalProducts = allProducts.Count;
            _logger.LogDebug("Sales agent {SalesAgentId} has {TotalProducts} products", 
                salesAgentId, totalProducts);

            // Get all orders for this sales agent
            var allOrders = await _context.Orders
                .Where(o => o.SaleAgentId == salesAgentId)
                .Include(o => o.Customer)
                    .ThenInclude(u => u.Profile)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .ToListAsync();

            // Calculate date range based on period (null = all time)
            // Use "this period" approach (e.g., this month = from 1st to now)
            var (startDate, endDate) = GetDateRangeForCurrentPeriod(period);

            // Filter orders by period and exclude cancelled orders
            var periodOrders = allOrders
                .Where(o => (startDate == null || o.OrderDate >= startDate.Value) 
                         && (endDate == null || o.OrderDate <= endDate.Value)
                         && o.Status != OrderStatus.Cancelled)
                .ToList();

            var totalOrders = periodOrders.Count;
            var totalRevenue = periodOrders.Sum(o => o.GrandTotal);

            _logger.LogDebug("Period: {Period}, StartDate: {Start}, EndDate: {End}, Orders: {Orders}, Revenue: {Revenue}",
                period ?? "all-time", startDate?.ToString("yyyy-MM-dd") ?? "N/A", endDate?.ToString("yyyy-MM-dd") ?? "N/A", totalOrders, totalRevenue);

            // Top 5 low stock products (quantity <= 10)
            var lowStockProducts = allProducts
                .Where(p => p.Quantity <= 10)
                .OrderBy(p => p.Quantity)
                .Take(5)
                .Select(p => new LowStockProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    CategoryName = p.Category?.Name,
                    Quantity = p.Quantity,
                    ImageUrl = p.ImageUrl,
                    Status = p.Status.ToApiString()
                })
                .ToList();

            _logger.LogDebug("Found {Count} low stock products", lowStockProducts.Count);

            // Top 5 best-selling products for this sales agent (all time)
            var topSellingProducts = await _context.OrderItems
                .Where(oi => oi.Product.SaleAgentId == salesAgentId)
                .GroupBy(oi => new { 
                    oi.ProductId, 
                    ProductName = oi.Product.Name, 
                    CategoryName = oi.Product.Category.Name, 
                    oi.Product.ImageUrl 
                })
                .Select(g => new
                {
                    ProductId = g.Key.ProductId,
                    ProductName = g.Key.ProductName,
                    CategoryName = g.Key.CategoryName,
                    ImageUrl = g.Key.ImageUrl,
                    SoldCount = g.Sum(oi => oi.Quantity),
                    Revenue = g.Sum(oi => oi.Quantity * oi.UnitSalePrice)
                })
                .OrderByDescending(p => p.SoldCount)
                .Take(5)
                .ToListAsync();

            var topSellingProductDtos = topSellingProducts
                .Select(p => new TopSellingProductDto
                {
                    Id = p.ProductId,
                    Name = p.ProductName,
                    CategoryName = p.CategoryName,
                    SoldCount = p.SoldCount,
                    Revenue = p.Revenue,
                    ImageUrl = p.ImageUrl
                })
                .ToList();

            _logger.LogDebug("Found {Count} top-selling products", topSellingProductDtos.Count);

            // Top 5 recent orders (all time, sorted by date)
            var recentOrders = allOrders
                .OrderByDescending(o => o.OrderDate)
                .Take(5)
                .Select(o => new RecentOrderDto
                {
                    Id = o.Id,
                    CustomerName = o.Customer?.Profile?.FullName ?? o.Customer?.Username ?? "Unknown",
                    OrderDate = o.OrderDate,
                    TotalAmount = o.GrandTotal,
                    Status = o.Status.ToApiString()
                })
                .ToList();

            _logger.LogDebug("Found {Count} recent orders", recentOrders.Count);

            var summary = new SalesAgentDashboardSummaryResponse
            {
                TotalProducts = totalProducts,
                TotalOrders = totalOrders,
                TotalRevenue = totalRevenue,
                LowStockProducts = lowStockProducts,
                TopSellingProducts = topSellingProductDtos,
                RecentOrders = recentOrders
            };

            _logger.LogInformation(
                "Dashboard summary calculated for agent {SalesAgentId}: Products={Products}, Orders={Orders}, Revenue={Revenue}",
                salesAgentId, totalProducts, totalOrders, totalRevenue);

            return summary;
        }
        catch (UnauthorizedAccessException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating dashboard summary");
            throw;
        }
    }

    public async Task<RevenueChartResponse> GetRevenueChartAsync(string period = "week")
    {
        try
        {
            var currentUserId = _currentUserService.UserId;
            if (!currentUserId.HasValue)
            {
                throw new UnauthorizedAccessException("User not authenticated");
            }

            var salesAgentId = currentUserId.Value;
            _logger.LogInformation("Calculating revenue chart for sales agent {SalesAgentId}, period: {Period}", 
                salesAgentId, period);

            // Get all orders for this sales agent
            var allOrders = await _context.Orders
                .Where(o => o.SaleAgentId == salesAgentId && o.Status != OrderStatus.Cancelled)
                .ToListAsync();

            var chartResponse = new RevenueChartResponse();

            // Generate labels and calculate revenue based on period
            switch (period.ToLower())
            {
                case "day":
                    // Hourly data for today (24 hours)
                    chartResponse = GenerateHourlyChart(allOrders);
                    break;

                case "week":
                    // Daily data for this week (7 days: Mon-Sun)
                    chartResponse = GenerateWeeklyChart(allOrders);
                    break;

                case "month":
                    // Daily data for this month (1st to today)
                    chartResponse = GenerateMonthlyChart(allOrders);
                    break;

                case "year":
                    // Monthly data for this year (Jan to current month)
                    chartResponse = GenerateYearlyChart(allOrders);
                    break;

                default:
                    _logger.LogWarning("Invalid chart period: {Period}, defaulting to week", period);
                    chartResponse = GenerateWeeklyChart(allOrders);
                    break;
            }

            _logger.LogInformation(
                "Revenue chart calculated for agent {SalesAgentId}: {DataPoints} data points",
                salesAgentId, chartResponse.Labels.Count);

            return chartResponse;
        }
        catch (UnauthorizedAccessException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating revenue chart");
            throw;
        }
    }

    #region Chart Generation Methods

    /// <summary>
    /// Generate hourly chart for today (24 hours)
    /// </summary>
    private RevenueChartResponse GenerateHourlyChart(List<Data.Entities.Order> orders)
    {
        var today = DateTime.UtcNow.Date;
        var chartResponse = new RevenueChartResponse();

        // Generate 24 hour labels (0-23)
        for (int hour = 0; hour < 24; hour++)
        {
            var hourStart = today.AddHours(hour);
            var hourEnd = hourStart.AddHours(1);

            // Format: "00:00", "01:00", etc.
            chartResponse.Labels.Add($"{hour:D2}:00");

            // Calculate revenue for this hour
            var hourRevenue = orders
                .Where(o => o.OrderDate >= hourStart && o.OrderDate < hourEnd)
                .Sum(o => o.GrandTotal);

            chartResponse.Data.Add(hourRevenue);
        }

        return chartResponse;
    }

    /// <summary>
    /// Generate daily chart for this week (Mon-Sun)
    /// </summary>
    private RevenueChartResponse GenerateWeeklyChart(List<Data.Entities.Order> orders)
    {
        var now = DateTime.UtcNow;
        var startOfWeek = GetStartOfWeek(now);
        var chartResponse = new RevenueChartResponse();

        // Generate 7 day labels (Monday to Sunday)
        var dayNames = new[] { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" };

        for (int i = 0; i < 7; i++)
        {
            var dayStart = startOfWeek.AddDays(i).Date;
            var dayEnd = dayStart.AddDays(1);

            chartResponse.Labels.Add(dayNames[i]);

            // Calculate revenue for this day
            var dayRevenue = orders
                .Where(o => o.OrderDate >= dayStart && o.OrderDate < dayEnd)
                .Sum(o => o.GrandTotal);

            chartResponse.Data.Add(dayRevenue);
        }

        return chartResponse;
    }

    /// <summary>
    /// Generate daily chart for this month (1st to today)
    /// </summary>
    private RevenueChartResponse GenerateMonthlyChart(List<Data.Entities.Order> orders)
    {
        var now = DateTime.UtcNow;
        var startOfMonth = now.StartOfMonth();
        var daysInMonth = DateTime.DaysInMonth(now.Year, now.Month);
        var chartResponse = new RevenueChartResponse();

        // Generate labels for each day of the month up to today
        var currentDay = (int)now.Day;

        for (int day = 1; day <= currentDay; day++)
        {
            var dayStart = new DateTime(now.Year, now.Month, day);
            var dayEnd = dayStart.AddDays(1);

            // Format: "1", "2", "3", ... "31"
            chartResponse.Labels.Add(day.ToString());

            // Calculate revenue for this day
            var dayRevenue = orders
                .Where(o => o.OrderDate >= dayStart && o.OrderDate < dayEnd)
                .Sum(o => o.GrandTotal);

            chartResponse.Data.Add(dayRevenue);
        }

        return chartResponse;
    }

    /// <summary>
    /// Generate monthly chart for this year (Jan to current month)
    /// </summary>
    private RevenueChartResponse GenerateYearlyChart(List<Data.Entities.Order> orders)
    {
        var now = DateTime.UtcNow;
        var chartResponse = new RevenueChartResponse();

        // Month abbreviations
        var monthNames = new[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun", 
                                "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };

        // Generate labels for each month up to current month
        for (int month = 1; month <= now.Month; month++)
        {
            var monthStart = new DateTime(now.Year, month, 1);
            var monthEnd = monthStart.AddMonths(1);

            chartResponse.Labels.Add(monthNames[month - 1]);

            // Calculate revenue for this month
            var monthRevenue = orders
                .Where(o => o.OrderDate >= monthStart && o.OrderDate < monthEnd)
                .Sum(o => o.GrandTotal);

            chartResponse.Data.Add(monthRevenue);
        }

        return chartResponse;
    }

    #endregion

    /// <summary>
    /// Get date range for the current period (e.g., "this month" = from 1st to now)
    /// Returns (null, null) for all-time
    /// </summary>
    private static (DateTime? startDate, DateTime? endDate) GetDateRangeForCurrentPeriod(string? period)
    {
        if (string.IsNullOrWhiteSpace(period))
        {
            return (null, null); // All time
        }

        var now = DateTime.UtcNow;

        return period.ToLower() switch
        {
            "day" => (now.Date, now),                                    // Today: 00:00:00 to now
            "week" => (GetStartOfWeek(now), now),                        // This week: Monday to now
            "month" => (now.StartOfMonth(), now),                        // This month: 1st to now
            "year" => (new DateTime(now.Year, 1, 1), now),               // This year: Jan 1 to now
            _ => (null, null)                                            // Invalid period = all time
        };
    }

    /// <summary>
    /// Get start of week (Monday)
    /// </summary>
    private static DateTime GetStartOfWeek(DateTime date)
    {
        int diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
        return date.AddDays(-1 * diff).Date;
    }

    public async Task<AdminDashboardSummaryResponse> GetAdminSummaryAsync(string? period = null)
    {
        try
        {
            _logger.LogInformation("Calculating admin dashboard summary for period: {Period}", period ?? "all-time");

            // Calculate date range based on period (null = all time)
            var (startDate, endDate) = GetDateRangeForCurrentPeriod(period);

            // Get all orders across all sales agents
            var allOrders = await _context.Orders
                .Include(o => o.Customer)
                    .ThenInclude(u => u.Profile)
                .Include(o => o.SaleAgent)
                    .ThenInclude(sa => sa.Profile)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Where(o => o.Status != OrderStatus.Cancelled)
                .ToListAsync();

            // Filter orders by period
            var periodOrders = allOrders
                .Where(o => (startDate == null || o.OrderDate >= startDate.Value)
                         && (endDate == null || o.OrderDate <= endDate.Value))
                .ToList();

            // Calculate metrics
            var platformFee = _configuration.GetValue<decimal>("BusinessSettings:PlatformFee"); // Get platform fee rate from config

            var totalGmv = periodOrders.Sum(o => o.GrandTotal);
            var adminCommission = Math.Round(totalGmv * platformFee, 2);
            var totalOrders = periodOrders.Count;

            // Get count of active sales agents (agents with at least 1 order in the period)
            var activeSalesAgents = periodOrders
                .Select(o => o.SaleAgentId)
                .Distinct()
                .Count();

            // Get total products across all agents
            var totalProducts = await _context.Products.CountAsync();

            _logger.LogDebug("Admin summary: Period={Period}, GMV={GMV}, Orders={Orders}, ActiveAgents={Agents}",
                period ?? "all-time", totalGmv, totalOrders, activeSalesAgents);

            // Top 5 best-selling products across entire platform
            var topSellingProducts = await _context.OrderItems
                .Where(oi => startDate == null || oi.Order.OrderDate >= startDate.Value)
                .Where(oi => endDate == null || oi.Order.OrderDate <= endDate.Value)
                .Where(oi => oi.Order.Status != OrderStatus.Cancelled)
                .GroupBy(oi => new
                {
                    oi.ProductId,
                    ProductName = oi.Product.Name,
                    CategoryName = oi.Product.Category.Name,
                    oi.Product.ImageUrl
                })
                .Select(g => new
                {
                    ProductId = g.Key.ProductId,
                    ProductName = g.Key.ProductName,
                    CategoryName = g.Key.CategoryName,
                    ImageUrl = g.Key.ImageUrl,
                    SoldCount = g.Sum(oi => oi.Quantity),
                    Revenue = g.Sum(oi => oi.Quantity * oi.UnitSalePrice)
                })
                .OrderByDescending(p => p.SoldCount)
                .Take(5)
                .ToListAsync();

            var topSellingProductDtos = topSellingProducts
                .Select(p => new TopSellingProductDto
                {
                    Id = p.ProductId,
                    Name = p.ProductName,
                    CategoryName = p.CategoryName,
                    SoldCount = p.SoldCount,
                    Revenue = p.Revenue,
                    ImageUrl = p.ImageUrl
                })
                .ToList();

            _logger.LogDebug("Found {Count} top-selling products (platform-wide)", topSellingProductDtos.Count);

            // Top 5 sales agents by GMV
            var topSalesAgents = periodOrders
                .GroupBy(o => new
                {
                    o.SaleAgentId,
                    AgentName = o.SaleAgent?.Profile?.FullName ?? o.SaleAgent?.Username ?? "Unknown",
                    AgentEmail = o.SaleAgent?.Email ?? ""
                })
                .Select(g => new
                {
                    AgentId = g.Key.SaleAgentId,
                    AgentName = g.Key.AgentName,
                    AgentEmail = g.Key.AgentEmail,
                    TotalGmv = g.Sum(o => o.GrandTotal),
                    OrderCount = g.Count()
                })
                .OrderByDescending(a => a.TotalGmv)
                .Take(5)
                .ToList();

            var topSalesAgentDtos = new List<TopSalesAgentDto>();
            foreach (var agent in topSalesAgents)
            {
                // Get product count for this agent
                var productCount = await _context.Products
                    .Where(p => p.SaleAgentId == agent.AgentId)
                    .CountAsync();

                // Calculate agent commission (95% of GMV, as 5% goes to platform)
                var agentCommission = Math.Round(agent.TotalGmv * (1 - platformFee), 2);

                topSalesAgentDtos.Add(new TopSalesAgentDto
                {
                    Id = agent.AgentId,
                    Name = agent.AgentName,
                    Email = agent.AgentEmail,
                    TotalGmv = agent.TotalGmv,
                    Commission = agentCommission,
                    ProductCount = productCount,
                    OrderCount = agent.OrderCount,
                    //Rating = 4.5 // Placeholder - TODO: implement real rating system
                });
            }

            _logger.LogDebug("Found {Count} top sales agents", topSalesAgentDtos.Count);

            var summary = new AdminDashboardSummaryResponse
            {
                TotalGmv = totalGmv,
                AdminCommission = adminCommission,
                ActiveSalesAgents = activeSalesAgents,
                TotalProducts = totalProducts,
                TotalOrders = totalOrders,
                TotalRevenue = totalGmv,
                TopSellingProducts = topSellingProductDtos,
                TopSalesAgents = topSalesAgentDtos
            };

            _logger.LogInformation(
                "Admin dashboard summary calculated: GMV={GMV}, Commission={Commission}, Orders={Orders}, Agents={Agents}",
                totalGmv, adminCommission, totalOrders, activeSalesAgents);

            return summary;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating admin dashboard summary");
            throw;
        }
    }

    public async Task<AdminRevenueChartResponse> GetAdminRevenueChartAsync(string period = "week")
    {
        try
        {
            _logger.LogInformation("Calculating admin revenue chart for period: {Period}", period);

            // Get all orders across all sales agents (exclude cancelled)
            var allOrders = await _context.Orders
                .Where(o => o.Status != OrderStatus.Cancelled)
                .ToListAsync();

            var chartResponse = new AdminRevenueChartResponse();

            // Generate labels and calculate revenue/commission based on period
            switch (period.ToLower())
            {
                case "day":
                    // Hourly data for today (24 hours)
                    chartResponse = GenerateAdminHourlyChart(allOrders);
                    break;

                case "week":
                    // Daily data for this week (7 days: Mon-Sun)
                    chartResponse = GenerateAdminWeeklyChart(allOrders);
                    break;

                case "month":
                    // Daily data for this month (1st to today)
                    chartResponse = GenerateAdminMonthlyChart(allOrders);
                    break;

                case "year":
                    // Monthly data for this year (Jan to current month)
                    chartResponse = GenerateAdminYearlyChart(allOrders);
                    break;

                default:
                    _logger.LogWarning("Invalid chart period: {Period}, defaulting to week", period);
                    chartResponse = GenerateAdminWeeklyChart(allOrders);
                    break;
            }

            _logger.LogInformation(
                "Admin revenue chart calculated: {DataPoints} data points",
                chartResponse.Labels.Count);

            return chartResponse;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating admin revenue chart");
            throw;
        }
    }

    #region Admin Chart Generation Methods

    /// <summary>
    /// Generate hourly chart for today (24 hours) - Admin version
    /// </summary>
    private AdminRevenueChartResponse GenerateAdminHourlyChart(List<Data.Entities.Order> orders)
    {
        var today = DateTime.UtcNow.Date;
        var chartResponse = new AdminRevenueChartResponse();

        // Generate 24 hour labels (0-23)
        for (int hour = 0; hour < 24; hour++)
        {
            var hourStart = today.AddHours(hour);
            var hourEnd = hourStart.AddHours(1);

            // Format: "0", "1", "2", ... "23"
            chartResponse.Labels.Add(hour.ToString());

            // Calculate revenue for this hour (platform-wide)
            var hourRevenue = orders
                .Where(o => o.OrderDate >= hourStart && o.OrderDate < hourEnd)
                .Sum(o => o.GrandTotal);

            // Calculate admin commission (5% of revenue)
            var platformFee = _configuration.GetValue<decimal>("BusinessSettings:PlatformFee");

            var hourCommission = Math.Round(hourRevenue * platformFee, 2);

            chartResponse.RevenueData.Add(hourRevenue);
            chartResponse.CommissionData.Add(hourCommission);
        }

        return chartResponse;
    }

    /// <summary>
    /// Generate daily chart for this week (Mon-Sun) - Admin version
    /// </summary>
    private AdminRevenueChartResponse GenerateAdminWeeklyChart(List<Data.Entities.Order> orders)
    {
        var now = DateTime.UtcNow;
        var startOfWeek = GetStartOfWeek(now);
        var chartResponse = new AdminRevenueChartResponse();

        // Generate 7 day labels (Monday to Sunday)
        var dayNames = new[] { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" };

        for (int i = 0; i < 7; i++)
        {
            var dayStart = startOfWeek.AddDays(i).Date;
            var dayEnd = dayStart.AddDays(1);

            chartResponse.Labels.Add(dayNames[i]);

            // Calculate revenue for this day (platform-wide)
            var dayRevenue = orders
                .Where(o => o.OrderDate >= dayStart && o.OrderDate < dayEnd)
                .Sum(o => o.GrandTotal);

            // Calculate admin commission (5% of revenue)
            var platformFee = _configuration.GetValue<decimal>("BusinessSettings:PlatformFee");

            var dayCommission = Math.Round(dayRevenue * platformFee, 2);

            chartResponse.RevenueData.Add(dayRevenue);
            chartResponse.CommissionData.Add(dayCommission);
        }

        return chartResponse;
    }

    /// <summary>
    /// Generate daily chart for this month (1st to today) - Admin version
    /// </summary>
    private AdminRevenueChartResponse GenerateAdminMonthlyChart(List<Data.Entities.Order> orders)
    {
        var now = DateTime.UtcNow;
        var chartResponse = new AdminRevenueChartResponse();

        // Generate labels for each day of the month up to today
        var currentDay = (int)now.Day;

        for (int day = 1; day <= currentDay; day++)
        {
            var dayStart = new DateTime(now.Year, now.Month, day);
            var dayEnd = dayStart.AddDays(1);

            // Format: "1", "2", "3", ... "31"
            chartResponse.Labels.Add(day.ToString());

            // Calculate revenue for this day (platform-wide)
            var dayRevenue = orders
                .Where(o => o.OrderDate >= dayStart && o.OrderDate < dayEnd)
                .Sum(o => o.GrandTotal);

            // Calculate admin commission (5% of revenue)
            var platformFee = _configuration.GetValue<decimal>("BusinessSettings:PlatformFee");

            var dayCommission = Math.Round(dayRevenue * platformFee, 2);

            chartResponse.RevenueData.Add(dayRevenue);
            chartResponse.CommissionData.Add(dayCommission);
        }

        return chartResponse;
    }

    /// <summary>
    /// Generate monthly chart for this year (Jan to current month) - Admin version
    /// </summary>
    private AdminRevenueChartResponse GenerateAdminYearlyChart(List<Data.Entities.Order> orders)
    {
        var now = DateTime.UtcNow;
        var chartResponse = new AdminRevenueChartResponse();

        // Month abbreviations
        var monthNames = new[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun",
                                "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };

        // Generate labels for each month up to current month
        for (int month = 1; month <= now.Month; month++)
        {
            var monthStart = new DateTime(now.Year, month, 1);
            var monthEnd = monthStart.AddMonths(1);

            chartResponse.Labels.Add(monthNames[month - 1]);

            // Calculate revenue for this month (platform-wide)
            var monthRevenue = orders
                .Where(o => o.OrderDate >= monthStart && o.OrderDate < monthEnd)
                .Sum(o => o.GrandTotal);

            // Calculate admin commission (5% of revenue)
            var platformFee = _configuration.GetValue<decimal>("BusinessSettings:PlatformFee");

            var monthCommission = Math.Round(monthRevenue * platformFee, 2);

            chartResponse.RevenueData.Add(monthRevenue);
            chartResponse.CommissionData.Add(monthCommission);
        }

        return chartResponse;
    }

    #endregion
}
