using MyShop.Core.Common;
using MyShop.Shared.Models;

namespace MyShop.Core.Interfaces.Facades;

/// <summary>
/// Facade pattern for product management operations
/// Aggregates: IProductRepository, ICategoryRepository, IValidationService, IToastService
/// Handles: Browse, Search, Filter, Paging, Export, CRUD operations
/// </summary>
public interface IProductFacade
{
    /// <summary>
    /// Load products with paging, search, and filter.
    /// Orchestrates: Repository.GetAll → Apply filters → Paging → Return PagedResult
    /// </summary>
    /// <param name="searchQuery">Search keyword</param>
    /// <param name="categoryName">Category filter</param>
    /// <param name="manufacturerName">Manufacturer/Brand filter</param>
    /// <param name="minPrice">Minimum price filter</param>
    /// <param name="maxPrice">Maximum price filter</param>
    /// <param name="stockStatus">Stock status filter (e.g., "InStock", "LowStock", "OutOfStock")</param>
    /// <param name="sortBy">Sort field (e.g., "name", "price", "date")</param>
    /// <param name="sortDescending">Sort direction</param>
    /// <param name="page">Current page (1-based)</param>
    /// <param name="pageSize">Items per page</param>
    Task<Result<PagedList<Product>>> LoadProductsAsync(
        string? searchQuery = null,
        string? categoryName = null,
        string? manufacturerName = null,
        string? brandName = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        string? stockStatus = null,
        string sortBy = "name",
        bool sortDescending = false,
        int page = 1,
        int pageSize = Common.PaginationConstants.ProductsPageSize);

    /// <summary>
    /// Get product by ID
    /// </summary>
    Task<Result<Product>> GetProductByIdAsync(Guid productId);

    /// <summary>
    /// Add new product with validation.
    /// Orchestrates: Validation → Repository.Add → Toast notification
    /// </summary>
    Task<Result<Product>> AddProductAsync(
        string name,
        string sku,
        string description,
        string imageUrl,
        decimal importPrice,
        decimal sellingPrice,
        int quantity,
        string categoryName,
        string manufacturer,
        string deviceType,
        decimal commissionRate);

    /// <summary>
    /// Update product with validation.
    /// </summary>
    Task<Result<Product>> UpdateProductAsync(
        Guid productId,
        string name,
        string sku,
        string description,
        string imageUrl,
        decimal importPrice,
        decimal sellingPrice,
        int quantity,
        string categoryName,
        string manufacturer,
        string deviceType,
        decimal commissionRate,
        string status);

    /// <summary>
    /// Delete product
    /// </summary>
    Task<Result<Unit>> DeleteProductAsync(Guid productId);

    /// <summary>
    /// Export products to CSV
    /// Returns file path to exported CSV
    /// </summary>
    Task<Result<string>> ExportProductsToCsvAsync(
        string? searchQuery = null,
        string? categoryName = null,
        decimal? minPrice = null,
        decimal? maxPrice = null);

    /// <summary>
    /// Load all categories for filter dropdown
    /// </summary>
    Task<Result<List<Category>>> LoadCategoriesAsync();

    /// <summary>
    /// Load all brands (manufacturers) for filter dropdown
    /// </summary>
    Task<Result<List<string>>> LoadBrandsAsync();

    /// <summary>
    /// Update product stock quantity
    /// </summary>
    Task<Result<Unit>> UpdateStockAsync(Guid productId, int newQuantity);

    /// <summary>
    /// Create a new product from Product model (simplified API)
    /// </summary>
    Task<Result<Product>> CreateProductAsync(Product product);

    /// <summary>
    /// Update an existing product from Product model (simplified API)
    /// </summary>
    Task<Result<Product>> UpdateProductAsync(Guid id, Product product);

    /// <summary>
    /// Search for products by query
    /// </summary>
    Task<Result<List<Product>>> SearchProductsAsync(string searchQuery);

    /// <summary>
    /// Upload image for a product
    /// </summary>
    Task<Result<string>> UploadProductImageAsync(Guid productId, string imageFilePath);

    /// <summary>
    /// Upload product image from file path (for new products without ID yet)
    /// </summary>
    Task<Result<string>> UploadProductImageForNewProductAsync(string imageFilePath);

    /// <summary>
    /// Create multiple products at once
    /// </summary>
    Task<Result<BulkImportResult>> BulkCreateProductsAsync(List<Product> products);
}
