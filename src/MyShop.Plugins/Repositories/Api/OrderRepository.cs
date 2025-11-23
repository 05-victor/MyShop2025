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
            var response = await _api.GetAllAsync();
            
            if (response.IsSuccessStatusCode && response.Content != null)
            {
                var apiResponse = response.Content;
                if (apiResponse.Success && apiResponse.Result != null)
                {
                    return apiResponse.Result.Select(MapToOrder);
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
            // Note: Backend may need dedicated endpoint for this filter
            var response = await _api.GetAllAsync();
            
            if (response.IsSuccessStatusCode && response.Content != null)
            {
                var apiResponse = response.Content;
                if (apiResponse.Success && apiResponse.Result != null)
                {
                    return apiResponse.Result
                        .Where(o => o.CustomerId == salesAgentId) // May need adjustment based on backend schema
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
            OrderCode = dto.OrderNumber,
            CustomerId = dto.CustomerId,
            CustomerName = dto.CustomerName,
            CustomerAddress = dto.ShippingAddress,
            Status = dto.Status,
            FinalPrice = dto.TotalAmount,
            Subtotal = dto.TotalAmount, // May need adjustment if backend provides subtotal
            Notes = dto.Notes,
            OrderDate = dto.CreatedAt,
            CreatedAt = dto.CreatedAt,
            UpdatedAt = dto.UpdatedAt,
            Items = dto.Items.Select(MapToOrderItem).ToList(),
            OrderItems = dto.Items.Select(MapToOrderItem).ToList()
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
            ProductName = dto.ProductName,
            Quantity = dto.Quantity,
            UnitPrice = dto.UnitPrice,
            Total = dto.Subtotal,
            TotalPrice = dto.Subtotal
        };
    }
}
