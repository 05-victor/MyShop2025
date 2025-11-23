using MyShop.Shared.DTOs.Requests;
using MyShop.Shared.DTOs.Responses;
using Refit;

namespace MyShop.Plugins.API.Orders;

/// <summary>
/// Refit interface for Orders API endpoints
/// </summary>
[Headers("User-Agent: MyShop-Client/1.0")]
public interface IOrdersApi
{
    [Get("/api/v1/orders")]
    Task<Refit.ApiResponse<MyShop.Shared.DTOs.Common.ApiResponse<List<OrderResponse>>>> GetAllAsync();

    [Get("/api/v1/orders/{id}")]
    Task<Refit.ApiResponse<MyShop.Shared.DTOs.Common.ApiResponse<OrderResponse>>> GetByIdAsync(Guid id);

    [Get("/api/v1/orders/my-orders")]
    Task<Refit.ApiResponse<MyShop.Shared.DTOs.Common.ApiResponse<List<OrderResponse>>>> GetMyOrdersAsync();

    [Post("/api/v1/orders")]
    Task<Refit.ApiResponse<MyShop.Shared.DTOs.Common.ApiResponse<OrderResponse>>> CreateAsync([Body] CreateOrderRequest request);

    [Put("/api/v1/orders/{id}/status")]
    Task<Refit.ApiResponse<MyShop.Shared.DTOs.Common.ApiResponse<bool>>> UpdateStatusAsync(Guid id, [Body] UpdateOrderStatusRequest request);
}
