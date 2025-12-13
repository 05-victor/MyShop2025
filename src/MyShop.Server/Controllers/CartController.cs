using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyShop.Server.Services.Interfaces;
using MyShop.Shared.DTOs.Common;
using MyShop.Shared.DTOs.Requests;
using MyShop.Shared.DTOs.Responses;

namespace MyShop.Server.Controllers;

[ApiController]
[Route("api/v1/cart")]
[Authorize]
public class CartController : ControllerBase
{
    private readonly ICartService _cartService;
    private readonly ILogger<CartController> _logger;

    public CartController(
        ICartService cartService,
        ILogger<CartController> logger)
    {
        _cartService = cartService;
        _logger = logger;
    }

    /// <summary>
    /// Get the current user's cart
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<CartResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    [ProducesResponseType(typeof(ApiResponse<object>), 500)]
    public async Task<ActionResult<ApiResponse<CartResponse>>> GetMyCartAsync()
    {
        var cart = await _cartService.GetMyCartAsync();
        return Ok(ApiResponse<CartResponse>.SuccessResponse(cart));
    }

    /// <summary>
    /// Add an item to the cart
    /// </summary>
    [HttpPost("items")]
    [ProducesResponseType(typeof(ApiResponse<CartResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    [ProducesResponseType(typeof(ApiResponse<object>), 500)]
    public async Task<ActionResult<ApiResponse<CartResponse>>> AddItemAsync([FromBody] AddToCartRequest request)
    {
        var cart = await _cartService.AddToCartAsync(request.ProductId, request.Quantity);
        return Ok(ApiResponse<CartResponse>.SuccessResponse(cart, "Item added to cart successfully", 200));
    }

    /// <summary>
    /// Update cart item quantity
    /// </summary>
    [HttpPut("items/{productId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<CartResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    [ProducesResponseType(typeof(ApiResponse<object>), 500)]
    public async Task<ActionResult<ApiResponse<CartResponse>>> UpdateItemAsync(
        [FromRoute] Guid productId, 
        [FromBody] UpdateCartItemRequest request)
    {
        var cart = await _cartService.UpdateCartItemQuantityAsync(productId, request.Quantity);
        return Ok(ApiResponse<CartResponse>.SuccessResponse(cart, "Cart item updated successfully", 200));
    }

    /// <summary>
    /// Remove an item from the cart
    /// </summary>
    [HttpDelete("items/{productId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    [ProducesResponseType(typeof(ApiResponse<object>), 500)]
    public async Task<ActionResult<ApiResponse<bool>>> RemoveItemAsync([FromRoute] Guid productId)
    {
        var result = await _cartService.RemoveFromCartAsync(productId);
        
        if (!result)
        {
            return NotFound(ApiResponse<bool>.ErrorResponse("Cart item not found", 404));
        }

        return Ok(ApiResponse<bool>.SuccessResponse(result, "Item removed from cart successfully", 200));
    }

    /// <summary>
    /// Clear all items from the cart
    /// </summary>
    [HttpDelete("clear")]
    [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    [ProducesResponseType(typeof(ApiResponse<object>), 500)]
    public async Task<ActionResult<ApiResponse<bool>>> ClearCartAsync()
    {
        var result = await _cartService.ClearCartAsync();
        return Ok(ApiResponse<bool>.SuccessResponse(result, "Cart cleared successfully", 200));
    }
}
