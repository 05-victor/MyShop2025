using MyShop.Shared.Models;
using MyShop.Core.Interfaces.Repositories;
using MyShop.Plugins.Mocks.Data;

namespace MyShop.Plugins.Repositories.Mocks;

/// <summary>
/// Mock implementation for Order management - delegates to MockOrderData
/// </summary>
public class MockOrderRepository : IOrderRepository
{

    public async Task<IEnumerable<Order>> GetAllAsync()
    {
        try
        {
            var orders = await MockOrderData.GetAllAsync();
            System.Diagnostics.Debug.WriteLine($"[MockOrderRepository] GetAllAsync returned {orders.Count} orders");
            return orders;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockOrderRepository] GetAllAsync error: {ex.Message}");
            return new List<Order>();
        }
    }

    public async Task<Order?> GetByIdAsync(Guid id)
    {
        try
        {
            var order = await MockOrderData.GetByIdAsync(id);
            System.Diagnostics.Debug.WriteLine($"[MockOrderRepository] GetByIdAsync({id}) - Found: {order != null}");
            return order;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockOrderRepository] GetByIdAsync error: {ex.Message}");
            return null;
        }
    }

    public async Task<IEnumerable<Order>> GetByCustomerIdAsync(Guid customerId)
    {
        try
        {
            var orders = await MockOrderData.GetByCustomerIdAsync(customerId);
            System.Diagnostics.Debug.WriteLine($"[MockOrderRepository] Found {orders.Count} orders for customer");
            return orders;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockOrderRepository] GetByCustomerIdAsync error: {ex.Message}");
            return new List<Order>();
        }
    }

    public async Task<IEnumerable<Order>> GetBySalesAgentIdAsync(Guid salesAgentId)
    {
        try
        {
            var orders = await MockOrderData.GetBySalesAgentIdAsync(salesAgentId);
            System.Diagnostics.Debug.WriteLine($"[MockOrderRepository] Found {orders.Count} orders for sales agent");
            return orders;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockOrderRepository] GetBySalesAgentIdAsync error: {ex.Message}");
            return new List<Order>();
        }
    }

    public async Task<IEnumerable<Order>> GetBySalesAgentAsync(Guid salesAgentId)
    {
        return await GetBySalesAgentIdAsync(salesAgentId);
    }

    public async Task<IEnumerable<Order>> GetByStatusAsync(string status)
    {
        try
        {
            var allOrders = await MockOrderData.GetAllAsync();
            var orders = allOrders.Where(o => o.Status == status).ToList();
            System.Diagnostics.Debug.WriteLine($"[MockOrderRepository] Found {orders.Count} orders with status: {status}");
            return orders;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockOrderRepository] GetByStatusAsync error: {ex.Message}");
            return new List<Order>();
        }
    }

    public async Task<IEnumerable<Order>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate)
    {
        try
        {
            var allOrders = await MockOrderData.GetAllAsync();
            var orders = allOrders.Where(o => o.OrderDate >= fromDate && o.OrderDate <= toDate).ToList();
            System.Diagnostics.Debug.WriteLine($"[MockOrderRepository] Found {orders.Count} orders in date range");
            return orders;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockOrderRepository] GetByDateRangeAsync error: {ex.Message}");
            return new List<Order>();
        }
    }

    public async Task<Order> CreateAsync(Order order)
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
            return created;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockOrderRepository] CreateAsync error: {ex.Message}");
            throw;
        }
    }

    public async Task<Order> UpdateAsync(Order order)
    {
        try
        {
            var updated = await MockOrderData.UpdateAsync(order);
            System.Diagnostics.Debug.WriteLine($"[MockOrderRepository] Updated order: {updated.Id}");
            return updated;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockOrderRepository] UpdateAsync error: {ex.Message}");
            throw;
        }
    }

    public async Task<bool> UpdateStatusAsync(Guid orderId, string status)
    {
        try
        {
            var result = await MockOrderData.UpdateStatusAsync(orderId, status);
            System.Diagnostics.Debug.WriteLine($"[MockOrderRepository] UpdateStatusAsync - Success: {result}");
            return result;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockOrderRepository] UpdateStatusAsync error: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> MarkAsPaidAsync(Guid orderId)
    {
        return await UpdateStatusAsync(orderId, "PAID");
    }

    public async Task<bool> CancelAsync(Guid orderId, string reason)
    {
        try
        {
            var result = await UpdateStatusAsync(orderId, "CANCELLED");
            if (result)
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
            return false;
        }
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        try
        {
            var result = await MockOrderData.DeleteAsync(id);
            System.Diagnostics.Debug.WriteLine($"[MockOrderRepository] DeleteAsync - Success: {result}");
            return result;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockOrderRepository] DeleteAsync error: {ex.Message}");
            return false;
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
}
