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
    /// Checkout cart - create order from cart items.
    /// Orchestrates: Validation → Check all stock → Create order → Clear cart → Navigate
    /// </summary>
    Task<Result<Order>> CheckoutAsync(string shippingAddress, string notes);
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
