using MyShop.Core.Common;
using MyShop.Shared.Models;

namespace MyShop.Core.Interfaces.Facades;

/// <summary>
/// Facade pattern for category management
/// Aggregates: ICategoryRepository, IValidationService, IToastService
/// </summary>
public interface ICategoryFacade
{
    /// <summary>
    /// Load all categories
    /// </summary>
    Task<Result<List<Category>>> LoadCategoriesAsync();

    /// <summary>
    /// Get category by ID
    /// </summary>
    Task<Result<Category>> GetCategoryByIdAsync(Guid categoryId);

    /// <summary>
    /// Create new category
    /// </summary>
    Task<Result<Category>> CreateCategoryAsync(string name, string description);

    /// <summary>
    /// Update category
    /// </summary>
    Task<Result<Category>> UpdateCategoryAsync(Guid categoryId, string name, string description);

    /// <summary>
    /// Delete category
    /// </summary>
    Task<Result<Unit>> DeleteCategoryAsync(Guid categoryId);

    /// <summary>
    /// Get product count by category
    /// </summary>
    Task<Result<Dictionary<string, int>>> GetProductCountByCategoryAsync();
}
