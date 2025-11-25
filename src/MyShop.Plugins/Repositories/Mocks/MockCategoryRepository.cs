using MyShop.Shared.Models;
using MyShop.Core.Interfaces.Repositories;
using MyShop.Plugins.Mocks.Data;
using MyShop.Core.Common;

namespace MyShop.Plugins.Repositories.Mocks;

/// <summary>
/// Mock implementation of ICategoryRepository - delegates to MockCategoryData
/// </summary>
public class MockCategoryRepository : ICategoryRepository
{

    public async Task<Result<IEnumerable<Category>>> GetAllAsync()
    {
        try
        {
            var categories = await MockCategoryData.GetAllAsync();
            System.Diagnostics.Debug.WriteLine($"[MockCategoryRepository] GetAllAsync returned {categories.Count} categories");
            return Result<IEnumerable<Category>>.Success(categories);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockCategoryRepository] GetAllAsync error: {ex.Message}");
            return Result<IEnumerable<Category>>.Failure($"Failed to get categories: {ex.Message}");
        }
    }

    public async Task<Result<Category>> GetByIdAsync(Guid id)
    {
        try
        {
            var category = await MockCategoryData.GetByIdAsync(id);
            System.Diagnostics.Debug.WriteLine($"[MockCategoryRepository] GetByIdAsync({id}) - Found: {category != null}");
            return category != null
                ? Result<Category>.Success(category)
                : Result<Category>.Failure($"Category with ID {id} not found");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockCategoryRepository] GetByIdAsync error: {ex.Message}");
            return Result<Category>.Failure($"Failed to get category: {ex.Message}");
        }
    }

    public async Task<Result<Category>> CreateAsync(Category category)
    {
        try
        {
            category.Id = Guid.NewGuid();
            category.CreatedAt = DateTime.UtcNow;
            
            var created = await MockCategoryData.CreateAsync(category);
            System.Diagnostics.Debug.WriteLine($"[MockCategoryRepository] Created category: {created.Name}");
            return Result<Category>.Success(created);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockCategoryRepository] CreateAsync error: {ex.Message}");
            return Result<Category>.Failure($"Failed to create category: {ex.Message}");
        }
    }

    public async Task<Result<Category>> UpdateAsync(Category category)
    {
        try
        {
            var updated = await MockCategoryData.UpdateAsync(category);
            System.Diagnostics.Debug.WriteLine($"[MockCategoryRepository] Updated category: {updated.Name}");
            return Result<Category>.Success(updated);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockCategoryRepository] UpdateAsync error: {ex.Message}");
            return Result<Category>.Failure($"Failed to update category: {ex.Message}");
        }
    }

    public async Task<Result<bool>> DeleteAsync(Guid id)
    {
        try
        {
            var result = await MockCategoryData.DeleteAsync(id);
            System.Diagnostics.Debug.WriteLine($"[MockCategoryRepository] DeleteAsync - Success: {result}");
            return result
                ? Result<bool>.Success(true)
                : Result<bool>.Failure($"Failed to delete category with ID {id}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockCategoryRepository] DeleteAsync error: {ex.Message}");
            return Result<bool>.Failure($"Failed to delete category: {ex.Message}");
        }
    }
}
