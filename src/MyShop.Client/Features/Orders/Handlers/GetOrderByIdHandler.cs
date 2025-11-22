using MediatR;
using MyShop.Client.Features.Orders.Queries;
using MyShop.Core.Common;
using MyShop.Core.Interfaces.Repositories;
using MyShop.Shared.Models;

namespace MyShop.Client.Features.Orders.Handlers;

/// <summary>
/// Handler for GetOrderByIdQuery
/// </summary>
public class GetOrderByIdHandler : IRequestHandler<GetOrderByIdQuery, Result<Order>>
{
    private readonly IOrderRepository _orderRepository;

    public GetOrderByIdHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<Result<Order>> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var order = await _orderRepository.GetByIdAsync(request.OrderId);

            if (order == null)
            {
                return Result<Order>.Failure($"Order with ID {request.OrderId} not found");
            }

            return Result<Order>.Success(order);
        }
        catch (Exception ex)
        {
            return Result<Order>.Failure($"Error loading order: {ex.Message}");
        }
    }
}
