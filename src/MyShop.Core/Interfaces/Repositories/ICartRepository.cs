using MyShop.Shared.Models;
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
    Task<IEnumerable<CartItem>> GetCartItemsAsync(Guid userId);

    /// <summary>
    /// Add product to cart or update quantity if already exists
    /// </summary>
    Task<bool> AddToCartAsync(Guid userId, Guid productId, int quantity = 1);

    /// <summary>
    /// Update quantity of cart item
    /// </summary>
    Task<bool> UpdateQuantityAsync(Guid userId, Guid productId, int quantity);

    /// <summary>
    /// Remove item from cart
    /// </summary>
    Task<bool> RemoveFromCartAsync(Guid userId, Guid productId);

    /// <summary>
    /// Clear all items from cart
    /// </summary>
    Task<bool> ClearCartAsync(Guid userId);

    /// <summary>
    /// Get total count of items in cart
    /// </summary>
    Task<int> GetCartCountAsync(Guid userId);

    /// <summary>
    /// Get cart summary (total amount, count, etc.)
    /// </summary>
    Task<CartSummary> GetCartSummaryAsync(Guid userId);
}

/// <summary>
/// Shopping cart item
/// </summary>
public class CartItem
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ProductImage { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public decimal Subtotal => Price * Quantity;
    public string? CategoryName { get; set; }
    public int StockAvailable { get; set; }
    public DateTime AddedAt { get; set; }
}

/// <summary>
/// Cart summary with totals
/// </summary>
public class CartSummary
{
    public int ItemCount { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Tax { get; set; }
    public decimal ShippingFee { get; set; }
    public decimal Total { get; set; }
}
