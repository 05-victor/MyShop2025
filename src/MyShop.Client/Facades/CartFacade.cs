using MyShop.Core.Common;
using MyShop.Core.Interfaces.Facades;
using MyShop.Core.Interfaces.Repositories;
using MyShop.Core.Interfaces.Services;
using MyShop.Shared.Models;
using MyShop.Shared.DTOs.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyShop.Client.Facades;

/// <summary>
/// Implementation of ICartFacade - shopping cart operations
/// </summary>
public class CartFacade : ICartFacade
{
    private readonly ICartRepository _cartRepository;
    private readonly IProductRepository _productRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IAuthRepository _authRepository;
    private readonly IValidationService _validationService;
    private readonly IToastService _toastService;

    public CartFacade(
        ICartRepository cartRepository,
        IProductRepository productRepository,
        IOrderRepository orderRepository,
        IAuthRepository authRepository,
        IValidationService validationService,
        IToastService toastService)
    {
        _cartRepository = cartRepository ?? throw new ArgumentNullException(nameof(cartRepository));
        _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        _authRepository = authRepository ?? throw new ArgumentNullException(nameof(authRepository));
        _validationService = validationService ?? throw new ArgumentNullException(nameof(validationService));
        _toastService = toastService ?? throw new ArgumentNullException(nameof(toastService));
    }

