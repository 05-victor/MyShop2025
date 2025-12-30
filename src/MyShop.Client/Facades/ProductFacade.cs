using MyShop.Core.Common;
using MyShop.Core.Interfaces.Facades;
using MyShop.Core.Interfaces.Repositories;
using MyShop.Core.Interfaces.Services;
using MyShop.Client.Common.Helpers;
using MyShop.Plugins.API.Files;
using MyShop.Plugins.API.Products;
using MyShop.Shared.Models;
using Refit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.Security.Cryptography;

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
    private readonly IFilesApi _filesApi;
    private readonly IProductsApi _productsApi;

    public ProductFacade(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        IValidationService validationService,
        IToastService toastService,
        IExportService exportService,
        IFilesApi filesApi,
        IProductsApi productsApi)
    {
        _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
        _categoryRepository = categoryRepository ?? throw new ArgumentNullException(nameof(categoryRepository));
        _validationService = validationService ?? throw new ArgumentNullException(nameof(validationService));
        _toastService = toastService ?? throw new ArgumentNullException(nameof(toastService));
        _exportService = exportService ?? throw new ArgumentNullException(nameof(exportService));
        _filesApi = filesApi ?? throw new ArgumentNullException(nameof(filesApi));
        _productsApi = productsApi ?? throw new ArgumentNullException(nameof(productsApi));
    }

    /// <inheritdoc/>
    public async Task<Result<PagedList<Product>>> LoadProductsAsync(
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
        int pageSize = AppConstants.DEFAULT_PAGE_SIZE)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"[ProductFacade.LoadProductsAsync] START - Page: {page}, PageSize: {pageSize}, Search: '{searchQuery}', Category: '{categoryName}', StockStatus: '{stockStatus}'");

            // Validate page parameters
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = AppConstants.DEFAULT_PAGE_SIZE;
            if (pageSize > AppConstants.MAX_PAGE_SIZE) pageSize = AppConstants.MAX_PAGE_SIZE; // Max 100 items per page

            // Call repository's paged method (server-side paging and filtering)
            var result = await _productRepository.GetPagedAsync(
                page, pageSize, searchQuery, categoryName, manufacturerName, brandName, minPrice, maxPrice, stockStatus, sortBy, sortDescending);

            if (!result.IsSuccess || result.Data == null)
            {
                System.Diagnostics.Debug.WriteLine($"[ProductFacade.LoadProductsAsync] ❌ FAILED - {result.ErrorMessage}");
                return Result<PagedList<Product>>.Failure(result.ErrorMessage ?? "Failed to load products");
            }

            var pagedList = result.Data;

            System.Diagnostics.Debug.WriteLine($"[ProductFacade.LoadProductsAsync] API Response Details:");
            System.Diagnostics.Debug.WriteLine($"  - Items Count: {pagedList.Items.Count}");
            System.Diagnostics.Debug.WriteLine($"  - TotalCount: {pagedList.TotalCount}");
            System.Diagnostics.Debug.WriteLine($"  - PageNumber: {pagedList.PageNumber}");
            System.Diagnostics.Debug.WriteLine($"  - PageSize: {pagedList.PageSize}");
            System.Diagnostics.Debug.WriteLine($"  - TotalPages: {pagedList.TotalPages}");
            System.Diagnostics.Debug.WriteLine($"  - HasNext: {pagedList.HasNext}");
            System.Diagnostics.Debug.WriteLine($"  - HasPrevious: {pagedList.HasPrevious}");
            System.Diagnostics.Debug.WriteLine($"[ProductFacade.LoadProductsAsync] ✅ SUCCESS - Loaded {pagedList.Items.Count} products (Page {pagedList.PageNumber}/{pagedList.TotalPages})");

            return Result<PagedList<Product>>.Success(pagedList);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ProductFacade.LoadProductsAsync] ❌ Exception: {ex.Message}\nStackTrace: {ex.StackTrace}");
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

    /// <summary>
    /// Creates a new product with comprehensive logging and user feedback.
    /// </summary>
    public async Task<Result<Product>> CreateProductAsync(Product product)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            System.Diagnostics.Debug.WriteLine($"[ProductFacade.CreateProductAsync] START - Product: {product?.Name}, Category: {product?.CategoryId}");

            System.Diagnostics.Debug.WriteLine($"[ProductFacade.CreateProductAsync] Calling _productRepository.CreateAsync()");
            var result = await _productRepository.CreateAsync(product);

            stopwatch.Stop();
            System.Diagnostics.Debug.WriteLine($"[ProductFacade] API Response - Result.IsSuccess: {result.IsSuccess}, ElapsedMs: {stopwatch.ElapsedMilliseconds}");

            if (result.IsSuccess)
            {
                System.Diagnostics.Debug.WriteLine($"[ProductFacade.CreateProductAsync] ✅ SUCCESS - Created product ID: {result.Data?.Id}, Name: {result.Data?.Name}");

                // Show success toast
                if (_toastService != null)
                {
                    await _toastService.ShowSuccess($"Product '{product.Name}' created successfully!");
                }

                return result;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[ProductFacade.CreateProductAsync] ❌ FAILED - {result.ErrorMessage}");

                if (_toastService != null)
                {
                    await _toastService.ShowError($"Failed to create product: {result.ErrorMessage}");
                }

                return result;
            }
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            System.Diagnostics.Debug.WriteLine($"[ProductFacade.CreateProductAsync] ❌ EXCEPTION - {ex.GetType().Name}: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"[ProductFacade.CreateProductAsync] Stack Trace: {ex.StackTrace}");

            if (_toastService != null)
            {
                await _toastService.ShowError($"Error creating product: {ex.Message}");
            }

            return Result<Product>.Failure(ex.Message);
        }
    }

    /// <summary>
    /// Updates an existing product with comprehensive logging and user feedback.
    /// </summary>
    public async Task<Result<Product>> UpdateProductAsync(Guid id, Product product)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            System.Diagnostics.Debug.WriteLine($"[ProductFacade.UpdateProductAsync] START - ID: {id}, Product: {product?.Name}, Category: {product?.CategoryId}");

            System.Diagnostics.Debug.WriteLine($"[ProductFacade.UpdateProductAsync] Calling _productRepository.UpdateAsync()");
            var result = await _productRepository.UpdateAsync(product);

            stopwatch.Stop();
            System.Diagnostics.Debug.WriteLine($"[ProductFacade] API Response - Result.IsSuccess: {result.IsSuccess}, ElapsedMs: {stopwatch.ElapsedMilliseconds}");

            if (result.IsSuccess)
            {
                System.Diagnostics.Debug.WriteLine($"[ProductFacade.UpdateProductAsync] ✅ SUCCESS - Updated product ID: {result.Data?.Id}, Name: {result.Data?.Name}");

                // Show success toast
                if (_toastService != null)
                {
                    await _toastService.ShowSuccess($"Product '{product.Name}' updated successfully!");
                }

                return result;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[ProductFacade.UpdateProductAsync] ❌ FAILED - {result.ErrorMessage}");

                if (_toastService != null)
                {
                    await _toastService.ShowError($"Failed to update product: {result.ErrorMessage}");
                }

                return result;
            }
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            System.Diagnostics.Debug.WriteLine($"[ProductFacade.UpdateProductAsync] ❌ EXCEPTION - {ex.GetType().Name}: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"[ProductFacade.UpdateProductAsync] Stack Trace: {ex.StackTrace}");

            if (_toastService != null)
            {
                await _toastService.ShowError($"Error updating product: {ex.Message}");
            }

            return Result<Product>.Failure(ex.Message);
        }
    }

    /// <summary>
    /// Searches for products with comprehensive logging and user feedback.
    /// </summary>
    public async Task<Result<List<Product>>> SearchProductsAsync(string searchQuery)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            System.Diagnostics.Debug.WriteLine($"[ProductFacade.SearchProductsAsync] START - Query: '{searchQuery}'");

            if (string.IsNullOrWhiteSpace(searchQuery))
            {
                System.Diagnostics.Debug.WriteLine($"[ProductFacade.SearchProductsAsync] ❌ VALIDATION FAILED - Search query is empty");
                return Result<List<Product>>.Failure("Search query cannot be empty");
            }

            System.Diagnostics.Debug.WriteLine($"[ProductFacade.SearchProductsAsync] Calling _productRepository.SearchAsync()");
            var result = await _productRepository.SearchAsync(searchQuery);

            stopwatch.Stop();
            System.Diagnostics.Debug.WriteLine($"[ProductFacade] API Response - Result.Success: {result.IsSuccess}, ElapsedMs: {stopwatch.ElapsedMilliseconds}");

            if (result.IsSuccess && result.Data != null)
            {
                var products = result.Data.ToList();
                System.Diagnostics.Debug.WriteLine($"[ProductFacade.SearchProductsAsync] ✅ SUCCESS - Found {products.Count} products matching '{searchQuery}'");

                return Result<List<Product>>.Success(products);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[ProductFacade.SearchProductsAsync] ❌ FAILED - {result.ErrorMessage}");
                return Result<List<Product>>.Failure(result.ErrorMessage ?? "Search failed");
            }
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            System.Diagnostics.Debug.WriteLine($"[ProductFacade.SearchProductsAsync] ❌ EXCEPTION - {ex.GetType().Name}: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"[ProductFacade.SearchProductsAsync] Stack Trace: {ex.StackTrace}");

            return Result<List<Product>>.Failure(ex.Message);
        }
    }

    /// <summary>
    /// Upload product image for existing product using product ID
    /// </summary>
    public async Task<Result<string>> UploadProductImageAsync(Guid productId, string imageFilePath)
    {
        return await UploadProductImageForExistingProductAsync(productId, imageFilePath);
    }

    /// <summary>
    /// Upload product image from StorageFile (for new products without ID yet)
    /// </summary>
    public async Task<Result<string>> UploadProductImageAsync(Windows.Storage.StorageFile imageFile)
    {
        if (imageFile == null)
        {
            return Result<string>.Failure("Image file is required");
        }
        return await UploadProductImageForNewProductAsync(imageFile.Path);
    }

    /// <summary>
    /// Upload product image for existing product using product ID (internal method)
    /// </summary>
    public async Task<Result<string>> UploadProductImageForExistingProductAsync(Guid productId, string imageFilePath)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            System.Diagnostics.Debug.WriteLine($"[ProductFacade.UploadProductImageForExistingProductAsync] START - ProductId: {productId}, File: {imageFilePath}");

            if (string.IsNullOrEmpty(imageFilePath))
            {
                System.Diagnostics.Debug.WriteLine($"[ProductFacade.UploadProductImageForExistingProductAsync] ❌ VALIDATION FAILED - Image file path is empty");
                return Result<string>.Failure("Image file path is required");
            }

            // Get file from path
            var imageFile = await Windows.Storage.StorageFile.GetFileFromPathAsync(imageFilePath);
            if (imageFile == null)
            {
                System.Diagnostics.Debug.WriteLine($"[ProductFacade.UploadProductImageForExistingProductAsync] ❌ VALIDATION FAILED - File not found at path: {imageFilePath}");
                return Result<string>.Failure("Image file not found");
            }

            // Validate file size (max 5MB)
            var properties = await imageFile.GetBasicPropertiesAsync();
            const long maxFileSize = 5 * 1024 * 1024; // 5MB
            if (properties.Size > maxFileSize)
            {
                System.Diagnostics.Debug.WriteLine($"[ProductFacade.UploadProductImageForExistingProductAsync] ❌ VALIDATION FAILED - File size ({properties.Size} bytes) exceeds max ({maxFileSize} bytes)");
                return Result<string>.Failure("Image file size cannot exceed 5MB");
            }

            // Validate file type
            var validExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var fileExtension = System.IO.Path.GetExtension(imageFile.Name).ToLowerInvariant();
            if (!validExtensions.Contains(fileExtension))
            {
                System.Diagnostics.Debug.WriteLine($"[ProductFacade.UploadProductImageForExistingProductAsync] ❌ VALIDATION FAILED - Invalid file type: {fileExtension}");
                return Result<string>.Failure("Only image files (JPG, PNG, GIF, WebP) are allowed");
            }

            System.Diagnostics.Debug.WriteLine($"[ProductFacade.UploadProductImageForExistingProductAsync] File validation passed - Size: {properties.Size} bytes, Type: {fileExtension}");

            // Read file as byte array
            var buffer = await Windows.Storage.FileIO.ReadBufferAsync(imageFile);
            // Convert IBuffer to byte array using DataReader
            var dataReader = Windows.Storage.Streams.DataReader.FromBuffer(buffer);
            var bytes = new byte[buffer.Length];
            dataReader.ReadBytes(bytes);

            System.Diagnostics.Debug.WriteLine($"[ProductFacade.UploadProductImageForExistingProductAsync] Image file prepared for upload - Size: {bytes.Length} bytes");

            // Call API to upload image to actual server
            var fileStream = new System.IO.MemoryStream(bytes);
            var fileName = imageFile.Name;

            var streamPart = new StreamPart(fileStream, fileName, "image/" + fileExtension.TrimStart('.'));

            System.Diagnostics.Debug.WriteLine($"[ProductFacade.UploadProductImageForExistingProductAsync] Calling API /api/v1/products/{productId}/uploadImage");

            var response = await _productsApi.UploadImageAsync(productId, streamPart);

            if (response.IsSuccessStatusCode && response.Content != null && response.Content.Success)
            {
                var imageUrl = response.Content.Result;
                stopwatch.Stop();
                System.Diagnostics.Debug.WriteLine($"[ProductFacade] Upload Complete - ElapsedMs: {stopwatch.ElapsedMilliseconds}");
                System.Diagnostics.Debug.WriteLine($"[ProductFacade.UploadProductImageForExistingProductAsync] ✅ SUCCESS - Image uploaded: {imageUrl}");

                if (_toastService != null)
                {
                    await _toastService.ShowSuccess($"Image '{imageFile.Name}' uploaded successfully!");
                }

                return Result<string>.Success(imageUrl);
            }
            else
            {
                stopwatch.Stop();
                var errorMessage = response.Content?.Message ?? "Unknown error uploading file";
                System.Diagnostics.Debug.WriteLine($"[ProductFacade.UploadProductImageForExistingProductAsync] ❌ API ERROR - {response.StatusCode}: {errorMessage}");
                return Result<string>.Failure($"Failed to upload image: {errorMessage}");
            }
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            System.Diagnostics.Debug.WriteLine($"[ProductFacade.UploadProductImageForExistingProductAsync] ❌ EXCEPTION - {ex.GetType().Name}: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"[ProductFacade.UploadProductImageForExistingProductAsync] Stack Trace: {ex.StackTrace}");
            return Result<string>.Failure($"Error uploading image: {ex.Message}");
        }
    }

    /// <summary>
    /// Upload product image for new product using file path
    /// </summary>
    public async Task<Result<string>> UploadProductImageForNewProductAsync(string imageFilePath)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            System.Diagnostics.Debug.WriteLine($"[ProductFacade.UploadProductImageForNewProductAsync] START - File: {imageFilePath}");

            if (string.IsNullOrEmpty(imageFilePath))
            {
                System.Diagnostics.Debug.WriteLine($"[ProductFacade.UploadProductImageForNewProductAsync] ❌ VALIDATION FAILED - Image file path is empty");
                return Result<string>.Failure("Image file path is required");
            }

            // Get file from path
            var imageFile = await Windows.Storage.StorageFile.GetFileFromPathAsync(imageFilePath);
            if (imageFile == null)
            {
                System.Diagnostics.Debug.WriteLine($"[ProductFacade.UploadProductImageForNewProductAsync] ❌ VALIDATION FAILED - File not found at path: {imageFilePath}");
                return Result<string>.Failure("Image file not found");
            }

            // Validate file size (max 5MB)
            var properties = await imageFile.GetBasicPropertiesAsync();
            const long maxFileSize = 5 * 1024 * 1024; // 5MB
            if (properties.Size > maxFileSize)
            {
                System.Diagnostics.Debug.WriteLine($"[ProductFacade.UploadProductImageForNewProductAsync] ❌ VALIDATION FAILED - File size ({properties.Size} bytes) exceeds max ({maxFileSize} bytes)");
                return Result<string>.Failure("Image file size cannot exceed 5MB");
            }

            // Validate file type
            var validExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var fileExtension = System.IO.Path.GetExtension(imageFile.Name).ToLowerInvariant();
            if (!validExtensions.Contains(fileExtension))
            {
                System.Diagnostics.Debug.WriteLine($"[ProductFacade.UploadProductImageForNewProductAsync] ❌ VALIDATION FAILED - Invalid file type: {fileExtension}");
                return Result<string>.Failure("Only image files (JPG, PNG, GIF, WebP) are allowed");
            }

            System.Diagnostics.Debug.WriteLine($"[ProductFacade.UploadProductImageForNewProductAsync] File validation passed - Size: {properties.Size} bytes, Type: {fileExtension}");

            // Read file as byte array
            var buffer = await Windows.Storage.FileIO.ReadBufferAsync(imageFile);
            // Convert IBuffer to byte array using DataReader
            var dataReader = Windows.Storage.Streams.DataReader.FromBuffer(buffer);
            var bytes = new byte[buffer.Length];
            dataReader.ReadBytes(bytes);

            System.Diagnostics.Debug.WriteLine($"[ProductFacade.UploadProductImageForNewProductAsync] Image file prepared for upload - Size: {bytes.Length} bytes");

            // Call API to upload image to actual server
            var fileStream = new System.IO.MemoryStream(bytes);
            var fileName = imageFile.Name;
            var fileIdentifier = Guid.NewGuid().ToString();

            var streamPart = new StreamPart(fileStream, fileName, "image/" + fileExtension.TrimStart('.'));

            System.Diagnostics.Debug.WriteLine($"[ProductFacade.UploadProductImageForNewProductAsync] Calling API /api/v1/files/upload with identifier: {fileIdentifier}");

            var response = await _filesApi.UploadImageAsync(streamPart, fileIdentifier);

            if (response.IsSuccessStatusCode && response.Content != null && response.Content.Success)
            {
                var imageUrl = response.Content.Result;
                stopwatch.Stop();
                System.Diagnostics.Debug.WriteLine($"[ProductFacade] Upload Complete - ElapsedMs: {stopwatch.ElapsedMilliseconds}");
                System.Diagnostics.Debug.WriteLine($"[ProductFacade.UploadProductImageForNewProductAsync] ✅ SUCCESS - Image uploaded: {imageUrl}");

                if (_toastService != null)
                {
                    await _toastService.ShowSuccess($"Image '{imageFile.Name}' uploaded successfully!");
                }

                return Result<string>.Success(imageUrl);
            }
            else
            {
                stopwatch.Stop();
                var errorMessage = response.Content?.Message ?? "Unknown error uploading file";
                System.Diagnostics.Debug.WriteLine($"[ProductFacade.UploadProductImageForNewProductAsync] ❌ API ERROR - {response.StatusCode}: {errorMessage}");
                return Result<string>.Failure($"Failed to upload image: {errorMessage}");
            }
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            System.Diagnostics.Debug.WriteLine($"[ProductFacade.UploadProductImageForNewProductAsync] ❌ EXCEPTION - {ex.GetType().Name}: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"[ProductFacade.UploadProductImageForNewProductAsync] Stack Trace: {ex.StackTrace}");
            return Result<string>.Failure($"Error uploading image: {ex.Message}");
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
                1, 10000, searchQuery, categoryName, null, null, minPrice, maxPrice, null, "name", false);

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
                .Select(p => p.Manufacturer!)
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

    public async Task<Result<BulkImportResult>> BulkCreateProductsAsync(List<Product> products)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"[ProductFacade] BulkCreateProductsAsync - Creating {products.Count} products");

            var result = await _productRepository.BulkCreateAsync(products);

            if (!result.IsSuccess || result.Data == null)
            {
                return Result<BulkImportResult>.Failure(result.ErrorMessage ?? "Bulk create failed");
            }

            var bulkResult = result.Data;

            if (bulkResult.SuccessCount > 0)
            {
                await _toastService.ShowSuccess($"✅ Imported {bulkResult.SuccessCount}/{bulkResult.TotalSubmitted} products");
            }

            if (bulkResult.FailureCount > 0)
            {
                await _toastService.ShowError($"❌ {bulkResult.FailureCount} products failed");
            }

            return Result<BulkImportResult>.Success(bulkResult);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ProductFacade] BulkCreateProductsAsync failed: {ex.Message}");
            return Result<BulkImportResult>.Failure("Failed to bulk create products", ex);
        }
    }
}
