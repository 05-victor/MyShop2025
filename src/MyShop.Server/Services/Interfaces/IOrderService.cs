using MyShop.Shared.DTOs.Commons;
using MyShop.Shared.DTOs.Requests;
using MyShop.Shared.DTOs.Responses;

namespace MyShop.Server.Services.Interfaces;

public interface IOrderService
{
    Task<PagedResult<OrderResponse>> GetAllAsync(PaginationRequest request);
    Task<OrderResponse?> GetByIdAsync(Guid id);
    Task<OrderResponse> CreateAsync(CreateOrderRequest createOrderRequest);
    Task<OrderResponse> UpdateAsync(Guid id, UpdateOrderRequest updateOrderRequest);
    Task<bool> DeleteAsync(Guid id);
}
