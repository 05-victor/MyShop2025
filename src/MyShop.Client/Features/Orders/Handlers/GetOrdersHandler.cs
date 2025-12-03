using MediatR;
using MyShop.Client.Features.Orders.Queries;
using MyShop.Core.Common;
using MyShop.Core.Interfaces.Repositories;
using MyShop.Shared.Models;

namespace MyShop.Client.Features.Orders.Handlers;

/// <summary>
/// Handler for GetOrdersQuery
/// </summary>
public class GetOrdersHandler : IRequestHandler<GetOrdersQuery, Result<List<Order>>>
{
    private readonly IOrderRepository _orderRepository;

    public GetOrdersHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<Result<List<Order>>> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
    {
        try
        {
            Result<IEnumerable<Order>> result;

            if (request.CustomerId.HasValue)
            {
                result = await _orderRepository.GetByCustomerIdAsync(request.CustomerId.Value);
            }
            else
            {
                result = await _orderRepository.GetAllAsync();
            }
            
            if (!result.IsSuccess)
            {
                return Result<List<Order>>.Failure(result.ErrorMessage);
            }

            return Result<List<Order>>.Success(result.Data.ToList());
        }
        catch (Exception ex)
        {
            return Result<List<Order>>.Failure($"Error loading orders: {ex.Message}");
        }
    }
}
