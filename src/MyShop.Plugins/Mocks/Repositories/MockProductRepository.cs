using MyShop.Core.Common;
using MyShop.Shared.Models;
using MyShop.Core.Interfaces.Repositories;
using MyShop.Shared.DTOs.Responses;
using System.Text.Json;

namespace MyShop.Plugins.Mocks.Repositories;

/// <summary>
/// Mock implementation of IProductRepository using JSON data
/// </summary>
public class MockProductRepository : IProductRepository
{
    private readonly List<Product> _products;
    private readonly string _jsonFilePath;

    public MockProductRepository()
    {
        _jsonFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Mocks", "Data", "Json", "products.json");
        _products = LoadProductsFromJson();
    }

    private List<Product> LoadProductsFromJson()
    {
        try
        {
            if (!File.Exists(_jsonFilePath))
            {
                System.Diagnostics.Debug.WriteLine($"[MockProductRepository] JSON file not found: {_jsonFilePath}");
                return new List<Product>();
            }

            var json = File.ReadAllText(_jsonFilePath);
            var jsonDoc = JsonDocument.Parse(json);
            var productsArray = jsonDoc.RootElement.GetProperty("products");

            var products = new List<Product>();

            foreach (var item in productsArray.EnumerateArray())
            {
                var product = new Product
                {
                    Id = Guid.Parse(item.GetProperty("id").GetString()!),
                    SKU = item.GetProperty("sku").GetString(),
                    Name = item.GetProperty("name").GetString(),
                    Manufacturer = item.GetProperty("manufacturer").GetString(),
                    DeviceType = item.GetProperty("deviceType").GetString(),
                    ImportPrice = item.GetProperty("importPrice").GetInt32(),
                    SellingPrice = item.GetProperty("sellingPrice").GetInt32(),
                    Quantity = item.GetProperty("quantity").GetInt32(),
                    CommissionRate = item.GetProperty("commissionRate").GetDouble(),
                    Status = item.GetProperty("status").GetString(),
                    Description = item.GetProperty("description").GetString(),
                    ImageUrl = item.GetProperty("imageUrl").GetString(),
                    CreatedAt = DateTime.Parse(item.GetProperty("createdAt").GetString()!),
                    UpdatedAt = item.TryGetProperty("updatedAt", out var updatedAt) && updatedAt.ValueKind != JsonValueKind.Null
                        ? DateTime.Parse(updatedAt.GetString()!)
                        : null
                };

                // Load CategoryId if available
                if (item.TryGetProperty("categoryId", out var categoryId))
                {
                    product.CategoryId = Guid.Parse(categoryId.GetString()!);
                }

                products.Add(product);
            }

            System.Diagnostics.Debug.WriteLine($"[MockProductRepository] Loaded {products.Count} products from JSON");
            return products;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockProductRepository] Error loading JSON: {ex.Message}");
            return new List<Product>();
        }
    }

    public async Task<IEnumerable<Product>> GetAllAsync()
    {
        await Task.Delay(300); // Simulate network delay
        System.Diagnostics.Debug.WriteLine($"[MockProductRepository] GetAllAsync called, returning {_products.Count} products");
        return _products.ToList();
    }

    public async Task<Product?> GetByIdAsync(Guid id)
    {
        await Task.Delay(200);
        var product = _products.FirstOrDefault(p => p.Id == id);
        System.Diagnostics.Debug.WriteLine($"[MockProductRepository] GetByIdAsync({id}) - Found: {product != null}");
        return product;
    }

    public async Task<Product> CreateAsync(Product product)
    {
        await Task.Delay(500);
        
        product.Id = Guid.NewGuid();
        product.CreatedAt = DateTime.UtcNow;
        product.UpdatedAt = null;
        
        _products.Add(product);
        
        System.Diagnostics.Debug.WriteLine($"[MockProductRepository] Created product: {product.Name} (ID: {product.Id})");
        return product;
    }

    public async Task<Product> UpdateAsync(Product product)
    {
        await Task.Delay(400);
        
        var existingProduct = _products.FirstOrDefault(p => p.Id == product.Id);
        if (existingProduct == null)
        {
            throw new InvalidOperationException($"Product with ID {product.Id} not found");
        }

        // Update properties
        existingProduct.SKU = product.SKU;
        existingProduct.Name = product.Name;
        existingProduct.Manufacturer = product.Manufacturer;
        existingProduct.DeviceType = product.DeviceType;
        existingProduct.ImportPrice = product.ImportPrice;
        existingProduct.SellingPrice = product.SellingPrice;
        existingProduct.Quantity = product.Quantity;
        existingProduct.CommissionRate = product.CommissionRate;
        existingProduct.Status = product.Status;
        existingProduct.Description = product.Description;
        existingProduct.ImageUrl = product.ImageUrl;
        existingProduct.UpdatedAt = DateTime.UtcNow;

        System.Diagnostics.Debug.WriteLine($"[MockProductRepository] Updated product: {existingProduct.Name}");
        return existingProduct;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        await Task.Delay(300);
        
        var product = _products.FirstOrDefault(p => p.Id == id);
        if (product == null)
        {
            return false;
        }

        _products.Remove(product);
        System.Diagnostics.Debug.WriteLine($"[MockProductRepository] Deleted product: {product.Name}");
        return true;
    }

    /// <summary>
    /// Get products with low stock (quantity less than threshold)
    /// </summary>
    public async Task<IEnumerable<Product>> GetLowStockAsync(int threshold = 10)
    {
        await Task.Delay(250);
        var lowStockProducts = _products.Where(p => p.Quantity < threshold).ToList();
        System.Diagnostics.Debug.WriteLine($"[MockProductRepository] Found {lowStockProducts.Count} low stock products (< {threshold})");
        return lowStockProducts;
    }

    /// <summary>
    /// Get products by category ID
    /// </summary>
    public async Task<IEnumerable<Product>> GetByCategoryAsync(Guid categoryId)
    {
        await Task.Delay(250);
        var products = _products.Where(p => p.CategoryId == categoryId).ToList();
        System.Diagnostics.Debug.WriteLine($"[MockProductRepository] Found {products.Count} products in category ID: {categoryId}");
        return products;
    }

    /// <summary>
    /// Search products by name or manufacturer
    /// </summary>
    public async Task<IEnumerable<Product>> SearchAsync(string query)
    {
        await Task.Delay(200);
        var results = _products.Where(p => 
            (p.Name?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false) ||
            (p.Manufacturer?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false) ||
            (p.DeviceType?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false)
        ).ToList();
        
        System.Diagnostics.Debug.WriteLine($"[MockProductRepository] Search '{query}' returned {results.Count} results");
        return results;
    }
}
