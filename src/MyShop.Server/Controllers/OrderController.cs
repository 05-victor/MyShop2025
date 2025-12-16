using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyShop.Server.Services.Interfaces;
using MyShop.Shared.DTOs.Common;
using MyShop.Shared.DTOs.Commons;
using MyShop.Shared.DTOs.Requests;
using MyShop.Shared.DTOs.Responses;

namespace MyShop.Server.Controllers;

[ApiController]
[Route("api/v1/orders")]
[Authorize]
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
    [Authorize(Roles = "Admin,SalesAgent")]
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
    [Authorize(Roles = "Admin,SalesAgent")]
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
    [Authorize(Roles = "Admin")]
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

    /// <summary>
    /// Get all orders for the current sales agent
    /// </summary>
    [HttpGet("my-sales")]
    [Authorize(Roles = "SalesAgent")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<OrderResponse>>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    [ProducesResponseType(typeof(ApiResponse<object>), 403)]
    [ProducesResponseType(typeof(ApiResponse<object>), 500)]
    public async Task<ActionResult<ApiResponse<PagedResult<OrderResponse>>>> GetMySalesOrdersAsync(
        [FromQuery] PaginationRequest request,
        [FromQuery] string? status = null)
    {
        try
        {
            var pagedResult = await _orderService.GetMySalesOrdersAsync(request, status);
            return Ok(ApiResponse<PagedResult<OrderResponse>>.SuccessResponse(pagedResult));
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access to sales orders");
            return Unauthorized(ApiResponse.ErrorResponse("Unauthorized", 401));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving sales agent orders");
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while retrieving orders", 500));
        }
    }

    // Not used currently
    /// <summary>
    /// Get a specific order for the current sales agent
    /// </summary>
    [HttpGet("my-sales/{id:guid}")]
    [Authorize(Roles = "SalesAgent")]
    [ProducesResponseType(typeof(ApiResponse<OrderResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    [ProducesResponseType(typeof(ApiResponse<object>), 403)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    [ProducesResponseType(typeof(ApiResponse<object>), 500)]
    public async Task<ActionResult<ApiResponse<OrderResponse>>> GetMySalesOrderByIdAsync([FromRoute] Guid id)
    {
        try
        {
            var order = await _orderService.GetMySalesOrderByIdAsync(id);
            if (order is null)
            {
                return NotFound(ApiResponse<OrderResponse>.ErrorResponse("Order not found", 404));
            }
            return Ok(ApiResponse<OrderResponse>.SuccessResponse(order));
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access to order {OrderId}", id);
            return Unauthorized(ApiResponse.ErrorResponse("You are not authorized to view this order", 401));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving order {OrderId}", id);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while retrieving the order", 500));
        }
    }

    // Not used currently. Used UpdateAsync instead.
    /// <summary>
    /// Update order status for sales agent's own orders
    /// </summary>
    [HttpPatch("my-sales/{id:guid}/status")]
    [Authorize(Roles = "SalesAgent")]
    [ProducesResponseType(typeof(ApiResponse<OrderResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    [ProducesResponseType(typeof(ApiResponse<object>), 403)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    [ProducesResponseType(typeof(ApiResponse<object>), 500)]
    public async Task<ActionResult<ApiResponse<OrderResponse>>> UpdateMySalesOrderStatusAsync(
        [FromRoute] Guid id,
        [FromBody] UpdateOrderStatusRequest request)
    {
        try
        {
            var updatedOrder = await _orderService.UpdateOrderStatusAsync(id, request);
            return Ok(ApiResponse<OrderResponse>.SuccessResponse(updatedOrder, "Order status updated successfully", 200));
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized status update attempt for order {OrderId}", id);
            return Unauthorized(ApiResponse.ErrorResponse("You are not authorized to update this order", 401));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating order status for order {OrderId}", id);
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while updating the order status", 500));
        }
    }
}
