using MyShop.Data.Repositories.Interfaces;
using MyShop.Server.Exceptions;
using MyShop.Server.Mappings;
using MyShop.Server.Services.Interfaces;
using MyShop.Shared.DTOs.Responses;

namespace MyShop.Server.Services.Implementations;

/// <summary>
/// Service implementation for cart operations
/// </summary>
public class CartService : ICartService
{
    private readonly ICartRepository _cartRepository;
    private readonly IProductRepository _productRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<CartService> _logger;

    public CartService(
        ICartRepository cartRepository,
        IProductRepository productRepository,
        ICurrentUserService currentUserService,
        ILogger<CartService> logger)
    {
        _cartRepository = cartRepository;
        _productRepository = productRepository;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<CartResponse> GetMyCartAsync()
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
        {
            throw new AuthenticationException("User not authenticated");
        }

        try
        {
            var cartItems = await _cartRepository.GetCartItemsByUserIdAsync(userId.Value);
            return CartMapper.ToCartResponse(cartItems, userId.Value);
        }
        catch (Exception ex) when (ex is not BaseApplicationException)
        {
            _logger.LogError(ex, "Error retrieving cart for user {UserId}", userId);
            throw InfrastructureException.DatabaseError("Failed to retrieve cart", ex);
        }
    }

    public async Task<CartResponse> AddToCartAsync(Guid productId, int quantity)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
        {
            throw new AuthenticationException("User not authenticated");
        }

        if (quantity <= 0)
        {
            throw new ValidationException("Quantity must be greater than 0");
        }

        // Verify product exists and has sufficient stock
        var product = await _productRepository.GetByIdAsync(productId);
        if (product == null)
        {
            throw NotFoundException.ForEntity("Product", productId);
        }

        // Check if adding to cart would exceed available stock
        var existingCartItem = await _cartRepository.GetCartItemAsync(userId.Value, productId);
        var totalQuantity = (existingCartItem?.Quantity ?? 0) + quantity;

        if (totalQuantity > product.Quantity)
        {
            throw new BusinessRuleException($"Insufficient stock. Only {product.Quantity} units available");
        }

        try
        {
            await _cartRepository.AddToCartAsync(userId.Value, productId, quantity);
            _logger.LogInformation("User {UserId} added {Quantity} of product {ProductId} to cart", 
                userId, quantity, productId);

            // Return updated cart
            var cartItems = await _cartRepository.GetCartItemsByUserIdAsync(userId.Value);
            return CartMapper.ToCartResponse(cartItems, userId.Value);
        }
        catch (Exception ex) when (ex is not BaseApplicationException)
        {
            _logger.LogError(ex, "Error adding product {ProductId} to cart for user {UserId}", 
                productId, userId);
            throw InfrastructureException.DatabaseError("Failed to add item to cart", ex);
        }
    }

    public async Task<CartResponse> UpdateCartItemQuantityAsync(Guid productId, int quantity)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
        {
            throw new AuthenticationException("User not authenticated");
        }

        if (quantity <= 0)
        {
            throw new ValidationException("Quantity must be greater than 0. Use remove endpoint to delete items");
        }

        // Verify cart item exists
        var cartItem = await _cartRepository.GetCartItemAsync(userId.Value, productId);
        if (cartItem == null)
        {
            throw NotFoundException.ForEntity("Cart item", productId);
        }

        // Verify product has sufficient stock
        var product = await _productRepository.GetByIdAsync(productId);
        if (product == null)
        {
            throw NotFoundException.ForEntity("Product", productId);
        }

        if (quantity > product.Quantity)
        {
            throw new BusinessRuleException($"Insufficient stock. Only {product.Quantity} units available");
        }

        try
        {
            await _cartRepository.UpdateCartItemQuantityAsync(userId.Value, productId, quantity);
            _logger.LogInformation("User {UserId} updated cart item {ProductId} to quantity {Quantity}", 
                userId, productId, quantity);

            // Return updated cart
            var cartItems = await _cartRepository.GetCartItemsByUserIdAsync(userId.Value);
            return CartMapper.ToCartResponse(cartItems, userId.Value);
        }
        catch (Exception ex) when (ex is not BaseApplicationException)
        {
            _logger.LogError(ex, "Error updating cart item {ProductId} for user {UserId}", 
                productId, userId);
            throw InfrastructureException.DatabaseError("Failed to update cart item", ex);
        }
    }

    public async Task<bool> RemoveFromCartAsync(Guid productId)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
        {
            throw new AuthenticationException("User not authenticated");
        }

        try
        {
            var result = await _cartRepository.RemoveFromCartAsync(userId.Value, productId);
            
            if (result)
            {
                _logger.LogInformation("User {UserId} removed product {ProductId} from cart", 
                    userId, productId);
            }
            else
            {
                _logger.LogWarning("Cart item not found for user {UserId} and product {ProductId}", 
                    userId, productId);
            }

            return result;
        }
        catch (Exception ex) when (ex is not BaseApplicationException)
        {
            _logger.LogError(ex, "Error removing product {ProductId} from cart for user {UserId}", 
                productId, userId);
            throw InfrastructureException.DatabaseError("Failed to remove item from cart", ex);
        }
    }

    public async Task<bool> ClearCartAsync()
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
        {
            throw new AuthenticationException("User not authenticated");
        }

        try
        {
            var result = await _cartRepository.ClearCartAsync(userId.Value);
            
            if (result)
            {
                _logger.LogInformation("User {UserId} cleared their cart", userId);
            }

            return result;
        }
        catch (Exception ex) when (ex is not BaseApplicationException)
        {
            _logger.LogError(ex, "Error clearing cart for user {UserId}", userId);
            throw InfrastructureException.DatabaseError("Failed to clear cart", ex);
        }
    }
}
