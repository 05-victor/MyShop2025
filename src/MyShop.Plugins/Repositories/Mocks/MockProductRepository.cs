using MyShop.Shared.Models;
using MyShop.Core.Interfaces.Repositories;
using MyShop.Plugins.Mocks.Data;
using MyShop.Core.Common;

namespace MyShop.Plugins.Repositories.Mocks;

/// <summary>
/// Mock implementation of IProductRepository - delegates to MockProductData
/// </summary>
public class MockProductRepository : IProductRepository
{
    public async Task<Result<IEnumerable<Product>>> GetAllAsync()
    {
        try
        {
            var products = await MockProductData.GetAllAsync();
            System.Diagnostics.Debug.WriteLine($"[MockProductRepository] GetAllAsync returned {products.Count} products");
            return Result<IEnumerable<Product>>.Success(products);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockProductRepository] GetAllAsync error: {ex.Message}");
            return Result<IEnumerable<Product>>.Failure($"Failed to get products: {ex.Message}");
        }
    }

    public async Task<Result<Product>> GetByIdAsync(Guid id)
    {
        try
        {
            var product = await MockProductData.GetByIdAsync(id);
            System.Diagnostics.Debug.WriteLine($"[MockProductRepository] GetByIdAsync({id}) - Found: {product != null}");
            return product != null
                ? Result<Product>.Success(product)
                : Result<Product>.Failure($"Product with ID {id} not found");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockProductRepository] GetByIdAsync error: {ex.Message}");
            return Result<Product>.Failure($"Failed to get product: {ex.Message}");
        }
    }

    public async Task<Result<Product>> CreateAsync(Product product)
    {
        try
        {
            product.Id = Guid.NewGuid();
            var created = await MockProductData.CreateAsync(product);
            System.Diagnostics.Debug.WriteLine($"[MockProductRepository] Created product: {created.Name} (ID: {created.Id})");
            return Result<Product>.Success(created);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockProductRepository] CreateAsync error: {ex.Message}");
            return Result<Product>.Failure($"Failed to create product: {ex.Message}");
        }
    }

    public async Task<Result<Product>> UpdateAsync(Product product)
    {
        try
        {
            var updated = await MockProductData.UpdateAsync(product);
            System.Diagnostics.Debug.WriteLine($"[MockProductRepository] Updated product: {updated.Name}");
            return Result<Product>.Success(updated);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockProductRepository] UpdateAsync error: {ex.Message}");
            return Result<Product>.Failure($"Failed to update product: {ex.Message}");
        }
    }

    public async Task<Result<bool>> DeleteAsync(Guid id)
    {
        try
        {
            var result = await MockProductData.DeleteAsync(id);
            System.Diagnostics.Debug.WriteLine($"[MockProductRepository] DeleteAsync({id}) - Success: {result}");
            return result
                ? Result<bool>.Success(true)
                : Result<bool>.Failure($"Failed to delete product with ID {id}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockProductRepository] DeleteAsync error: {ex.Message}");
            return Result<bool>.Failure($"Failed to delete product: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<Product>>> GetLowStockAsync(int threshold = 10)
    {
        try
        {
            var products = await MockProductData.GetLowStockAsync(threshold);
            System.Diagnostics.Debug.WriteLine($"[MockProductRepository] Found {products.Count} low stock products");
            return Result<IEnumerable<Product>>.Success(products);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockProductRepository] GetLowStockAsync error: {ex.Message}");
            return Result<IEnumerable<Product>>.Failure($"Failed to get low stock products: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<Product>>> GetByCategoryAsync(Guid categoryId)
    {
        try
        {
            var products = await MockProductData.GetByCategoryAsync(categoryId);
            System.Diagnostics.Debug.WriteLine($"[MockProductRepository] Found {products.Count} products in category");
            return Result<IEnumerable<Product>>.Success(products);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockProductRepository] GetByCategoryAsync error: {ex.Message}");
            return Result<IEnumerable<Product>>.Failure($"Failed to get products by category: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<Product>>> SearchAsync(string query)
    {
        try
        {
            var products = await MockProductData.SearchAsync(query);
            System.Diagnostics.Debug.WriteLine($"[MockProductRepository] Search '{query}' returned {products.Count} results");
            return Result<IEnumerable<Product>>.Success(products);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockProductRepository] SearchAsync error: {ex.Message}");
            return Result<IEnumerable<Product>>.Failure($"Failed to search products: {ex.Message}");
        }
    }

    public async Task<Result<PagedList<Product>>> GetPagedAsync(
        int page = 1,
        int pageSize = 20,
        string? searchQuery = null,
        string? categoryName = null,
        string? manufacturerName = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        string sortBy = "name",
        bool sortDescending = false)
    {
        try
        {
            var (items, totalCount) = await MockProductData.GetPagedAsync(
                page, pageSize, searchQuery, categoryName, manufacturerName, minPrice, maxPrice, sortBy, sortDescending);

            var pagedList = new PagedList<Product>(items, totalCount, page, pageSize);

            System.Diagnostics.Debug.WriteLine($"[MockProductRepository] GetPagedAsync: Page {page}, Size {pageSize}, Total {totalCount}");
            return Result<PagedList<Product>>.Success(pagedList);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockProductRepository] GetPagedAsync error: {ex.Message}");
            return Result<PagedList<Product>>.Failure($"Failed to get paged products: {ex.Message}");
        }
    }
}
