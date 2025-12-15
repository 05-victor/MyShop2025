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
    Task<OrderResponse> CreateOrderFromCartItemsAsync(
        Guid customerId,
        Guid salesAgentId,
        IEnumerable<Data.Entities.CartItem> cartItems,
        string shippingAddress,
        string? note,
        string paymentMethod,
        int discountAmount);
}
