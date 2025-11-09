
using MyShop.Data.Entities;
using MyShop.Data.Repositories.Interfaces;
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
        var categories = await _categoryRepository.GetAllAsync();
        return categories.Select(c => CategoryMapper.ToCategoryResponse(c));
    }
    public async Task<CategoryResponse?> GetByIdAsync(Guid id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        return category is null ? null : CategoryMapper.ToCategoryResponse(category);
    }
    public async Task<CategoryResponse> CreateAsync(CreateCategoryRequest createCategoryRequest)
    {
        var category = new Category
        {
            Name = createCategoryRequest.Name,
            Description = createCategoryRequest.Description
        };
        var createdCategory = await _categoryRepository.CreateAsync(category);
        _logger.LogInformation("Created category with ID: {CategoryId}, Name: {Name}, Description: {Description}", createdCategory.Id, createdCategory.Name, createdCategory.Description);
        return CategoryMapper.ToCategoryResponse(createdCategory);
    }
    public async Task<CategoryResponse> UpdateAsync(Guid id, UpdateCategoryRequest updateCategoryRequest)
    {
        var existingCategory = await _categoryRepository.GetByIdAsync(id);
        if (existingCategory is null)
        {
            throw new System.Collections.Generic.KeyNotFoundException("Category not found");
        }

        // TODO: Check nullable fields before updating
        existingCategory.Name = updateCategoryRequest.Name;
        existingCategory.Description = updateCategoryRequest.Description;
        var updatedCategory = await _categoryRepository.UpdateAsync(existingCategory);
        return CategoryMapper.ToCategoryResponse(updatedCategory);
    }
    public async Task<bool> DeleteAsync(Guid id)
    {
        var existingCategory = await _categoryRepository.GetByIdAsync(id);
        if (existingCategory is null)
        {
            return false;
        }
        await _categoryRepository.DeleteAsync(id);
        return true;
    }
}