using MyShop.Shared.Models;
using MyShop.Core.Interfaces.Repositories;
using MyShop.Plugins.Mocks.Data;
using MyShop.Core.Common;
using MyShop.Shared.DTOs.Requests;

namespace MyShop.Plugins.Repositories.Mocks;

/// <summary>
/// Mock implementation for Order management - delegates to MockOrderData
/// </summary>
public class MockOrderRepository : IOrderRepository
{

    public async Task<Result<IEnumerable<Order>>> GetAllAsync()
    {
        try
        {
            var orders = await MockOrderData.GetAllAsync();
            System.Diagnostics.Debug.WriteLine($"[MockOrderRepository] GetAllAsync returned {orders.Count} orders");
            return Result<IEnumerable<Order>>.Success(orders);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockOrderRepository] GetAllAsync error: {ex.Message}");
            return Result<IEnumerable<Order>>.Failure($"Failed to get orders: {ex.Message}");
        }
    }

    public async Task<Result<Order>> GetByIdAsync(Guid id)
    {
        try
        {
            var order = await MockOrderData.GetByIdAsync(id);
            System.Diagnostics.Debug.WriteLine($"[MockOrderRepository] GetByIdAsync({id}) - Found: {order != null}");
            return order != null
                ? Result<Order>.Success(order)
                : Result<Order>.Failure($"Order with ID {id} not found");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockOrderRepository] GetByIdAsync error: {ex.Message}");
            return Result<Order>.Failure($"Failed to get order: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<Order>>> GetByCustomerIdAsync(Guid customerId)
    {
        try
        {
            var orders = await MockOrderData.GetByCustomerIdAsync(customerId);
            System.Diagnostics.Debug.WriteLine($"[MockOrderRepository] Found {orders.Count} orders for customer");
            return Result<IEnumerable<Order>>.Success(orders);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockOrderRepository] GetByCustomerIdAsync error: {ex.Message}");
            return Result<IEnumerable<Order>>.Failure($"Failed to get orders by customer: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<Order>>> GetBySalesAgentIdAsync(Guid salesAgentId)
    {
        try
        {
            var orders = await MockOrderData.GetBySalesAgentIdAsync(salesAgentId);
            System.Diagnostics.Debug.WriteLine($"[MockOrderRepository] Found {orders.Count} orders for sales agent");
            return Result<IEnumerable<Order>>.Success(orders);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockOrderRepository] GetBySalesAgentIdAsync error: {ex.Message}");
            return Result<IEnumerable<Order>>.Failure($"Failed to get orders by sales agent: {ex.Message}");
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
            var allOrders = await MockOrderData.GetAllAsync();
            var orders = allOrders.Where(o => o.Status == status).ToList();
            System.Diagnostics.Debug.WriteLine($"[MockOrderRepository] Found {orders.Count} orders with status: {status}");
            return Result<IEnumerable<Order>>.Success(orders);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockOrderRepository] GetByStatusAsync error: {ex.Message}");
            return Result<IEnumerable<Order>>.Failure($"Failed to get orders by status: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<Order>>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate)
    {
        try
        {
            var allOrders = await MockOrderData.GetAllAsync();
            var orders = allOrders.Where(o => o.OrderDate >= fromDate && o.OrderDate <= toDate).ToList();
            System.Diagnostics.Debug.WriteLine($"[MockOrderRepository] Found {orders.Count} orders in date range");
            return Result<IEnumerable<Order>>.Success(orders);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockOrderRepository] GetByDateRangeAsync error: {ex.Message}");
            return Result<IEnumerable<Order>>.Failure($"Failed to get orders by date range: {ex.Message}");
        }
    }

    public async Task<Result<Order>> CreateAsync(Order order)
    {
        try
        {
            order.Id = Guid.NewGuid();
            order.OrderDate = DateTime.UtcNow;
            order.Status = "CREATED";

            foreach (var item in order.OrderItems)
            {
                item.Id = Guid.NewGuid();
                item.OrderId = order.Id;
            }

            var created = await MockOrderData.CreateAsync(order);
            System.Diagnostics.Debug.WriteLine($"[MockOrderRepository] Created order: {created.Id}");
            return Result<Order>.Success(created);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockOrderRepository] CreateAsync error: {ex.Message}");
            return Result<Order>.Failure($"Failed to create order: {ex.Message}");
        }
    }

    public async Task<Result<Order>> UpdateAsync(Order order)
    {
        try
        {
            var updated = await MockOrderData.UpdateAsync(order);
            System.Diagnostics.Debug.WriteLine($"[MockOrderRepository] Updated order: {updated.Id}");
            return Result<Order>.Success(updated);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockOrderRepository] UpdateAsync error: {ex.Message}");
            return Result<Order>.Failure($"Failed to update order: {ex.Message}");
        }
    }

    public async Task<Result<bool>> UpdateStatusAsync(Guid orderId, string status)
    {
        try
        {
            var result = await MockOrderData.UpdateStatusAsync(orderId, status);
            System.Diagnostics.Debug.WriteLine($"[MockOrderRepository] UpdateStatusAsync - Success: {result}");
            return result
                ? Result<bool>.Success(true)
                : Result<bool>.Failure($"Failed to update status for order {orderId}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockOrderRepository] UpdateStatusAsync error: {ex.Message}");
            return Result<bool>.Failure($"Failed to update order status: {ex.Message}");
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
            var result = await UpdateStatusAsync(orderId, "CANCELLED");
            if (result.IsSuccess && result.Data)
            {
                var order = await MockOrderData.GetByIdAsync(orderId);
                if (order != null)
                {
                    order.Notes = $"Cancelled: {reason}";
                    await MockOrderData.UpdateAsync(order);
                }
            }
            return result;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockOrderRepository] CancelAsync error: {ex.Message}");
            return Result<bool>.Failure($"Failed to cancel order: {ex.Message}");
        }
    }

    public async Task<Result<bool>> DeleteAsync(Guid id)
    {
        try
        {
            var result = await MockOrderData.DeleteAsync(id);
            System.Diagnostics.Debug.WriteLine($"[MockOrderRepository] DeleteAsync - Success: {result}");
            return result
                ? Result<bool>.Success(true)
                : Result<bool>.Failure($"Failed to delete order with ID {id}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockOrderRepository] DeleteAsync error: {ex.Message}");
            return Result<bool>.Failure($"Failed to delete order: {ex.Message}");
        }
    }

    public async Task<decimal> GetTodayRevenueAsync()
    {
        try
        {
            var allOrders = await MockOrderData.GetAllAsync();
            var today = DateTime.Today;
            var revenue = allOrders
                .Where(o => o.OrderDate.Date == today && o.Status == "PAID")
                .Sum(o => o.FinalPrice);

            System.Diagnostics.Debug.WriteLine($"[MockOrderRepository] Today's revenue: {revenue:C}");
            return revenue;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockOrderRepository] GetTodayRevenueAsync error: {ex.Message}");
            return 0;
        }
    }

    public async Task<decimal> GetRevenueByDateRangeAsync(DateTime fromDate, DateTime toDate)
    {
        try
        {
            var allOrders = await MockOrderData.GetAllAsync();
            var revenue = allOrders
                .Where(o => o.OrderDate >= fromDate && o.OrderDate <= toDate && o.Status == "PAID")
                .Sum(o => o.FinalPrice);

            System.Diagnostics.Debug.WriteLine($"[MockOrderRepository] Revenue in date range: {revenue:C}");
            return revenue;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockOrderRepository] GetRevenueByDateRangeAsync error: {ex.Message}");
            return 0;
        }
    }

    public Task<Result<bool>> ProcessCardPaymentAsync(Guid orderId, ProcessCardPaymentRequest request)
    {
        // Mock implementation: assume payment always succeeds
        System.Diagnostics.Debug.WriteLine($"[MockOrderRepository] ProcessCardPaymentAsync called for order {orderId}");
        return Task.FromResult(Result<bool>.Success(true));
    }

    public async Task<Result<PagedList<Order>>> GetPagedAsync(
        int page = 1,
        int pageSize = 20,
        string? status = null,
        string? paymentStatus = null,
        Guid? customerId = null,
        Guid? salesAgentId = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        string sortBy = "orderDate",
        bool sortDescending = true)
    {
        try
        {
            var allOrders = await MockOrderData.GetAllAsync();
            var query = allOrders.AsEnumerable();

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

            System.Diagnostics.Debug.WriteLine($"[MockOrderRepository] GetPagedAsync: Page {page}/{pagedList.TotalPages}, Total {totalCount}");
            return Result<PagedList<Order>>.Success(pagedList);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockOrderRepository] GetPagedAsync error: {ex.Message}");
            return Result<PagedList<Order>>.Failure($"Failed to get paged orders: {ex.Message}");
        }
    }
}
