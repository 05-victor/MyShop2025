using MyShop.Data.Repositories.Interfaces;
using MyShop.Server.EntityMappings;
using MyShop.Server.Exceptions;
using MyShop.Server.Factories.Interfaces;
using MyShop.Server.Mappings;
using MyShop.Server.Services.Interfaces;
using MyShop.Shared.DTOs.Requests;
using MyShop.Shared.DTOs.Responses;

namespace MyShop.Server.Services.Implementations;

/// <summary>
/// Service for managing order operations
/// </summary>
public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUserRepository _userRepository;
    private readonly IOrderFactory _orderFactory;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<OrderService> _logger;

    public OrderService(
        IOrderRepository orderRepository,
        IUserRepository userRepository,
        IOrderFactory orderFactory,
        ICurrentUserService currentUserService,
        ILogger<OrderService> logger)
    {
        _orderRepository = orderRepository;
        _userRepository = userRepository;
        _orderFactory = orderFactory;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<IEnumerable<OrderResponse>> GetAllAsync()
    {
        try
        {
            var orders = await _orderRepository.GetAllAsync();
            return orders.Select(o => OrderMapper.ToOrderResponse(o));
        }
        catch (Exception ex) when (ex is not BaseApplicationException)
        {
            _logger.LogError(ex, "Error retrieving all orders");
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
}
