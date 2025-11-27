using MyShop.Core.Common;
using MyShop.Core.Interfaces.Facades;
using MyShop.Core.Interfaces.Repositories;
using MyShop.Core.Interfaces.Services;
using MyShop.Shared.Models;

namespace MyShop.Client.Facades.Products;

/// <summary>
/// Facade for category management operations
/// Aggregates: ICategoryRepository, IValidationService, IToastService
/// </summary>
public class CategoryFacade : ICategoryFacade
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IValidationService _validationService;
    private readonly IToastService _toastService;

    public CategoryFacade(
        ICategoryRepository categoryRepository,
        IValidationService validationService,
        IToastService toastService)
    {
        _categoryRepository = categoryRepository ?? throw new ArgumentNullException(nameof(categoryRepository));
        _validationService = validationService ?? throw new ArgumentNullException(nameof(validationService));
        _toastService = toastService ?? throw new ArgumentNullException(nameof(toastService));
    }

    public async Task<Result<List<Category>>> LoadCategoriesAsync()
    {
        try
        {
            var result = await _categoryRepository.GetAllAsync();
            if (!result.IsSuccess || result.Data == null)
            {
                await _toastService.ShowError("Failed to load categories");
                return Result<List<Category>>.Failure(result.ErrorMessage ?? "Failed to load categories");
            }
            return Result<List<Category>>.Success(result.Data.ToList());
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[CategoryFacade] Error loading categories: {ex.Message}");
            await _toastService.ShowError($"Error: {ex.Message}");
            return Result<List<Category>>.Failure($"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Load categories with server-side paging, search, and sorting
    /// </summary>
    public async Task<Result<PagedList<Category>>> LoadCategoriesAsync(
        int page = 1,
        int pageSize = 20,
        string? searchQuery = null,
        string sortBy = "name",
        bool sortDescending = false)
    {
        try
        {
            var result = await _categoryRepository.GetPagedAsync(
                page: page,
                pageSize: pageSize,
                searchQuery: searchQuery,
                sortBy: sortBy,
                sortDescending: sortDescending);

            if (!result.IsSuccess)
            {
                await _toastService.ShowError("Failed to load categories");
            }

            return result;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[CategoryFacade] Error loading paged categories: {ex.Message}");
            await _toastService.ShowError($"Error: {ex.Message}");
            return Result<PagedList<Category>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<Category>> GetCategoryByIdAsync(Guid categoryId)
    {
        try
        {
            var result = await _categoryRepository.GetByIdAsync(categoryId);
            if (!result.IsSuccess)
            {
                await _toastService.ShowError("Category not found");
            }
            return result;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[CategoryFacade] Error getting category: {ex.Message}");
            await _toastService.ShowError($"Error: {ex.Message}");
            return Result<Category>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<Category>> CreateCategoryAsync(string name, string description)
    {
        try
        {
            // Validate name
            var nameValidation = await _validationService.ValidateRequired(name, "Category Name");
            if (!nameValidation.IsSuccess || nameValidation.Data == null || !nameValidation.Data.IsValid)
            {
                var error = nameValidation.Data?.ErrorMessage ?? "Category name is required";
                await _toastService.ShowError(error);
                return Result<Category>.Failure(error);
            }

            var category = new Category
            {
                Id = Guid.NewGuid(),
                Name = name,
                Description = description,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var result = await _categoryRepository.CreateAsync(category);
            if (result.IsSuccess)
            {
                await _toastService.ShowSuccess($"Category '{name}' created successfully");
            }
            else
            {
                await _toastService.ShowError("Failed to create category");
            }

            return result;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[CategoryFacade] Error creating category: {ex.Message}");
            await _toastService.ShowError($"Error: {ex.Message}");
            return Result<Category>.Failure($"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Create category from model (overload for ViewModel usage)
    /// </summary>
    public async Task<Result<Category>> CreateCategoryAsync(Category category)
    {
        try
        {
            // Validate name
            var nameValidation = await _validationService.ValidateRequired(category.Name, "Category Name");
            if (!nameValidation.IsSuccess || nameValidation.Data == null || !nameValidation.Data.IsValid)
            {
                var error = nameValidation.Data?.ErrorMessage ?? "Category name is required";
                return Result<Category>.Failure(error);
            }

            var result = await _categoryRepository.CreateAsync(category);
            return result;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[CategoryFacade] Error creating category: {ex.Message}");
            return Result<Category>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<Category>> UpdateCategoryAsync(Guid categoryId, string name, string description)
    {
        try
        {
            // Validate name
            var nameValidation = await _validationService.ValidateRequired(name, "Category Name");
            if (!nameValidation.IsSuccess || nameValidation.Data == null || !nameValidation.Data.IsValid)
            {
                var error = nameValidation.Data?.ErrorMessage ?? "Category name is required";
                await _toastService.ShowError(error);
                return Result<Category>.Failure(error);
            }

            // Get existing category
            var getResult = await _categoryRepository.GetByIdAsync(categoryId);
            if (!getResult.IsSuccess || getResult.Data == null)
            {
                await _toastService.ShowError("Category not found");
                return Result<Category>.Failure("Category not found");
            }

            var category = getResult.Data;
            category.Name = name;
            category.Description = description;
            category.UpdatedAt = DateTime.UtcNow;

            var result = await _categoryRepository.UpdateAsync(category);
            if (result.IsSuccess)
            {
                await _toastService.ShowSuccess($"Category '{name}' updated successfully");
            }
            else
            {
                await _toastService.ShowError("Failed to update category");
            }

            return result;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[CategoryFacade] Error updating category: {ex.Message}");
            await _toastService.ShowError($"Error: {ex.Message}");
            return Result<Category>.Failure($"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Update category from model (overload for ViewModel usage)
    /// </summary>
    public async Task<Result<Category>> UpdateCategoryAsync(Category category)
    {
        try
        {
            // Validate name
            var nameValidation = await _validationService.ValidateRequired(category.Name, "Category Name");
            if (!nameValidation.IsSuccess || nameValidation.Data == null || !nameValidation.Data.IsValid)
            {
                var error = nameValidation.Data?.ErrorMessage ?? "Category name is required";
                return Result<Category>.Failure(error);
            }

            category.UpdatedAt = DateTime.UtcNow;
            var result = await _categoryRepository.UpdateAsync(category);
            return result;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[CategoryFacade] Error updating category: {ex.Message}");
            return Result<Category>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<Unit>> DeleteCategoryAsync(Guid categoryId)
    {
        try
        {
            var result = await _categoryRepository.DeleteAsync(categoryId);
            if (result.IsSuccess)
            {
                await _toastService.ShowSuccess("Category deleted successfully");
                return Result<Unit>.Success(Unit.Value);
            }
            else
            {
                await _toastService.ShowError("Failed to delete category");
                return Result<Unit>.Failure(result.ErrorMessage ?? "Failed to delete category");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[CategoryFacade] Error deleting category: {ex.Message}");
            await _toastService.ShowError($"Error: {ex.Message}");
            return Result<Unit>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<Dictionary<string, int>>> GetProductCountByCategoryAsync()
    {
        try
        {
            var categoriesResult = await _categoryRepository.GetAllAsync();
            if (!categoriesResult.IsSuccess || categoriesResult.Data == null)
            {
                await _toastService.ShowError("Failed to load categories");
                return Result<Dictionary<string, int>>.Failure("Failed to load categories");
            }

            // Mock product counts - in real implementation, would query products
            var counts = categoriesResult.Data.ToDictionary(
                c => c.Name ?? "Unknown",
                c => 0 // Would be actual product count
            );

            return Result<Dictionary<string, int>>.Success(counts);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[CategoryFacade] Error getting product counts: {ex.Message}");
            await _toastService.ShowError($"Error: {ex.Message}");
            return Result<Dictionary<string, int>>.Failure($"Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Get product count for a specific category
    /// </summary>
    public async Task<Result<int>> GetProductCountByCategoryAsync(Guid categoryId)
    {
        try
        {
            // Mock implementation - in real implementation, would query products by category
            await Task.Delay(1); // Simulate async operation
            var productCount = 0; // Would be actual product count from product repository
            return Result<int>.Success(productCount);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[CategoryFacade] Error getting product count for category: {ex.Message}");
            return Result<int>.Failure($"Error: {ex.Message}");
        }
    }
}
