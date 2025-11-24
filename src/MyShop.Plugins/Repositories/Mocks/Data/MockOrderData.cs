using MyShop.Shared.Models;
using System.Text.Json;

namespace MyShop.Plugins.Mocks.Data;

/// <summary>
/// Mock data provider for orders - loads from JSON file
/// </summary>
public static class MockOrderData
{
    private static List<OrderDataModel>? _orders;
    private static readonly object _lock = new object();
    private static readonly string _jsonFilePath = Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory, "Mocks", "Data", "Json", "orders.json");

    private static void EnsureDataLoaded()
    {
        if (_orders != null) return;

        lock (_lock)
        {
            if (_orders != null) return;

            try
            {
                if (!File.Exists(_jsonFilePath))
                {
                    System.Diagnostics.Debug.WriteLine($"Orders JSON file not found at: {_jsonFilePath}");
                    InitializeDefaultData();
                    return;
                }

                var jsonString = File.ReadAllText(_jsonFilePath);
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var data = JsonSerializer.Deserialize<OrderDataContainer>(jsonString, options);

                if (data?.Orders != null)
                {
                    _orders = data.Orders;
                    System.Diagnostics.Debug.WriteLine($"Loaded {_orders.Count} orders from orders.json");
                }
                else
                {
                    InitializeDefaultData();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading orders.json: {ex.Message}");
                InitializeDefaultData();
            }
        }
    }

    private static void InitializeDefaultData()
    {
        _orders = new List<OrderDataModel>
        {
            new OrderDataModel
            {
                Id = "30000000-0000-0000-0000-000000000001",
                CustomerId = "00000000-0000-0000-0000-000000000012",
                CustomerName = "Nguyễn Văn A",
                SalesAgentId = "00000000-0000-0000-0000-000000000002",
                FinalPrice = 29990000,
                Status = "PAID",
                CreatedAt = DateTime.Parse("2025-11-05T14:30:00Z"),
                Items = new List<OrderItemDataModel>()
            }
        };
    }

    public static async Task<List<Order>> GetAllAsync()
    {
        EnsureDataLoaded();

        // Simulate network delay
        await Task.Delay(400);

        return _orders!.Select(MapToOrder).ToList();
    }

    public static async Task<Order?> GetByIdAsync(Guid id)
    {
        EnsureDataLoaded();

        // Simulate network delay
        await Task.Delay(250);

        var orderData = _orders!.FirstOrDefault(o => o.Id == id.ToString());
        if (orderData == null) return null;

        return MapToOrder(orderData);
    }

    public static async Task<Order> CreateAsync(Order order)
    {
        EnsureDataLoaded();

        // Simulate network delay
        await Task.Delay(600);

        var newOrderData = new OrderDataModel
        {
            Id = order.Id.ToString(),
            CustomerId = order.CustomerId?.ToString(),
            CustomerName = order.CustomerName,
            CustomerPhone = order.CustomerPhone,
            CustomerAddress = order.CustomerAddress,
            SalesAgentId = order.SalesAgentId.ToString(),
            FinalPrice = order.FinalPrice,
            Status = order.Status,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null,
            Items = order.OrderItems?.Select(i => new OrderItemDataModel
            {
                Id = i.Id.ToString(),
                OrderId = i.OrderId.ToString(),
                ProductId = i.ProductId.ToString(),
                ProductName = i.ProductName,
                Quantity = i.Quantity,
                UnitSalePrice = i.UnitPrice,
                TotalPrice = i.TotalPrice,
                CreatedAt = DateTime.UtcNow
            }).ToList() ?? new List<OrderItemDataModel>()
        };

        _orders!.Add(newOrderData);

        // Persist to JSON
        await SaveDataToJsonAsync();

        return order;
    }

    public static async Task<Order> UpdateAsync(Order order)
    {
        EnsureDataLoaded();

        // Simulate network delay
        await Task.Delay(450);

        var existing = _orders!.FirstOrDefault(o => o.Id == order.Id.ToString());
        if (existing == null)
        {
            throw new InvalidOperationException($"Order with ID {order.Id} not found");
        }

        // Update properties
        existing.CustomerName = order.CustomerName;
        existing.CustomerPhone = order.CustomerPhone;
        existing.CustomerAddress = order.CustomerAddress;
        existing.Status = order.Status;
        existing.FinalPrice = order.FinalPrice;
        existing.UpdatedAt = DateTime.UtcNow;

        // Persist to JSON
        await SaveDataToJsonAsync();

        return order;
    }

    public static async Task<bool> DeleteAsync(Guid id)
    {
        EnsureDataLoaded();

        // Simulate network delay
        await Task.Delay(350);

        var order = _orders!.FirstOrDefault(o => o.Id == id.ToString());
        if (order == null) return false;

        _orders.Remove(order);

        // Persist to JSON
        await SaveDataToJsonAsync();

        return true;
    }

    public static async Task<bool> UpdateStatusAsync(Guid orderId, string status)
    {
        EnsureDataLoaded();

        // Simulate network delay
        await Task.Delay(300);

        var order = _orders!.FirstOrDefault(o => o.Id == orderId.ToString());
        if (order == null) return false;

        order.Status = status;
        order.UpdatedAt = DateTime.UtcNow;

        // Persist to JSON
        await SaveDataToJsonAsync();

        return true;
    }

    public static async Task<List<Order>> GetByCustomerIdAsync(Guid customerId)
    {
        EnsureDataLoaded();

        // Simulate network delay
        await Task.Delay(300);

        return _orders!
            .Where(o => o.CustomerId == customerId.ToString())
            .Select(MapToOrder)
            .ToList();
    }

    public static async Task<List<Order>> GetBySalesAgentIdAsync(Guid salesAgentId)
    {
        EnsureDataLoaded();

        // Simulate network delay
        await Task.Delay(300);

        return _orders!
            .Where(o => o.SalesAgentId == salesAgentId.ToString())
            .Select(MapToOrder)
            .ToList();
    }

    private static Order MapToOrder(OrderDataModel data)
    {
        return new Order
        {
            Id = Guid.Parse(data.Id),
            OrderDate = data.OrderDate ?? data.CreatedAt,
            CustomerId = !string.IsNullOrEmpty(data.CustomerId) ? Guid.Parse(data.CustomerId) : null,
            CustomerName = data.CustomerName ?? string.Empty,
            CustomerPhone = data.CustomerPhone,
            CustomerAddress = data.CustomerAddress,
            SalesAgentId = !string.IsNullOrEmpty(data.SalesAgentId) ? Guid.Parse(data.SalesAgentId) : null,
            Subtotal = data.Subtotal,
            Discount = data.Discount,
            FinalPrice = data.FinalPrice,
            Status = data.Status,
            Notes = data.Notes,
            CreatedAt = data.CreatedAt,
            UpdatedAt = data.UpdatedAt,
            OrderItems = data.Items?.Select(i => new OrderItem
            {
                Id = Guid.Parse(i.Id),
                OrderId = Guid.Parse(i.OrderId),
                ProductId = Guid.Parse(i.ProductId),
                ProductName = i.ProductName,
                Quantity = i.Quantity,
                UnitPrice = i.UnitSalePrice,
                TotalPrice = i.TotalPrice
            }).ToList() ?? new List<OrderItem>()
        };
    }

    private static async Task SaveDataToJsonAsync()
    {
        try
        {
            var container = new OrderDataContainer
            {
                Orders = _orders!
            };

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var jsonString = JsonSerializer.Serialize(container, options);
            await File.WriteAllTextAsync(_jsonFilePath, jsonString);

            System.Diagnostics.Debug.WriteLine("Successfully saved orders data to JSON");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving orders.json: {ex.Message}");
        }
    }

    // Data container classes for JSON deserialization
    private class OrderDataContainer
    {
        public List<OrderDataModel> Orders { get; set; } = new();
    }

    private class OrderDataModel
    {
        public string Id { get; set; } = string.Empty;
        public DateTime? OrderDate { get; set; }
        public string? CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public string? CustomerPhone { get; set; }
        public string? CustomerAddress { get; set; }
        public string SalesAgentId { get; set; } = string.Empty;
        public decimal Subtotal { get; set; }
        public decimal Discount { get; set; }
        public decimal FinalPrice { get; set; }
        public string Status { get; set; } = "PENDING";
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<OrderItemDataModel>? Items { get; set; }
    }

    private class OrderItemDataModel
    {
        public string Id { get; set; } = string.Empty;
        public string OrderId { get; set; } = string.Empty;
        public string ProductId { get; set; } = string.Empty;
        public string? ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitSalePrice { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
