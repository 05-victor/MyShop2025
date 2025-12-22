using MyShop.Core.Common;
using MyShop.Core.Interfaces.Repositories;
using MyShop.Plugins.API.Products;
using MyShop.Plugins.Adapters;
using MyShop.Shared.Models;
using Refit;

namespace MyShop.Plugins.Repositories.Api;

/// <summary>
/// API-based Product Repository implementation
/// Calls backend via IProductsApi (Refit)
/// </summary>
public class ProductRepository : IProductRepository
{
    private readonly IProductsApi _api;

    public ProductRepository(IProductsApi api)
    {
        _api = api;
    }

    public async Task<Result<IEnumerable<Product>>> GetAllAsync()
    {
        try
        {
            var response = await _api.GetAllAsync(pageNumber: 1, pageSize: int.MaxValue);
            System.Diagnostics.Debug.WriteLine($"[ProductRepository.GetAllAsync] API Response Status: {response.StatusCode}");

            if (response.IsSuccessStatusCode && response.Content != null)
            {
                var apiResponse = response.Content;
                System.Diagnostics.Debug.WriteLine($"[ProductRepository.GetAllAsync] API Response Success: {apiResponse.Success}");
                System.Diagnostics.Debug.WriteLine($"[ProductRepository.GetAllAsync] API Response Result: {(apiResponse.Result != null ? "NotNull" : "Null")}");

                if (apiResponse.Success && apiResponse.Result != null)
                {
                    var pagedResult = apiResponse.Result;
                    System.Diagnostics.Debug.WriteLine($"[ProductRepository.GetAllAsync] PagedResult - Items: {pagedResult.Items.Count}, TotalCount: {pagedResult.TotalCount}, TotalPages: {pagedResult.TotalPages}, Page: {pagedResult.Page}, PageSize: {pagedResult.PageSize}");

                    // Map ProductResponse[] to Product[] using ProductAdapter
                    // Extract Items from PagedResult
                    var products = ProductAdapter.ToModelList(pagedResult.Items);
                    System.Diagnostics.Debug.WriteLine($"[ProductRepository.GetAllAsync] Mapped {products.Count()} products");
                    return Result<IEnumerable<Product>>.Success(products);
                }
            }
            System.Diagnostics.Debug.WriteLine($"[ProductRepository.GetAllAsync] ❌ Failed - IsSuccess: {response.IsSuccessStatusCode}, Content: {(response.Content != null ? "NotNull" : "Null")}");
            return Result<IEnumerable<Product>>.Failure("Failed to retrieve products");
        }
        catch (ApiException ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ProductRepository.GetAllAsync] ❌ API Error: {ex.StatusCode} - {ex.Message}");
            return Result<IEnumerable<Product>>.Failure($"API error retrieving products: {ex.Message}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ProductRepository.GetAllAsync] ❌ Unexpected Error: {ex.Message}\nStackTrace: {ex.StackTrace}");
            return Result<IEnumerable<Product>>.Failure($"Error retrieving products: {ex.Message}");
        }
    }

    public async Task<Result<Product>> GetByIdAsync(Guid id)
    {
        try
        {
            var response = await _api.GetByIdAsync(id);
            if (response.IsSuccessStatusCode && response.Content != null)
            {
                var apiResponse = response.Content;
                if (apiResponse.Success && apiResponse.Result != null)
                {
                    var product = ProductAdapter.ToModel(apiResponse.Result);
                    return Result<Product>.Success(product);
                }
            }
            return Result<Product>.Failure($"Product with ID {id} not found");
        }
        catch (ApiException ex)
        {
            System.Diagnostics.Debug.WriteLine($"API Error in GetByIdAsync: {ex.StatusCode} - {ex.Message}");
            return Result<Product>.Failure($"API error retrieving product: {ex.Message}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Unexpected Error in GetByIdAsync: {ex.Message}");
            return Result<Product>.Failure($"Error retrieving product: {ex.Message}");
        }
    }

    public async Task<Result<Product>> CreateAsync(Product product)
    {
        try
        {
            // Map Product to CreateProductRequest (anonymous object for now)
            var request = new
            {
                name = product.Name,
                description = product.Description,
                price = product.SellingPrice,
                stock = product.Quantity,
                categoryId = product.CategoryId,
                imageUrl = product.ImageUrl
            };

            var response = await _api.CreateAsync(request);

            if (response.IsSuccessStatusCode && response.Content != null)
            {
                var createdProduct = ProductAdapter.ToModel(response.Content.Result);
                return Result<Product>.Success(createdProduct);
            }
            return Result<Product>.Failure("Failed to create product");
        }
        catch (ApiException ex)
        {
            System.Diagnostics.Debug.WriteLine($"API Error in CreateAsync: {ex.StatusCode} - {ex.Message}");
            return Result<Product>.Failure($"Failed to create product: {ex.Message}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Unexpected Error in CreateAsync: {ex.Message}");
            return Result<Product>.Failure($"Error creating product: {ex.Message}");
        }
    }

    public async Task<Result<Product>> UpdateAsync(Product product)
    {
        try
        {
            // Map Product to UpdateProductRequest (anonymous object for now)
            var request = new
            {
                name = product.Name,
                description = product.Description,
                price = product.SellingPrice,
                stock = product.Quantity,
                categoryId = product.CategoryId,
                imageUrl = product.ImageUrl
            };

            var response = await _api.UpdateAsync(product.Id, request);

            if (response.IsSuccessStatusCode && response.Content != null)
            {
                var updatedProduct = ProductAdapter.ToModel(response.Content.Result);
                return Result<Product>.Success(updatedProduct);
            }
            return Result<Product>.Failure("Failed to update product");
        }
        catch (ApiException ex)
        {
            System.Diagnostics.Debug.WriteLine($"API Error in UpdateAsync: {ex.StatusCode} - {ex.Message}");
            return Result<Product>.Failure($"Failed to update product: {ex.Message}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Unexpected Error in UpdateAsync: {ex.Message}");
            return Result<Product>.Failure($"Error updating product: {ex.Message}");
        }
    }

    public async Task<Result<bool>> DeleteAsync(Guid id)
    {
        try
        {
            var response = await _api.DeleteAsync(id);
            if (response.IsSuccessStatusCode && response.Content?.Result == true)
            {
                return Result<bool>.Success(true);
            }
            return Result<bool>.Failure("Failed to delete product");
        }
        catch (ApiException ex)
        {
            System.Diagnostics.Debug.WriteLine($"API Error in DeleteAsync: {ex.StatusCode} - {ex.Message}");
            return Result<bool>.Failure($"API error deleting product: {ex.Message}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Unexpected Error in DeleteAsync: {ex.Message}");
            return Result<bool>.Failure($"Error deleting product: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<Product>>> GetLowStockAsync(int threshold = 10)
    {
        try
        {
            var allProductsResult = await GetAllAsync();
            if (!allProductsResult.IsSuccess || allProductsResult.Data == null)
            {
                return Result<IEnumerable<Product>>.Failure("Failed to retrieve products");
            }

            var lowStockProducts = allProductsResult.Data.Where(p => p.Quantity < threshold).ToList();
            return Result<IEnumerable<Product>>.Success(lowStockProducts);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in GetLowStockAsync: {ex.Message}");
            return Result<IEnumerable<Product>>.Failure($"Error retrieving low stock products: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<Product>>> GetByCategoryAsync(Guid categoryId)
    {
        try
        {
            var allProductsResult = await GetAllAsync();
            if (!allProductsResult.IsSuccess || allProductsResult.Data == null)
            {
                return Result<IEnumerable<Product>>.Failure("Failed to retrieve products");
            }

            var categoryProducts = allProductsResult.Data.Where(p => p.CategoryId == categoryId).ToList();
            return Result<IEnumerable<Product>>.Success(categoryProducts);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in GetByCategoryAsync: {ex.Message}");
            return Result<IEnumerable<Product>>.Failure($"Error retrieving products by category: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<Product>>> SearchAsync(string query)
    {
        try
        {
            var allProductsResult = await GetAllAsync();
            if (!allProductsResult.IsSuccess || allProductsResult.Data == null)
            {
                return Result<IEnumerable<Product>>.Failure("Failed to retrieve products");
            }

            var search = query.ToLower();
            var searchResults = allProductsResult.Data.Where(p =>
                p.Name.ToLower().Contains(search) ||
                (p.SKU != null && p.SKU.ToLower().Contains(search)) ||
                (p.Description != null && p.Description.ToLower().Contains(search))).ToList();

            return Result<IEnumerable<Product>>.Success(searchResults);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in SearchAsync: {ex.Message}");
            return Result<IEnumerable<Product>>.Failure($"Error searching products: {ex.Message}");
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
        string? stockStatus = null,
        string sortBy = "name",
        bool sortDescending = false)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"[ProductRepository.GetPagedAsync] START - Page: {page}, PageSize: {pageSize}, Search: '{searchQuery}', Category: '{categoryName}', StockStatus: '{stockStatus}'");

            // Note: Backend API doesn't support server-side paging yet
            // Fallback: fetch all products and apply client-side paging/filtering
            var allProductsResult = await GetAllAsync();
            if (!allProductsResult.IsSuccess || allProductsResult.Data == null)
            {
                System.Diagnostics.Debug.WriteLine($"[ProductRepository.GetPagedAsync] ❌ GetAllAsync failed: {allProductsResult.ErrorMessage}");
                return Result<PagedList<Product>>.Failure(allProductsResult.ErrorMessage ?? "Failed to retrieve products");
            }

            var allProducts = allProductsResult.Data.ToList();
            System.Diagnostics.Debug.WriteLine($"[ProductRepository.GetPagedAsync] Fetched {allProducts.Count} total products");

            var query = allProducts.AsEnumerable();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                var search = searchQuery.ToLower();
                query = query.Where(p =>
                    p.Name.ToLower().Contains(search) ||
                    (p.SKU != null && p.SKU.ToLower().Contains(search)) ||
                    (p.Description != null && p.Description.ToLower().Contains(search)));
                System.Diagnostics.Debug.WriteLine($"[ProductRepository.GetPagedAsync] After search filter: {query.Count()} products");
            }

            if (!string.IsNullOrWhiteSpace(categoryName))
            {
                query = query.Where(p => p.CategoryName != null &&
                    p.CategoryName.Equals(categoryName, StringComparison.OrdinalIgnoreCase));
                System.Diagnostics.Debug.WriteLine($"[ProductRepository.GetPagedAsync] After category filter: {query.Count()} products");
            }

            if (!string.IsNullOrWhiteSpace(manufacturerName))
            {
                query = query.Where(p => p.Manufacturer != null &&
                    p.Manufacturer.Equals(manufacturerName, StringComparison.OrdinalIgnoreCase));
                System.Diagnostics.Debug.WriteLine($"[ProductRepository.GetPagedAsync] After manufacturer filter: {query.Count()} products");
            }

            if (minPrice.HasValue)
            {
                query = query.Where(p => p.SellingPrice >= minPrice.Value);
                System.Diagnostics.Debug.WriteLine($"[ProductRepository.GetPagedAsync] After minPrice filter: {query.Count()} products");
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(p => p.SellingPrice <= maxPrice.Value);
                System.Diagnostics.Debug.WriteLine($"[ProductRepository.GetPagedAsync] After maxPrice filter: {query.Count()} products");
            }

            // Apply stock status filter
            if (!string.IsNullOrWhiteSpace(stockStatus))
            {
                query = stockStatus.ToLower() switch
                {
                    "instock" => query.Where(p => p.Quantity > 0),
                    "lowstock" => query.Where(p => p.Quantity > 0 && p.Quantity <= 10),
                    "outofstock" => query.Where(p => p.Quantity == 0),
                    _ => query
                };
                System.Diagnostics.Debug.WriteLine($"[ProductRepository.GetPagedAsync] After stock status filter '{stockStatus}': {query.Count()} products");
            }

            // Apply sorting
            query = sortBy.ToLower() switch
            {
                "name" => sortDescending
                    ? query.OrderByDescending(p => p.Name)
                    : query.OrderBy(p => p.Name),
                "price" or "sellingprice" => sortDescending
                    ? query.OrderByDescending(p => p.SellingPrice)
                    : query.OrderBy(p => p.SellingPrice),
                "stock" or "quantity" => sortDescending
                    ? query.OrderByDescending(p => p.Quantity)
                    : query.OrderBy(p => p.Quantity),
                "category" or "categoryname" => sortDescending
                    ? query.OrderByDescending(p => p.CategoryName)
                    : query.OrderBy(p => p.CategoryName),
                _ => sortDescending
                    ? query.OrderByDescending(p => p.Name)
                    : query.OrderBy(p => p.Name)
            };

            var totalCount = query.Count();
            var items = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            var pagedList = new PagedList<Product>(items, totalCount, page, pageSize);
            System.Diagnostics.Debug.WriteLine($"[ProductRepository.GetPagedAsync] ✅ Returning PagedList - Items: {items.Count}, TotalCount: {totalCount}, Page: {page}, PageSize: {pageSize}, TotalPages: {pagedList.TotalPages}");
            return Result<PagedList<Product>>.Success(pagedList);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ProductRepository.GetPagedAsync] ❌ Error: {ex.Message}\nStackTrace: {ex.StackTrace}");
            return Result<PagedList<Product>>.Failure($"Error retrieving paged products: {ex.Message}");
        }
    }
}
