using MyShop.Shared.Models;
using MyShop.Core.Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyShop.Core.Interfaces.Repositories;

/// <summary>
/// Repository interface for order management
/// </summary>
public interface IOrderRepository
{
    Task<Result<IEnumerable<Order>>> GetAllAsync();
    Task<Result<Order>> GetByIdAsync(Guid id);
    Task<Result<IEnumerable<Order>>> GetByCustomerIdAsync(Guid customerId);
    Task<Result<IEnumerable<Order>>> GetBySalesAgentIdAsync(Guid salesAgentId);
    Task<Result<Order>> CreateAsync(Order order);
    Task<Result<Order>> UpdateAsync(Order order);
    Task<Result<bool>> UpdateStatusAsync(Guid orderId, string status);
    Task<Result<bool>> DeleteAsync(Guid id);
}
