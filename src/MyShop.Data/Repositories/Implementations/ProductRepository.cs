using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyShop.Data.Entities;
using MyShop.Data.Repositories.Interfaces;
using MyShop.Shared.DTOs.Commons;

namespace MyShop.Data.Repositories.Implementations;

public class ProductRepository : IProductRepository
{
    private readonly ShopContext _context;
    private readonly ILogger<ProductRepository> _logger;

    public ProductRepository(ShopContext context, ILogger<ProductRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<Product>> GetAllAsync()
    {
        return await _context.Products
            .Include(p => p.Category)
            .Include(p => p.SaleAgent)
                .ThenInclude(u => u.Profile)
            .ToListAsync();
    }

    public async Task<PagedResult<Product>> GetAllAsync(int pageNumber, int pageSize)
    {
        var query = _context.Products
            .Include(p => p.Category)
            .Include(p => p.SaleAgent)
                .ThenInclude(u => u.Profile);

        var totalCount = await query.CountAsync();
        
        var items = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<Product>
        {
            Items = items,
            TotalCount = totalCount,
            Page = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<Product?> GetByIdAsync(Guid id)
    {
        return await _context.Products
            .Include(p => p.Category)
            .Include(p => p.SaleAgent)
                .ThenInclude(u => u.Profile)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Product> CreateAsync(Product product)
    {
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        // Reload with navigation properties
        await _context.Entry(product)
            .Reference(p => p.Category)
            .LoadAsync();

        if (product.SaleAgentId.HasValue)
        {
            await _context.Entry(product)
                .Reference(p => p.SaleAgent)
                .LoadAsync();

            if (product.SaleAgent != null)
            {
                await _context.Entry(product.SaleAgent)
                    .Reference(u => u.Profile)
                    .LoadAsync();
            }
        }

        return product;
    }

    public async Task<Product> UpdateAsync(Product product)
    {
        _context.Products.Update(product);
        await _context.SaveChangesAsync();
        return product;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var product = await GetByIdAsync(id);
        if (product == null)
            return false;

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
        return true;
    }
}