    /// <inheritdoc/>
    public async Task<Result<List<CartItem>>> LoadCartAsync()
    {
        try
        {
            var userIdResult = await _authRepository.GetCurrentUserIdAsync();
            if (!userIdResult.IsSuccess)
            {
                return Result<List<CartItem>>.Failure(userIdResult.ErrorMessage ?? "User not authenticated");
            }

            var result = await _cartRepository.GetCartItemsAsync(userIdResult.Data);
            if (!result.IsSuccess || result.Data == null)
            {
                return Result<List<CartItem>>.Failure(result.ErrorMessage ?? "Failed to load cart");
            }

            return Result<List<CartItem>>.Success(result.Data.ToList());
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[CartFacade] LoadCartAsync failed: {ex.Message}");
            return Result<List<CartItem>>.Failure("Failed to load cart", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<Result<Core.Interfaces.Facades.CartSummary>> GetCartSummaryAsync()
    {
        try
        {
            var cartResult = await LoadCartAsync();
            if (!cartResult.IsSuccess || cartResult.Data == null)
            {
                return Result<Core.Interfaces.Facades.CartSummary>.Failure("Failed to load cart");
            }

            var items = cartResult.Data;
            var subtotal = items.Sum(i => i.Price * i.Quantity);
            var tax = subtotal * 0.1m; // 10% tax
            var shippingFee = subtotal > 500000 ? 0 : 30000; // Free shipping over 500k
            var total = subtotal + tax + shippingFee;

            var summary = new Core.Interfaces.Facades.CartSummary
            {
                TotalItems = items.Sum(i => i.Quantity),
                Subtotal = subtotal,
                Tax = tax,
                ShippingFee = shippingFee,
                Total = total
            };

            return Result<Core.Interfaces.Facades.CartSummary>.Success(summary);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[CartFacade] GetCartSummaryAsync failed: {ex.Message}");
            return Result<Core.Interfaces.Facades.CartSummary>.Failure("Failed to calculate cart summary", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<Result<Unit>> AddToCartAsync(Guid productId, int quantity = 1)
    {
        try
        {
            // Validate quantity
            if (quantity < 1)
            {
                return Result<Unit>.Failure("Quantity must be at least 1");
            }

            var userIdResult = await _authRepository.GetCurrentUserIdAsync();
            if (!userIdResult.IsSuccess)
            {
                return Result<Unit>.Failure(userIdResult.ErrorMessage ?? "User not authenticated");
            }

            // Check product exists and has stock
            var productResult = await _productRepository.GetByIdAsync(productId);
            if (!productResult.IsSuccess || productResult.Data == null)
            {
                return Result<Unit>.Failure("Product not found");
            }

            var product = productResult.Data;
            if (product.Quantity < quantity)
            {
                return Result<Unit>.Failure($"Insufficient stock. Only {product.Quantity} units available");
            }

            // Add to cart
            var result = await _cartRepository.AddToCartAsync(userIdResult.Data, productId, quantity);
            if (!result.IsSuccess)
            {
                return Result<Unit>.Failure(result.ErrorMessage ?? "Failed to add to cart");
            }

            await _toastService.ShowSuccess($"Added {quantity} x {product.Name} to cart");
            return Result<Unit>.Success(Unit.Value);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[CartFacade] AddToCartAsync failed: {ex.Message}");
            return Result<Unit>.Failure("Failed to add to cart", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<Result<Unit>> UpdateCartItemQuantityAsync(Guid cartItemId, int newQuantity)
    {
        try
        {
            // Validate quantity
            if (newQuantity < 1)
            {
                return Result<Unit>.Failure("Quantity must be at least 1");
            }

            var userIdResult = await _authRepository.GetCurrentUserIdAsync();
            if (!userIdResult.IsSuccess)
            {
                return Result<Unit>.Failure(userIdResult.ErrorMessage ?? "User not authenticated");
            }

            // Get cart item to check stock
            var cartResult = await LoadCartAsync();
            if (!cartResult.IsSuccess || cartResult.Data == null)
            {
                return Result<Unit>.Failure("Failed to load cart");
            }

            var cartItem = cartResult.Data.FirstOrDefault(i => i.Id == cartItemId);
            if (cartItem == null)
            {
                return Result<Unit>.Failure("Cart item not found");
            }

            // Check stock availability
            if (cartItem.StockAvailable < newQuantity)
            {
                return Result<Unit>.Failure($"Insufficient stock. Only {cartItem.StockAvailable} units available");
            }

            // Update quantity using UpdateQuantityAsync (productId, not cartItemId)
            var result = await _cartRepository.UpdateQuantityAsync(userIdResult.Data, cartItem.ProductId, newQuantity);
            if (!result.IsSuccess)
            {
                return Result<Unit>.Failure(result.ErrorMessage ?? "Failed to update cart");
            }

            await _toastService.ShowSuccess("Cart updated");
            return Result<Unit>.Success(Unit.Value);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[CartFacade] UpdateCartItemQuantityAsync failed: {ex.Message}");
            return Result<Unit>.Failure("Failed to update cart", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<Result<Unit>> RemoveFromCartAsync(Guid cartItemId)
    {
        try
        {
            var userIdResult = await _authRepository.GetCurrentUserIdAsync();
            if (!userIdResult.IsSuccess)
            {
                return Result<Unit>.Failure(userIdResult.ErrorMessage ?? "User not authenticated");
            }

            // Need to get productId from cartItemId first
            var cartResult = await LoadCartAsync();
            if (!cartResult.IsSuccess || cartResult.Data == null)
            {
                return Result<Unit>.Failure("Failed to load cart");
            }

            var cartItem = cartResult.Data.FirstOrDefault(i => i.Id == cartItemId);
            if (cartItem == null)
            {
                return Result<Unit>.Failure("Cart item not found");
            }

            var result = await _cartRepository.RemoveFromCartAsync(userIdResult.Data, cartItem.ProductId);
            if (!result.IsSuccess)
            {
                return Result<Unit>.Failure(result.ErrorMessage ?? "Failed to remove from cart");
            }

            await _toastService.ShowSuccess("Item removed from cart");
            return Result<Unit>.Success(Unit.Value);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[CartFacade] RemoveFromCartAsync failed: {ex.Message}");
            return Result<Unit>.Failure("Failed to remove from cart", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<Result<Unit>> ClearCartAsync()
    {
        try
        {
            var userIdResult = await _authRepository.GetCurrentUserIdAsync();
            if (!userIdResult.IsSuccess)
            {
                return Result<Unit>.Failure(userIdResult.ErrorMessage ?? "User not authenticated");
            }

            var result = await _cartRepository.ClearCartAsync(userIdResult.Data);
            if (!result.IsSuccess)
            {
                return Result<Unit>.Failure(result.ErrorMessage ?? "Failed to clear cart");
            }

            await _toastService.ShowInfo("Cart cleared");
            return Result<Unit>.Success(Unit.Value);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[CartFacade] ClearCartAsync failed: {ex.Message}");
            return Result<Unit>.Failure("Failed to clear cart", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<Result<Order>> CheckoutBySalesAgentAsync(Guid salesAgentId, string shippingAddress, string notes, string paymentMethod = "COD")
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"[CartFacade] CheckoutBySalesAgentAsync - SalesAgent: {salesAgentId}, PaymentMethod: {paymentMethod}");

            // Validate address
            if (string.IsNullOrWhiteSpace(shippingAddress))
            {
                return Result<Order>.Failure("Shipping address is required");
            }

            // Call backend checkout/sales-agent endpoint
            var request = new CheckoutBySalesAgentRequest
            {
                SalesAgentId = salesAgentId,
                ShippingAddress = shippingAddress,
                Note = notes,
                PaymentMethod = paymentMethod,
                DiscountAmount = 0
            };

            var result = await _cartRepository.CheckoutBySalesAgentAsync(request);

            if (result.IsSuccess && result.Data != null)
            {
                await _toastService.ShowSuccess($"Order placed successfully!");
                System.Diagnostics.Debug.WriteLine($"[CartFacade] Order created: {result.Data.OrderCode}, Total: {result.Data.FinalPrice}");
                return result;
            }

            System.Diagnostics.Debug.WriteLine($"[CartFacade] Checkout failed: {result.ErrorMessage}");
            return Result<Order>.Failure(result.ErrorMessage ?? "Failed to checkout");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[CartFacade] CheckoutBySalesAgentAsync failed: {ex.Message}");
            return Result<Order>.Failure("Failed to checkout", ex);
        }
    }
}
