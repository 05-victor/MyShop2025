using MyShop.Shared.Adapters;
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
            var response = await _api.GetAllAsync(pageNumber: 1, pageSize: int.MaxValue);
            
            if (response.IsSuccessStatusCode && response.Content != null)
            {
                var apiResponse = response.Content;
                if (apiResponse.Success && apiResponse.Result != null)
                {
                    var categories = CategoryAdapter.ToModelList(apiResponse.Result);
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
                    var category = CategoryAdapter.ToModel(apiResponse.Result);
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
                    var createdCategory = CategoryAdapter.ToModel(apiResponse.Result);
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
                    var updatedCategory = CategoryAdapter.ToModel(apiResponse.Result);
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

    public async Task<Result<PagedList<Category>>> GetPagedAsync(
        int page = 1,
        int pageSize = 20,
        string? searchQuery = null,
        string sortBy = "name",
        bool sortDescending = false)
    {
        try
        {
            // Note: Backend API doesn't support server-side paging yet
            // Fallback: fetch all categories and apply client-side paging/filtering
            var allCategoriesResult = await GetAllAsync();
            if (!allCategoriesResult.IsSuccess || allCategoriesResult.Data == null)
            {
                return Result<PagedList<Category>>.Failure(allCategoriesResult.ErrorMessage ?? "Failed to retrieve categories");
            }

            var query = allCategoriesResult.Data.AsEnumerable();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                var search = searchQuery.ToLower();
                query = query.Where(c => 
                    c.Name.ToLower().Contains(search) ||
                    (c.Description != null && c.Description.ToLower().Contains(search)));
            }

            // Apply sorting
            query = sortBy.ToLower() switch
            {
                "name" => sortDescending 
                    ? query.OrderByDescending(c => c.Name) 
                    : query.OrderBy(c => c.Name),
                "description" => sortDescending 
                    ? query.OrderByDescending(c => c.Description) 
                    : query.OrderBy(c => c.Description),
                _ => sortDescending 
                    ? query.OrderByDescending(c => c.Name) 
                    : query.OrderBy(c => c.Name)
            };

            var totalCount = query.Count();
            var items = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            var pagedList = new PagedList<Category>(items, totalCount, page, pageSize);
            return Result<PagedList<Category>>.Success(pagedList);
        }
        catch (Exception ex)
        {
            return Result<PagedList<Category>>.Failure($"Error retrieving paged categories: {ex.Message}");
        }
    }
}
