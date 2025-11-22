using MyShop.Core.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyShop.Plugins.Repositories.Mocks;

/// <summary>
/// Mock implementation for shopping cart using in-memory storage
/// </summary>
public class MockCartRepository : ICartRepository
{
    // In-memory storage: UserId -> List of CartItems
    private static readonly Dictionary<Guid, List<CartItem>> _carts = new();
    private readonly IProductRepository _productRepository;

    public MockCartRepository(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public Task<IEnumerable<CartItem>> GetCartItemsAsync(Guid userId)
    {
        if (!_carts.ContainsKey(userId))
        {
            _carts[userId] = new List<CartItem>();
        }

        return Task.FromResult(_carts[userId].AsEnumerable());
    }

    public async Task<bool> AddToCartAsync(Guid userId, Guid productId, int quantity = 1)
    {
        try
        {
            if (!_carts.ContainsKey(userId))
            {
                _carts[userId] = new List<CartItem>();
            }

            var cart = _carts[userId];

            // Get product details
            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null)
            {
                System.Diagnostics.Debug.WriteLine($"[MockCart] Product {productId} not found");
                return false;
            }

            // Check stock availability
            if (product.Quantity < quantity)
            {
                System.Diagnostics.Debug.WriteLine($"[MockCart] Insufficient stock for {product.Name}");
                return false;
            }

            // Check if product already in cart
            var existingItem = cart.FirstOrDefault(c => c.ProductId == productId);
            
            if (existingItem != null)
            {
                // Update quantity
                var newQuantity = existingItem.Quantity + quantity;
                
                // Check total quantity against stock
                if (newQuantity > product.Quantity)
                {
                    System.Diagnostics.Debug.WriteLine($"[MockCart] Total quantity exceeds stock");
                    return false;
                }

                existingItem.Quantity = newQuantity;
                System.Diagnostics.Debug.WriteLine($"[MockCart] Updated {product.Name} quantity to {newQuantity}");
            }
            else
            {
                // Add new item
                var cartItem = new CartItem
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    ProductId = productId,
                    ProductName = product.Name,
                    ProductImage = product.ImageUrl,
                    Price = product.SellingPrice,
                    Quantity = quantity,
                    CategoryName = product.CategoryName ?? product.Category,
                    StockAvailable = product.Quantity,
                    AddedAt = DateTime.Now
                };

                cart.Add(cartItem);
                System.Diagnostics.Debug.WriteLine($"[MockCart] Added {product.Name} to cart");
            }

            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockCart] Error adding to cart: {ex.Message}");
            return false;
        }
    }

    public Task<bool> UpdateQuantityAsync(Guid userId, Guid productId, int quantity)
    {
        try
        {
            if (!_carts.ContainsKey(userId))
            {
                return Task.FromResult(false);
            }

            var cart = _carts[userId];
            var item = cart.FirstOrDefault(c => c.ProductId == productId);

            if (item == null)
            {
                return Task.FromResult(false);
            }

            if (quantity <= 0)
            {
                cart.Remove(item);
                System.Diagnostics.Debug.WriteLine($"[MockCart] Removed {item.ProductName} from cart");
                return Task.FromResult(true);
            }

            if (quantity > item.StockAvailable)
            {
                System.Diagnostics.Debug.WriteLine($"[MockCart] Quantity exceeds stock");
                return Task.FromResult(false);
            }

            item.Quantity = quantity;
            System.Diagnostics.Debug.WriteLine($"[MockCart] Updated {item.ProductName} quantity to {quantity}");
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockCart] Error updating quantity: {ex.Message}");
            return Task.FromResult(false);
        }
    }

    public Task<bool> RemoveFromCartAsync(Guid userId, Guid productId)
    {
        try
        {
            if (!_carts.ContainsKey(userId))
            {
                return Task.FromResult(false);
            }

            var cart = _carts[userId];
            var item = cart.FirstOrDefault(c => c.ProductId == productId);

            if (item == null)
            {
                return Task.FromResult(false);
            }

            cart.Remove(item);
            System.Diagnostics.Debug.WriteLine($"[MockCart] Removed {item.ProductName} from cart");
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockCart] Error removing from cart: {ex.Message}");
            return Task.FromResult(false);
        }
    }

    public Task<bool> ClearCartAsync(Guid userId)
    {
        try
        {
            if (_carts.ContainsKey(userId))
            {
                _carts[userId].Clear();
                System.Diagnostics.Debug.WriteLine($"[MockCart] Cleared cart for user {userId}");
            }
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockCart] Error clearing cart: {ex.Message}");
            return Task.FromResult(false);
        }
    }

    public Task<int> GetCartCountAsync(Guid userId)
    {
        if (!_carts.ContainsKey(userId))
        {
            return Task.FromResult(0);
        }

        var totalCount = _carts[userId].Sum(item => item.Quantity);
        return Task.FromResult(totalCount);
    }

    public Task<CartSummary> GetCartSummaryAsync(Guid userId)
    {
        var summary = new CartSummary();

        if (!_carts.ContainsKey(userId))
        {
            return Task.FromResult(summary);
        }

        var cart = _carts[userId];
        
        summary.ItemCount = cart.Sum(item => item.Quantity);
        summary.Subtotal = cart.Sum(item => item.Subtotal);
        
        // Calculate tax (10% VAT)
        summary.Tax = Math.Round(summary.Subtotal * 0.10m, 0);
        
        // Calculate shipping fee (free if > 5,000,000 VND)
        summary.ShippingFee = summary.Subtotal > 5000000 ? 0 : 50000;
        
        summary.Total = summary.Subtotal + summary.Tax + summary.ShippingFee;

        return Task.FromResult(summary);
    }
}
