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
            var result = await _orderRepository.GetByIdAsync(request.OrderId);

            if (!result.IsSuccess)
            {
                return Result<Order>.Failure(result.ErrorMessage);
            }

            return Result<Order>.Success(result.Data);
        }
        catch (Exception ex)
        {
            return Result<Order>.Failure($"Error loading order: {ex.Message}");
        }
    }
}
