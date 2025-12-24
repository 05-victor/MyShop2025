using MyShop.Core.Common;
using MyShop.Shared.Models;

namespace MyShop.Core.Interfaces.Facades;

/// <summary>
/// Facade pattern for shopping cart operations.
/// Aggregates: ICartRepository, IProductRepository, IValidationService, IToastService.
/// Handles cart management, item quantity updates, and checkout workflow.
/// </summary>
public interface ICartFacade
{
    /// <summary>
    /// Load current user's cart items.
    /// </summary>
    Task<Result<List<CartItem>>> LoadCartAsync();

    /// <summary>
    /// Load current user's cart grouped by sales agents.
    /// Used for displaying cart with commission breakdown.
    /// </summary>
    Task<Result<GroupedCart>> LoadGroupedCartAsync();

    /// <summary>
    /// Get cart summary (total items, subtotal, etc.).
    /// </summary>
    Task<Result<CartSummary>> GetCartSummaryAsync();

    /// <summary>
    /// Add product to cart with validation.
    /// Orchestrates: Validation → Check stock → Repository.AddToCart → Toast notification
    /// </summary>
    Task<Result<Unit>> AddToCartAsync(Guid productId, int quantity = 1);

    /// <summary>
    /// Update cart item quantity.
    /// Orchestrates: Validation → Check stock → Repository.Update → Recalculate total
    /// </summary>
    Task<Result<Unit>> UpdateCartItemQuantityAsync(Guid cartItemId, int newQuantity);

    /// <summary>
    /// Remove item from cart.
    /// </summary>
    Task<Result<Unit>> RemoveFromCartAsync(Guid cartItemId);

    /// <summary>
    /// Clear entire cart.
    /// </summary>
    Task<Result<Unit>> ClearCartAsync();

    /// <summary>
    /// Checkout cart for a specific sales agent - create order from cart items.
    /// Orchestrates: Validation → Check all stock → Create order → Clear cart items → Navigate
    /// </summary>
    Task<Result<Order>> CheckoutBySalesAgentAsync(Guid salesAgentId, string shippingAddress, string notes);
}

/// <summary>
/// Cart summary containing totals and item counts.
/// Used for displaying cart overview in UI.
/// </summary>
public class CartSummary
{
    public int TotalItems { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Tax { get; set; }
    public decimal ShippingFee { get; set; }
    public decimal Total { get; set; }
}

/// <summary>
/// Grouped cart by sales agents for commission tracking.
/// </summary>
public class GroupedCart
{
    public List<SalesAgentGroup> SalesAgentGroups { get; set; } = new();
    public decimal GrandTotal { get; set; }
    public int TotalItemCount { get; set; }
}

/// <summary>
/// Cart items grouped by a single sales agent.
/// </summary>
public class SalesAgentGroup
{
    public Guid SalesAgentId { get; set; }
    public string SalesAgentUsername { get; set; } = string.Empty;
    public string SalesAgentFullName { get; set; } = string.Empty;
    public List<CartItem> Items { get; set; } = new();
    public decimal Subtotal { get; set; }
    public decimal Total { get; set; }
    public int ItemCount { get; set; }
}
