using MyShop.Shared.Adapters;
using MyShop.Core.Common;
using MyShop.Core.Interfaces.Repositories;
using MyShop.Plugins.API.Orders;
using MyShop.Shared.DTOs.Requests;
using MyShop.Shared.Models;

namespace MyShop.Plugins.Repositories.Api;

/// <summary>
/// API-based Order Repository implementation
/// </summary>
public class OrderRepository : IOrderRepository
{
    private readonly IOrdersApi _api;

    public OrderRepository(IOrdersApi api)
    {
        _api = api;
    }

    public async Task<Result<IEnumerable<Order>>> GetAllAsync()
    {
        try
        {
            var response = await _api.GetAllAsync();
            
            if (response.IsSuccessStatusCode && response.Content != null)
            {
                var apiResponse = response.Content;
                if (apiResponse.Success && apiResponse.Result != null)
                {
                    var orders = OrderAdapter.ToModelList(apiResponse.Result);
                    return Result<IEnumerable<Order>>.Success(orders);
                }
            }

            return Result<IEnumerable<Order>>.Failure("Failed to retrieve orders");
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<Order>>.Failure($"Error retrieving orders: {ex.Message}");
        }
    }

    public async Task<Result<Order>> GetByIdAsync(Guid id)
    {
        try
        {
            var response = await _api.GetByIdAsync(id);
            
            if (response.IsSuccessStatusCode && response.Content != null)
            {
                var apiResponse = response.Content;
                if (apiResponse.Success && apiResponse.Result != null)
                {
                    var order = OrderAdapter.ToModel(apiResponse.Result);
                    return Result<Order>.Success(order);
                }
            }

            return Result<Order>.Failure($"Order with ID {id} not found");
        }
        catch (Exception ex)
        {
            return Result<Order>.Failure($"Error retrieving order: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<Order>>> GetByCustomerIdAsync(Guid customerId)
    {
        try
        {
            // Note: Using GetMyOrdersAsync - assumes JWT contains customer ID
            var response = await _api.GetMyOrdersAsync();
            
            if (response.IsSuccessStatusCode && response.Content != null)
            {
                var apiResponse = response.Content;
                if (apiResponse.Success && apiResponse.Result != null)
                {
                    var orders = apiResponse.Result
                        .Where(o => o.CustomerId == customerId)
                        .Select(OrderAdapter.ToModel)
                        .ToList();
                    return Result<IEnumerable<Order>>.Success(orders);
                }
            }

            return Result<IEnumerable<Order>>.Failure("Failed to retrieve customer orders");
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<Order>>.Failure($"Error retrieving customer orders: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<Order>>> GetBySalesAgentIdAsync(Guid salesAgentId)
    {
        try
        {
            // Note: Backend may need dedicated endpoint for this filter
            var response = await _api.GetAllAsync();
            
            if (response.IsSuccessStatusCode && response.Content != null)
            {
                var apiResponse = response.Content;
                if (apiResponse.Success && apiResponse.Result != null)
                {
                    var orders = apiResponse.Result
                        .Where(o => o.CustomerId == salesAgentId) // May need adjustment based on backend schema
                        .Select(OrderAdapter.ToModel)
                        .ToList();
                    return Result<IEnumerable<Order>>.Success(orders);
                }
            }

            return Result<IEnumerable<Order>>.Failure("Failed to retrieve sales agent orders");
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<Order>>.Failure($"Error retrieving sales agent orders: {ex.Message}");
        }
    }

    public async Task<Result<Order>> CreateAsync(Order order)
    {
        try
        {
            var request = new CreateOrderRequest
            {
                Items = order.Items.Select(item => new OrderItemRequest
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity
                }).ToList(),
                ShippingAddress = order.CustomerAddress ?? string.Empty,
                PaymentMethod = "CASH", // Default
                Notes = order.Notes
            };

            var response = await _api.CreateAsync(request);
            
            if (response.IsSuccessStatusCode && response.Content != null)
            {
                var apiResponse = response.Content;
                if (apiResponse.Success && apiResponse.Result != null)
                {
                    var createdOrder = OrderAdapter.ToModel(apiResponse.Result);
                    return Result<Order>.Success(createdOrder);
                }
            }

            return Result<Order>.Failure("Failed to create order");
        }
        catch (Exception ex)
        {
            return Result<Order>.Failure($"Error creating order: {ex.Message}");
        }
    }

    public async Task<Result<Order>> UpdateAsync(Order order)
    {
        try
        {
            // Note: Backend may need dedicated PUT /orders/{id} endpoint for full update
            // Currently only UpdateStatusAsync is available
            return Result<Order>.Failure("Full order update not yet implemented in backend API");
        }
        catch (Exception ex)
        {
            return Result<Order>.Failure($"Error updating order: {ex.Message}");
        }
    }

    public async Task<Result<bool>> UpdateStatusAsync(Guid orderId, string status)
    {
        try
        {
            var request = new UpdateOrderStatusRequest
            {
                Status = status,
                Notes = null
            };

            var response = await _api.UpdateStatusAsync(orderId, request);
            if (response.IsSuccessStatusCode && response.Content?.Result == true)
            {
                return Result<bool>.Success(true);
            }
            return Result<bool>.Failure("Failed to update order status");
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Error updating order status: {ex.Message}");
        }
    }

    public async Task<Result<bool>> DeleteAsync(Guid id)
    {
        try
        {
            // Note: Backend doesn't have DELETE endpoint - use status update to "CANCELLED"
            return await UpdateStatusAsync(id, "CANCELLED");
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Error deleting order: {ex.Message}");
        }
    }
}
