using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyShop.Data.Entities;
using MyShop.Data.Repositories.Interfaces;
using MyShop.Shared.DTOs.Commons;

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
}
