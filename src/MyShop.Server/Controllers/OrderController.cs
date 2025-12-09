using Microsoft.AspNetCore.Mvc;
using MyShop.Server.Services.Interfaces;
using MyShop.Shared.DTOs.Common;
using MyShop.Shared.DTOs.Commons;
using MyShop.Shared.DTOs.Requests;
using MyShop.Shared.DTOs.Responses;

namespace MyShop.Server.Controllers;

[ApiController]
[Route("api/v1/orders")]
public class OrderController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly ILogger<OrderController> _logger;

    public OrderController(IOrderService orderService, ILogger<OrderController> logger)
    {
        _orderService = orderService;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<OrderResponse>>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 500)]
    public async Task<ActionResult<ApiResponse<PagedResult<OrderResponse>>>> GetAllAsync([FromQuery] PaginationRequest request)
    {
        var pagedResult = await _orderService.GetAllAsync(request);
        return Ok(ApiResponse<PagedResult<OrderResponse>>.SuccessResponse(pagedResult));
    }

    [HttpGet("{id:guid}", Name = "GetOrderById")]
    [ProducesResponseType(typeof(ApiResponse<OrderResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    [ProducesResponseType(typeof(ApiResponse<object>), 500)]
    public async Task<ActionResult<ApiResponse<OrderResponse>>> GetByIdAsync([FromRoute] Guid id)
    {
        var order = await _orderService.GetByIdAsync(id);
        if (order is null)
        {
            return NotFound(ApiResponse<OrderResponse>.ErrorResponse("Order not found", 404));
        }
        return Ok(ApiResponse<OrderResponse>.SuccessResponse(order));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<OrderResponse>), 201)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    [ProducesResponseType(typeof(ApiResponse<object>), 500)]
    public async Task<ActionResult<ApiResponse<OrderResponse>>> CreateAsync([FromBody] CreateOrderRequest createOrderRequest)
    {
        var createdOrder = await _orderService.CreateAsync(createOrderRequest);
        return CreatedAtRoute("GetOrderById", new { id = createdOrder.Id }, 
            ApiResponse<OrderResponse>.SuccessResponse(createdOrder, "Order created successfully", 201));
    }

    [HttpPatch("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<OrderResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    [ProducesResponseType(typeof(ApiResponse<object>), 500)]
    public async Task<ActionResult<ApiResponse<OrderResponse>>> UpdateAsync([FromRoute] Guid id, [FromBody] UpdateOrderRequest updateOrderRequest)
    {
        var updatedOrder = await _orderService.UpdateAsync(id, updateOrderRequest);
        return Ok(ApiResponse<OrderResponse>.SuccessResponse(updatedOrder, "Order updated successfully", 200));
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse), 204)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    [ProducesResponseType(typeof(ApiResponse<object>), 500)]
    public async Task<ActionResult<ApiResponse>> DeleteAsync([FromRoute] Guid id)
    {
        var deleted = await _orderService.DeleteAsync(id);
        if (!deleted)
        {
            return NotFound(ApiResponse.ErrorResponse("Order not found", 404));
        }
        return NoContent();
    }
}
