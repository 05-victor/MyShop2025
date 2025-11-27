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
                page, pageSize, searchQuery, categoryName, minPrice, maxPrice, sortBy, sortDescending);

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

    /// <inheritdoc/>
    public async Task<Result<Product>> AddProductAsync(
        string name, string sku, string description, string imageUrl,
        decimal importPrice, decimal sellingPrice, int quantity,
        string categoryName, string manufacturer, string deviceType, decimal commissionRate)
    {
        try
        {
            // Validate inputs
            if (string.IsNullOrWhiteSpace(name))
                return Result<Product>.Failure("Product name is required");

            if (importPrice < 0)
                return Result<Product>.Failure("Import price cannot be negative");

            if (sellingPrice < 0)
                return Result<Product>.Failure("Selling price cannot be negative");

            if (sellingPrice < importPrice)
                return Result<Product>.Failure("Selling price must be greater than or equal to import price");

            if (quantity < 0)
                return Result<Product>.Failure("Quantity cannot be negative");

            if (commissionRate < 0 || commissionRate > 100)
                return Result<Product>.Failure("Commission rate must be between 0% and 100%");

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

            var result = await _productRepository.CreateAsync(product);

            if (!result.IsSuccess || result.Data == null)
            {
                return Result<Product>.Failure(result.ErrorMessage ?? "Failed to create product");
            }

            await _toastService.ShowSuccess($"Product '{name}' created successfully!");
            return Result<Product>.Success(result.Data);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ProductFacade] AddProductAsync failed: {ex.Message}");
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
            // Validate similar to AddProductAsync
            if (string.IsNullOrWhiteSpace(name))
                return Result<Product>.Failure("Product name is required");

            if (sellingPrice < importPrice)
                return Result<Product>.Failure("Selling price must be greater than or equal to import price");

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

            var result = await _productRepository.UpdateAsync(product);

            if (!result.IsSuccess || result.Data == null)
            {
                return Result<Product>.Failure(result.ErrorMessage ?? "Failed to update product");
            }

            await _toastService.ShowSuccess($"Product '{name}' updated successfully!");
            return Result<Product>.Success(result.Data);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ProductFacade] UpdateProductAsync failed: {ex.Message}");
            return Result<Product>.Failure("Failed to update product", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<Result<Unit>> DeleteProductAsync(Guid productId)
    {
        try
        {
            var result = await _productRepository.DeleteAsync(productId);
            if (!result.IsSuccess)
            {
                return Result<Unit>.Failure(result.ErrorMessage ?? "Failed to delete product");
            }

            await _toastService.ShowSuccess("Product deleted successfully!");
            return Result<Unit>.Success(Unit.Value);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ProductFacade] DeleteProductAsync failed: {ex.Message}");
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
                1, 10000, searchQuery, categoryName, minPrice, maxPrice, "name", false);

            if (!result.IsSuccess || result.Data == null)
            {
                return Result<string>.Failure("Failed to load products for export");
            }

            var products = result.Data.Items;

            // Use ExportService to generate CSV
            var exportResult = await _exportService.ExportToCsvAsync(
                products,
                "Products",
                product => new Dictionary<string, string>
                {
                    ["SKU"] = product.SKU ?? string.Empty,
                    ["Name"] = product.Name,
                    ["Category"] = product.CategoryName ?? string.Empty,
                    ["Import Price"] = product.ImportPrice.ToString("F2"),
                    ["Selling Price"] = product.SellingPrice.ToString("F2"),
                    ["Stock"] = product.Quantity.ToString(),
                    ["Manufacturer"] = product.Manufacturer ?? string.Empty,
                    ["Device Type"] = product.DeviceType ?? string.Empty,
                    ["Commission Rate"] = product.CommissionRate.ToString("F2"),
                    ["Status"] = product.Status ?? string.Empty
                });

            if (!exportResult.IsSuccess)
            {
                return exportResult;
            }

            await _toastService.ShowSuccess($"Exported {products.Count} products");
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
