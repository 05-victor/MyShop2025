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
            var response = await _api.GetAllAsync();
            if (response.IsSuccessStatusCode && response.Content != null)
            {
                var apiResponse = response.Content;
                if (apiResponse.Success && apiResponse.Result != null)
                {
                    // Map ProductResponse[] to Product[] using ProductAdapter
                    var products = ProductAdapter.ToModelList(apiResponse.Result);
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

    // DTO Mapping methods removed - now using ProductAdapter
}
