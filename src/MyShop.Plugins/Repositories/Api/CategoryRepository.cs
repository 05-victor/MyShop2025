using MyShop.Core.Common;
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

    public async Task<Result<IEnumerable<Category>>> GetAllAsync()
    {
        try
        {
            var response = await _api.GetAllAsync();
            
            if (response.IsSuccessStatusCode && response.Content != null)
            {
                var apiResponse = response.Content;
                if (apiResponse.Success && apiResponse.Result != null)
                {
                    var categories = apiResponse.Result.Select(MapToCategory).ToList();
                    return Result<IEnumerable<Category>>.Success(categories);
                }
            }

            return Result<IEnumerable<Category>>.Failure("Failed to retrieve categories");
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<Category>>.Failure($"Error retrieving categories: {ex.Message}");
        }
    }

    public async Task<Result<Category>> GetByIdAsync(Guid id)
    {
        try
        {
            var response = await _api.GetByIdAsync(id);
            
            if (response.IsSuccessStatusCode && response.Content != null)
            {
                var apiResponse = response.Content;
                if (apiResponse.Success && apiResponse.Result != null)
                {
                    var category = MapToCategory(apiResponse.Result);
                    return Result<Category>.Success(category);
                }
            }

            return Result<Category>.Failure($"Category with ID {id} not found");
        }
        catch (Exception ex)
        {
            return Result<Category>.Failure($"Error retrieving category: {ex.Message}");
        }
    }

    public async Task<Result<Category>> CreateAsync(Category category)
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
                    var createdCategory = MapToCategory(apiResponse.Result);
                    return Result<Category>.Success(createdCategory);
                }
            }

            return Result<Category>.Failure("Failed to create category");
        }
        catch (Exception ex)
        {
            return Result<Category>.Failure($"Error creating category: {ex.Message}");
        }
    }

    public async Task<Result<Category>> UpdateAsync(Category category)
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
                    var updatedCategory = MapToCategory(apiResponse.Result);
                    return Result<Category>.Success(updatedCategory);
                }
            }

            return Result<Category>.Failure("Failed to update category");
        }
        catch (Exception ex)
        {
            return Result<Category>.Failure($"Error updating category: {ex.Message}");
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
            return Result<bool>.Failure("Failed to delete category");
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Error deleting category: {ex.Message}");
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
