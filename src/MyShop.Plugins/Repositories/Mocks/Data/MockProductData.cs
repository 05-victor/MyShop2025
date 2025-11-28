using MyShop.Shared.Models;
using System.Text.Json;

namespace MyShop.Plugins.Mocks.Data;

/// <summary>
/// Mock data provider for products - loads from JSON file
/// </summary>
public static class MockProductData
{
    private static List<ProductDataModel>? _products;
    private static readonly object _lock = new object();
    private static readonly string _jsonFilePath = Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory, "Mocks", "Data", "Json", "products.json");

    private static void EnsureDataLoaded()
    {
        if (_products != null) return;

        lock (_lock)
        {
            if (_products != null) return;

            try
            {
                if (!File.Exists(_jsonFilePath))
                {
                    System.Diagnostics.Debug.WriteLine($"Products JSON file not found at: {_jsonFilePath}");
                    InitializeDefaultData();
                    return;
                }

                var jsonString = File.ReadAllText(_jsonFilePath);
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var data = JsonSerializer.Deserialize<ProductDataContainer>(jsonString, options);

                if (data?.Products != null)
                {
                    _products = data.Products;
                    System.Diagnostics.Debug.WriteLine($"Loaded {_products.Count} products from products.json");
                }
                else
                {
                    InitializeDefaultData();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading products.json: {ex.Message}");
                InitializeDefaultData();
            }
        }
    }

    private static void InitializeDefaultData()
    {
        // Initialize empty list - data should be loaded from products.json
        _products = new List<ProductDataModel>();
        System.Diagnostics.Debug.WriteLine("[MockProductData] JSON file not found - initialized with empty product list");
    }

    public static async Task<List<Product>> GetAllAsync()
    {
        EnsureDataLoaded();

        // Simulate network delay
        await Task.Delay(300);

        return _products!.Select(p => new Product
        {
            Id = Guid.Parse(p.Id),
            SKU = p.SKU,
            Name = p.Name,
            Manufacturer = p.Manufacturer,
            DeviceType = p.DeviceType,
            ImportPrice = p.ImportPrice,
            SellingPrice = p.SellingPrice,
            Quantity = p.Quantity,
            CommissionRate = p.CommissionRate,
            Rating = p.Rating,
            RatingCount = p.RatingCount,
            Status = p.Status,
            Description = p.Description,
            ImageUrl = p.ImageUrl,
            CategoryId = !string.IsNullOrEmpty(p.CategoryId) ? Guid.Parse(p.CategoryId) : null,
            CategoryName = p.DeviceType, // Map DeviceType to CategoryName for display
            Category = p.DeviceType, // Also set Category for backward compatibility
            CreatedAt = p.CreatedAt,
            UpdatedAt = p.UpdatedAt
        }).ToList();
    }

    public static async Task<Product?> GetByIdAsync(Guid id)
    {
        EnsureDataLoaded();

        // Simulate network delay
        await Task.Delay(200);

        var productData = _products!.FirstOrDefault(p => p.Id == id.ToString());
        if (productData == null) return null;

        return new Product
        {
            Id = Guid.Parse(productData.Id),
            SKU = productData.SKU,
            Name = productData.Name,
            Manufacturer = productData.Manufacturer,
            DeviceType = productData.DeviceType,
            ImportPrice = productData.ImportPrice,
            SellingPrice = productData.SellingPrice,
            Quantity = productData.Quantity,
            CommissionRate = productData.CommissionRate,
            Rating = productData.Rating,
            RatingCount = productData.RatingCount,
            Status = productData.Status,
            Description = productData.Description,
            ImageUrl = productData.ImageUrl,
            CategoryId = !string.IsNullOrEmpty(productData.CategoryId) ? Guid.Parse(productData.CategoryId) : null,
            CategoryName = productData.DeviceType, // Map DeviceType to CategoryName for display
            Category = productData.DeviceType, // Also set Category for backward compatibility
            CreatedAt = productData.CreatedAt,
            UpdatedAt = productData.UpdatedAt
        };
    }

    public static async Task<Product> CreateAsync(Product product)
    {
        EnsureDataLoaded();

        // Simulate network delay
        await Task.Delay(500);

        var newProductData = new ProductDataModel
        {
            Id = product.Id.ToString(),
            SKU = product.SKU,
            Name = product.Name,
            Manufacturer = product.Manufacturer,
            DeviceType = product.DeviceType,
            ImportPrice = (int)product.ImportPrice,
            SellingPrice = (int)product.SellingPrice,
            Quantity = product.Quantity,
            CommissionRate = product.CommissionRate,
            Status = product.Status ?? "AVAILABLE",
            Description = product.Description,
            ImageUrl = product.ImageUrl,
            CategoryId = product.CategoryId?.ToString(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null
        };

        _products!.Add(newProductData);

        // Persist to JSON
        await SaveDataToJsonAsync();

        return product;
    }

    public static async Task<Product> UpdateAsync(Product product)
    {
        EnsureDataLoaded();

        // Simulate network delay
        await Task.Delay(400);

        var existing = _products!.FirstOrDefault(p => p.Id == product.Id.ToString());
        if (existing == null)
        {
            throw new InvalidOperationException($"Product with ID {product.Id} not found");
        }

        // Update properties
        existing.SKU = product.SKU;
        existing.Name = product.Name;
        existing.Manufacturer = product.Manufacturer;
        // DeviceType stores category - use CategoryName or DeviceType from product
        existing.DeviceType = !string.IsNullOrEmpty(product.CategoryName) 
            ? product.CategoryName 
            : (!string.IsNullOrEmpty(product.DeviceType) ? product.DeviceType : existing.DeviceType);
        existing.ImportPrice = (int)product.ImportPrice;
        existing.SellingPrice = (int)product.SellingPrice;
        existing.Quantity = product.Quantity;
        existing.CommissionRate = product.CommissionRate;
        existing.Status = product.Status ?? existing.Status;
        existing.Description = product.Description;
        existing.ImageUrl = product.ImageUrl;
        existing.CategoryId = product.CategoryId?.ToString();
        existing.UpdatedAt = DateTime.UtcNow;

        // Persist to JSON
        await SaveDataToJsonAsync();

        return product;
    }

    public static async Task<bool> DeleteAsync(Guid id)
    {
        EnsureDataLoaded();

        // Simulate network delay
        await Task.Delay(300);

        var product = _products!.FirstOrDefault(p => p.Id == id.ToString());
        if (product == null) return false;

        _products.Remove(product);

        // Persist to JSON
        await SaveDataToJsonAsync();

        return true;
    }

    public static async Task<List<Product>> GetByCategoryAsync(Guid categoryId)
    {
        EnsureDataLoaded();

        // Simulate network delay
        await Task.Delay(250);

        return _products!
            .Where(p => p.CategoryId == categoryId.ToString())
            .Select(p => new Product
            {
                Id = Guid.Parse(p.Id),
                SKU = p.SKU,
                Name = p.Name,
                Manufacturer = p.Manufacturer,
                DeviceType = p.DeviceType,
                ImportPrice = p.ImportPrice,
                SellingPrice = p.SellingPrice,
                Quantity = p.Quantity,
                CommissionRate = p.CommissionRate,
                Status = p.Status,
                Description = p.Description,
                ImageUrl = p.ImageUrl,
                CategoryId = !string.IsNullOrEmpty(p.CategoryId) ? Guid.Parse(p.CategoryId) : null,
                CategoryName = p.DeviceType, // Map DeviceType to CategoryName for display
                Category = p.DeviceType, // Also set Category for backward compatibility
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            }).ToList();
    }

    public static async Task<List<Product>> SearchAsync(string query)
    {
        EnsureDataLoaded();

        // Simulate network delay
        await Task.Delay(350);

        if (string.IsNullOrWhiteSpace(query))
        {
            return await GetAllAsync();
        }

        var lowerQuery = query.ToLower();
        return _products!
            .Where(p => p.Name.ToLower().Contains(lowerQuery) ||
                       (p.SKU != null && p.SKU.ToLower().Contains(lowerQuery)) ||
                       (p.Manufacturer != null && p.Manufacturer.ToLower().Contains(lowerQuery)))
            .Select(p => new Product
            {
                Id = Guid.Parse(p.Id),
                SKU = p.SKU,
                Name = p.Name,
                Manufacturer = p.Manufacturer,
                DeviceType = p.DeviceType,
                ImportPrice = p.ImportPrice,
                SellingPrice = p.SellingPrice,
                Quantity = p.Quantity,
                CommissionRate = p.CommissionRate,
                Status = p.Status,
                Description = p.Description,
                ImageUrl = p.ImageUrl,
                CategoryId = !string.IsNullOrEmpty(p.CategoryId) ? Guid.Parse(p.CategoryId) : null,
                CategoryName = p.DeviceType, // Map DeviceType to CategoryName for display
                Category = p.DeviceType, // Also set Category for backward compatibility
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            }).ToList();
    }

    public static async Task<List<Product>> GetLowStockAsync(int threshold = 10)
    {
        EnsureDataLoaded();

        // Simulate network delay
        await Task.Delay(200);

        return _products!
            .Where(p => p.Quantity <= threshold)
            .Select(p => new Product
            {
                Id = Guid.Parse(p.Id),
                SKU = p.SKU,
                Name = p.Name,
                Manufacturer = p.Manufacturer,
                DeviceType = p.DeviceType,
                ImportPrice = p.ImportPrice,
                SellingPrice = p.SellingPrice,
                Quantity = p.Quantity,
                CommissionRate = p.CommissionRate,
                Status = p.Status,
                Description = p.Description,
                ImageUrl = p.ImageUrl,
                CategoryId = !string.IsNullOrEmpty(p.CategoryId) ? Guid.Parse(p.CategoryId) : null,
                CategoryName = p.DeviceType, // Map DeviceType to CategoryName for display
                Category = p.DeviceType, // Also set Category for backward compatibility
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            }).ToList();
    }

    /// <summary>
    /// Get paged products with server-side filtering and sorting (mock)
    /// </summary>
    public static async Task<(List<Product> Items, int TotalCount)> GetPagedAsync(
        int page = 1,
        int pageSize = 20,
        string? searchQuery = null,
        string? categoryName = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        string sortBy = "name",
        bool sortDescending = false)
    {
        EnsureDataLoaded();

        // Simulate network delay
        await Task.Delay(300);

        var query = _products!.AsEnumerable();

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(searchQuery))
        {
            var lowerQuery = searchQuery.ToLower();
            query = query.Where(p => 
                p.Name.ToLower().Contains(lowerQuery) ||
                (p.SKU != null && p.SKU.ToLower().Contains(lowerQuery)) ||
                (p.Manufacturer != null && p.Manufacturer.ToLower().Contains(lowerQuery)));
        }

        // Apply category filter
        if (!string.IsNullOrWhiteSpace(categoryName))
        {
            query = query.Where(p => p.DeviceType != null && p.DeviceType.Equals(categoryName, StringComparison.OrdinalIgnoreCase));
        }

        // Apply price filters
        if (minPrice.HasValue)
        {
            query = query.Where(p => p.SellingPrice >= minPrice.Value);
        }
        if (maxPrice.HasValue)
        {
            query = query.Where(p => p.SellingPrice <= maxPrice.Value);
        }

        // Apply sorting
        query = sortBy.ToLower() switch
        {
            "name" => sortDescending ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
            "price" => sortDescending ? query.OrderByDescending(p => p.SellingPrice) : query.OrderBy(p => p.SellingPrice),
            "rating" => sortDescending ? query.OrderByDescending(p => p.Rating) : query.OrderBy(p => p.Rating),
            "date" => sortDescending ? query.OrderByDescending(p => p.CreatedAt) : query.OrderBy(p => p.CreatedAt),
            _ => query.OrderBy(p => p.Name)
        };

        var totalCount = query.Count();

        // Apply paging
        var pagedData = query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new Product
            {
                Id = Guid.Parse(p.Id),
                SKU = p.SKU,
                Name = p.Name,
                Manufacturer = p.Manufacturer,
                DeviceType = p.DeviceType,
                ImportPrice = p.ImportPrice,
                SellingPrice = p.SellingPrice,
                Quantity = p.Quantity,
                CommissionRate = p.CommissionRate,
                Rating = p.Rating,
                RatingCount = p.RatingCount,
                Status = p.Status,
                Description = p.Description,
                ImageUrl = p.ImageUrl,
                CategoryId = !string.IsNullOrEmpty(p.CategoryId) ? Guid.Parse(p.CategoryId) : null,
                CategoryName = p.DeviceType, // Map DeviceType to CategoryName for display
                Category = p.DeviceType, // Also set Category for backward compatibility
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            })
            .ToList();

        System.Diagnostics.Debug.WriteLine($"[MockProductData] GetPagedAsync: Page {page}/{Math.Ceiling(totalCount / (double)pageSize)}, Total: {totalCount}");

        return (pagedData, totalCount);
    }

    private static async Task SaveDataToJsonAsync()
    {
        try
        {
            var container = new ProductDataContainer
            {
                Products = _products!
            };

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var jsonString = JsonSerializer.Serialize(container, options);
            await File.WriteAllTextAsync(_jsonFilePath, jsonString);

            System.Diagnostics.Debug.WriteLine("Successfully saved products data to JSON");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving products.json: {ex.Message}");
        }
    }

    // Data container classes for JSON deserialization
    private class ProductDataContainer
    {
        public List<ProductDataModel> Products { get; set; } = new();
    }

    private class ProductDataModel
    {
        public string Id { get; set; } = string.Empty;
        public string? SKU { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Manufacturer { get; set; }
        public string? DeviceType { get; set; }
        public int ImportPrice { get; set; }
        public int SellingPrice { get; set; }
        public int Quantity { get; set; }
        public double CommissionRate { get; set; }
        public double Rating { get; set; }
        public int RatingCount { get; set; }
        public string Status { get; set; } = "AVAILABLE";
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public string? CategoryId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
