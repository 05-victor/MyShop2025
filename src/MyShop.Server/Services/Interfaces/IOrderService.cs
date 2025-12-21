using MyShop.Shared.DTOs.Commons;
using MyShop.Shared.DTOs.Requests;
using MyShop.Shared.DTOs.Responses;

namespace MyShop.Server.Services.Interfaces;

/// <summary>
/// Service interface for order operations
/// </summary>
public interface IOrderService
{
    Task<PagedResult<OrderResponse>> GetAllAsync(PaginationRequest request);
    Task<OrderResponse?> GetByIdAsync(Guid id);
    Task<OrderResponse> CreateAsync(CreateOrderRequest createOrderRequest);
    Task<OrderResponse> UpdateAsync(Guid id, UpdateOrderRequest updateOrderRequest);
    Task<bool> DeleteAsync(Guid id);
    
    /// <summary>
    /// Create order from cart items for a specific sales agent
    /// </summary>
    Task<OrderResponse> CreateOrderFromCartAsync(CreateOrderFromCartRequest request);

    /// <summary>
    /// Get all orders for the current sales agent (authenticated user)
    /// </summary>
    Task<PagedResult<OrderResponse>> GetMySalesOrdersAsync(PaginationRequest request, string? status = null, string? paymentStatus = null);

    /// <summary>
    /// Get a specific order for the current sales agent
    /// </summary>
    Task<OrderResponse?> GetMySalesOrderByIdAsync(Guid orderId);

    /// <summary>
    /// Update order status (for sales agent's own orders)
    /// </summary>
    Task<OrderResponse> UpdateOrderStatusAsync(Guid orderId, UpdateOrderStatusRequest request);

    /// <summary>
    /// Get all orders for the current customer (authenticated user)
    /// </summary>
    Task<PagedResult<OrderResponse>> GetMyCustomerOrdersAsync(PaginationRequest request, string? status = null, string? paymentStatus = null);

    /// <summary>
    /// Process card payment for an order
    /// </summary>
    Task<ProcessCardPaymentResponse> ProcessCardPaymentAsync(ProcessCardPaymentRequest request);
}
