using MyShop.Data.Entities;
using MyShop.Shared.DTOs.Commons;

namespace MyShop.Data.Repositories.Interfaces;

public interface IOrderRepository
{
    Task<IEnumerable<Order>> GetAllAsync();
    Task<Order?> GetByIdAsync(Guid id);
    Task<Order> CreateAsync(Order order);
    Task<Order> UpdateAsync(Order order);
    Task<bool> DeleteAsync(Guid id);

    Task<PagedResult<Order>> GetOrdersBySalesAgentIdAsync(int pageNumber, int pageSize, Guid salesAgentId);
    Task<PagedResult<Order>> GetOrdersByCustomerIdAsync(int pageNumber, int pageSize, Guid customerId);
    
    /// <summary>
    /// Get filtered orders for a sales agent with optional filters (for earnings history)
    /// </summary>
    Task<PagedResult<Order>> GetFilteredOrdersBySalesAgentAsync(
        Guid salesAgentId,
        int pageNumber,
        int pageSize,
        DateTime? startDate = null,
        DateTime? endDate = null,
        string? status = null,
        string? paymentStatus = null,
        string sortBy = "OrderDate",
        bool sortDescending = true);
}
