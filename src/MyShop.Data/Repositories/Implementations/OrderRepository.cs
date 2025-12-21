using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyShop.Data.Entities;
using MyShop.Data.Repositories.Interfaces;
using MyShop.Shared.DTOs.Commons;
using MyShop.Shared.Enums;
using MyShop.Shared.Extensions;

namespace MyShop.Data.Repositories.Implementations;

public class OrderRepository : IOrderRepository
{
    private readonly ShopContext _context;
    private readonly ILogger<OrderRepository> _logger;

    public OrderRepository(ShopContext context, ILogger<OrderRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<Order>> GetAllAsync()
    {
        return await _context.Orders
            .Include(o => o.Customer)
                .ThenInclude(u => u.Profile)
            .Include(o => o.SaleAgent)
                .ThenInclude(u => u.Profile)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .ToListAsync();
    }

    public async Task<Order?> GetByIdAsync(Guid id)
    {
        return await _context.Orders
            .Include(o => o.Customer)
                .ThenInclude(u => u.Profile)
            .Include(o => o.SaleAgent)
                .ThenInclude(u => u.Profile)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<Order> CreateAsync(Order order)
    {
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        // Reload with navigation properties
        await _context.Entry(order)
            .Reference(o => o.Customer)
            .LoadAsync();

        if (order.Customer != null)
        {
            await _context.Entry(order.Customer)
                .Reference(u => u.Profile)
                .LoadAsync();
        }

        await _context.Entry(order)
            .Reference(o => o.SaleAgent)
            .LoadAsync();

        if (order.SaleAgent != null)
        {
            await _context.Entry(order.SaleAgent)
                .Reference(u => u.Profile)
                .LoadAsync();
        }

        // Load order items with products
        await _context.Entry(order)
            .Collection(o => o.OrderItems)
            .LoadAsync();

        foreach (var orderItem in order.OrderItems)
        {
            await _context.Entry(orderItem)
                .Reference(oi => oi.Product)
                .LoadAsync();
        }

        return order;
    }

    public async Task<Order> UpdateAsync(Order order)
    {
        _context.Orders.Update(order);
        await _context.SaveChangesAsync();
        return order;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var order = await GetByIdAsync(id);
        if (order == null)
            return false;

        _context.Orders.Remove(order);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<PagedResult<Order>> GetOrdersBySalesAgentIdAsync(int pageNumber, int pageSize, Guid salesAgentId)
    {
        var query = _context.Orders
            .Where(o => o.SaleAgentId == salesAgentId)
            .Include(o => o.Customer)
                .ThenInclude(u => u.Profile)
            .Include(o => o.SaleAgent)
                .ThenInclude(u => u.Profile)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .AsNoTracking();

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<Order>
        {
            Items = items,
            TotalCount = totalCount,
            Page = pageNumber,
            PageSize = pageSize,
        };
    }

    public async Task<PagedResult<Order>> GetOrdersByCustomerIdAsync(int pageNumber, int pageSize, Guid customerId)
    {
        var query = _context.Orders
            .Where(o => o.CustomerId == customerId)
            .Include(o => o.Customer)
                .ThenInclude(u => u.Profile)
            .Include(o => o.SaleAgent)
                .ThenInclude(u => u.Profile)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .AsNoTracking();

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<Order>
        {
            Items = items,
            TotalCount = totalCount,
            Page = pageNumber,
            PageSize = pageSize,
        };
    }

    public async Task<PagedResult<Order>> GetFilteredOrdersBySalesAgentAsync(
        Guid salesAgentId,
        int pageNumber,
        int pageSize,
        DateTime? startDate = null,
        DateTime? endDate = null,
        string? status = null,
        string? paymentStatus = null,
        string sortBy = "OrderDate",
        bool sortDescending = true)
    {
        _logger.LogInformation(
            "Getting filtered orders for sales agent {SalesAgentId}: Page={Page}, PageSize={PageSize}, StartDate={StartDate}, EndDate={EndDate}, Status={Status}, PaymentStatus={PaymentStatus}",
            salesAgentId, pageNumber, pageSize, startDate, endDate, status, paymentStatus);

        var query = _context.Orders
            .Where(o => o.SaleAgentId == salesAgentId)
            .Include(o => o.Customer)
                .ThenInclude(u => u.Profile)
            .Include(o => o.SaleAgent)
                .ThenInclude(u => u.Profile)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .AsNoTracking();

        // Apply date filters
        if (startDate.HasValue)
        {
            query = query.Where(o => o.OrderDate >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            // Include the entire end date (up to 23:59:59)
            var endOfDay = endDate.Value.Date.AddDays(1).AddSeconds(-1);
            query = query.Where(o => o.OrderDate <= endOfDay);
        }

        // Apply status filters
        if (!string.IsNullOrWhiteSpace(status))
        {
            var statusEnum = StatusEnumExtensions.ParseApiString<OrderStatus>(status);
            query = query.Where(o => o.Status == statusEnum);
        }

        if (!string.IsNullOrWhiteSpace(paymentStatus))
        {
            var paymentStatusEnum = StatusEnumExtensions.ParseApiString<PaymentStatus>(paymentStatus);
            query = query.Where(o => o.PaymentStatus == paymentStatusEnum);
        }

        // Apply sorting
        query = sortBy.ToLower() switch
        {
            "orderdate" => sortDescending
                ? query.OrderByDescending(o => o.OrderDate)
                : query.OrderBy(o => o.OrderDate),
            "grandtotal" or "amount" => sortDescending
                ? query.OrderByDescending(o => o.GrandTotal)
                : query.OrderBy(o => o.GrandTotal),
            "status" => sortDescending
                ? query.OrderByDescending(o => o.Status)
                : query.OrderBy(o => o.Status),
            "paymentstatus" => sortDescending
                ? query.OrderByDescending(o => o.PaymentStatus)
                : query.OrderBy(o => o.PaymentStatus),
            _ => sortDescending
                ? query.OrderByDescending(o => o.OrderDate)
                : query.OrderBy(o => o.OrderDate)
        };

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        _logger.LogInformation(
            "Loaded {Count} orders (page {Page}/{TotalPages}, total: {TotalCount})",
            items.Count, pageNumber, (int)Math.Ceiling((double)totalCount / pageSize), totalCount);

        return new PagedResult<Order>
        {
            Items = items,
            TotalCount = totalCount,
            Page = pageNumber,
            PageSize = pageSize,
        };
    }
}
