using MyShop.Core.Interfaces.Repositories;
using MyShop.Plugins.Mocks.Data;
using MyShop.Shared.Models;

namespace MyShop.Plugins.Repositories.Mocks;

/// <summary>
/// Mock implementation for shopping cart - delegates to MockCartData
/// </summary>
public class MockCartRepository : ICartRepository
{
    private readonly IProductRepository _productRepository;

    public MockCartRepository(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<IEnumerable<CartItem>> GetCartItemsAsync(Guid userId)
    {
        try
        {
            var items = await MockCartData.GetCartItemsAsync(userId);
            System.Diagnostics.Debug.WriteLine($"[MockCartRepository] Got {items.Count} items for user {userId}");
            return items;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockCartRepository] GetCartItemsAsync error: {ex.Message}");
            return new List<CartItem>();
        }
    }

    public async Task<bool> AddToCartAsync(Guid userId, Guid productId, int quantity = 1)
    {
        try
        {
            // Get product details for validation
            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null)
            {
                System.Diagnostics.Debug.WriteLine($"[MockCartRepository] Product {productId} not found");
                return false;
            }

            // Check stock
            if (product.Quantity < quantity)
            {
                System.Diagnostics.Debug.WriteLine($"[MockCartRepository] Insufficient stock for {product.Name}");
                return false;
            }

            var cartItem = await MockCartData.AddToCartAsync(userId, productId, product.Name, product.SellingPrice, quantity);
            System.Diagnostics.Debug.WriteLine($"[MockCartRepository] AddToCart result: {cartItem != null}");
            return cartItem != null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockCartRepository] AddToCartAsync error: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> UpdateQuantityAsync(Guid userId, Guid productId, int quantity)
    {
        try
        {
            var result = await MockCartData.UpdateCartItemAsync(userId, productId, quantity);
            System.Diagnostics.Debug.WriteLine($"[MockCartRepository] UpdateQuantity result: {result}");
            return result;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockCartRepository] UpdateQuantityAsync error: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> RemoveFromCartAsync(Guid userId, Guid productId)
    {
        try
        {
            var result = await MockCartData.RemoveFromCartAsync(userId, productId);
            System.Diagnostics.Debug.WriteLine($"[MockCartRepository] RemoveFromCart result: {result}");
            return result;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockCartRepository] RemoveFromCartAsync error: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> ClearCartAsync(Guid userId)
    {
        try
        {
            var result = await MockCartData.ClearCartAsync(userId);
            System.Diagnostics.Debug.WriteLine($"[MockCartRepository] ClearCart result: {result}");
            return result;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockCartRepository] ClearCartAsync error: {ex.Message}");
            return false;
        }
    }

    public async Task<int> GetCartCountAsync(Guid userId)
    {
        try
        {
            var items = await MockCartData.GetCartItemsAsync(userId);
            var count = items.Sum(item => item.Quantity);
            return count;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockCartRepository] GetCartCountAsync error: {ex.Message}");
            return 0;
        }
    }

    public async Task<CartSummary> GetCartSummaryAsync(Guid userId)
    {
        try
        {
            var items = await MockCartData.GetCartItemsAsync(userId);
            
            var subtotal = items.Sum(item => item.Subtotal);
            var itemCount = items.Sum(item => item.Quantity);
            
            // Calculate tax (10% VAT)
            var tax = Math.Round(subtotal * 0.10m, 0);
            
            // Calculate shipping fee (free if > 5,000,000 VND)
            var shippingFee = subtotal > 5000000 ? 0 : 50000;
            
            var total = subtotal + tax + shippingFee;

            return new CartSummary
            {
                ItemCount = itemCount,
                Subtotal = subtotal,
                Tax = tax,
                ShippingFee = shippingFee,
                Total = total
            };
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockCartRepository] GetCartSummaryAsync error: {ex.Message}");
            return new CartSummary();
        }
    }
}
