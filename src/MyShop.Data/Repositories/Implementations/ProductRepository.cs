using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyShop.Data.Entities;
using MyShop.Data.Repositories.Interfaces;

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
        return await _context.Products.Include(p => p.Category).ToListAsync();
    }

    public async Task<Product?> GetByIdAsync(Guid id)
    {
        return await _context.Products.Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Product> CreateAsync(Product product)
    {
        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        return product;
    }

    public async Task<Product> UpdateAsync(Product product)
    {
        _context.Products.Update(product);

        // _context.Entry(product).State = EntityState.Modified;
        // _context.Entry(product).Property(p => p.CategoryId).IsModified = false; // không đụng FK

        //_logger.LogInformation("Before SaveChanges: CategoryId = {CategoryId}", product.CategoryId);

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
