using MyShop.Core.Common;
using MyShop.Shared.Models;

namespace MyShop.Core.Interfaces.Facades;

/// <summary>
/// Facade pattern for order management operations
/// Aggregates: IOrderRepository, IProductRepository, IValidationService, IToastService
/// </summary>
public interface IOrderFacade
{
    /// <summary>
    /// Load orders với paging and filtering
    /// </summary>
    /// <param name="customerId">Filter by customer ID (customer's own orders)</param>
    /// <param name="salesAgentId">Filter by sales agent ID (sales agent's orders)</param>
    Task<Result<PagedList<Order>>> LoadOrdersAsync(
        string? searchQuery = null,
        string? status = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int page = 1,
        int pageSize = Common.PaginationConstants.DefaultPageSize,
        Guid? customerId = null,
        Guid? salesAgentId = null);

    /// <summary>
    /// Load orders with paging (alias for LoadOrdersAsync)
    /// </summary>
    /// <param name="customerId">Filter by customer ID (customer's own orders)</param>
    /// <param name="salesAgentId">Filter by sales agent ID (sales agent's orders)</param>
    Task<Result<PagedList<Order>>> LoadOrdersPagedAsync(
        int page = 1,
        int pageSize = Common.PaginationConstants.OrdersPageSize,
        string? status = null,
        string? searchQuery = null,
        string sortBy = "orderDate",
        bool sortDescending = true,
        Guid? customerId = null,
        Guid? salesAgentId = null);

    /// <summary>
    /// Get order by ID with details
    /// </summary>
    Task<Result<Order>> GetOrderByIdAsync(Guid orderId);

    /// <summary>
    /// Create new order
    /// </summary>
    Task<Result<Order>> CreateOrderAsync(
        List<(Guid ProductId, int Quantity)> items,
        string shippingAddress,
        string notes);

    /// <summary>
    /// Update order status
    /// </summary>
    Task<Result<Order>> UpdateOrderStatusAsync(Guid orderId, string newStatus, string? reason = null);

    /// <summary>
    /// Cancel order
    /// </summary>
    Task<Result<Unit>> CancelOrderAsync(Guid orderId, string reason);

    /// <summary>
    /// Get orders by customer
    /// </summary>
    Task<Result<List<Order>>> GetOrdersByCustomerAsync(Guid customerId);

    /// <summary>
    /// Get orders by sales agent
    /// </summary>
    Task<Result<List<Order>>> GetOrdersBySalesAgentAsync(Guid agentId);

    /// <summary>
    /// Export orders to CSV
    /// </summary>
    /// <param name="customerId">Filter by customer ID</param>
    /// <param name="salesAgentId">Filter by sales agent ID</param>
    Task<Result<string>> ExportOrdersToCsvAsync(
        string? status = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        Guid? customerId = null,
        Guid? salesAgentId = null);
}
