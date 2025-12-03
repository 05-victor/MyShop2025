using MyShop.Shared.Models;
using System.Text.Json;

namespace MyShop.Plugins.Mocks.Data;

/// <summary>
/// Mock data provider for cart - stores in-memory cart data
/// </summary>
public static class MockCartData
{
    private static Dictionary<string, List<CartItemDataModel>>? _carts;
    private static readonly object _lock = new object();

    private static void EnsureDataLoaded()
    {
        if (_carts != null) return;

        lock (_lock)
        {
            if (_carts != null) return;
            _carts = new Dictionary<string, List<CartItemDataModel>>();
            System.Diagnostics.Debug.WriteLine("Initialized empty cart storage");
        }
    }

    public static async Task<List<CartItem>> GetCartItemsAsync(Guid userId)
    {
        EnsureDataLoaded();

        // Simulate network delay
        // await Task.Delay(100);

        var userKey = userId.ToString();
        System.Diagnostics.Debug.WriteLine($"[MockCartData] GetCartItems: userId={userKey}");
        
        if (!_carts!.ContainsKey(userKey))
        {
            System.Diagnostics.Debug.WriteLine($"[MockCartData] No cart found for user {userKey}");
            return new List<CartItem>();
        }

        var items = _carts[userKey].Select(MapToCartItem).ToList();
        System.Diagnostics.Debug.WriteLine($"[MockCartData] Found {items.Count} items in cart for user {userKey}");
        return items;
    }

    public static async Task<CartItem> AddToCartAsync(Guid userId, Guid productId, string productName, decimal price, int quantity)
    {
        EnsureDataLoaded();

        // Simulate network delay
        // await Task.Delay(100);

        var userKey = userId.ToString();
        System.Diagnostics.Debug.WriteLine($"[MockCartData] AddToCart: userId={userKey}, productId={productId}, name={productName}, qty={quantity}");
        
        if (!_carts!.ContainsKey(userKey))
        {
            _carts[userKey] = new List<CartItemDataModel>();
            System.Diagnostics.Debug.WriteLine($"[MockCartData] Created new cart for user {userKey}");
        }

        var existingItem = _carts[userKey].FirstOrDefault(i => i.ProductId == productId.ToString());
        
        if (existingItem != null)
        {
            // Update quantity
            existingItem.Quantity += quantity;
            existingItem.UpdatedAt = DateTime.UtcNow;
            System.Diagnostics.Debug.WriteLine($"[MockCartData] Updated existing item, new qty={existingItem.Quantity}");
        }
        else
        {
            // Add new item
            var newItem = new CartItemDataModel
            {
                Id = Guid.NewGuid().ToString(),
                UserId = userKey,
                ProductId = productId.ToString(),
                ProductName = productName,
                Price = price,
                Quantity = quantity,
                CreatedAt = DateTime.UtcNow
            };
            _carts[userKey].Add(newItem);
            existingItem = newItem;
            System.Diagnostics.Debug.WriteLine($"[MockCartData] Added new item to cart, total items={_carts[userKey].Count}");
        }

        return MapToCartItem(existingItem);
    }

    public static async Task<bool> UpdateCartItemAsync(Guid userId, Guid cartItemId, int quantity)
    {
        EnsureDataLoaded();

        // Simulate network delay
        // await Task.Delay(250);

        var userKey = userId.ToString();
        if (!_carts!.ContainsKey(userKey))
        {
            return false;
        }

        var item = _carts[userKey].FirstOrDefault(i => i.Id == cartItemId.ToString());
        if (item == null) return false;

        item.Quantity = quantity;
        item.UpdatedAt = DateTime.UtcNow;
        return true;
    }

    public static async Task<bool> RemoveFromCartAsync(Guid userId, Guid cartItemId)
    {
        EnsureDataLoaded();

        // Simulate network delay
        // await Task.Delay(250);

        var userKey = userId.ToString();
        if (!_carts!.ContainsKey(userKey))
        {
            return false;
        }

        var item = _carts[userKey].FirstOrDefault(i => i.Id == cartItemId.ToString());
        if (item == null) return false;

        _carts[userKey].Remove(item);
        return true;
    }

    public static async Task<bool> ClearCartAsync(Guid userId)
    {
        EnsureDataLoaded();

        // Simulate network delay
        // await Task.Delay(200);

        var userKey = userId.ToString();
        if (!_carts!.ContainsKey(userKey))
        {
            return true;
        }

        _carts[userKey].Clear();
        return true;
    }

    private static CartItem MapToCartItem(CartItemDataModel data)
    {
        return new CartItem
        {
            Id = Guid.Parse(data.Id),
            UserId = Guid.Parse(data.UserId),
            ProductId = Guid.Parse(data.ProductId),
            ProductName = data.ProductName,
            Price = data.Price,
            Quantity = data.Quantity,
            CreatedAt = data.CreatedAt,
            UpdatedAt = data.UpdatedAt
        };
    }

    // Data model for cart items
    private class CartItemDataModel
    {
        public string Id { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string ProductId { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
