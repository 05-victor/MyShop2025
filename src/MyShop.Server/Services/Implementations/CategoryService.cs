using MyShop.Data.Entities;
using MyShop.Data.Repositories.Interfaces;
using MyShop.Server.Exceptions;
using MyShop.Server.Services.Interfaces;
using MyShop.Server.Services.Mappings;
using MyShop.Shared.DTOs.Requests;
using MyShop.Shared.DTOs.Responses;

namespace MyShop.Server.Services.Implementations;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly ILogger<CategoryService> _logger;

    public CategoryService(ICategoryRepository categoryRepository, ILogger<CategoryService> logger)
    {
        _categoryRepository = categoryRepository;
        _logger = logger;   
    }

    public async Task<IEnumerable<CategoryResponse>> GetAllAsync()
    {
        try
        {
            var categories = await _categoryRepository.GetAllAsync();
            return categories.Select(c => CategoryMapper.ToCategoryResponse(c));
        }
        catch (Exception ex) when (ex is not BaseApplicationException)
        {
            _logger.LogError(ex, "Error retrieving all categories");
            throw InfrastructureException.DatabaseError("Failed to retrieve categories", ex);
        }
    }

    public async Task<CategoryResponse?> GetByIdAsync(Guid id)
    {
        try
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            return category is null ? null : CategoryMapper.ToCategoryResponse(category);
        }
        catch (Exception ex) when (ex is not BaseApplicationException)
        {
            _logger.LogError(ex, "Error retrieving category {CategoryId}", id);
            throw InfrastructureException.DatabaseError($"Failed to retrieve category with ID {id}", ex);
        }
    }

    public async Task<CategoryResponse> CreateAsync(CreateCategoryRequest createCategoryRequest)
    {
        // Validate required fields
        if (string.IsNullOrWhiteSpace(createCategoryRequest.Name))
        {
            throw ValidationException.ForField("Name", "Category name is required");
        }

        try
        {
            var category = new Category
            {
                Name = createCategoryRequest.Name.Trim(),
                Description = createCategoryRequest.Description?.Trim()
            };

            var createdCategory = await _categoryRepository.CreateAsync(category);
            _logger.LogInformation("Created category with ID: {CategoryId}, Name: {Name}", 
                createdCategory.Id, createdCategory.Name);

            return CategoryMapper.ToCategoryResponse(createdCategory);
        }
        catch (Exception ex) when (ex is not BaseApplicationException)
        {
            _logger.LogError(ex, "Error creating category");
            throw InfrastructureException.DatabaseError("Failed to create category", ex);
        }
    }

    public async Task<CategoryResponse> UpdateAsync(Guid id, UpdateCategoryRequest updateCategoryRequest)
    {
        var existingCategory = await _categoryRepository.GetByIdAsync(id);
        if (existingCategory is null)
        {
            throw NotFoundException.ForEntity("Category", id);
        }

        try
        {
            // Update only non-null fields
            if (!string.IsNullOrWhiteSpace(updateCategoryRequest.Name))
            {
                existingCategory.Name = updateCategoryRequest.Name.Trim();
            }

            if (updateCategoryRequest.Description != null)
            {
                existingCategory.Description = updateCategoryRequest.Description.Trim();
            }

            var updatedCategory = await _categoryRepository.UpdateAsync(existingCategory);
            return CategoryMapper.ToCategoryResponse(updatedCategory);
        }
        catch (Exception ex) when (ex is not BaseApplicationException)
        {
            _logger.LogError(ex, "Error updating category {CategoryId}", id);
            throw InfrastructureException.DatabaseError($"Failed to update category with ID {id}", ex);
        }
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var existingCategory = await _categoryRepository.GetByIdAsync(id);
        if (existingCategory is null)
        {
            return false;
        }

        try
        {
            await _categoryRepository.DeleteAsync(id);
            _logger.LogInformation("Category {CategoryId} deleted", id);
            return true;
        }
        catch (Exception ex) when (ex is not BaseApplicationException)
        {
            _logger.LogError(ex, "Error deleting category {CategoryId}", id);
            throw InfrastructureException.DatabaseError($"Failed to delete category with ID {id}", ex);
        }
    }
}