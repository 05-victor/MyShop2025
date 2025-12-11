using MyShop.Core.Common;
using MyShop.Core.Interfaces.Repositories;
using MyShop.Plugins.API.Products;
using MyShop.Shared.Adapters;
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
            if (response.IsSuccessStatusCode && response.Content != null)
            {
                var apiResponse = response.Content;
                if (apiResponse.Success && apiResponse.Result != null)
                {
                    // Map ProductResponse[] to Product[] using ProductAdapter
                    // Extract Items from PagedResult
                    var products = ProductAdapter.ToModelList(apiResponse.Result.Items);
                    return Result<IEnumerable<Product>>.Success(products);
                }
            }
            return Result<IEnumerable<Product>>.Failure("Failed to retrieve products");
        }
        catch (ApiException ex)
        {
            System.Diagnostics.Debug.WriteLine($"API Error in GetAllAsync: {ex.StatusCode} - {ex.Message}");
            return Result<IEnumerable<Product>>.Failure($"API error retrieving products: {ex.Message}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Unexpected Error in GetAllAsync: {ex.Message}");
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
        decimal? minPrice = null,
        decimal? maxPrice = null,
        string sortBy = "name",
        bool sortDescending = false)
    {
        try
        {
            // Note: Backend API doesn't support server-side paging yet
            // Fallback: fetch all products and apply client-side paging/filtering
            var allProductsResult = await GetAllAsync();
            if (!allProductsResult.IsSuccess || allProductsResult.Data == null)
            {
                return Result<PagedList<Product>>.Failure(allProductsResult.ErrorMessage ?? "Failed to retrieve products");
            }

            var query = allProductsResult.Data.AsEnumerable();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                var search = searchQuery.ToLower();
                query = query.Where(p => 
                    p.Name.ToLower().Contains(search) ||
                    (p.SKU != null && p.SKU.ToLower().Contains(search)) ||
                    (p.Description != null && p.Description.ToLower().Contains(search)));
            }

            if (!string.IsNullOrWhiteSpace(categoryName))
            {
                query = query.Where(p => p.CategoryName != null && 
                    p.CategoryName.Equals(categoryName, StringComparison.OrdinalIgnoreCase));
            }

            if (minPrice.HasValue)
            {
                query = query.Where(p => p.SellingPrice >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(p => p.SellingPrice <= maxPrice.Value);
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
            return Result<PagedList<Product>>.Success(pagedList);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error in GetPagedAsync: {ex.Message}");
            return Result<PagedList<Product>>.Failure($"Error retrieving paged products: {ex.Message}");
        }
    }
}
