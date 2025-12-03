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

    public async Task<IEnumerable<Order>> GetAllAsync()
    {
        try
        {
            var response = await _api.GetAllAsync(pageNumber: 1, pageSize: int.MaxValue);
            
            if (response.IsSuccessStatusCode && response.Content != null)
            {
                var apiResponse = response.Content;
                if (apiResponse.Success && apiResponse.Result != null)
                {
                    return apiResponse.Result.Items.Select(MapToOrder);
                }
            }

            return Enumerable.Empty<Order>();
        }
        catch (Exception)
        {
            return Enumerable.Empty<Order>();
        }
    }

    public async Task<Order?> GetByIdAsync(Guid id)
    {
        try
        {
            var response = await _api.GetByIdAsync(id);
            
            if (response.IsSuccessStatusCode && response.Content != null)
            {
                var apiResponse = response.Content;
                if (apiResponse.Success && apiResponse.Result != null)
                {
                    return MapToOrder(apiResponse.Result);
                }
            }

            return null;
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task<IEnumerable<Order>> GetByCustomerIdAsync(Guid customerId)
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
                    return apiResponse.Result
                        .Where(o => o.CustomerId == customerId)
                        .Select(MapToOrder);
                }
            }

            return Enumerable.Empty<Order>();
        }
        catch (Exception)
        {
            return Enumerable.Empty<Order>();
        }
    }

    public async Task<IEnumerable<Order>> GetBySalesAgentIdAsync(Guid salesAgentId)
    {
        try
        {
            var response = await _api.GetAllAsync(pageNumber: 1, pageSize: int.MaxValue);
            
            if (response.IsSuccessStatusCode && response.Content != null)
            {
                var apiResponse = response.Content;
                if (apiResponse.Success && apiResponse.Result != null)
                {
                    return apiResponse.Result.Items
                        .Where(o => o.SaleAgentId == salesAgentId)
                        .Select(MapToOrder);
                }
            }

            return Enumerable.Empty<Order>();
        }
        catch (Exception)
        {
            return Enumerable.Empty<Order>();
        }
    }

    public async Task<Order> CreateAsync(Order order)
    {
        try
        {
            var request = new CreateOrderRequest
            {
                CustomerId = order.CustomerId,
                Note = order.Notes,
                OrderItems = order.Items?.Select(item => new CreateOrderItemRequest
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitSalePrice = (int)item.UnitPrice
                }).ToList()
            };

            var response = await _api.CreateAsync(request);
            
            if (response.IsSuccessStatusCode && response.Content != null)
            {
                var apiResponse = response.Content;
                if (apiResponse.Success && apiResponse.Result != null)
                {
                    return MapToOrder(apiResponse.Result);
                }
            }

            throw new InvalidOperationException("Failed to create order");
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error creating order: {ex.Message}", ex);
        }
    }

    public async Task<Order> UpdateAsync(Order order)
    {
        try
        {
            // Note: Backend may need dedicated PUT /orders/{id} endpoint for full update
            // Currently only UpdateStatusAsync is available
            throw new NotImplementedException("Full order update not yet implemented in backend API");
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error updating order: {ex.Message}", ex);
        }
    }

    public async Task<bool> UpdateStatusAsync(Guid orderId, string status)
    {
        try
        {
            var request = new UpdateOrderStatusRequest
            {
                Status = status,
                Notes = null
            };

            var response = await _api.UpdateStatusAsync(orderId, request);
            return response.IsSuccessStatusCode && response.Content?.Result == true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        try
        {
            // Note: Backend doesn't have DELETE endpoint - use status update to "CANCELLED"
            return await UpdateStatusAsync(id, "CANCELLED");
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>
    /// Map OrderResponse DTO to Order domain model
    /// </summary>
    private static Order MapToOrder(MyShop.Shared.DTOs.Responses.OrderResponse dto)
    {
        return new Order
        {
            Id = dto.Id,
            OrderCode = dto.Id.ToString().Substring(0, 8),
            CustomerId = dto.CustomerId,
            CustomerName = dto.CustomerFullName ?? dto.CustomerUsername ?? string.Empty,
            CustomerAddress = string.Empty,
            Status = dto.Status ?? "PENDING",
            FinalPrice = dto.GrandTotal,
            Subtotal = dto.TotalAmount,
            Notes = dto.Note,
            OrderDate = dto.OrderDate,
            CreatedAt = dto.CreatedAt,
            UpdatedAt = dto.UpdatedAt,
            Items = dto.OrderItems?.Select(MapToOrderItem).ToList() ?? new List<OrderItem>(),
            OrderItems = dto.OrderItems?.Select(MapToOrderItem).ToList() ?? new List<OrderItem>()
        };
    }

    /// <summary>
    /// Map OrderItemResponse DTO to OrderItem domain model
    /// </summary>
    private static OrderItem MapToOrderItem(MyShop.Shared.DTOs.Responses.OrderItemResponse dto)
    {
        return new OrderItem
        {
            Id = dto.Id,
            ProductId = dto.ProductId,
            ProductName = dto.ProductName ?? string.Empty,
            Quantity = dto.Quantity,
            UnitPrice = dto.UnitSalePrice,
            Total = dto.TotalPrice,
            TotalPrice = dto.TotalPrice
        };
    }
}
