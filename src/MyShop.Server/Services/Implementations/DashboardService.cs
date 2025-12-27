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
            var dayStart = new DateTime(now.Year, now.Month, day, 0, 0, 0, DateTimeKind.Utc);
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
            var monthStart = new DateTime(now.Year, month, 1, 0, 0, 0, DateTimeKind.Utc);
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
            "year" => (new DateTime(now.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc), now),  // This year: Jan 1 to now (UTC)
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
            var dayStart = new DateTime(now.Year, now.Month, day, 0, 0, 0, DateTimeKind.Utc);
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
            var monthStart = new DateTime(now.Year, month, 1, 0, 0, 0, DateTimeKind.Utc);
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

    public async Task<AdminReportsResponse> GetAdminReportsAsync(
        DateTime from,
        DateTime to,
        Guid? categoryId = null,
        int pageNumber = 1,
        int pageSize = 10)
    {
        try
        {
            _logger.LogInformation(
                "Generating admin reports from {From} to {To}, categoryId={CategoryId}, page={Page}, pageSize={PageSize}",
                from.ToString("yyyy-MM-dd"), to.ToString("yyyy-MM-dd"), categoryId, pageNumber, pageSize);

            var response = new AdminReportsResponse
            {
                Period = new PeriodInfo
                {
                    From = from,
                    To = to
                }
            };

            // Get all orders in the period (exclude cancelled)
            var ordersQuery = _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                        .ThenInclude(p => p.Category)
                .Include(o => o.SaleAgent)
                    .ThenInclude(sa => sa.Profile)
                .Where(o => o.OrderDate >= from && o.OrderDate <= to)
                .Where(o => o.Status != OrderStatus.Cancelled);

            var orders = await ordersQuery.ToListAsync();

            _logger.LogDebug("Found {Count} orders in the period", orders.Count);

            // 1. Revenue Trend (daily breakdown)
            response.RevenueTrend = await GenerateRevenueTrendAsync(from, to, orders);
            _logger.LogDebug("Revenue trend: {Count} data points", response.RevenueTrend.Count);

            // 2. Orders by Category
            response.OrdersByCategory = await GenerateOrdersByCategoryAsync(orders);
            _logger.LogDebug("Orders by category: {Count} categories", response.OrdersByCategory.Count);

            // 3. Product Ratings Analysis
            response.ProductRatings = await GenerateProductRatingsAnalysisAsync();
            _logger.LogDebug("Product ratings: {Total} total ratings", response.ProductRatings.TotalRatings);

            // 4. Salesperson Contributions (top 10 by revenue)
            response.SalespersonContributions = await GenerateSalespersonContributionsAsync(orders);
            _logger.LogDebug("Salesperson contributions: {Count} agents", response.SalespersonContributions.Count);

            // 5. Product Summary (paginated)
            response.ProductSummary = await GenerateProductSummaryAsync(from, to, categoryId, pageNumber, pageSize);
            _logger.LogDebug("Product summary: {Count} products, total {Total}",
                response.ProductSummary.Data.Count, response.ProductSummary.TotalCount);

            _logger.LogInformation("Admin reports generated successfully");

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating admin reports");
            throw;
        }
    }

    #region Admin Reports Helper Methods

    /// <summary>
    /// Generate revenue trend data (daily breakdown)
    /// </summary>
    private async Task<List<RevenueTrendItem>> GenerateRevenueTrendAsync(
        DateTime from,
        DateTime to,
        List<Data.Entities.Order> orders)
    {
        var trend = new List<RevenueTrendItem>();
        
        // If no orders, return empty trend
        if (!orders.Any())
        {
            return trend;
        }

        // Use provided date range (controller already validated and set defaults)
        var startDate = from.Date;
        var endDate = to.Date;

        _logger.LogDebug("Generating revenue trend from {Start} to {End}", 
            startDate.ToString("yyyy-MM-dd"), endDate.ToString("yyyy-MM-dd"));

        var currentDate = startDate;

        while (currentDate <= endDate)
        {
            var dayOrders = orders.Where(o => o.OrderDate.Date == currentDate).ToList();
            var dayRevenue = dayOrders.Sum(o => o.GrandTotal);
            var dayOrderCount = dayOrders.Count;

            trend.Add(new RevenueTrendItem
            {
                Date = currentDate.ToString("yyyy-MM-dd"),
                Revenue = dayRevenue,
                OrderCount = dayOrderCount,
                AverageOrderValue = dayOrderCount > 0 ? dayRevenue / dayOrderCount : 0
            });

            currentDate = currentDate.AddDays(1);
        }

        return await Task.FromResult(trend);
    }

    /// <summary>
    /// Generate orders by category analysis
    /// </summary>
    private async Task<List<OrdersByCategoryItem>> GenerateOrdersByCategoryAsync(
        List<Data.Entities.Order> orders)
    {
        var totalRevenue = orders.Sum(o => o.GrandTotal);

        var categoryStats = orders
            .SelectMany(o => o.OrderItems)
            .GroupBy(oi => new
            {
                CategoryId = oi.Product.CategoryId,
                CategoryName = oi.Product.Category.Name
            })
            .Select(g => new
            {
                g.Key.CategoryId,
                g.Key.CategoryName,
                OrderCount = g.Select(oi => oi.OrderId).Distinct().Count(),
                Revenue = g.Sum(oi => oi.TotalPrice)
            })
            .OrderByDescending(c => c.Revenue)
            .ToList();

        var result = categoryStats.Select(c => new OrdersByCategoryItem
        {
            CategoryId = c.CategoryId,
            CategoryName = c.CategoryName,
            OrderCount = c.OrderCount,
            Revenue = c.Revenue,
            Percentage = totalRevenue > 0 ? Math.Round((c.Revenue / (decimal)totalRevenue) * 100, 1) : 0
        }).ToList();

        return await Task.FromResult(result);
    }

    /// <summary>
    /// Generate product ratings analysis (distribution of 1-5 star ratings)
    /// </summary>
    private async Task<ProductRatingAnalysis> GenerateProductRatingsAnalysisAsync()
    {
        // TODO: Implement when product rating system is added to database
        // Current Product entity doesn't have Rating/RatingCount properties
        // For now, return placeholder data
        
        _logger.LogDebug("Product ratings analysis not yet implemented - returning placeholder data");
        
        var analysis = new ProductRatingAnalysis
        {
            Excellent = 0,
            VeryGood = 0,
            Good = 0,
            Fair = 0,
            Poor = 0,
            TotalRatings = 0,
            AverageRating = 0
        };

        return await Task.FromResult(analysis);
        
        // FUTURE IMPLEMENTATION:
        // Once Rating table is added, query like this:
        // var ratings = await _context.ProductRatings
        //     .Where(r => r.CreatedAt >= from && r.CreatedAt <= to)
        //     .GroupBy(r => r.Rating)
        //     .Select(g => new { Rating = g.Key, Count = g.Count() })
        //     .ToListAsync();
    }

    /// <summary>
    /// Generate salesperson contributions (top 10 by revenue)
    /// </summary>
    private async Task<List<SalespersonContribution>> GenerateSalespersonContributionsAsync(
        List<Data.Entities.Order> orders)
    {
        var totalRevenue = orders.Sum(o => o.GrandTotal);

        var platformFee = _configuration.GetValue<decimal>("BusinessSettings:PlatformFee");

        var agentStats = orders
            .GroupBy(o => new
            {
                o.SaleAgentId,
                FirstName = o.SaleAgent?.Profile?.FullName?.Split(' ').FirstOrDefault() ?? "Unknown",
                LastName = o.SaleAgent?.Profile?.FullName?.Split(' ').LastOrDefault() ?? "",
                Avatar = o.SaleAgent?.Profile?.Avatar ?? "",
                Email = o.SaleAgent?.Email ?? ""
            })
            .Select(g => new
            {
                g.Key.SaleAgentId,
                g.Key.FirstName,
                g.Key.LastName,
                g.Key.Avatar,
                g.Key.Email,
                TotalSales = g.Count(),
                TotalRevenue = g.Sum(o => o.GrandTotal)
            })
            .OrderByDescending(a => a.TotalRevenue)
            .Take(10)
            .ToList();

        var result = agentStats.Select(a => new SalespersonContribution
        {
            SalespersonId = a.SaleAgentId,
            FirstName = a.FirstName,
            LastName = a.LastName,
            Email = a.Email,
            Avatar = a.Avatar,
            TotalSales = a.TotalSales,
            TotalRevenue = a.TotalRevenue,
            Commission = Math.Round(a.TotalRevenue * (1 - platformFee), 2), // Agent gets 95%, platform gets 5%
            Percentage = totalRevenue > 0 ? Math.Round((a.TotalRevenue / (decimal)totalRevenue) * 100, 1) : 0
        }).ToList();

        return await Task.FromResult(result);
    }

    /// <summary>
    /// Generate product summary (paginated, top products by revenue)
    /// </summary>
    private async Task<PagedProductSummary> GenerateProductSummaryAsync(
        DateTime from,
        DateTime to,
        Guid? categoryId = null,
        int pageNumber = 1,
        int pageSize = 10)
    {
        // Get product statistics from orders in the period
        var productStatsQuery = _context.OrderItems
            .Where(oi => oi.Order.OrderDate >= from && oi.Order.OrderDate <= to)
            .Where(oi => oi.Order.Status != OrderStatus.Cancelled)
            .GroupBy(oi => new
            {
                oi.ProductId,
                ProductName = oi.Product.Name,
                CategoryName = oi.Product.Category.Name,
                CategoryId = oi.Product.CategoryId,
                oi.Product.Quantity,
                oi.Product.Status
            })
            .Select(g => new
            {
                g.Key.ProductId,
                g.Key.ProductName,
                g.Key.CategoryName,
                g.Key.CategoryId,
                g.Key.Quantity,
                g.Key.Status,
                TotalOrders = g.Select(oi => oi.OrderId).Distinct().Count(),
                TotalRevenue = g.Sum(oi => oi.TotalPrice)
            });

        // Apply category filter if specified
        if (categoryId.HasValue)
        {
            productStatsQuery = productStatsQuery.Where(p => p.CategoryId == categoryId.Value);
        }

        // Order by revenue descending
        productStatsQuery = productStatsQuery.OrderByDescending(p => p.TotalRevenue);

        // Get total count
        var totalCount = await productStatsQuery.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        // Apply pagination
        var pagedData = await productStatsQuery
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var items = pagedData.Select(p => new ProductSummaryItem
        {
            ProductId = p.ProductId,
            ProductName = p.ProductName,
            CategoryName = p.CategoryName,
            TotalOrders = p.TotalOrders,
            TotalRevenue = p.TotalRevenue,
            AverageRating = 0, // TODO: Implement when rating system is added
            StockLevel = p.Quantity,
            Status = GetStockStatus(p.Quantity, p.Status),
            StatusColor = GetStockStatusColor(p.Quantity, p.Status),
            LowStockThreshold = 10
        }).ToList();

        return new PagedProductSummary
        {
            Data = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages
        };
    }

    /// <summary>
    /// Determine stock status based on quantity and product status
    /// </summary>
    private static string GetStockStatus(int quantity, ProductStatus status)
    {
        if (status == ProductStatus.Discontinued)
            return "DISCONTINUED";
        if (quantity == 0)
            return "OUT_OF_STOCK";
        if (quantity <= 10)
            return "LOW_STOCK";
        return "IN_STOCK";
    }

    /// <summary>
    /// Get status color for UI display
    /// </summary>
    private static string GetStockStatusColor(int quantity, ProductStatus status)
    {
        return GetStockStatus(quantity, status) switch
        {
            "IN_STOCK" => "#10b981",        // Green
            "LOW_STOCK" => "#f59e0b",       // Orange
            "OUT_OF_STOCK" => "#ef4444",    // Red
            "DISCONTINUED" => "#6b7280",    // Gray
            _ => "#10b981"
        };
    }

    #endregion

    #region Sales Agent Reports

    public async Task<SalesAgentReportsResponse> GetSalesAgentReportsAsync(string period, Guid? categoryId = null)
    {
        try
        {
            var currentUserId = _currentUserService.UserId;
            if (!currentUserId.HasValue)
            {
                throw new UnauthorizedAccessException("User not authenticated");
            }

            var salesAgentId = currentUserId.Value;
            _logger.LogInformation(
                "Generating sales agent reports for {AgentId}, period: {Period}, categoryId: {CategoryId}",
                salesAgentId, period, categoryId);

            var response = new SalesAgentReportsResponse();

            // Get date range based on period
            var (startDate, endDate) = GetDateRangeForPeriod(period);

            _logger.LogDebug("Date range: {Start} to {End}", startDate, endDate);

            // Get all orders for this sales agent in the period (exclude cancelled)
            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                        .ThenInclude(p => p.Category)
                .Where(o => o.SaleAgentId == salesAgentId)
                .Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate)
                .Where(o => o.Status != OrderStatus.Cancelled)
                .ToListAsync();

            _logger.LogDebug("Found {Count} orders for sales agent in period", orders.Count);

            // 1. Revenue Trend
            response.RevenueTrend = await GenerateSalesAgentRevenueTrendAsync(period, startDate, endDate, orders);
            _logger.LogDebug("Revenue trend: {Count} data points", response.RevenueTrend.Count);

            // 2. Orders by Category
            response.OrdersByCategory = await GenerateSalesAgentOrdersByCategoryAsync(orders, categoryId);
            _logger.LogDebug("Orders by category: {Count} categories", response.OrdersByCategory.Count);

            // 3. Top Products (top 5)
            response.TopProducts = await GenerateSalesAgentTopProductsAsync(orders, categoryId);
            _logger.LogDebug("Top products: {Count} products", response.TopProducts.Count);

            _logger.LogInformation("Sales agent reports generated successfully for {AgentId}", salesAgentId);

            return response;
        }
        catch (UnauthorizedAccessException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating sales agent reports");
            throw;
        }
    }

    #region Sales Agent Reports Helper Methods

    /// <summary>
    /// Get date range for period (always "current" period, e.g., "this week", "this month")
    /// </summary>
    private static (DateTime startDate, DateTime endDate) GetDateRangeForPeriod(string period)
    {
        var now = DateTime.UtcNow;

        return period.ToLower() switch
        {
            "day" => (now.Date, now),                                    // Today: 00:00:00 to now
            "week" => (GetStartOfWeek(now), now),                        // This week: Monday to now
            "month" => (now.StartOfMonth(), now),                        // This month: 1st to now
            "year" => (new DateTime(now.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc), now),  // This year: Jan 1 to now
            _ => throw new ArgumentException($"Invalid period: {period}. Valid values are: day, week, month, year")
        };
    }

    /// <summary>
    /// Generate revenue trend for sales agent based on period
    /// </summary>
    private async Task<List<SalesAgentRevenueTrendItem>> GenerateSalesAgentRevenueTrendAsync(
        string period,
        DateTime startDate,
        DateTime endDate,
        List<Data.Entities.Order> orders)
    {
        var trend = new List<SalesAgentRevenueTrendItem>();

        switch (period.ToLower())
        {
            case "day":
                // Group by 3-hour blocks for today
                for (int hour = 0; hour < 24; hour += 3)
                {
                    var blockStart = startDate.AddHours(hour);
                    var blockEnd = blockStart.AddHours(3);

                    var blockOrders = orders.Where(o => o.OrderDate >= blockStart && o.OrderDate < blockEnd).ToList();
                    var blockRevenue = blockOrders.Sum(o => o.GrandTotal);
                    var blockOrderCount = blockOrders.Count;

                    trend.Add(new SalesAgentRevenueTrendItem
                    {
                        Date = $"{hour:D2}:00-{(hour + 3):D2}:00",
                        Revenue = blockRevenue,
                        OrderCount = blockOrderCount,
                        AverageOrderValue = blockOrderCount > 0 ? blockRevenue / blockOrderCount : 0
                    });
                }
                break;

            case "week":
                // Daily data for the week (Mon-Sun)
                var dayNames = new[] { "Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun" };
                for (int i = 0; i < 7; i++)
                {
                    var dayStart = startDate.AddDays(i).Date;
                    var dayEnd = dayStart.AddDays(1);

                    var dayOrders = orders.Where(o => o.OrderDate >= dayStart && o.OrderDate < dayEnd).ToList();
                    var dayRevenue = dayOrders.Sum(o => o.GrandTotal);
                    var dayOrderCount = dayOrders.Count;

                    trend.Add(new SalesAgentRevenueTrendItem
                    {
                        Date = dayNames[i],
                        Revenue = dayRevenue,
                        OrderCount = dayOrderCount,
                        AverageOrderValue = dayOrderCount > 0 ? dayRevenue / dayOrderCount : 0
                    });
                }
                break;

            case "month":
                // Weekly data for the month
                var currentWeekStart = startDate;
                int weekNumber = 1;

                while (currentWeekStart < endDate)
                {
                    var weekEnd = currentWeekStart.AddDays(7);
                    if (weekEnd > endDate) weekEnd = endDate;

                    var weekOrders = orders.Where(o => o.OrderDate >= currentWeekStart && o.OrderDate < weekEnd).ToList();
                    var weekRevenue = weekOrders.Sum(o => o.GrandTotal);
                    var weekOrderCount = weekOrders.Count;

                    var weekLabel = $"Week {weekNumber} ({currentWeekStart:MMM d}-{weekEnd.AddDays(-1):d})";

                    trend.Add(new SalesAgentRevenueTrendItem
                    {
                        Date = weekLabel,
                        Revenue = weekRevenue,
                        OrderCount = weekOrderCount,
                        AverageOrderValue = weekOrderCount > 0 ? weekRevenue / weekOrderCount : 0
                    });

                    currentWeekStart = weekEnd;
                    weekNumber++;
                }
                break;

            case "year":
                // Monthly data for the year
                var monthNames = new[] { "January", "February", "March", "April", "May", "June",
                                        "July", "August", "September", "October", "November", "December" };

                for (int month = 1; month <= endDate.Month; month++)
                {
                    var monthStart = new DateTime(startDate.Year, month, 1, 0, 0, 0, DateTimeKind.Utc);
                    var monthEnd = monthStart.AddMonths(1);
                    if (monthEnd > endDate) monthEnd = endDate;

                    var monthOrders = orders.Where(o => o.OrderDate >= monthStart && o.OrderDate < monthEnd).ToList();
                    var monthRevenue = monthOrders.Sum(o => o.GrandTotal);
                    var monthOrderCount = monthOrders.Count;

                    trend.Add(new SalesAgentRevenueTrendItem
                    {
                        Date = monthNames[month - 1],
                        Revenue = monthRevenue,
                        OrderCount = monthOrderCount,
                        AverageOrderValue = monthOrderCount > 0 ? monthRevenue / monthOrderCount : 0
                    });
                }
                break;
        }

        return await Task.FromResult(trend);
    }

    /// <summary>
    /// Generate orders by category for sales agent
    /// </summary>
    private async Task<List<SalesAgentOrdersByCategoryItem>> GenerateSalesAgentOrdersByCategoryAsync(
        List<Data.Entities.Order> orders,
        Guid? categoryId)
    {
        var platformFee = _configuration.GetValue<decimal>("BusinessSettings:PlatformFee");
        var totalRevenue = orders.Sum(o => o.GrandTotal);

        var categoryStats = orders
            .SelectMany(o => o.OrderItems)
            .Where(oi => !categoryId.HasValue || oi.Product.CategoryId == categoryId.Value)
            .GroupBy(oi => new
            {
                CategoryId = oi.Product.CategoryId,
                CategoryName = oi.Product.Category.Name
            })
            .Select(g => new
            {
                g.Key.CategoryId,
                g.Key.CategoryName,
                OrderCount = g.Select(oi => oi.OrderId).Distinct().Count(),
                Revenue = g.Sum(oi => oi.TotalPrice)
            })
            .OrderByDescending(c => c.Revenue)
            .ToList();

        var result = categoryStats.Select(c => new SalesAgentOrdersByCategoryItem
        {
            CategoryId = c.CategoryId,
            CategoryName = c.CategoryName,
            OrderCount = c.OrderCount,
            Revenue = c.Revenue,
            Percentage = totalRevenue > 0 ? Math.Round((decimal)((c.Revenue / totalRevenue) * 100), 1) : 0,
            Commission = Math.Round(c.Revenue * (1 - platformFee), 2) // Agent gets 95%
        }).ToList();

        return await Task.FromResult(result);
    }

    /// <summary>
    /// Generate top products for sales agent (top 5 by revenue)
    /// </summary>
    private async Task<List<SalesAgentTopProduct>> GenerateSalesAgentTopProductsAsync(
        List<Data.Entities.Order> orders,
        Guid? categoryId)
    {
        var totalRevenue = orders.Sum(o => o.GrandTotal);

        var productStats = orders
            .SelectMany(o => o.OrderItems)
            .Where(oi => !categoryId.HasValue || oi.Product.CategoryId == categoryId.Value)
            .GroupBy(oi => new
            {
                oi.ProductId,
                ProductName = oi.Product.Name,
                CategoryName = oi.Product.Category.Name
            })
            .Select(g => new
            {
                g.Key.ProductId,
                g.Key.ProductName,
                g.Key.CategoryName,
                UnitsSold = g.Sum(oi => oi.Quantity),
                Revenue = g.Sum(oi => oi.TotalPrice)
            })
            .OrderByDescending(p => p.Revenue)
            .Take(5)
            .ToList();

        var result = productStats.Select(p => new SalesAgentTopProduct
        {
            ProductId = p.ProductId,
            ProductName = p.ProductName,
            CategoryName = p.CategoryName,
            UnitsSold = p.UnitsSold,
            Revenue = p.Revenue,
            AverageRating = 0, // TODO: Implement when rating system is added
            Percentage = totalRevenue > 0 ? Math.Round((decimal)((p.Revenue / totalRevenue) * 100), 1) : 0
        }).ToList();

        return await Task.FromResult(result);
    }

    #endregion

    #endregion
}
