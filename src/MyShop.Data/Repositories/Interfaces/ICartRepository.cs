using MyShop.Data.Entities;

namespace MyShop.Data.Repositories.Interfaces;

/// <summary>
/// Repository interface for cart operations
/// </summary>
public interface ICartRepository
{
    /// <summary>
    /// Get all cart items for a user
    /// </summary>
    Task<IEnumerable<CartItem>> GetCartItemsByUserIdAsync(Guid userId);

    /// <summary>
    /// Get a specific cart item
    /// </summary>
    Task<CartItem?> GetCartItemAsync(Guid userId, Guid productId);

    /// <summary>
    /// Add an item to the cart
    /// </summary>
    Task<CartItem> AddToCartAsync(Guid userId, Guid productId, int quantity);

    /// <summary>
    /// Update cart item quantity
    /// </summary>
    Task<CartItem> UpdateCartItemQuantityAsync(Guid userId, Guid productId, int quantity);

    /// <summary>
    /// Remove an item from the cart
    /// </summary>
    Task<bool> RemoveFromCartAsync(Guid userId, Guid productId);

    /// <summary>
    /// Clear all cart items for a user
    /// </summary>
    Task<bool> ClearCartAsync(Guid userId);
}
