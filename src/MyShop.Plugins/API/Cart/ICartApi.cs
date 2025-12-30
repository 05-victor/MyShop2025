using MyShop.Shared.DTOs.Requests;
using MyShop.Shared.DTOs.Responses;
using Refit;

namespace MyShop.Plugins.API.Cart;

/// <summary>
/// Refit interface for Cart API endpoints
/// </summary>
[Headers("User-Agent: MyShop-Client/1.0")]
public interface ICartApi
{
    [Get("/api/v1/cart")]
    Task<Refit.ApiResponse<MyShop.Shared.DTOs.Common.ApiResponse<CartResponse>>> GetMyCartAsync();

    [Get("/api/v1/cart/grouped")]
    Task<Refit.ApiResponse<MyShop.Shared.DTOs.Common.ApiResponse<GroupedCartResponse>>> GetMyCartGroupedAsync();

    [Post("/api/v1/cart/items")]
    Task<Refit.ApiResponse<MyShop.Shared.DTOs.Common.ApiResponse<CartResponse>>> AddItemAsync([Body] AddToCartRequest request);

    [Patch("/api/v1/cart/items/{productId}")]
    Task<Refit.ApiResponse<MyShop.Shared.DTOs.Common.ApiResponse<CartResponse>>> UpdateItemAsync(Guid productId, [Body] UpdateCartItemRequest request);

    [Delete("/api/v1/cart/items/{itemId}")]
    Task<Refit.ApiResponse<MyShop.Shared.DTOs.Common.ApiResponse<bool>>> RemoveItemAsync(Guid itemId);

    [Delete("/api/v1/cart/clear")]
    Task<Refit.ApiResponse<MyShop.Shared.DTOs.Common.ApiResponse<bool>>> ClearCartAsync();

    [Post("/api/v1/cart/checkout/sales-agent")]
    Task<Refit.ApiResponse<MyShop.Shared.DTOs.Common.ApiResponse<OrderResponse>>> CheckoutBySalesAgentAsync([Body] CheckoutBySalesAgentRequest request);
}
