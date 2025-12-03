using MyShop.Core.Interfaces.Repositories;
using MyShop.Plugins.API.Products;
using MyShop.Shared.DTOs.Commons;
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

    public async Task<IEnumerable<Product>> GetAllAsync()
    {
        try
        {
            var response = await _api.GetAllAsync(pageNumber: 1, pageSize: int.MaxValue);
            if (response.IsSuccessStatusCode && response.Content != null)
            {
                var apiResponse = response.Content;
                if (apiResponse.Success && apiResponse.Result != null)
                {
                    return apiResponse.Result.Items.Select(MapToModel).ToList();
                }
            }
            return Enumerable.Empty<Product>();
        }
        catch (ApiException ex)
        {
            System.Diagnostics.Debug.WriteLine($"API Error in GetAllAsync: {ex.StatusCode} - {ex.Message}");
            return Enumerable.Empty<Product>();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Unexpected Error in GetAllAsync: {ex.Message}");
            return Enumerable.Empty<Product>();
        }
    }

    public async Task<PagedResult<Product>> GetAllAsync(int pageNumber = 1, int pageSize = 10)
    {
        try
        {
            var response = await _api.GetAllAsync(pageNumber, pageSize);
            if (response.IsSuccessStatusCode && response.Content != null)
            {
                var apiResponse = response.Content;
                if (apiResponse.Success && apiResponse.Result != null)
                {
                    return new PagedResult<Product>
                    {
                        Items = apiResponse.Result.Items.Select(MapToModel).ToList(),
                        TotalCount = apiResponse.Result.TotalCount,
                        Page = apiResponse.Result.Page,
                        PageSize = apiResponse.Result.PageSize
                    };
                }
            }
            return new PagedResult<Product>
            {
                Items = new List<Product>(),
                TotalCount = 0,
                Page = pageNumber,
                PageSize = pageSize
            };
        }
        catch (ApiException ex)
        {
            System.Diagnostics.Debug.WriteLine($"API Error in GetAllAsync: {ex.StatusCode} - {ex.Message}");
            return new PagedResult<Product>
            {
                Items = new List<Product>(),
                TotalCount = 0,
                Page = pageNumber,
                PageSize = pageSize
            };
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Unexpected Error in GetAllAsync: {ex.Message}");
            return new PagedResult<Product>
            {
                Items = new List<Product>(),
                TotalCount = 0,
                Page = pageNumber,
                PageSize = pageSize
            };
        }
    }

    public async Task<Product?> GetByIdAsync(Guid id)
    {
        try
        {
            var response = await _api.GetByIdAsync(id);
            if (response.IsSuccessStatusCode && response.Content != null)
            {
                var apiResponse = response.Content;
                if (apiResponse.Success && apiResponse.Result != null)
                {
                    return MapToModel(apiResponse.Result);
                }
            }
            return null;
        }
        catch (ApiException ex)
        {
            System.Diagnostics.Debug.WriteLine($"API Error in GetByIdAsync: {ex.StatusCode} - {ex.Message}");
            return null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Unexpected Error in GetByIdAsync: {ex.Message}");
            return null;
        }
    }

    public async Task<Product> CreateAsync(Product product)
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
                return MapToModel(response.Content);
            }
            return product; // Return original if failed
        }
        catch (ApiException ex)
        {
            System.Diagnostics.Debug.WriteLine($"API Error in CreateAsync: {ex.StatusCode} - {ex.Message}");
            throw new Exception($"Failed to create product: {ex.Message}", ex);
        }
    }

    public async Task<Product> UpdateAsync(Product product)
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
                return MapToModel(response.Content);
            }
            return product; // Return original if failed
        }
        catch (ApiException ex)
        {
            System.Diagnostics.Debug.WriteLine($"API Error in UpdateAsync: {ex.StatusCode} - {ex.Message}");
            throw new Exception($"Failed to update product: {ex.Message}", ex);
        }
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        try
        {
            var response = await _api.DeleteAsync(id);
            return response.IsSuccessStatusCode && response.Content?.Result == true;
        }
        catch (ApiException ex)
        {
            System.Diagnostics.Debug.WriteLine($"API Error in DeleteAsync: {ex.StatusCode} - {ex.Message}");
            return false;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Unexpected Error in DeleteAsync: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Maps ProductResponse DTO to Product Model
    /// TODO: Use ProductAdapter when available
    /// </summary>
    private Product MapToModel(dynamic dto)
    {
        return new Product
        {
            Id = dto.Id,
            Name = dto.Name ?? string.Empty,
            Description = dto.Description ?? string.Empty,
            SellingPrice = dto.Price,
            Quantity = dto.Stock,
            ImageUrl = dto.ImageUrl,
            CategoryId = dto.CategoryId,
            CreatedAt = dto.CreatedAt,
            UpdatedAt = dto.UpdatedAt
        };
    }
}
