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
            var response = await _api.GetAllAsync(pageNumber: 1, pageSize: int.MaxValue);
            
            if (response.IsSuccessStatusCode && response.Content != null)
            {
                var apiResponse = response.Content;
                if (apiResponse.Success && apiResponse.Result != null)
                {
                    var orders = OrderAdapter.ToModelList(apiResponse.Result.Items);
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
            var response = await _api.GetAllAsync(pageNumber: 1, pageSize: int.MaxValue);
            
            if (response.IsSuccessStatusCode && response.Content != null)
            {
                var apiResponse = response.Content;
                if (apiResponse.Success && apiResponse.Result != null)
                {
                    var orders = apiResponse.Result.Items
                        .Where(o => o.SaleAgentId == salesAgentId)
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

    public async Task<Result<IEnumerable<Order>>> GetBySalesAgentAsync(Guid salesAgentId)
    {
        return await GetBySalesAgentIdAsync(salesAgentId);
    }

    public async Task<Result<IEnumerable<Order>>> GetByStatusAsync(string status)
    {
        try
        {
            var allOrdersResult = await GetAllAsync();
            if (!allOrdersResult.IsSuccess || allOrdersResult.Data == null)
            {
                return Result<IEnumerable<Order>>.Failure("Failed to retrieve orders");
            }

            var filteredOrders = allOrdersResult.Data.Where(o => o.Status.Equals(status, StringComparison.OrdinalIgnoreCase)).ToList();
            return Result<IEnumerable<Order>>.Success(filteredOrders);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<Order>>.Failure($"Error retrieving orders by status: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<Order>>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate)
    {
        try
        {
            var allOrdersResult = await GetAllAsync();
            if (!allOrdersResult.IsSuccess || allOrdersResult.Data == null)
            {
                return Result<IEnumerable<Order>>.Failure("Failed to retrieve orders");
            }

            var filteredOrders = allOrdersResult.Data.Where(o => o.OrderDate >= fromDate && o.OrderDate <= toDate).ToList();
            return Result<IEnumerable<Order>>.Success(filteredOrders);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<Order>>.Failure($"Error retrieving orders by date range: {ex.Message}");
        }
    }

    public async Task<Result<Order>> CreateAsync(Order order)
    {
        try
        {
            var request = new CreateOrderRequest
            {
                CustomerId = order.CustomerId ?? Guid.Empty,
                OrderItems = order.Items.Select(item => new CreateOrderItemRequest
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    UnitSalePrice = (int)item.UnitPrice
                }).ToList(),
                ShippingAddress = order.CustomerAddress,
                PaymentMethod = "CASH",
                Note = order.Notes
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

    public async Task<Result<bool>> MarkAsPaidAsync(Guid orderId)
    {
        return await UpdateStatusAsync(orderId, "PAID");
    }

    public async Task<Result<bool>> CancelAsync(Guid orderId, string reason)
    {
        try
        {
            var request = new UpdateOrderStatusRequest
            {
                Status = "CANCELLED",
                Notes = $"Cancelled: {reason}"
            };

            var response = await _api.UpdateStatusAsync(orderId, request);
            if (response.IsSuccessStatusCode && response.Content?.Result == true)
            {
                return Result<bool>.Success(true);
            }
            return Result<bool>.Failure("Failed to cancel order");
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Error cancelling order: {ex.Message}");
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

    public async Task<decimal> GetTodayRevenueAsync()
    {
        try
        {
            var allOrdersResult = await GetAllAsync();
            if (!allOrdersResult.IsSuccess || allOrdersResult.Data == null)
            {
                return 0;
            }

            var today = DateTime.Today;
            var revenue = allOrdersResult.Data
                .Where(o => o.OrderDate.Date == today && o.Status == "PAID")
                .Sum(o => o.FinalPrice);

            return revenue;
        }
        catch
        {
            return 0;
        }
    }

    public async Task<decimal> GetRevenueByDateRangeAsync(DateTime fromDate, DateTime toDate)
    {
        try
        {
            var allOrdersResult = await GetAllAsync();
            if (!allOrdersResult.IsSuccess || allOrdersResult.Data == null)
            {
                return 0;
            }

            var revenue = allOrdersResult.Data
                .Where(o => o.OrderDate >= fromDate && o.OrderDate <= toDate && o.Status == "PAID")
                .Sum(o => o.FinalPrice);

            return revenue;
        }
        catch
        {
            return 0;
        }
    }

    public async Task<Result<PagedList<Order>>> GetPagedAsync(
        int page = 1,
        int pageSize = 20,
        string? status = null,
        Guid? customerId = null,
        Guid? salesAgentId = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        string sortBy = "orderDate",
        bool sortDescending = true)
    {
        try
        {
            // Note: Backend API doesn't support server-side paging yet
            // Fallback: fetch all orders and apply client-side paging/filtering
            var allOrdersResult = await GetAllAsync();
            if (!allOrdersResult.IsSuccess || allOrdersResult.Data == null)
            {
                return Result<PagedList<Order>>.Failure(allOrdersResult.ErrorMessage ?? "Failed to retrieve orders");
            }

            var query = allOrdersResult.Data.AsEnumerable();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(o => o.Status.Equals(status, StringComparison.OrdinalIgnoreCase));
            }

            if (customerId.HasValue)
            {
                query = query.Where(o => o.CustomerId == customerId.Value);
            }

            if (salesAgentId.HasValue)
            {
                query = query.Where(o => o.SalesAgentId == salesAgentId.Value);
            }

            if (startDate.HasValue)
            {
                query = query.Where(o => o.OrderDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(o => o.OrderDate <= endDate.Value);
            }

            // Apply sorting
            query = sortBy.ToLower() switch
            {
                "orderdate" => sortDescending 
                    ? query.OrderByDescending(o => o.OrderDate) 
                    : query.OrderBy(o => o.OrderDate),
                "finalprice" or "amount" => sortDescending 
                    ? query.OrderByDescending(o => o.FinalPrice) 
                    : query.OrderBy(o => o.FinalPrice),
                "status" => sortDescending 
                    ? query.OrderByDescending(o => o.Status) 
                    : query.OrderBy(o => o.Status),
                _ => sortDescending 
                    ? query.OrderByDescending(o => o.OrderDate) 
                    : query.OrderBy(o => o.OrderDate)
            };

            var totalCount = query.Count();
            var items = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            var pagedList = new PagedList<Order>(items, totalCount, page, pageSize);
            return Result<PagedList<Order>>.Success(pagedList);
        }
        catch (Exception ex)
        {
            return Result<PagedList<Order>>.Failure($"Error retrieving paged orders: {ex.Message}");
        }
    }
}
