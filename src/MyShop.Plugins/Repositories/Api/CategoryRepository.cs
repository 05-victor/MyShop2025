using MyShop.Core.Interfaces.Repositories;
using MyShop.Plugins.API.Categories;
using MyShop.Shared.DTOs.Requests;
using MyShop.Shared.Models;

namespace MyShop.Plugins.Repositories.Api;

/// <summary>
/// API-based Category Repository implementation
/// </summary>
public class CategoryRepository : ICategoryRepository
{
    private readonly ICategoriesApi _api;

    public CategoryRepository(ICategoriesApi api)
    {
        _api = api;
    }

    public async Task<IEnumerable<Category>> GetAllAsync()
    {
        try
        {
            var response = await _api.GetAllAsync();
            
            if (response.IsSuccessStatusCode && response.Content != null)
            {
                var apiResponse = response.Content;
                if (apiResponse.Success && apiResponse.Result != null)
                {
                    return apiResponse.Result.Select(MapToCategory);
                }
            }

            return Enumerable.Empty<Category>();
        }
        catch (Exception)
        {
            return Enumerable.Empty<Category>();
        }
    }

    public async Task<Category?> GetByIdAsync(Guid id)
    {
        try
        {
            var response = await _api.GetByIdAsync(id);
            
            if (response.IsSuccessStatusCode && response.Content != null)
            {
                var apiResponse = response.Content;
                if (apiResponse.Success && apiResponse.Result != null)
                {
                    return MapToCategory(apiResponse.Result);
                }
            }

            return null;
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task<Category> CreateAsync(Category category)
    {
        try
        {
            var request = new CreateCategoryRequest
            {
                Name = category.Name,
                Description = category.Description
            };

            var response = await _api.CreateAsync(request);
            
            if (response.IsSuccessStatusCode && response.Content != null)
            {
                var apiResponse = response.Content;
                if (apiResponse.Success && apiResponse.Result != null)
                {
                    return MapToCategory(apiResponse.Result);
                }
            }

            throw new InvalidOperationException("Failed to create category");
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error creating category: {ex.Message}", ex);
        }
    }

    public async Task<Category> UpdateAsync(Category category)
    {
        try
        {
            var request = new UpdateCategoryRequest
            {
                Name = category.Name,
                Description = category.Description
            };

            var response = await _api.UpdateAsync(category.Id, request);
            
            if (response.IsSuccessStatusCode && response.Content != null)
            {
                var apiResponse = response.Content;
                if (apiResponse.Success && apiResponse.Result != null)
                {
                    return MapToCategory(apiResponse.Result);
                }
            }

            throw new InvalidOperationException("Failed to update category");
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error updating category: {ex.Message}", ex);
        }
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        try
        {
            var response = await _api.DeleteAsync(id);
            return response.IsSuccessStatusCode && response.Content?.Result == true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>
    /// Map CategoryResponse DTO to Category domain model
    /// </summary>
    private static Category MapToCategory(MyShop.Shared.DTOs.Responses.CategoryResponse dto)
    {
        return new Category
        {
            Id = dto.Id,
            Name = dto.Name,
            Description = dto.Description,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }
}
