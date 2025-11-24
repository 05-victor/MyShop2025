using MyShop.Shared.Models;
using MyShop.Core.Interfaces.Repositories;
using MyShop.Plugins.Mocks.Data;

namespace MyShop.Plugins.Repositories.Mocks;

/// <summary>
/// Mock implementation of IProductRepository - delegates to MockProductData
/// </summary>
public class MockProductRepository : IProductRepository
{
    public async Task<IEnumerable<Product>> GetAllAsync()
    {
        try
        {
            var products = await MockProductData.GetAllAsync();
            System.Diagnostics.Debug.WriteLine($"[MockProductRepository] GetAllAsync returned {products.Count} products");
            return products;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockProductRepository] GetAllAsync error: {ex.Message}");
            return new List<Product>();
        }
    }

    public async Task<Product?> GetByIdAsync(Guid id)
    {
        try
        {
            var product = await MockProductData.GetByIdAsync(id);
            System.Diagnostics.Debug.WriteLine($"[MockProductRepository] GetByIdAsync({id}) - Found: {product != null}");
            return product;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockProductRepository] GetByIdAsync error: {ex.Message}");
            return null;
        }
    }

    public async Task<Product> CreateAsync(Product product)
    {
        try
        {
            product.Id = Guid.NewGuid();
            var created = await MockProductData.CreateAsync(product);
            System.Diagnostics.Debug.WriteLine($"[MockProductRepository] Created product: {created.Name} (ID: {created.Id})");
            return created;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockProductRepository] CreateAsync error: {ex.Message}");
            throw;
        }
    }

    public async Task<Product> UpdateAsync(Product product)
    {
        try
        {
            var updated = await MockProductData.UpdateAsync(product);
            System.Diagnostics.Debug.WriteLine($"[MockProductRepository] Updated product: {updated.Name}");
            return updated;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockProductRepository] UpdateAsync error: {ex.Message}");
            throw;
        }
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        try
        {
            var result = await MockProductData.DeleteAsync(id);
            System.Diagnostics.Debug.WriteLine($"[MockProductRepository] DeleteAsync({id}) - Success: {result}");
            return result;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockProductRepository] DeleteAsync error: {ex.Message}");
            return false;
        }
    }

    public async Task<IEnumerable<Product>> GetLowStockAsync(int threshold = 10)
    {
        try
        {
            var products = await MockProductData.GetLowStockAsync(threshold);
            System.Diagnostics.Debug.WriteLine($"[MockProductRepository] Found {products.Count} low stock products");
            return products;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockProductRepository] GetLowStockAsync error: {ex.Message}");
            return new List<Product>();
        }
    }

    public async Task<IEnumerable<Product>> GetByCategoryAsync(Guid categoryId)
    {
        try
        {
            var products = await MockProductData.GetByCategoryAsync(categoryId);
            System.Diagnostics.Debug.WriteLine($"[MockProductRepository] Found {products.Count} products in category");
            return products;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockProductRepository] GetByCategoryAsync error: {ex.Message}");
            return new List<Product>();
        }
    }

    public async Task<IEnumerable<Product>> SearchAsync(string query)
    {
        try
        {
            var products = await MockProductData.SearchAsync(query);
            System.Diagnostics.Debug.WriteLine($"[MockProductRepository] Search '{query}' returned {products.Count} results");
            return products;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockProductRepository] SearchAsync error: {ex.Message}");
            return new List<Product>();
        }
    }
}
