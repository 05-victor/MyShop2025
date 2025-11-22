using MyShop.Shared.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyShop.Core.Interfaces.Repositories;

/// <summary>
/// Repository interface for order management
/// </summary>
public interface IOrderRepository
{
    Task<IEnumerable<Order>> GetAllAsync();
    Task<Order?> GetByIdAsync(Guid id);
    Task<IEnumerable<Order>> GetByCustomerIdAsync(Guid customerId);
    Task<IEnumerable<Order>> GetBySalesAgentIdAsync(Guid salesAgentId);
    Task<Order> CreateAsync(Order order);
    Task<Order> UpdateAsync(Order order);
    Task<bool> UpdateStatusAsync(Guid orderId, string status);
    Task<bool> DeleteAsync(Guid id);
}
