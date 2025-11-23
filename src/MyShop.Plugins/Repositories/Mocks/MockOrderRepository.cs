using System.Text.Json;
using MyShop.Shared.Models;
using System.Diagnostics;
using MyShop.Core.Interfaces.Repositories;

namespace MyShop.Plugins.Repositories.Mocks;

/// <summary>
/// Mock implementation for Order management using JSON data
/// </summary>
public class MockOrderRepository : IOrderRepository
{
    private readonly List<Order> _orders;
    private readonly string _jsonFilePath;

    public MockOrderRepository()
    {
        _jsonFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Mocks", "Data", "Json", "orders.json");
        _orders = LoadOrdersFromJson();
    }

    private List<Order> LoadOrdersFromJson()
    {
        try
        {
            if (!File.Exists(_jsonFilePath))
            {
                System.Diagnostics.Debug.WriteLine($"[MockOrderRepository] JSON file not found: {_jsonFilePath}");
                return new List<Order>();
            }

            var json = File.ReadAllText(_jsonFilePath);
            var jsonDoc = JsonDocument.Parse(json);
            var ordersArray = jsonDoc.RootElement.GetProperty("orders");

            var orders = new List<Order>();

            foreach (var item in ordersArray.EnumerateArray())
            {
                var order = new Order
                {
                    Id = Guid.Parse(item.GetProperty("id").GetString()!),
                    OrderDate = DateTime.Parse(item.GetProperty("orderDate").GetString()!),
                    Status = item.GetProperty("status").GetString()!,
                    CustomerName = item.GetProperty("customerName").GetString() ?? string.Empty,
                    CustomerPhone = item.GetProperty("customerPhone").GetString(),
                    CustomerAddress = item.GetProperty("customerAddress").GetString(),
                    SalesAgentId = Guid.Parse(item.GetProperty("salesAgentId").GetString()!),
                    Subtotal = item.GetProperty("subtotal").GetDecimal(),
                    Discount = item.GetProperty("discount").GetDecimal(),
                    FinalPrice = item.GetProperty("finalPrice").GetDecimal(),
                    Notes = item.TryGetProperty("notes", out var notes) && notes.ValueKind != JsonValueKind.Null
                        ? notes.GetString()
                        : null,
                    CreatedAt = DateTime.Parse(item.GetProperty("createdAt").GetString()!),
                    UpdatedAt = item.TryGetProperty("updatedAt", out var updatedAt) && updatedAt.ValueKind != JsonValueKind.Null
                        ? DateTime.Parse(updatedAt.GetString()!)
                        : null,
                    OrderItems = new List<OrderItem>()
                };

                // Load order items
                if (item.TryGetProperty("items", out var itemsArray))
                {
                    foreach (var orderItem in itemsArray.EnumerateArray())
                    {
                        var oi = new OrderItem
                        {
                            Id = Guid.Parse(orderItem.GetProperty("id").GetString()!),
                            OrderId = order.Id,
                            ProductId = Guid.Parse(orderItem.GetProperty("productId").GetString()!),
                            Quantity = orderItem.GetProperty("quantity").GetInt32(),
                            UnitPrice = orderItem.GetProperty("unitPrice").GetDecimal(),
                            TotalPrice = orderItem.GetProperty("totalPrice").GetDecimal()
                        };
                        order.OrderItems.Add(oi);
                    }
                }

                orders.Add(order);
            }

            System.Diagnostics.Debug.WriteLine($"[MockOrderRepository] Loaded {orders.Count} orders from JSON");
            return orders;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockOrderRepository] Error loading JSON: {ex.Message}");
            return new List<Order>();
        }
    }

    public async Task<IEnumerable<Order>> GetAllAsync()
    {
        await Task.Delay(350);
        System.Diagnostics.Debug.WriteLine($"[MockOrderRepository] GetAllAsync called, returning {_orders.Count} orders");
        return _orders.ToList();
    }

    public async Task<Order?> GetByIdAsync(Guid id)
    {
        await Task.Delay(250);
        var order = _orders.FirstOrDefault(o => o.Id == id);
        System.Diagnostics.Debug.WriteLine($"[MockOrderRepository] GetByIdAsync({id}) - Found: {order != null}");
        return order;
    }

    public async Task<IEnumerable<Order>> GetByCustomerIdAsync(Guid customerId)
    {
        await Task.Delay(300);
        var orders = _orders.Where(o => o.CustomerId == customerId).ToList();
        System.Diagnostics.Debug.WriteLine($"[MockOrderRepository] Found {orders.Count} orders for customer: {customerId}");
        return orders;
    }

    public async Task<IEnumerable<Order>> GetBySalesAgentIdAsync(Guid salesAgentId)
    {
        await Task.Delay(300);
        var orders = _orders.Where(o => o.SalesAgentId == salesAgentId).ToList();
        System.Diagnostics.Debug.WriteLine($"[MockOrderRepository] Found {orders.Count} orders for sales agent: {salesAgentId}");
        return orders;
    }

    public async Task<IEnumerable<Order>> GetBySalesAgentAsync(Guid salesAgentId)
    {
        await Task.Delay(300);
        var orders = _orders.Where(o => o.SalesAgentId == salesAgentId).ToList();
        System.Diagnostics.Debug.WriteLine($"[MockOrderRepository] Found {orders.Count} orders for sales agent: {salesAgentId}");
        return orders;
    }

    public async Task<IEnumerable<Order>> GetByStatusAsync(string status)
    {
        await Task.Delay(250);
        var orders = _orders.Where(o => o.Status == status).ToList();
        System.Diagnostics.Debug.WriteLine($"[MockOrderRepository] Found {orders.Count} orders with status: {status}");
        return orders;
    }

    public async Task<IEnumerable<Order>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate)
    {
        await Task.Delay(300);
        var orders = _orders.Where(o => o.OrderDate >= fromDate && o.OrderDate <= toDate).ToList();
        System.Diagnostics.Debug.WriteLine($"[MockOrderRepository] Found {orders.Count} orders between {fromDate:yyyy-MM-dd} and {toDate:yyyy-MM-dd}");
        return orders;
    }

    public async Task<Order> CreateAsync(Order order)
    {
        await Task.Delay(600);
        
        order.Id = Guid.NewGuid();
        order.OrderDate = DateTime.UtcNow;
        order.CreatedAt = DateTime.UtcNow;
        order.Status = "CREATED";
        
        // Generate IDs for order items
        foreach (var item in order.OrderItems)
        {
            item.Id = Guid.NewGuid();
            item.OrderId = order.Id;
        }
        
        _orders.Add(order);
        
        System.Diagnostics.Debug.WriteLine($"[MockOrderRepository] Created order: {order.Id} for customer: {order.CustomerName}");
        return order;
    }

    public async Task<Order> UpdateAsync(Order order)
    {
        await Task.Delay(500);
        
        var existingOrder = _orders.FirstOrDefault(o => o.Id == order.Id);
        if (existingOrder == null)
        {
            throw new InvalidOperationException($"Order with ID {order.Id} not found");
        }

        // Update properties
        existingOrder.CustomerName = order.CustomerName;
        existingOrder.CustomerPhone = order.CustomerPhone;
        existingOrder.CustomerAddress = order.CustomerAddress;
        existingOrder.Subtotal = order.Subtotal;
        existingOrder.Discount = order.Discount;
        existingOrder.FinalPrice = order.FinalPrice;
        existingOrder.Notes = order.Notes;
        existingOrder.UpdatedAt = DateTime.UtcNow;

        System.Diagnostics.Debug.WriteLine($"[MockOrderRepository] Updated order: {existingOrder.Id}");
        return existingOrder;
    }

    public async Task<bool> UpdateStatusAsync(Guid orderId, string status)
    {
        await Task.Delay(400);
        
        var order = _orders.FirstOrDefault(o => o.Id == orderId);
        if (order == null) return false;

        order.Status = status;
        order.UpdatedAt = DateTime.UtcNow;
        
        System.Diagnostics.Debug.WriteLine($"[MockOrderRepository] Updated order {orderId} status to: {status}");
        return true;
    }

    public async Task<bool> MarkAsPaidAsync(Guid orderId)
    {
        await Task.Delay(400);
        
        var order = _orders.FirstOrDefault(o => o.Id == orderId);
        if (order == null) return false;

        order.Status = "PAID";
        order.UpdatedAt = DateTime.UtcNow;
        
        System.Diagnostics.Debug.WriteLine($"[MockOrderRepository] Marked order {orderId} as PAID");
        return true;
    }

    public async Task<bool> CancelAsync(Guid orderId, string reason)
    {
        await Task.Delay(400);
        
        var order = _orders.FirstOrDefault(o => o.Id == orderId);
        if (order == null) return false;

        order.Status = "CANCELLED";
        order.Notes = $"Cancelled: {reason}";
        order.UpdatedAt = DateTime.UtcNow;
        
        System.Diagnostics.Debug.WriteLine($"[MockOrderRepository] Cancelled order {orderId}. Reason: {reason}");
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        await Task.Delay(350);
        
        var order = _orders.FirstOrDefault(o => o.Id == id);
        if (order == null) return false;

        _orders.Remove(order);
        System.Diagnostics.Debug.WriteLine($"[MockOrderRepository] Deleted order: {id}");
        return true;
    }

    /// <summary>
    /// Get today's revenue
    /// </summary>
    public async Task<decimal> GetTodayRevenueAsync()
    {
        await Task.Delay(200);
        var today = DateTime.Today;
        var revenue = _orders
            .Where(o => o.OrderDate.Date == today && o.Status == "PAID")
            .Sum(o => o.FinalPrice);
        
        System.Diagnostics.Debug.WriteLine($"[MockOrderRepository] Today's revenue: {revenue:C}");
        return revenue;
    }

    /// <summary>
    /// Get revenue for date range
    /// </summary>
    public async Task<decimal> GetRevenueByDateRangeAsync(DateTime fromDate, DateTime toDate)
    {
        await Task.Delay(250);
        var revenue = _orders
            .Where(o => o.OrderDate >= fromDate && o.OrderDate <= toDate && o.Status == "PAID")
            .Sum(o => o.FinalPrice);
        
        System.Diagnostics.Debug.WriteLine($"[MockOrderRepository] Revenue from {fromDate:yyyy-MM-dd} to {toDate:yyyy-MM-dd}: {revenue:C}");
        return revenue;
    }
}
