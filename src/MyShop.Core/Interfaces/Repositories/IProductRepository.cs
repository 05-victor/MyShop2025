using MyShop.Shared.DTOs.Commons;
using MyShop.Shared.Models;
using MyShop.Core.Common;

namespace MyShop.Core.Interfaces.Repositories;

/// <summary>
/// Repository interface for product management
/// </summary>
public interface IProductRepository
{
    Task<Result<IEnumerable<Product>>> GetAllAsync();
    Task<Result<Product>> GetByIdAsync(Guid id);
    Task<Result<Product>> CreateAsync(Product product);
    Task<Result<Product>> UpdateAsync(Product product);
    Task<Result<bool>> DeleteAsync(Guid id);

    /// <summary>
    /// Get products with low stock (below threshold)
    /// </summary>
    Task<Result<IEnumerable<Product>>> GetLowStockAsync(int threshold = 10);

    /// <summary>
    /// Get products by category ID
    /// </summary>
    Task<Result<IEnumerable<Product>>> GetByCategoryAsync(Guid categoryId);

    /// <summary>
    /// Search products by query
    /// </summary>
    Task<Result<IEnumerable<Product>>> SearchAsync(string query);

    /// <summary>
    /// Get paged products with optional search and filters (server-side)
    /// </summary>
    Task<Result<PagedList<Product>>> GetPagedAsync(
        int page = 1,
        int pageSize = Common.PaginationConstants.ProductsPageSize,
        string? searchQuery = null,
        string? categoryName = null,
        string? manufacturerName = null,
        string? brandName = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        string? stockStatus = null,
        string sortBy = "name",
        bool sortDescending = false);

    /// <summary>
    /// Create multiple products at once using bulk API
    /// </summary>
    Task<Result<BulkImportResult>> BulkCreateAsync(List<Product> products);
}
