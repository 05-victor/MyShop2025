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
    private readonly IOrderRepository _orderRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<CartService> _logger;
    private readonly CartMapper _cartMapper;

    public CartService(
        ICartRepository cartRepository,
        IProductRepository productRepository,
        IOrderRepository orderRepository,
        ICurrentUserService currentUserService,
        ILogger<CartService> logger,
        CartMapper cartMapper)
    {
        _cartRepository = cartRepository;
        _productRepository = productRepository;
        _orderRepository = orderRepository;
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

    public async Task<OrderResponse> CheckoutAsync(CheckoutFromCartRequest request)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue)
        {
            throw new AuthenticationException("User not authenticated");
        }

        try
        {
            // Get cart items
            var cartItems = await _cartRepository.GetCartItemsByUserIdAsync(userId.Value);
            var cartItemsList = cartItems.ToList();

            if (!cartItemsList.Any())
            {
                throw new ValidationException("Cart is empty. Cannot checkout with an empty cart.");
            }

            // Validate stock availability for all items
            foreach (var cartItem in cartItemsList)
            {
                var product = await _productRepository.GetByIdAsync(cartItem.ProductId);
                if (product == null)
                {
                    throw NotFoundException.ForEntity("Product", cartItem.ProductId);
                }

                if (product.Quantity < cartItem.Quantity)
                {
                    throw new BusinessRuleException(
                        $"Insufficient stock for product '{product.Name}'. Available: {product.Quantity}, Requested: {cartItem.Quantity}");
                }
            }

            // Calculate order totals
            var totalAmount = cartItemsList.Sum(ci => ci.Product.SellingPrice * ci.Quantity);
            var taxAmount = (int)(totalAmount * 0.1m); // 10% tax
            var shippingFee = totalAmount >= 500000 ? 0 : 30000; // Free shipping over 500k VND
            var grandTotal = totalAmount - request.DiscountAmount + shippingFee + taxAmount;

            // Create order entity
            var order = new Order
            {
                Id = Guid.NewGuid(),
                CustomerId = userId.Value,
                SaleAgentId = userId.Value, // Auto-assign current user as sale agent
                OrderDate = DateTime.UtcNow,
                Status = "PENDING",
                PaymentStatus = "UNPAID",
                TotalAmount = totalAmount,
                DiscountAmount = request.DiscountAmount,
                ShippingFee = shippingFee,
                TaxAmount = taxAmount,
                GrandTotal = grandTotal,
                Note = request.Note,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                OrderItems = cartItemsList.Select(ci => new OrderItem
                {
                    Id = Guid.NewGuid(),
                    ProductId = ci.ProductId,
                    Quantity = ci.Quantity,
                    UnitSalePrice = ci.Product.SellingPrice,
                    TotalPrice = ci.Product.SellingPrice * ci.Quantity,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }).ToList()
            };

            // Create order
            var createdOrder = await _orderRepository.CreateAsync(order);

            _logger.LogInformation(
                "Order {OrderId} created from cart checkout by user {UserId}. Total: {Total}, Items: {ItemCount}",
                createdOrder.Id, userId.Value, grandTotal, cartItemsList.Count);

            // Update product stock
            foreach (var cartItem in cartItemsList)
            {
                var product = await _productRepository.GetByIdAsync(cartItem.ProductId);
                if (product != null)
                {
                    product.Quantity -= cartItem.Quantity;
                    await _productRepository.UpdateAsync(product);
                }
            }

            // Clear cart after successful order
            await _cartRepository.ClearCartAsync(userId.Value);
            _logger.LogInformation("Cart cleared for user {UserId} after successful checkout", userId.Value);

            // Convert to response
            return OrderMapper.ToOrderResponse(createdOrder);
        }
        catch (Exception ex) when (ex is not BaseApplicationException)
        {
            _logger.LogError(ex, "Error during checkout for user {UserId}", userId);
            throw InfrastructureException.DatabaseError("Failed to complete checkout", ex);
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

            // Validate stock availability for all items
            foreach (var cartItem in salesAgentCartItems)
            {
                var product = await _productRepository.GetByIdAsync(cartItem.ProductId);
                if (product == null)
                {
                    throw NotFoundException.ForEntity("Product", cartItem.ProductId);
                }

                if (product.Quantity < cartItem.Quantity)
                {
                    throw new BusinessRuleException(
                        $"Insufficient stock for product '{product.Name}'. Available: {product.Quantity}, Requested: {cartItem.Quantity}");
                }
            }

            // Calculate order totals
            var totalAmount = salesAgentCartItems.Sum(ci => ci.Product.SellingPrice * ci.Quantity);
            var taxAmount = (int)(totalAmount * 0.1m); // 10% tax
            var shippingFee = totalAmount >= 500000 ? 0 : 30000; // Free shipping over 500k VND
            var grandTotal = totalAmount - request.DiscountAmount + shippingFee + taxAmount;

            // Create order entity
            var order = new Order
            {
                Id = Guid.NewGuid(),
                CustomerId = userId.Value,
                SaleAgentId = request.SalesAgentId, // Assign to the product's sales agent
                OrderDate = DateTime.UtcNow,
                Status = "PENDING",
                PaymentStatus = "UNPAID",
                TotalAmount = totalAmount,
                DiscountAmount = request.DiscountAmount,
                ShippingFee = shippingFee,
                TaxAmount = taxAmount,
                GrandTotal = grandTotal,
                Note = request.Note,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                OrderItems = salesAgentCartItems.Select(ci => new OrderItem
                {
                    Id = Guid.NewGuid(),
                    ProductId = ci.ProductId,
                    Quantity = ci.Quantity,
                    UnitSalePrice = ci.Product.SellingPrice,
                    TotalPrice = ci.Product.SellingPrice * ci.Quantity,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }).ToList()
            };

            // Create order
            var createdOrder = await _orderRepository.CreateAsync(order);

            _logger.LogInformation(
                "Order {OrderId} created from cart checkout by customer {CustomerId} for sales agent {SaleAgentId}. Total: {Total}, Items: {ItemCount}",
                createdOrder.Id, userId.Value, request.SalesAgentId, grandTotal, salesAgentCartItems.Count);

            // Update product stock
            foreach (var cartItem in salesAgentCartItems)
            {
                var product = await _productRepository.GetByIdAsync(cartItem.ProductId);
                if (product != null)
                {
                    product.Quantity -= cartItem.Quantity;
                    await _productRepository.UpdateAsync(product);
                }
            }

            // Remove only the checked-out items from cart (not all items)
            foreach (var cartItem in salesAgentCartItems)
            {
                await _cartRepository.RemoveFromCartAsync(userId.Value, cartItem.ProductId);
            }
            
            _logger.LogInformation(
                "Removed {Count} items from cart for user {UserId} after checkout for sales agent {SaleAgentId}", 
                salesAgentCartItems.Count, userId.Value, request.SalesAgentId);

            // Convert to response
            return OrderMapper.ToOrderResponse(createdOrder);
        }
        catch (Exception ex) when (ex is not BaseApplicationException)
        {
            _logger.LogError(ex, "Error during sales agent checkout for user {UserId}", userId);
            throw InfrastructureException.DatabaseError("Failed to complete checkout", ex);
        }
    }
}
