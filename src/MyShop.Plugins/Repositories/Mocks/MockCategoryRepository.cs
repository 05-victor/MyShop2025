using MyShop.Shared.Models;
using MyShop.Core.Interfaces.Repositories;
using System.Text.Json;

namespace MyShop.Plugins.Repositories.Mocks;

/// <summary>
/// Mock implementation of ICategoryRepository using JSON data
/// </summary>
public class MockCategoryRepository : ICategoryRepository
{
    private readonly List<Category> _categories;
    private readonly string _jsonFilePath;

    public MockCategoryRepository()
    {
        _jsonFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Mocks", "Data", "Json", "categories.json");
        _categories = LoadCategoriesFromJson();
    }

    private List<Category> LoadCategoriesFromJson()
    {
        try
        {
            if (!File.Exists(_jsonFilePath))
            {
                System.Diagnostics.Debug.WriteLine($"[MockCategoryRepository] JSON file not found: {_jsonFilePath}");
                return new List<Category>();
            }

            var json = File.ReadAllText(_jsonFilePath);
            var jsonDoc = JsonDocument.Parse(json);
            var categoriesArray = jsonDoc.RootElement.GetProperty("categories");

            var categories = new List<Category>();

            foreach (var item in categoriesArray.EnumerateArray())
            {
                var category = new Category
                {
                    Id = Guid.Parse(item.GetProperty("id").GetString()!),
                    Name = item.GetProperty("name").GetString()!,
                    Description = item.TryGetProperty("description", out var desc) && desc.ValueKind != JsonValueKind.Null
                        ? desc.GetString()
                        : null,
                    CreatedAt = DateTime.Parse(item.GetProperty("createdAt").GetString()!),
                    UpdatedAt = item.TryGetProperty("updatedAt", out var updatedAt) && updatedAt.ValueKind != JsonValueKind.Null
                        ? DateTime.Parse(updatedAt.GetString()!)
                        : null
                };

                categories.Add(category);
            }

            System.Diagnostics.Debug.WriteLine($"[MockCategoryRepository] Loaded {categories.Count} categories from JSON");
            return categories;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockCategoryRepository] Error loading JSON: {ex.Message}");
            return new List<Category>();
        }
    }

    public async Task<IEnumerable<Category>> GetAllAsync()
    {
        await Task.Delay(200);
        System.Diagnostics.Debug.WriteLine($"[MockCategoryRepository] GetAllAsync called, returning {_categories.Count} categories");
        return _categories.ToList();
    }

    public async Task<Category?> GetByIdAsync(Guid id)
    {
        await Task.Delay(150);
        var category = _categories.FirstOrDefault(c => c.Id == id);
        System.Diagnostics.Debug.WriteLine($"[MockCategoryRepository] GetByIdAsync({id}) - Found: {category != null}");
        return category;
    }

    public async Task<Category> CreateAsync(Category category)
    {
        await Task.Delay(400);
        
        category.Id = Guid.NewGuid();
        category.CreatedAt = DateTime.UtcNow;
        category.UpdatedAt = null;
        
        _categories.Add(category);
        
        System.Diagnostics.Debug.WriteLine($"[MockCategoryRepository] Created category: {category.Name} (ID: {category.Id})");
        return category;
    }

    public async Task<Category> UpdateAsync(Category category)
    {
        await Task.Delay(350);
        
        var existingCategory = _categories.FirstOrDefault(c => c.Id == category.Id);
        if (existingCategory == null)
        {
            throw new InvalidOperationException($"Category with ID {category.Id} not found");
        }

        existingCategory.Name = category.Name;
        existingCategory.Description = category.Description;
        existingCategory.UpdatedAt = DateTime.UtcNow;

        System.Diagnostics.Debug.WriteLine($"[MockCategoryRepository] Updated category: {existingCategory.Name}");
        return existingCategory;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        await Task.Delay(300);
        
        var category = _categories.FirstOrDefault(c => c.Id == id);
        if (category == null)
        {
            return false;
        }

        // Check if category has products (simulated)
        // In real implementation, check against products table
        // For now, just remove
        _categories.Remove(category);
        
        System.Diagnostics.Debug.WriteLine($"[MockCategoryRepository] Deleted category: {category.Name}");
        return true;
    }
}
