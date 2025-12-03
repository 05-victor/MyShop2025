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
        int pageSize = 20,
        Guid? customerId = null,
        Guid? salesAgentId = null)
    {
        try
        {
            // Validate paging parameters
            if (page < 1)
            {
                _ = _toastService.ShowError("Page number must be at least 1");
                return Result<PagedList<Order>>.Failure("Invalid page number");
            }

            if (pageSize < 1 || pageSize > 100)
            {
                _ = _toastService.ShowError("Page size must be between 1 and 100");
                return Result<PagedList<Order>>.Failure("Invalid page size");
            }

            // Validate date range
            if (startDate.HasValue && endDate.HasValue && startDate.Value > endDate.Value)
            {
                _ = _toastService.ShowError("Start date cannot be after end date");
                return Result<PagedList<Order>>.Failure("Invalid date range");
            }

            // Use Repository's GetPagedAsync - filter by customerId or salesAgentId
            var result = await _orderRepository.GetPagedAsync(
                page: page,
                pageSize: pageSize,
                status: status,
                customerId: customerId,
                salesAgentId: salesAgentId,
                startDate: startDate,
                endDate: endDate,
                sortDescending: true);

            if (!result.IsSuccess || result.Data == null)
            {
                _ = _toastService.ShowError("Failed to load orders");
                return Result<PagedList<Order>>.Failure(result.ErrorMessage ?? "Failed to load orders");
            }

            var pagedResult = result.Data;

            // Apply search query filter (client-side for text search)
            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                var query = searchQuery.ToLower();
                var filteredItems = pagedResult.Items.Where(o =>
                    o.Id.ToString().Contains(query) ||
                    (o.OrderCode?.ToLower().Contains(query) ?? false) ||
                    (o.CustomerName?.ToLower().Contains(query) ?? false) ||
                    (o.CustomerPhone?.ToLower().Contains(query) ?? false) ||
                    (o.CustomerAddress?.ToLower().Contains(query) ?? false) ||
                    // Search in product names within order items
                    (o.OrderItems?.Any(item => item.ProductName?.ToLower().Contains(query) ?? false) ?? false) ||
                    (o.Items?.Any(item => item.ProductName?.ToLower().Contains(query) ?? false) ?? false)
                ).ToList();
                
                // Update count to match filtered items for current page view
                pagedResult = new PagedList<Order>(filteredItems, filteredItems.Count, page, pageSize);
            }

            System.Diagnostics.Debug.WriteLine($"[OrderFacade] Loaded {pagedResult.Items.Count} orders (page {page}/{pagedResult.TotalPages}, total: {pagedResult.TotalCount})");
            return Result<PagedList<Order>>.Success(pagedResult);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[OrderFacade] Error loading orders: {ex.Message}");
            _ = _toastService.ShowError($"Error loading orders: {ex.Message}");
            return Result<PagedList<Order>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<PagedList<Order>>> LoadOrdersPagedAsync(
        int page = 1,
        int pageSize = Core.Common.PaginationConstants.OrdersPageSize,
        string? status = null,
        string? searchQuery = null,
        string sortBy = "orderDate",
        bool sortDescending = true,
        Guid? customerId = null,
        Guid? salesAgentId = null)
    {
        try
        {
            // Call repository with explicit sort parameters so sorting works
            var result = await _orderRepository.GetPagedAsync(
                page: page,
                pageSize: pageSize,
                status: status,
                customerId: customerId,
                salesAgentId: salesAgentId,
                startDate: null,
                endDate: null,
                sortBy: sortBy,
                sortDescending: sortDescending);

            if (!result.IsSuccess || result.Data == null)
            {
                _ = _toastService.ShowError("Failed to load orders");
                return Result<PagedList<Order>>.Failure(result.ErrorMessage ?? "Failed to load orders");
            }

            var pagedResult = result.Data;

            // Apply search query filter (client-side for text search)
            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                var query = searchQuery.ToLower();
                var filteredItems = pagedResult.Items.Where(o =>
                    (o.OrderCode?.ToLower().Contains(query) ?? false) ||
                    (o.CustomerName?.ToLower().Contains(query) ?? false) ||
                    (o.CustomerPhone?.ToLower().Contains(query) ?? false) ||
                    (o.CustomerAddress?.ToLower().Contains(query) ?? false) ||
                    // Search in product names within order items
                    (o.OrderItems?.Any(item => item.ProductName?.ToLower().Contains(query) ?? false) ?? false) ||
                    (o.Items?.Any(item => item.ProductName?.ToLower().Contains(query) ?? false) ?? false)
                ).ToList();

                // Update count to match filtered items for current page view
                pagedResult = new PagedList<Order>(filteredItems, filteredItems.Count, page, pageSize);
            }

            System.Diagnostics.Debug.WriteLine($"[OrderFacade] Loaded {pagedResult.Items.Count} orders (page {page}/{pagedResult.TotalPages}, total: {pagedResult.TotalCount})");
            return Result<PagedList<Order>>.Success(pagedResult);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[OrderFacade] Error loading paged orders: {ex.Message}");
            _ = _toastService.ShowError($"Error loading orders: {ex.Message}");
            return Result<PagedList<Order>>.Failure($"Error: {ex.Message}");
        }
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
            _ = _toastService.ShowError($"Error creating order: {ex.Message}");
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
                _ = _toastService.ShowError($"Invalid status. Valid values: {string.Join(", ", validStatuses)}");
                return Result<Order>.Failure("Invalid order status");
            }

            // Get current order
            var orderResult = await _orderRepository.GetByIdAsync(orderId);
            if (!orderResult.IsSuccess || orderResult.Data == null)
            {
                _ = _toastService.ShowError("Order not found");
                return Result<Order>.Failure("Order not found");
            }

            var order = orderResult.Data;

            // Validate status transition
            if (order.Status == "Completed" && newStatus != "Completed")
            {
                _ = _toastService.ShowError("Cannot change status of completed order");
                return Result<Order>.Failure("Order already completed");
            }

            if (order.Status == "Cancelled")
            {
                _ = _toastService.ShowError("Cannot change status of cancelled order");
                return Result<Order>.Failure("Order already cancelled");
            }

            // Update status
            order.Status = newStatus;
            order.UpdatedAt = DateTime.UtcNow;

            var updateResult = await _orderRepository.UpdateAsync(order);
            if (!updateResult.IsSuccess)
            {
                _ = _toastService.ShowError("Failed to update order status");
                return Result<Order>.Failure("Failed to update status");
            }

            _ = _toastService.ShowSuccess($"Order status updated to {newStatus}");
            System.Diagnostics.Debug.WriteLine($"[OrderFacade] Order {orderId} status updated to {newStatus}");
            return Result<Order>.Success(order);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[OrderFacade] Error updating status: {ex.Message}");
            _ = _toastService.ShowError($"Error updating status: {ex.Message}");
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
                _ = _toastService.ShowError("Please provide a reason for cancellation");
                return Result<Unit>.Failure("Cancellation reason is required");
            }

            // Get order
            var orderResult = await _orderRepository.GetByIdAsync(orderId);
            if (!orderResult.IsSuccess || orderResult.Data == null)
            {
                _ = _toastService.ShowError("Order not found");
                return Result<Unit>.Failure("Order not found");
            }

            var order = orderResult.Data;

            // Validate can cancel
            if (order.Status == "Cancelled")
            {
                _ = _toastService.ShowError("Order is already cancelled");
                return Result<Unit>.Failure("Order already cancelled");
            }

            if (order.Status == "Completed")
            {
                _ = _toastService.ShowError("Cannot cancel completed order");
                return Result<Unit>.Failure("Cannot cancel completed order");
            }

            // Cancel order
            order.Status = "Cancelled";
            order.UpdatedAt = DateTime.UtcNow;

            var updateResult = await _orderRepository.UpdateAsync(order);
            if (!updateResult.IsSuccess)
            {
                _ = _toastService.ShowError("Failed to cancel order");
                return Result<Unit>.Failure("Failed to cancel order");
            }

            _ = _toastService.ShowSuccess($"Order cancelled: {reason}");
            System.Diagnostics.Debug.WriteLine($"[OrderFacade] Order {orderId} cancelled. Reason: {reason}");
            return Result<Unit>.Success(Unit.Value);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[OrderFacade] Error cancelling order: {ex.Message}");
            _ = _toastService.ShowError($"Error cancelling order: {ex.Message}");
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
                _ = _toastService.ShowError("Failed to load customer orders");
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
            _ = _toastService.ShowError($"Error loading orders: {ex.Message}");
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
                _ = _toastService.ShowError("Failed to load agent orders");
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
            _ = _toastService.ShowError($"Error loading orders: {ex.Message}");
            return Result<List<Order>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<string>> ExportOrdersToCsvAsync(
        string? status = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        Guid? customerId = null,
        Guid? salesAgentId = null)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"[OrderFacade.Export] Starting export with customerId: {customerId}, salesAgentId: {salesAgentId}");
            
            // Load orders efficiently - filter at repository level
            IEnumerable<Order> orders;
            
            if (salesAgentId.HasValue)
            {
                // Sales Agent view - load orders for this agent
                var agentResult = await _orderRepository.GetBySalesAgentIdAsync(salesAgentId.Value);
                if (!agentResult.IsSuccess || agentResult.Data == null)
                {
                    _ = _toastService.ShowError("Failed to load orders for export");
                    return Result<string>.Failure("Failed to load orders");
                }
                orders = agentResult.Data;
                System.Diagnostics.Debug.WriteLine($"[OrderFacade.Export] Loaded {orders.Count()} orders for SalesAgentId: {salesAgentId.Value}");
            }
            else if (customerId.HasValue)
            {
                // Customer view - load orders for this customer
                var userResult = await _orderRepository.GetByCustomerIdAsync(customerId.Value);
                if (!userResult.IsSuccess || userResult.Data == null)
                {
                    _ = _toastService.ShowError("Failed to load orders for export");
                    return Result<string>.Failure("Failed to load orders");
                }
                orders = userResult.Data;
                System.Diagnostics.Debug.WriteLine($"[OrderFacade.Export] Loaded {orders.Count()} orders for CustomerId: {customerId.Value}");
            }
            else
            {
                // Admin view - load all orders
                var allResult = await _orderRepository.GetAllAsync();
                if (!allResult.IsSuccess || allResult.Data == null)
                {
                    _ = _toastService.ShowError("Failed to load orders for export");
                    return Result<string>.Failure("Failed to load orders");
                }
                orders = allResult.Data;
                System.Diagnostics.Debug.WriteLine($"[OrderFacade.Export] Loaded {orders.Count()} total orders");
            }

            // Apply status filter (client-side)
            if (!string.IsNullOrWhiteSpace(status))
            {
                orders = orders.Where(o => o.Status?.Equals(status, StringComparison.OrdinalIgnoreCase) ?? false);
            }

            // Apply date filters (client-side)
            if (startDate.HasValue)
            {
                orders = orders.Where(o => o.OrderDate >= startDate.Value);
            }
            if (endDate.HasValue)
            {
                orders = orders.Where(o => o.OrderDate <= endDate.Value);
            }

            var ordersList = orders.OrderByDescending(o => o.OrderDate).ToList();
            System.Diagnostics.Debug.WriteLine($"[OrderFacade.Export] After filtering: {ordersList.Count} orders to export");

            if (ordersList.Count == 0)
            {
                _ = _toastService.ShowWarning("No orders to export");
                return Result<string>.Success(string.Empty);
            }

            // Build CSV content
            var csv = new System.Text.StringBuilder();
            csv.AppendLine("Order Code,Order Date,Customer Name,Customer Phone,Status,Total Amount,Products,Items Count");
            
            foreach (var order in ordersList)
            {
                // Use OrderItems (from JSON) or Items as fallback
                var orderItems = order.OrderItems?.Count > 0 ? order.OrderItems : order.Items;
                var itemsCount = orderItems?.Count ?? 0;
                var productNames = itemsCount > 0 
                    ? string.Join("; ", orderItems!.Select(i => $"{i.ProductName} x{i.Quantity}"))
                    : "No products";
                
                csv.AppendLine($"\"{order.OrderCode ?? string.Empty}\"," +
                    $"\"{order.OrderDate:yyyy-MM-dd HH:mm}\"," +
                    $"\"{order.CustomerName ?? string.Empty}\"," +
                    $"\"{order.CustomerPhone ?? string.Empty}\"," +
                    $"\"{order.Status ?? string.Empty}\"," +
                    $"\"{order.FinalPrice:N0}\"," +
                    $"\"{productNames.Replace("\"", "\"\"")}\"," +
                    $"\"{itemsCount}\"");
            }

            // Use ExportService with FileSavePicker
            var suggestedFileName = $"Orders_{DateTime.Now:yyyyMMdd_HHmmss}";
            var exportResult = await _exportService.ExportWithPickerAsync(suggestedFileName, csv.ToString());

            if (!exportResult.IsSuccess)
            {
                _ = _toastService.ShowError("Failed to export orders");
                return exportResult;
            }

            // Empty path means user cancelled
            if (string.IsNullOrEmpty(exportResult.Data))
            {
                return Result<string>.Success(string.Empty);
            }

            _ = _toastService.ShowSuccess($"Exported {ordersList.Count} orders successfully!");
            System.Diagnostics.Debug.WriteLine($"[OrderFacade] Exported {ordersList.Count} orders to {exportResult.Data}");

            return exportResult;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[OrderFacade] Error exporting orders: {ex.Message}");
            _ = _toastService.ShowError($"Error exporting orders: {ex.Message}");
            return Result<string>.Failure($"Error: {ex.Message}");
        }
    }
}
