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
    Task<Result<IEnumerable<Order>>> GetBySalesAgentAsync(Guid salesAgentId);
    Task<Result<IEnumerable<Order>>> GetByStatusAsync(string status);
    Task<Result<IEnumerable<Order>>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate);
    Task<Result<Order>> CreateAsync(Order order);
    Task<Result<Order>> UpdateAsync(Order order);
    Task<Result<bool>> UpdateStatusAsync(Guid orderId, string status);
    Task<Result<bool>> MarkAsPaidAsync(Guid orderId);
    Task<Result<bool>> CancelAsync(Guid orderId, string reason);
    Task<Result<bool>> DeleteAsync(Guid id);
    Task<decimal> GetTodayRevenueAsync();
    Task<decimal> GetRevenueByDateRangeAsync(DateTime fromDate, DateTime toDate);

    /// <summary>
    /// Get paginated orders with filtering and sorting
    /// </summary>
    Task<Result<PagedList<Order>>> GetPagedAsync(
        int page = 1,
        int pageSize = Common.PaginationConstants.OrdersPageSize,
        string? status = null,
        string? paymentStatus = null,
        Guid? customerId = null,
        Guid? salesAgentId = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        string sortBy = "orderDate",
        bool sortDescending = true);
}
