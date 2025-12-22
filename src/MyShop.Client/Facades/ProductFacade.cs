using MyShop.Core.Common;
using MyShop.Core.Interfaces.Facades;
using MyShop.Core.Interfaces.Repositories;
using MyShop.Core.Interfaces.Services;
using MyShop.Shared.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace MyShop.Client.Facades;

/// <summary>
/// Implementation of IProductFacade - product management operations
/// </summary>
public class ProductFacade : IProductFacade
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IValidationService _validationService;
    private readonly IToastService _toastService;
    private readonly IExportService _exportService;

    public ProductFacade(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        IValidationService validationService,
        IToastService toastService,
        IExportService exportService)
    {
        _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
        _categoryRepository = categoryRepository ?? throw new ArgumentNullException(nameof(categoryRepository));
        _validationService = validationService ?? throw new ArgumentNullException(nameof(validationService));
        _toastService = toastService ?? throw new ArgumentNullException(nameof(toastService));
        _exportService = exportService ?? throw new ArgumentNullException(nameof(exportService));
    }

    /// <inheritdoc/>
    public async Task<Result<PagedList<Product>>> LoadProductsAsync(
        string? searchQuery = null,
        string? categoryName = null,
        string? manufacturerName = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        string sortBy = "name",
        bool sortDescending = false,
        int page = 1,
        int pageSize = 20)
    {
        try
        {
            // Validate page parameters
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;
            if (pageSize > 100) pageSize = 100; // Max 100 items per page

            // Call repository's paged method (server-side paging and filtering)
            var result = await _productRepository.GetPagedAsync(
                page, pageSize, searchQuery, categoryName, manufacturerName, minPrice, maxPrice, sortBy, sortDescending);

            if (!result.IsSuccess || result.Data == null)
            {
                return Result<PagedList<Product>>.Failure(result.ErrorMessage ?? "Failed to load products");
            }

            var pagedList = result.Data;

            System.Diagnostics.Debug.WriteLine($"[ProductFacade] Loaded {pagedList.Items.Count} products (Page {page}/{pagedList.TotalPages})");
            return Result<PagedList<Product>>.Success(pagedList);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ProductFacade] LoadProductsAsync failed: {ex.Message}");
            return Result<PagedList<Product>>.Failure("Failed to load products", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<Result<Product>> GetProductByIdAsync(Guid productId)
    {
        try
        {
            var result = await _productRepository.GetByIdAsync(productId);
            if (!result.IsSuccess || result.Data == null)
            {
                return Result<Product>.Failure(result.ErrorMessage ?? "Product not found");
            }

            return Result<Product>.Success(result.Data);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ProductFacade] GetProductByIdAsync failed: {ex.Message}");
            return Result<Product>.Failure("Failed to get product", ex);
        }
    }

    /// <summary>
    /// Load all categories from API with optional paging
    /// Used by UI to populate category filters
    /// </summary>
    public async Task<Result<List<Category>>> GetCategoriesAsync(int pageNumber = 1, int pageSize = 100)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"[ProductFacade] GetCategoriesAsync: Starting category load from API (Page={pageNumber}, PageSize={pageSize})");

            // Call repository to get categories
            var result = await _categoryRepository.GetPagedAsync(pageNumber, pageSize);

            if (!result.IsSuccess || result.Data == null)
            {
                System.Diagnostics.Debug.WriteLine($"[ProductFacade] GetCategoriesAsync: Failed - {result.ErrorMessage}");
                return Result<List<Category>>.Failure(result.ErrorMessage ?? "Failed to load categories");
            }

            var categories = result.Data.Items;

            System.Diagnostics.Debug.WriteLine($"[ProductFacade] ✅ GetCategoriesAsync: Loaded {categories.Count} categories from API");
            return Result<List<Category>>.Success(categories.ToList());
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ProductFacade] ❌ GetCategoriesAsync: ERROR - {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"[ProductFacade] Stack trace: {ex.StackTrace}");
            return Result<List<Category>>.Failure("Failed to load categories", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<Result<Product>> AddProductAsync(
        string name, string sku, string description, string imageUrl,
        decimal importPrice, decimal sellingPrice, int quantity,
        string categoryName, string manufacturer, string deviceType, decimal commissionRate)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"[ProductFacade] AddProductAsync: Starting product creation");
            System.Diagnostics.Debug.WriteLine($"[ProductFacade] AddProductAsync: Name={name}, SKU={sku}, Category={categoryName}, Price={sellingPrice}");

            // Validate inputs
            if (string.IsNullOrWhiteSpace(name))
            {
                System.Diagnostics.Debug.WriteLine($"[ProductFacade] AddProductAsync: Validation failed - Product name is required");
                return Result<Product>.Failure("Product name is required");
            }

            if (importPrice < 0)
            {
                System.Diagnostics.Debug.WriteLine($"[ProductFacade] AddProductAsync: Validation failed - Import price cannot be negative");
                return Result<Product>.Failure("Import price cannot be negative");
            }

            if (sellingPrice < 0)
            {
                System.Diagnostics.Debug.WriteLine($"[ProductFacade] AddProductAsync: Validation failed - Selling price cannot be negative");
                return Result<Product>.Failure("Selling price cannot be negative");
            }

            if (sellingPrice < importPrice)
            {
                System.Diagnostics.Debug.WriteLine($"[ProductFacade] AddProductAsync: Validation failed - Selling price less than import price");
                return Result<Product>.Failure("Selling price must be greater than or equal to import price");
            }

            if (quantity < 0)
            {
                System.Diagnostics.Debug.WriteLine($"[ProductFacade] AddProductAsync: Validation failed - Quantity cannot be negative");
                return Result<Product>.Failure("Quantity cannot be negative");
            }

            if (commissionRate < 0 || commissionRate > 100)
            {
                System.Diagnostics.Debug.WriteLine($"[ProductFacade] AddProductAsync: Validation failed - Commission rate out of range");
                return Result<Product>.Failure("Commission rate must be between 0% and 100%");
            }

            // Create product object
            var product = new Product
            {
                Id = Guid.NewGuid(),
                SKU = sku,
                Name = name,
                Description = description,
                ImageUrl = imageUrl,
                ImportPrice = importPrice,
                SellingPrice = sellingPrice,
                Quantity = quantity,
                CategoryName = categoryName,
                Manufacturer = manufacturer,
                DeviceType = deviceType,
                CommissionRate = (double)commissionRate,
                Status = "AVAILABLE",
                CreatedAt = DateTime.UtcNow
            };

            System.Diagnostics.Debug.WriteLine($"[ProductFacade] AddProductAsync: Calling repository to create product");
            var result = await _productRepository.CreateAsync(product);

            if (!result.IsSuccess || result.Data == null)
            {
                System.Diagnostics.Debug.WriteLine($"[ProductFacade] ❌ AddProductAsync: Failed - {result.ErrorMessage}");
                return Result<Product>.Failure(result.ErrorMessage ?? "Failed to create product");
            }

            System.Diagnostics.Debug.WriteLine($"[ProductFacade] ✅ AddProductAsync: Product created successfully (ID={result.Data.Id})");
            await _toastService.ShowSuccess($"Product '{name}' created successfully!");
            return Result<Product>.Success(result.Data);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ProductFacade] ❌ AddProductAsync: Exception - {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"[ProductFacade] Stack trace: {ex.StackTrace}");
            return Result<Product>.Failure("Failed to create product", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<Result<Product>> UpdateProductAsync(
        Guid productId, string name, string sku, string description, string imageUrl,
        decimal importPrice, decimal sellingPrice, int quantity,
        string categoryName, string manufacturer, string deviceType,
        decimal commissionRate, string status)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"[ProductFacade] UpdateProductAsync: Starting product update");
            System.Diagnostics.Debug.WriteLine($"[ProductFacade] UpdateProductAsync: ID={productId}, Name={name}, Category={categoryName}, Price={sellingPrice}");

            // Validate similar to AddProductAsync
            if (string.IsNullOrWhiteSpace(name))
            {
                System.Diagnostics.Debug.WriteLine($"[ProductFacade] UpdateProductAsync: Validation failed - Product name is required");
                return Result<Product>.Failure("Product name is required");
            }

            if (sellingPrice < importPrice)
            {
                System.Diagnostics.Debug.WriteLine($"[ProductFacade] UpdateProductAsync: Validation failed - Selling price less than import price");
                return Result<Product>.Failure("Selling price must be greater than or equal to import price");
            }

            // Create updated product object
            var product = new Product
            {
                Id = productId,
                SKU = sku,
                Name = name,
                Description = description,
                ImageUrl = imageUrl,
                ImportPrice = importPrice,
                SellingPrice = sellingPrice,
                Quantity = quantity,
                CategoryName = categoryName,
                Manufacturer = manufacturer,
                DeviceType = deviceType,
                CommissionRate = (double)commissionRate,
                Status = status,
                UpdatedAt = DateTime.UtcNow
            };

            System.Diagnostics.Debug.WriteLine($"[ProductFacade] UpdateProductAsync: Calling repository to update product");
            var result = await _productRepository.UpdateAsync(product);

            if (!result.IsSuccess || result.Data == null)
            {
                System.Diagnostics.Debug.WriteLine($"[ProductFacade] ❌ UpdateProductAsync: Failed - {result.ErrorMessage}");
                return Result<Product>.Failure(result.ErrorMessage ?? "Failed to update product");
            }

            System.Diagnostics.Debug.WriteLine($"[ProductFacade] ✅ UpdateProductAsync: Product updated successfully");
            await _toastService.ShowSuccess($"Product '{name}' updated successfully!");
            return Result<Product>.Success(result.Data);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ProductFacade] ❌ UpdateProductAsync: Exception - {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"[ProductFacade] Stack trace: {ex.StackTrace}");
            return Result<Product>.Failure("Failed to update product", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<Result<Unit>> DeleteProductAsync(Guid productId)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"[ProductFacade] DeleteProductAsync: Starting product deletion (ID={productId})");

            var result = await _productRepository.DeleteAsync(productId);
            if (!result.IsSuccess)
            {
                System.Diagnostics.Debug.WriteLine($"[ProductFacade] ❌ DeleteProductAsync: Failed - {result.ErrorMessage}");
                return Result<Unit>.Failure(result.ErrorMessage ?? "Failed to delete product");
            }

            System.Diagnostics.Debug.WriteLine($"[ProductFacade] ✅ DeleteProductAsync: Product deleted successfully");
            await _toastService.ShowSuccess("Product deleted successfully!");
            return Result<Unit>.Success(Unit.Value);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ProductFacade] ❌ DeleteProductAsync: Exception - {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"[ProductFacade] Stack trace: {ex.StackTrace}");
            return Result<Unit>.Failure("Failed to delete product", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<Result<string>> ExportProductsToCsvAsync(
        string? searchQuery = null, string? categoryName = null,
        decimal? minPrice = null, decimal? maxPrice = null)
    {
        try
        {
            // Load filtered products with large page size to get all
            var result = await _productRepository.GetPagedAsync(
                1, 10000, searchQuery, categoryName, null, minPrice, maxPrice, "name", false);

            if (!result.IsSuccess || result.Data == null)
            {
                return Result<string>.Failure("Failed to load products for export");
            }

            var products = result.Data.Items;

            if (products.Count == 0)
            {
                await _toastService.ShowWarning("No products to export");
                return Result<string>.Success(string.Empty);
            }

            // Generate CSV content
            var csv = new StringBuilder();
            csv.AppendLine("SKU,Name,Category,Import Price,Selling Price,Stock,Manufacturer,Device Type,Commission Rate,Status");

            foreach (var product in products)
            {
                csv.AppendLine($"\"{product.SKU ?? string.Empty}\"," +
                    $"\"{product.Name}\"," +
                    $"\"{product.CategoryName ?? string.Empty}\"," +
                    $"\"{product.ImportPrice:F2}\"," +
                    $"\"{product.SellingPrice:F2}\"," +
                    $"\"{product.Quantity}\"," +
                    $"\"{product.Manufacturer ?? string.Empty}\"," +
                    $"\"{product.DeviceType ?? string.Empty}\"," +
                    $"\"{product.CommissionRate:F2}\"," +
                    $"\"{product.Status ?? string.Empty}\"");
            }

            // Use ExportService with FileSavePicker (reusable)
            var suggestedFileName = $"Products_{DateTime.Now:yyyyMMdd_HHmmss}";
            var exportResult = await _exportService.ExportWithPickerAsync(suggestedFileName, csv.ToString());

            if (!exportResult.IsSuccess)
            {
                await _toastService.ShowError("Failed to export products");
                return exportResult;
            }

            // Empty path means user cancelled
            if (string.IsNullOrEmpty(exportResult.Data))
            {
                return Result<string>.Success(string.Empty);
            }

            await _toastService.ShowSuccess($"Exported {products.Count} products successfully!");
            System.Diagnostics.Debug.WriteLine($"[ProductFacade] Exported {products.Count} products to {exportResult.Data}");

            return exportResult;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ProductFacade] ExportProductsToCsvAsync failed: {ex.Message}");
            return Result<string>.Failure("Failed to export products", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<Result<List<Category>>> LoadCategoriesAsync()
    {
        try
        {
            var result = await _categoryRepository.GetAllAsync();
            if (!result.IsSuccess || result.Data == null)
            {
                return Result<List<Category>>.Failure(result.ErrorMessage ?? "Failed to load categories");
            }

            return Result<List<Category>>.Success(result.Data.ToList());
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ProductFacade] LoadCategoriesAsync failed: {ex.Message}");
            return Result<List<Category>>.Failure("Failed to load categories", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<Result<List<string>>> LoadBrandsAsync()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"[ProductFacade] LoadBrandsAsync: Starting brand load");

            // Get all products to extract unique brands (manufacturers)
            var result = await _productRepository.GetAllAsync();
            if (!result.IsSuccess || result.Data == null)
            {
                System.Diagnostics.Debug.WriteLine($"[ProductFacade] LoadBrandsAsync: Failed to load products");
                return Result<List<string>>.Failure(result.ErrorMessage ?? "Failed to load brands");
            }

            // Extract distinct manufacturers and sort them
            var brands = result.Data
                .Where(p => !string.IsNullOrWhiteSpace(p.Manufacturer))
                .Select(p => p.Manufacturer)
                .Distinct()
                .OrderBy(b => b)
                .ToList();

            System.Diagnostics.Debug.WriteLine($"[ProductFacade] ✅ LoadBrandsAsync: Loaded {brands.Count} brands from products");
            return Result<List<string>>.Success(brands);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ProductFacade] ❌ LoadBrandsAsync failed: {ex.Message}");
            return Result<List<string>>.Failure("Failed to load brands", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<Result<Unit>> UpdateStockAsync(Guid productId, int newQuantity)
    {
        try
        {
            if (newQuantity < 0)
            {
                return Result<Unit>.Failure("Stock quantity cannot be negative");
            }

            // Get product first
            var productResult = await _productRepository.GetByIdAsync(productId);
            if (!productResult.IsSuccess || productResult.Data == null)
            {
                return Result<Unit>.Failure("Product not found");
            }

            var product = productResult.Data;

            // Update with new quantity
            product.Quantity = newQuantity;
            product.UpdatedAt = DateTime.UtcNow;

            var updateResult = await _productRepository.UpdateAsync(product);

            if (!updateResult.IsSuccess)
            {
                return Result<Unit>.Failure(updateResult.ErrorMessage ?? "Failed to update stock");
            }

            await _toastService.ShowSuccess($"Stock updated to {newQuantity} units");
            return Result<Unit>.Success(Unit.Value);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ProductFacade] UpdateStockAsync failed: {ex.Message}");
            return Result<Unit>.Failure("Failed to update stock", ex);
        }
    }
}
