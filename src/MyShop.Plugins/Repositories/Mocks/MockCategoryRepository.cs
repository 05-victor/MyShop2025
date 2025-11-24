using MyShop.Shared.Models;
using MyShop.Core.Interfaces.Repositories;
using MyShop.Plugins.Mocks.Data;

namespace MyShop.Plugins.Repositories.Mocks;

/// <summary>
/// Mock implementation of ICategoryRepository - delegates to MockCategoryData
/// </summary>
public class MockCategoryRepository : ICategoryRepository
{

    public async Task<IEnumerable<Category>> GetAllAsync()
    {
        try
        {
            var categories = await MockCategoryData.GetAllAsync();
            System.Diagnostics.Debug.WriteLine($"[MockCategoryRepository] GetAllAsync returned {categories.Count} categories");
            return categories;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockCategoryRepository] GetAllAsync error: {ex.Message}");
            return new List<Category>();
        }
    }

    public async Task<Category?> GetByIdAsync(Guid id)
    {
        try
        {
            var category = await MockCategoryData.GetByIdAsync(id);
            System.Diagnostics.Debug.WriteLine($"[MockCategoryRepository] GetByIdAsync({id}) - Found: {category != null}");
            return category;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockCategoryRepository] GetByIdAsync error: {ex.Message}");
            return null;
        }
    }

    public async Task<Category> CreateAsync(Category category)
    {
        try
        {
            category.Id = Guid.NewGuid();
            category.CreatedAt = DateTime.UtcNow;
            
            var created = await MockCategoryData.CreateAsync(category);
            System.Diagnostics.Debug.WriteLine($"[MockCategoryRepository] Created category: {created.Name}");
            return created;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockCategoryRepository] CreateAsync error: {ex.Message}");
            throw;
        }
    }

    public async Task<Category> UpdateAsync(Category category)
    {
        try
        {
            var updated = await MockCategoryData.UpdateAsync(category);
            System.Diagnostics.Debug.WriteLine($"[MockCategoryRepository] Updated category: {updated.Name}");
            return updated;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockCategoryRepository] UpdateAsync error: {ex.Message}");
            throw;
        }
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        try
        {
            var result = await MockCategoryData.DeleteAsync(id);
            System.Diagnostics.Debug.WriteLine($"[MockCategoryRepository] DeleteAsync - Success: {result}");
            return result;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockCategoryRepository] DeleteAsync error: {ex.Message}");
            return false;
        }
    }
}
