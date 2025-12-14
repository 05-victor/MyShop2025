using MyShop.Shared.DTOs.Responses;

namespace MyShop.Server.Services.Interfaces;

/// <summary>
/// Service interface for cart operations
/// </summary>
public interface ICartService
{
    /// <summary>
    /// Get current user's cart
    /// </summary>
    Task<CartResponse> GetMyCartAsync();

    /// <summary>
    /// Add an item to the cart
    /// </summary>
    Task<CartResponse> AddToCartAsync(Guid productId, int quantity);

    /// <summary>
    /// Update cart item quantity
    /// </summary>
    Task<CartResponse> UpdateCartItemQuantityAsync(Guid productId, int quantity);

    /// <summary>
    /// Remove an item from the cart
    /// </summary>
    Task<bool> RemoveFromCartAsync(Guid productId);

    /// <summary>
    /// Clear the entire cart
    /// </summary>
    Task<bool> ClearCartAsync();
}
