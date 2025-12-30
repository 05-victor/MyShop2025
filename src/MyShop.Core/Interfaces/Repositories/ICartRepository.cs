using MyShop.Shared.Models;
using MyShop.Core.Common;
using MyShop.Shared.DTOs.Responses;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyShop.Core.Interfaces.Repositories;

/// <summary>
/// Repository interface for shopping cart management
/// </summary>
public interface ICartRepository
{
    /// <summary>
    /// Get cart items for current user
    /// </summary>
    Task<Result<IEnumerable<CartItem>>> GetCartItemsAsync(Guid userId);

    /// <summary>
    /// Add product to cart or update quantity if already exists
    /// </summary>
    Task<Result<bool>> AddToCartAsync(Guid userId, Guid productId, int quantity = 1);

    /// <summary>
    /// Update quantity of cart item
    /// </summary>
    Task<Result<bool>> UpdateQuantityAsync(Guid userId, Guid productId, int quantity);

    /// <summary>
    /// Remove item from cart
    /// </summary>
    Task<Result<bool>> RemoveFromCartAsync(Guid userId, Guid productId);

    /// <summary>
    /// Clear all items from cart
    /// </summary>
    Task<Result<bool>> ClearCartAsync(Guid userId);

    /// <summary>
    /// Get total count of items in cart
    /// </summary>
    Task<Result<int>> GetCartCountAsync(Guid userId);

    /// <summary>
    /// Get cart items grouped by sales agents for current user
    /// </summary>
    Task<Result<GroupedCartResponse>> GetCartItemsGroupedAsync(Guid userId);

    /// <summary>
    /// Get cart summary (total amount, count, etc.)
    /// </summary>
    Task<Result<CartSummary>> GetCartSummaryAsync(Guid userId);

    /// <summary>
    /// Checkout cart items for a specific sales agent
    /// </summary>
    Task<Result<Order>> CheckoutBySalesAgentAsync(MyShop.Shared.DTOs.Requests.CheckoutBySalesAgentRequest request);
}
