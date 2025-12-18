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

    public DashboardService(
        IProductRepository productRepository,
        IOrderRepository orderRepository,
        ICurrentUserService currentUserService,
        ShopContext context,
        ILogger<DashboardService> logger)
    {
        _productRepository = productRepository;
        _orderRepository = orderRepository;
        _currentUserService = currentUserService;
        _context = context;
        _logger = logger;
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
            DateTime? startDate = null;
            if (!string.IsNullOrWhiteSpace(period))
            {
                var now = DateTime.UtcNow;
                startDate = GetStartDateForPeriod(period, now);
            }

            // Filter orders by period and exclude cancelled orders
            var periodOrders = startDate.HasValue
                ? allOrders.Where(o => o.OrderDate >= startDate.Value && o.Status != OrderStatus.Cancelled).ToList()
                : allOrders.Where(o => o.Status != OrderStatus.Cancelled).ToList();

            var totalOrders = periodOrders.Count;
            var totalRevenue = periodOrders.Sum(o => o.GrandTotal);

            _logger.LogDebug("Period: {Period}, Orders: {Orders}, Revenue: {Revenue}",
                period ?? "all-time", totalOrders, totalRevenue);

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

    /// <summary>
    /// Get start date for the specified period
    /// </summary>
    private static DateTime GetStartDateForPeriod(string period, DateTime now)
    {
        return period.ToLower() switch
        {
            "day" => now.Date,
            "week" => GetStartOfWeek(now),
            "month" => now.StartOfMonth(),
            "year" => new DateTime(now.Year, 1, 1),
            _ => DateTime.MinValue // Invalid period = all time
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
}
