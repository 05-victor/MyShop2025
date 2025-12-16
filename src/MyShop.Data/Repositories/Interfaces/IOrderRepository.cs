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
}
