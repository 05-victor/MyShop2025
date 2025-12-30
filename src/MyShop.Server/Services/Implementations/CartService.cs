using MyShop.Data.Repositories.Interfaces;
using MyShop.Server.Exceptions;
using MyShop.Server.Mappings;
using MyShop.Server.Services.Interfaces;
using MyShop.Shared.DTOs.Requests;
using MyShop.Shared.DTOs.Responses;
using MyShop.Data.Entities;

namespace MyShop.Server.Services.Implementations;

/// <summary>
/// Service implementation for cart operations
/// </summary>
public class CartService : ICartService
{
    private readonly ICartRepository _cartRepository;
    private readonly IProductRepository _productRepository;
    private readonly IOrderService _orderService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<CartService> _logger;
    private readonly CartMapper _cartMapper;

    public CartService(
        ICartRepository cartRepository,
        IProductRepository productRepository,
        IOrderService orderService,
        ICurrentUserService currentUserService,
        ILogger<CartService> logger,
        CartMapper cartMapper)
    {
        _cartRepository = cartRepository;
        _productRepository = productRepository;
        _orderService = orderService;
        _currentUserService = currentUserService;
        _logger = logger;
        _cartMapper = cartMapper;
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
            return _cartMapper.ToCartResponse(cartItems, userId.Value);
        }
        catch (Exception ex) when (ex is not BaseApplicationException)
        {
            _logger.LogError(ex, "Error retrieving cart for user {UserId}", userId);
            throw InfrastructureException.DatabaseError("Failed to retrieve cart", ex);
        }
    }

    public async Task<GroupedCartResponse> GetMyCartGroupedBySalesAgentAsync()
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
        {
            throw new AuthenticationException("User not authenticated");
        }

        try
        {
            var cartItems = await _cartRepository.GetCartItemsByUserIdAsync(userId.Value);
            return _cartMapper.ToGroupedCartResponse(cartItems, userId.Value);
        }
        catch (Exception ex) when (ex is not BaseApplicationException)
        {
            _logger.LogError(ex, "Error retrieving grouped cart for user {UserId}", userId);
            throw InfrastructureException.DatabaseError("Failed to retrieve grouped cart", ex);
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

        // Prevent agents from buying their own products
        if (product.SaleAgentId == userId.Value)
        {
            throw new BusinessRuleException("You cannot add your own products to cart");
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
            return _cartMapper.ToCartResponse(cartItems, userId.Value);
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
            return _cartMapper.ToCartResponse(cartItems, userId.Value);
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

    public async Task<OrderResponse> CheckoutBySalesAgentAsync(CheckoutBySalesAgentRequest request)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
        {
            throw new AuthenticationException("User not authenticated");
        }

        try
        {
            // Get all cart items for the user
            var allCartItems = await _cartRepository.GetCartItemsByUserIdAsync(userId.Value);
            
            // Filter cart items for the specified sales agent
            var salesAgentCartItems = allCartItems
                .Where(ci => ci.Product?.SaleAgentId == request.SalesAgentId)
                .ToList();

            if (!salesAgentCartItems.Any())
            {
                throw new ValidationException($"No cart items found for sales agent with ID {request.SalesAgentId}");
            }

            // Build request for order creation
            var createOrderRequest = new CreateOrderFromCartRequest
            {
                CustomerId = userId.Value,
                SalesAgentId = request.SalesAgentId,
                ShippingAddress = request.ShippingAddress,
                Note = request.Note,
                PaymentMethod = request.PaymentMethod,
                DiscountAmount = request.DiscountAmount,
                CartItems = salesAgentCartItems.Select(ci => new CartItemForOrder
                {
                    ProductId = ci.ProductId,
                    Quantity = ci.Quantity,
                    UnitPrice = ci.Product.SellingPrice
                }).ToList()
            };

            // Delegate order creation to OrderService
            var orderResponse = await _orderService.CreateOrderFromCartAsync(createOrderRequest);

            // Remove only the checked-out items from cart
            foreach (var cartItem in salesAgentCartItems)
            {
                await _cartRepository.RemoveFromCartAsync(userId.Value, cartItem.ProductId);
            }
            
            _logger.LogInformation(
                "Removed {Count} items from cart for user {UserId} after checkout for sales agent {SaleAgentId}", 
                salesAgentCartItems.Count, userId.Value, request.SalesAgentId);

            return orderResponse;
        }
        catch (Exception ex) when (ex is not BaseApplicationException)
        {
            _logger.LogError(ex, "Error during sales agent checkout for user {UserId}", userId);
            throw InfrastructureException.DatabaseError("Failed to complete checkout", ex);
        }
    }
}
