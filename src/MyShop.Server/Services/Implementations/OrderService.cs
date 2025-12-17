using AutoMapper;
using MyShop.Data.Entities;
using MyShop.Data.Repositories.Interfaces;
using MyShop.Server.EntityMappings;
using MyShop.Server.Exceptions;
using MyShop.Server.Factories.Interfaces;
using MyShop.Server.Mappings;
using MyShop.Server.Services.Interfaces;
using MyShop.Shared.DTOs.Commons;
using MyShop.Shared.DTOs.Requests;
using MyShop.Shared.DTOs.Responses;
using MyShop.Data.Entities;
using Microsoft.Extensions.Configuration;
using MyShop.Shared.Enums;
using MyShop.Shared.Extensions;

namespace MyShop.Server.Services.Implementations;

/// <summary>
/// Service for managing order operations
/// </summary>
public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUserRepository _userRepository;
    private readonly IProductRepository _productRepository;
    private readonly IOrderFactory _orderFactory;
    private readonly ICurrentUserService _currentUserService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<OrderService> _logger;

    public OrderService(
        IOrderRepository orderRepository,
        IUserRepository userRepository,
        IProductRepository productRepository,
        IOrderFactory orderFactory,
        ICurrentUserService currentUserService,
        IConfiguration configuration,
        ILogger<OrderService> logger)
    {
        _orderRepository = orderRepository;
        _userRepository = userRepository;
        _productRepository = productRepository;
        _orderFactory = orderFactory;
        _currentUserService = currentUserService;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<PagedResult<OrderResponse>> GetAllAsync(PaginationRequest request)
    {
        try
        {
            var orders = await _orderRepository.GetAllAsync();
            var orderResponses = orders.Select(o => OrderMapper.ToOrderResponse(o)).ToList();

            var pagedItems = orderResponses
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            return new PagedResult<OrderResponse>
            {
                Items = pagedItems,
                TotalCount = orderResponses.Count,
                Page = request.PageNumber,
                PageSize = request.PageSize
            };
        }
        catch (Exception ex) when (ex is not BaseApplicationException)
        {
            _logger.LogError(ex, "Error retrieving orders");
            throw InfrastructureException.DatabaseError("Failed to retrieve orders", ex);
        }
    }

    public async Task<OrderResponse?> GetByIdAsync(Guid id)
    {
        try
        {
            var order = await _orderRepository.GetByIdAsync(id);
            return order is null ? null : OrderMapper.ToOrderResponse(order);
        }
        catch (Exception ex) when (ex is not BaseApplicationException)
        {
            _logger.LogError(ex, "Error retrieving order {OrderId}", id);
            throw InfrastructureException.DatabaseError($"Failed to retrieve order with ID {id}", ex);
        }
    }

    public async Task<OrderResponse> CreateAsync(CreateOrderRequest createOrderRequest)
    {
        // Validate customer exists
        var customer = await _userRepository.GetByIdAsync(createOrderRequest.CustomerId);
        if (customer is null)
        {
            throw NotFoundException.ForEntity("Customer", createOrderRequest.CustomerId);
        }

        try
        {
            // Validate stock availability for all order items
            if (createOrderRequest.OrderItems != null && createOrderRequest.OrderItems.Any())
            {
                foreach (var item in createOrderRequest.OrderItems)
                {
                    var product = await _productRepository.GetByIdAsync(item.ProductId);
                    if (product == null)
                    {
                        throw NotFoundException.ForEntity("Product", item.ProductId);
                    }

                    if (product.Quantity < item.Quantity)
                    {
                        throw new BusinessRuleException(
                            $"Insufficient stock for product '{product.Name}'. Available: {product.Quantity}, Requested: {item.Quantity}");
                    }
                }
            }

            // Create order using factory (factory will throw ValidationException if invalid)
            var order = _orderFactory.Create(createOrderRequest);

            // Auto-assign current user as sale agent if not specified
            if (!createOrderRequest.SaleAgentId.HasValue || createOrderRequest.SaleAgentId == Guid.Empty)
            {
                var currentUserId = _currentUserService.UserId;
                if (currentUserId.HasValue)
                {
                    order.SaleAgentId = currentUserId.Value;
                    _logger.LogInformation("Auto-assigned sale agent {UserId} to order {OrderId}", 
                        currentUserId.Value, order.Id);
                }
                else
                {
                    _logger.LogWarning("No authenticated user found. Order created without sale agent.");
                }
            }
            else
            {
                // Validate sale agent exists if provided
                var saleAgent = await _userRepository.GetByIdAsync(createOrderRequest.SaleAgentId.Value);
                if (saleAgent is null)
                {
                    throw NotFoundException.ForEntity("Sale agent", createOrderRequest.SaleAgentId.Value);
                }
            }

            var createdOrder = await _orderRepository.CreateAsync(order);

            // Update product stock after successful order creation
            if (createOrderRequest.OrderItems != null && createOrderRequest.OrderItems.Any())
            {
                foreach (var item in createOrderRequest.OrderItems)
                {
                    var product = await _productRepository.GetByIdAsync(item.ProductId);
                    if (product != null)
                    {
                        product.Quantity -= item.Quantity;
                        await _productRepository.UpdateAsync(product);
                        _logger.LogInformation("Product {ProductId} stock reduced by {Quantity}. New stock: {NewStock}",
                            product.Id, item.Quantity, product.Quantity);
                    }
                }
            }
            
            _logger.LogInformation("Order {OrderId} created by sale agent {SaleAgentId} for customer {CustomerId}", 
                createdOrder.Id, createdOrder.SaleAgentId, createdOrder.CustomerId);

            return OrderMapper.ToOrderResponse(createdOrder);
        }
        catch (ArgumentException argEx)
        {
            // Convert ArgumentException from factory to ValidationException
            throw new ValidationException(argEx.Message);
        }
        catch (Exception ex) when (ex is not BaseApplicationException)
        {
            _logger.LogError(ex, "Error creating order");
            throw InfrastructureException.DatabaseError("Failed to create order", ex);
        }
    }

    public async Task<OrderResponse> UpdateAsync(Guid id, UpdateOrderRequest updateOrderRequest)
    {
        var existingOrder = await _orderRepository.GetByIdAsync(id);
        if (existingOrder is null)
        {
            throw NotFoundException.ForEntity("Order", id);
        }

        // Validate sale agent if being updated
        if (updateOrderRequest.SaleAgentId.HasValue)
        {
            var saleAgent = await _userRepository.GetByIdAsync(updateOrderRequest.SaleAgentId.Value);
            if (saleAgent is null)
            {
                throw NotFoundException.ForEntity("Sale agent", updateOrderRequest.SaleAgentId.Value);
            }
        }

        try
        {
            // Apply updates using Patch method
            existingOrder.Patch(updateOrderRequest);
            existingOrder.UpdatedAt = DateTime.UtcNow;

            // Update sale agent if specified
            if (updateOrderRequest.SaleAgentId.HasValue)
            {
                existingOrder.SaleAgentId = updateOrderRequest.SaleAgentId.Value;
                _logger.LogInformation("Sale agent updated to {SaleAgentId} for order {OrderId}",
                    updateOrderRequest.SaleAgentId, id);
            }

            var updatedOrder = await _orderRepository.UpdateAsync(existingOrder);
            return OrderMapper.ToOrderResponse(updatedOrder);
        }
        catch (Exception ex) when (ex is not BaseApplicationException)
        {
            _logger.LogError(ex, "Error updating order {OrderId}", id);
            throw InfrastructureException.DatabaseError($"Failed to update order with ID {id}", ex);
        }
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var existingOrder = await _orderRepository.GetByIdAsync(id);
        if (existingOrder is null)
        {
            return false;
        }

        try
        {
            await _orderRepository.DeleteAsync(id);
            _logger.LogInformation("Order {OrderId} deleted", id);
            return true;
        }
        catch (Exception ex) when (ex is not BaseApplicationException)
        {
            _logger.LogError(ex, "Error deleting order {OrderId}", id);
            throw InfrastructureException.DatabaseError($"Failed to delete order with ID {id}", ex);
        }
    }

    public async Task<OrderResponse> CreateOrderFromCartAsync(CreateOrderFromCartRequest request)
    {
        try
        {
            // Validate customer exists
            var customer = await _userRepository.GetByIdAsync(request.CustomerId);
            if (customer is null)
            {
                throw NotFoundException.ForEntity("Customer", request.CustomerId);
            }

            // Validate sales agent exists
            var salesAgent = await _userRepository.GetByIdAsync(request.SalesAgentId);
            if (salesAgent is null)
            {
                throw NotFoundException.ForEntity("Sales agent", request.SalesAgentId);
            }

            // Validate stock availability for all items
            foreach (var cartItem in request.CartItems)
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

            // Read business settings from configuration
            var taxRate = _configuration.GetValue<decimal>("BusinessSettings:TaxRate", 0.1m);
            var shippingFee = _configuration.GetValue<int>("BusinessSettings:ShippingFee", 30000);
            var freeShippingThreshold = _configuration.GetValue<int>("BusinessSettings:FreeShippingThreshold", 500000);
            var enableFreeShipping = _configuration.GetValue<bool>("BusinessSettings:EnableFreeShipping", true);

            // Calculate order totals
            var totalAmount = request.CartItems.Sum(ci => ci.UnitPrice * ci.Quantity);
            var taxAmount = (int)(totalAmount * taxRate);
            var shippingFeeAmount = enableFreeShipping && totalAmount >= freeShippingThreshold ? 0 : shippingFee;
            var grandTotal = totalAmount - request.DiscountAmount + shippingFeeAmount + taxAmount;

            // Create order entity
            var order = new Order
            {
                Id = Guid.NewGuid(),
                CustomerId = request.CustomerId,
                SaleAgentId = request.SalesAgentId,
                OrderDate = DateTime.UtcNow,
                Status = OrderStatus.Pending,
                PaymentStatus = PaymentStatus.Unpaid,
                TotalAmount = totalAmount,
                DiscountAmount = request.DiscountAmount,
                ShippingFee = shippingFeeAmount,
                TaxAmount = taxAmount,
                GrandTotal = grandTotal,
                Note = request.Note,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                OrderItems = request.CartItems.Select(ci => new OrderItem
                {
                    Id = Guid.NewGuid(),
                    ProductId = ci.ProductId,
                    Quantity = ci.Quantity,
                    UnitSalePrice = ci.UnitPrice,
                    TotalPrice = ci.UnitPrice * ci.Quantity,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }).ToList()
            };

            // Create order
            var createdOrder = await _orderRepository.CreateAsync(order);

            _logger.LogInformation(
                "Order {OrderId} created from cart items by customer {CustomerId} for sales agent {SaleAgentId}. Total: {Total}, Items: {ItemCount}",
                createdOrder.Id, request.CustomerId, request.SalesAgentId, grandTotal, request.CartItems.Count);

            // Update product stock
            foreach (var cartItem in request.CartItems)
            {
                var product = await _productRepository.GetByIdAsync(cartItem.ProductId);
                if (product != null)
                {
                    product.Quantity -= cartItem.Quantity;
                    await _productRepository.UpdateAsync(product);
                    _logger.LogInformation("Product {ProductId} stock reduced by {Quantity}. New stock: {NewStock}",
                        product.Id, cartItem.Quantity, product.Quantity);
                }
            }

            return OrderMapper.ToOrderResponse(createdOrder);
        }
        catch (Exception ex) when (ex is not BaseApplicationException)
        {
            _logger.LogError(ex, "Error creating order from cart items");
            throw InfrastructureException.DatabaseError("Failed to create order from cart items", ex);
        }
    }

    public async Task<PagedResult<OrderResponse>> GetMySalesOrdersAsync(PaginationRequest request, string? status = null, string? paymentStatus = null)
    {
        try
        {
            var currentUserId = _currentUserService.UserId;
            if (!currentUserId.HasValue)
            {
                throw new UnauthorizedAccessException("User not authenticated");
            }

            var pagedOrders = await _orderRepository.GetOrdersBySalesAgentIdAsync(request.PageNumber, request.PageSize, currentUserId.Value);

            // Filter by status if provided
            if (!string.IsNullOrWhiteSpace(status))
            {
                var statusEnum = StatusEnumExtensions.ParseApiString<OrderStatus>(status);
                pagedOrders.Items = pagedOrders.Items
                    .Where(o => o.Status == statusEnum)
                    .ToList();
            }

            // Filter by payment status if provided
            if (!string.IsNullOrWhiteSpace(paymentStatus))
            {
                var paymentStatusEnum = StatusEnumExtensions.ParseApiString<PaymentStatus>(paymentStatus);
                pagedOrders.Items = pagedOrders.Items
                    .Where(o => o.PaymentStatus == paymentStatusEnum)
                    .ToList();
            }

            return new PagedResult<OrderResponse>
            {
                Items = pagedOrders.Items.Select(o => OrderMapper.ToOrderResponse(o)).ToList(),
                TotalCount = pagedOrders.Items.Count,
                Page = pagedOrders.Page,
                PageSize = pagedOrders.PageSize
            };
        }
        catch (UnauthorizedAccessException)
        {
            throw;
        }
        catch (Exception ex) when (ex is not BaseApplicationException)
        {
            _logger.LogError(ex, "Error retrieving sales agent orders");
            throw InfrastructureException.DatabaseError("Failed to retrieve sales agent orders", ex);
        }
    }

    public async Task<OrderResponse?> GetMySalesOrderByIdAsync(Guid orderId)
    {
        try
        {
            var currentUserId = _currentUserService.UserId;
            if (!currentUserId.HasValue)
            {
                throw new UnauthorizedAccessException("User not authenticated");
            }

            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order is null)
            {
                return null;
            }

            // Verify that this order belongs to the current sales agent
            if (order.SaleAgentId != currentUserId.Value)
            {
                throw new UnauthorizedAccessException("You are not authorized to view this order");
            }

            return OrderMapper.ToOrderResponse(order);
        }
        catch (UnauthorizedAccessException)
        {
            throw;
        }
        catch (Exception ex) when (ex is not BaseApplicationException)
        {
            _logger.LogError(ex, "Error retrieving sales agent order {OrderId}", orderId);
            throw InfrastructureException.DatabaseError($"Failed to retrieve order with ID {orderId}", ex);
        }
    }

    public async Task<OrderResponse> UpdateOrderStatusAsync(Guid orderId, UpdateOrderStatusRequest request)
    {
        try
        {
            var currentUserId = _currentUserService.UserId;
            if (!currentUserId.HasValue)
            {
                throw new UnauthorizedAccessException("User not authenticated");
            }

            var existingOrder = await _orderRepository.GetByIdAsync(orderId);
            if (existingOrder is null)
            {
                throw NotFoundException.ForEntity("Order", orderId);
            }

            // Verify that this order belongs to the current sales agent
            if (existingOrder.SaleAgentId != currentUserId.Value)
            {
                throw new UnauthorizedAccessException("You are not authorized to update this order");
            }

            // Parse and validate status
            var orderStatus = StatusEnumExtensions.ParseApiString<OrderStatus>(request.Status);
            
            // Validate status is a valid enum value
            if (!Enum.IsDefined(typeof(OrderStatus), orderStatus))
            {
                var validStatuses = Enum.GetNames(typeof(OrderStatus));
                throw new ValidationException($"Invalid status. Valid statuses: {string.Join(", ", validStatuses)}");
            }

            // Update order status
            existingOrder.Status = orderStatus;
            existingOrder.UpdatedAt = DateTime.UtcNow;

            // Append notes if provided
            if (!string.IsNullOrWhiteSpace(request.Notes))
            {
                existingOrder.Note = string.IsNullOrWhiteSpace(existingOrder.Note)
                    ? request.Notes
                    : $"{existingOrder.Note}\n{DateTime.UtcNow:yyyy-MM-dd HH:mm}: {request.Notes}";
            }

            var updatedOrder = await _orderRepository.UpdateAsync(existingOrder);
            
            _logger.LogInformation("Order {OrderId} status updated to {Status} by sales agent {UserId}", 
                orderId, orderStatus.ToApiString(), currentUserId.Value);

            return OrderMapper.ToOrderResponse(updatedOrder);
        }
        catch (UnauthorizedAccessException)
        {
            throw;
        }
        catch (Exception ex) when (ex is not BaseApplicationException)
        {
            _logger.LogError(ex, "Error updating order status for order {OrderId}", orderId);
            throw InfrastructureException.DatabaseError($"Failed to update order status for order with ID {orderId}", ex);
        }
    }

    public async Task<PagedResult<OrderResponse>> GetMyCustomerOrdersAsync(PaginationRequest request, string? status = null)
    {
        try
        {
            var currentUserId = _currentUserService.UserId;
            if (!currentUserId.HasValue)
            {
                throw new UnauthorizedAccessException("User not authenticated");
            }

            var pagedOrders = await _orderRepository.GetOrdersByCustomerIdAsync(request.PageNumber, request.PageSize, currentUserId.Value);

            // Filter by status if provided
            if (!string.IsNullOrWhiteSpace(status))
            {
                var statusEnum = StatusEnumExtensions.ParseApiString<OrderStatus>(status);
                pagedOrders.Items = pagedOrders.Items
                    .Where(o => o.Status == statusEnum)
                    .ToList();
            }

            return new PagedResult<OrderResponse>
            {
                Items = pagedOrders.Items.Select(o => OrderMapper.ToOrderResponse(o)).ToList(),
                TotalCount = pagedOrders.Items.Count,
                Page = pagedOrders.Page,
                PageSize = pagedOrders.PageSize
            };
        }
        catch (UnauthorizedAccessException)
        {
            throw;
        }
        catch (Exception ex) when (ex is not BaseApplicationException)
        {
            _logger.LogError(ex, "Error retrieving customer orders");
            throw InfrastructureException.DatabaseError("Failed to retrieve customer orders", ex);
        }
    }

    public async Task<ProcessCardPaymentResponse> ProcessCardPaymentAsync(ProcessCardPaymentRequest request)
    {
        try
        {
            // Get the order
            var order = await _orderRepository.GetByIdAsync(request.OrderId);
            if (order is null)
            {
                throw NotFoundException.ForEntity("Order", request.OrderId);
            }

            // Validate order is not already paid
            if (order.PaymentStatus == PaymentStatus.Paid)
            {
                return new ProcessCardPaymentResponse
                {
                    OrderId = request.OrderId,
                    Success = false,
                    Message = "Order has already been paid",
                    PaymentStatus = order.PaymentStatus.ToApiString()
                };
            }

            // Validate order is not cancelled
            if (order.Status == OrderStatus.Cancelled)
            {
                return new ProcessCardPaymentResponse
                {
                    OrderId = request.OrderId,
                    Success = false,
                    Message = "Cannot process payment for cancelled order",
                    PaymentStatus = order.PaymentStatus.ToApiString()
                };
            }

            // Remove spaces from card number for validation
            var cardNumber = request.CardNumber.Replace(" ", "");

            // Simple demo payment processing logic
            PaymentStatus newPaymentStatus;
            string message;
            bool success;

            if (cardNumber == "1111111111111111")
            {
                // Success case
                newPaymentStatus = PaymentStatus.Paid;
                message = "Payment processed successfully";
                success = true;

                _logger.LogInformation("Payment successful for order {OrderId} using card ending in {Last4}",
                    order.Id, cardNumber.Substring(cardNumber.Length - 4));
            }
            else if (cardNumber == "0000000000000000")
            {
                // Failed payment case
                newPaymentStatus = PaymentStatus.Failed;
                message = "Payment failed. Please check your card details and try again";
                success = false;

                _logger.LogWarning("Payment failed for order {OrderId} using card ending in {Last4}",
                    order.Id, cardNumber.Substring(cardNumber.Length - 4));
            }
            else
            {
                // Invalid card case
                return new ProcessCardPaymentResponse
                {
                    OrderId = request.OrderId,
                    Success = false,
                    Message = "Invalid card number. This is a demo system. Use 1111 1111 1111 1111 for success or 0000 0000 0000 0000 for failure",
                    PaymentStatus = order.PaymentStatus.ToApiString()
                };
            }

            // Update order payment status
            order.PaymentStatus = newPaymentStatus;

            if (newPaymentStatus == PaymentStatus.Paid)
            {
                order.Status = OrderStatus.Confirmed;
            }

            order.UpdatedAt = DateTime.UtcNow;

            await _orderRepository.UpdateAsync(order);

            var response = new ProcessCardPaymentResponse
            {
                OrderId = request.OrderId,
                Success = success,
                Message = message,
                PaymentStatus = newPaymentStatus.ToApiString(),
                ProcessedAt = DateTime.UtcNow,
                TransactionId = success ? $"TXN-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}" : null
            };

            _logger.LogInformation("Payment processing completed for order {OrderId}. Status: {PaymentStatus}",
                order.Id, newPaymentStatus.ToApiString());

            return response;
        }
        catch (Exception ex) when (ex is not BaseApplicationException)
        {
            _logger.LogError(ex, "Error processing card payment for order {OrderId}", request.OrderId);
            throw InfrastructureException.DatabaseError("Failed to process card payment", ex);
        }
    }
}
