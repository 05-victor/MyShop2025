using MyShop.Shared.Models;
using System.Text.Json;

namespace MyShop.Plugins.Mocks.Data;

/// <summary>
/// Mock data provider for categories - loads from JSON file
/// </summary>
public static class MockCategoryData
{
    private static List<CategoryDataModel>? _categories;
    private static readonly object _lock = new object();
    private static readonly string _jsonFilePath = Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory, "Mocks", "Data", "Json", "categories.json");

    private static void EnsureDataLoaded()
    {
        if (_categories != null) return;

        lock (_lock)
        {
            if (_categories != null) return;

            try
            {
                if (!File.Exists(_jsonFilePath))
                {
                    System.Diagnostics.Debug.WriteLine($"Categories JSON file not found at: {_jsonFilePath}");
                    InitializeDefaultData();
                    return;
                }

                var jsonString = File.ReadAllText(_jsonFilePath);
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var data = JsonSerializer.Deserialize<CategoryDataContainer>(jsonString, options);

                if (data?.Categories != null)
                {
                    _categories = data.Categories;
                    System.Diagnostics.Debug.WriteLine($"Loaded {_categories.Count} categories from categories.json");
                }
                else
                {
                    InitializeDefaultData();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading categories.json: {ex.Message}");
                InitializeDefaultData();
            }
        }
    }

    private static void InitializeDefaultData()
    {
        _categories = new List<CategoryDataModel>
        {
            new CategoryDataModel
            {
                Id = "10000000-0000-0000-0000-000000000001",
                Name = "Smartphones",
                Description = "Điện thoại thông minh",
                CreatedAt = DateTime.Parse("2024-01-15T10:00:00Z")
            },
            new CategoryDataModel
            {
                Id = "10000000-0000-0000-0000-000000000002",
                Name = "Tablets",
                Description = "Máy tính bảng",
                CreatedAt = DateTime.Parse("2024-01-15T10:00:00Z")
            },
            new CategoryDataModel
            {
                Id = "10000000-0000-0000-0000-000000000003",
                Name = "Laptops",
                Description = "Laptop gaming, văn phòng",
                CreatedAt = DateTime.Parse("2024-01-15T10:00:00Z")
            }
        };
    }

    public static async Task<List<Category>> GetAllAsync()
    {
        EnsureDataLoaded();

        // Simulate network delay
        await Task.Delay(250);

        return _categories!.Select(c => new Category
        {
            Id = Guid.Parse(c.Id),
            Name = c.Name,
            Description = c.Description,
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt
        }).ToList();
    }

    public static async Task<Category?> GetByIdAsync(Guid id)
    {
        EnsureDataLoaded();

        // Simulate network delay
        await Task.Delay(150);

        var categoryData = _categories!.FirstOrDefault(c => c.Id == id.ToString());
        if (categoryData == null) return null;

        return new Category
        {
            Id = Guid.Parse(categoryData.Id),
            Name = categoryData.Name,
            Description = categoryData.Description,
            CreatedAt = categoryData.CreatedAt,
            UpdatedAt = categoryData.UpdatedAt
        };
    }

    public static async Task<Category> CreateAsync(Category category)
    {
        EnsureDataLoaded();

        // Simulate network delay
        await Task.Delay(400);

        var newCategoryData = new CategoryDataModel
        {
            Id = category.Id.ToString(),
            Name = category.Name,
            Description = category.Description,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null
        };

        _categories!.Add(newCategoryData);

        // Persist to JSON
        await SaveDataToJsonAsync();

        return category;
    }

    public static async Task<Category> UpdateAsync(Category category)
    {
        EnsureDataLoaded();

        // Simulate network delay
        await Task.Delay(350);

        var existing = _categories!.FirstOrDefault(c => c.Id == category.Id.ToString());
        if (existing == null)
        {
            throw new InvalidOperationException($"Category with ID {category.Id} not found");
        }

        // Update properties
        existing.Name = category.Name;
        existing.Description = category.Description;
        existing.UpdatedAt = DateTime.UtcNow;

        // Persist to JSON
        await SaveDataToJsonAsync();

        return category;
    }

    public static async Task<bool> DeleteAsync(Guid id)
    {
        EnsureDataLoaded();

        // Simulate network delay
        await Task.Delay(300);

        var category = _categories!.FirstOrDefault(c => c.Id == id.ToString());
        if (category == null) return false;

        _categories.Remove(category);

        // Persist to JSON
        await SaveDataToJsonAsync();

        return true;
    }

    private static async Task SaveDataToJsonAsync()
    {
        try
        {
            var container = new CategoryDataContainer
            {
                Categories = _categories!
            };

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var jsonString = JsonSerializer.Serialize(container, options);
            await File.WriteAllTextAsync(_jsonFilePath, jsonString);

            System.Diagnostics.Debug.WriteLine("Successfully saved categories data to JSON");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving categories.json: {ex.Message}");
        }
    }

    // Data container classes for JSON deserialization
    private class CategoryDataContainer
    {
        public List<CategoryDataModel> Categories { get; set; } = new();
    }

    private class CategoryDataModel
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
