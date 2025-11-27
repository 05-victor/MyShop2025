using MyShop.Core.Common;
using MyShop.Core.Interfaces.Facades;
using MyShop.Core.Interfaces.Repositories;
using MyShop.Core.Interfaces.Services;
using MyShop.Shared.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShop.Client.Facades;

/// <summary>
/// Full implementation of IOrderFacade
/// Aggregates: IOrderRepository, IProductRepository, IValidationService, IToastService
/// </summary>
public class OrderFacade : IOrderFacade
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly IValidationService _validationService;
    private readonly IToastService _toastService;
    private readonly IExportService _exportService;

    public OrderFacade(
        IOrderRepository orderRepository,
        IProductRepository productRepository,
        IValidationService validationService,
        IToastService toastService,
        IExportService exportService)
    {
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
        _validationService = validationService ?? throw new ArgumentNullException(nameof(validationService));
        _toastService = toastService ?? throw new ArgumentNullException(nameof(toastService));
        _exportService = exportService ?? throw new ArgumentNullException(nameof(exportService));
    }

    public async Task<Result<PagedList<Order>>> LoadOrdersAsync(
        string? searchQuery = null,
        string? status = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int page = 1,
        int pageSize = 20)
    {
        try
        {
            // Validate paging parameters
            if (page < 1)
            {
                _toastService.ShowError("Page number must be at least 1");
                return Result<PagedList<Order>>.Failure("Invalid page number");
            }

            if (pageSize < 1 || pageSize > 100)
            {
                _toastService.ShowError("Page size must be between 1 and 100");
                return Result<PagedList<Order>>.Failure("Invalid page size");
            }

            // Validate date range
            if (startDate.HasValue && endDate.HasValue && startDate.Value > endDate.Value)
            {
                _toastService.ShowError("Start date cannot be after end date");
                return Result<PagedList<Order>>.Failure("Invalid date range");
            }

            // Get all orders from repository
            var result = await _orderRepository.GetAllAsync();
            if (!result.IsSuccess || result.Data == null)
            {
                _toastService.ShowError("Failed to load orders");
                return Result<PagedList<Order>>.Failure(result.ErrorMessage ?? "Failed to load orders");
            }

            var orders = result.Data;

            // Apply filters
            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                var query = searchQuery.ToLower();
                orders = orders.Where(o =>
                    o.Id.ToString().Contains(query) ||
                    (o.CustomerName?.ToLower().Contains(query) ?? false) ||
                    (o.CustomerPhone?.ToLower().Contains(query) ?? false) ||
                    (o.CustomerAddress?.ToLower().Contains(query) ?? false)
                ).ToList();
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                orders = orders.Where(o => o.Status?.Equals(status, StringComparison.OrdinalIgnoreCase) ?? false).ToList();
            }

            if (startDate.HasValue)
            {
                orders = orders.Where(o => o.OrderDate >= startDate.Value).ToList();
            }

            if (endDate.HasValue)
            {
                orders = orders.Where(o => o.OrderDate <= endDate.Value).ToList();
            }

            // Order by date descending
            orders = orders.OrderByDescending(o => o.OrderDate).ToList();

            // Calculate paging
            var totalItems = orders.Count();
            var pagedOrders = orders.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            var pagedResult = new PagedList<Order>(pagedOrders, totalItems, page, pageSize);

            System.Diagnostics.Debug.WriteLine($"[OrderFacade] Loaded {pagedOrders.Count} orders (page {page}/{pagedResult.TotalPages})");
            return Result<PagedList<Order>>.Success(pagedResult);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[OrderFacade] Error loading orders: {ex.Message}");
            await _toastService.ShowError($"Error loading orders: {ex.Message}");
            return Result<PagedList<Order>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<PagedList<Order>>> LoadOrdersPagedAsync(
        int page = 1,
        int pageSize = Core.Common.PaginationConstants.OrdersPageSize,
        string? status = null,
        string? searchQuery = null,
        string sortBy = "orderDate",
        bool sortDescending = true)
    {
        return await LoadOrdersAsync(searchQuery, status, null, null, page, pageSize);
    }

    public Task<Result<Order>> GetOrderByIdAsync(Guid orderId)
    {
        return _orderRepository.GetByIdAsync(orderId);
    }

    public async Task<Result<Order>> CreateOrderAsync(
        List<(Guid ProductId, int Quantity)> items,
        string shippingAddress,
        string notes)
    {
        try
        {
            // Create order object
            var order = new Order
            {
                Id = Guid.NewGuid(),
                OrderCode = $"ORD-{DateTime.Now:yyyyMMddHHmmss}",
                CustomerAddress = shippingAddress,
                Notes = notes,
                OrderDate = DateTime.Now,
                CreatedAt = DateTime.Now,
                Status = "CREATED",
                OrderItems = items.Select(i => new OrderItem
                {
                    Id = Guid.NewGuid(),
                    ProductId = i.ProductId,
                    Quantity = i.Quantity
                }).ToList()
            };

            return await _orderRepository.CreateAsync(order);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[OrderFacade] Error creating order: {ex.Message}");
            _toastService.ShowError($"Error creating order: {ex.Message}");
            return Result<Order>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<Order>> UpdateOrderStatusAsync(Guid orderId, string newStatus, string? reason = null)
    {
        try
        {
            // Validate status
            var validStatuses = new[] { "Pending", "Processing", "Shipping", "Completed", "Cancelled" };
            if (!validStatuses.Contains(newStatus, StringComparer.OrdinalIgnoreCase))
            {
                _toastService.ShowError($"Invalid status. Valid values: {string.Join(", ", validStatuses)}");
                return Result<Order>.Failure("Invalid order status");
            }

            // Get current order
            var orderResult = await _orderRepository.GetByIdAsync(orderId);
            if (!orderResult.IsSuccess || orderResult.Data == null)
            {
                _toastService.ShowError("Order not found");
                return Result<Order>.Failure("Order not found");
            }

            var order = orderResult.Data;

            // Validate status transition
            if (order.Status == "Completed" && newStatus != "Completed")
            {
                _toastService.ShowError("Cannot change status of completed order");
                return Result<Order>.Failure("Order already completed");
            }

            if (order.Status == "Cancelled")
            {
                _toastService.ShowError("Cannot change status of cancelled order");
                return Result<Order>.Failure("Order already cancelled");
            }

            // Update status
            order.Status = newStatus;
            order.UpdatedAt = DateTime.UtcNow;

            var updateResult = await _orderRepository.UpdateAsync(order);
            if (!updateResult.IsSuccess)
            {
                _toastService.ShowError("Failed to update order status");
                return Result<Order>.Failure("Failed to update status");
            }

            _toastService.ShowSuccess($"Order status updated to {newStatus}");
            System.Diagnostics.Debug.WriteLine($"[OrderFacade] Order {orderId} status updated to {newStatus}");
            return Result<Order>.Success(order);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[OrderFacade] Error updating status: {ex.Message}");
            _toastService.ShowError($"Error updating status: {ex.Message}");
            return Result<Order>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<Unit>> CancelOrderAsync(Guid orderId, string reason)
    {
        try
        {
            // Validate reason
            if (string.IsNullOrWhiteSpace(reason))
            {
                _toastService.ShowError("Please provide a reason for cancellation");
                return Result<Unit>.Failure("Cancellation reason is required");
            }

            // Get order
            var orderResult = await _orderRepository.GetByIdAsync(orderId);
            if (!orderResult.IsSuccess || orderResult.Data == null)
            {
                _toastService.ShowError("Order not found");
                return Result<Unit>.Failure("Order not found");
            }

            var order = orderResult.Data;

            // Validate can cancel
            if (order.Status == "Cancelled")
            {
                _toastService.ShowError("Order is already cancelled");
                return Result<Unit>.Failure("Order already cancelled");
            }

            if (order.Status == "Completed")
            {
                _toastService.ShowError("Cannot cancel completed order");
                return Result<Unit>.Failure("Cannot cancel completed order");
            }

            // Cancel order
            order.Status = "Cancelled";
            order.UpdatedAt = DateTime.UtcNow;

            var updateResult = await _orderRepository.UpdateAsync(order);
            if (!updateResult.IsSuccess)
            {
                _toastService.ShowError("Failed to cancel order");
                return Result<Unit>.Failure("Failed to cancel order");
            }

            _toastService.ShowSuccess($"Order cancelled: {reason}");
            System.Diagnostics.Debug.WriteLine($"[OrderFacade] Order {orderId} cancelled. Reason: {reason}");
            return Result<Unit>.Success(Unit.Value);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[OrderFacade] Error cancelling order: {ex.Message}");
            _toastService.ShowError($"Error cancelling order: {ex.Message}");
            return Result<Unit>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<Order>>> GetOrdersByCustomerAsync(Guid customerId)
    {
        try
        {
            var result = await _orderRepository.GetAllAsync();
            if (!result.IsSuccess || result.Data == null)
            {
                _toastService.ShowError("Failed to load customer orders");
                return Result<List<Order>>.Failure("Failed to load orders");
            }

            var customerOrders = result.Data
                .Where(o => o.CustomerId == customerId)
                .OrderByDescending(o => o.OrderDate)
                .ToList();

            System.Diagnostics.Debug.WriteLine($"[OrderFacade] Loaded {customerOrders.Count} orders for customer {customerId}");
            return Result<List<Order>>.Success(customerOrders);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[OrderFacade] Error loading customer orders: {ex.Message}");
            _toastService.ShowError($"Error loading orders: {ex.Message}");
            return Result<List<Order>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<Order>>> GetOrdersBySalesAgentAsync(Guid agentId)
    {
        try
        {
            var result = await _orderRepository.GetAllAsync();
            if (!result.IsSuccess || result.Data == null)
            {
                _toastService.ShowError("Failed to load agent orders");
                return Result<List<Order>>.Failure("Failed to load orders");
            }

            var agentOrders = result.Data
                .Where(o => o.SalesAgentId == agentId)
                .OrderByDescending(o => o.OrderDate)
                .ToList();

            System.Diagnostics.Debug.WriteLine($"[OrderFacade] Loaded {agentOrders.Count} orders for agent {agentId}");
            return Result<List<Order>>.Success(agentOrders);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[OrderFacade] Error loading agent orders: {ex.Message}");
            _toastService.ShowError($"Error loading orders: {ex.Message}");
            return Result<List<Order>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<string>> ExportOrdersToCsvAsync(
        string? status = null,
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        try
        {
            // Load orders with filters
            var result = await LoadOrdersAsync(null, status, startDate, endDate, 1, int.MaxValue);
            if (!result.IsSuccess || result.Data?.Items == null)
            {
                _toastService.ShowError("Failed to load orders for export");
                return Result<string>.Failure("Failed to load orders");
            }

            var orders = result.Data.Items;

            // Use ExportService to generate CSV
            var exportResult = await _exportService.ExportToCsvAsync(
                orders,
                "Orders",
                order => new Dictionary<string, string>
                {
                    ["Order ID"] = order.Id.ToString(),
                    ["Order Code"] = order.OrderCode ?? string.Empty,
                    ["Order Date"] = order.OrderDate.ToString("yyyy-MM-dd HH:mm"),
                    ["Customer Name"] = order.CustomerName ?? string.Empty,
                    ["Customer Phone"] = order.CustomerPhone ?? string.Empty,
                    ["Customer Address"] = order.CustomerAddress ?? string.Empty,
                    ["Status"] = order.Status ?? string.Empty,
                    ["Subtotal"] = order.Subtotal.ToString("F2"),
                    ["Final Price"] = order.FinalPrice.ToString("F2"),
                    ["Sales Agent"] = order.SalesAgentName ?? string.Empty,
                    ["Items Count"] = (order.Items?.Count ?? 0).ToString()
                });

            if (!exportResult.IsSuccess)
            {
                _toastService.ShowError("Failed to export orders");
                return exportResult;
            }

            _toastService.ShowSuccess($"Exported {orders.Count} orders");
            System.Diagnostics.Debug.WriteLine($"[OrderFacade] Exported {orders.Count} orders to {exportResult.Data}");
            return exportResult;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[OrderFacade] Error exporting orders: {ex.Message}");
            _toastService.ShowError($"Error exporting orders: {ex.Message}");
            return Result<string>.Failure($"Error: {ex.Message}");
        }
    }
}
