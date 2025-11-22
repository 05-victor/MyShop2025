using MediatR;
using MyShop.Client.Features.Orders.Queries;
using MyShop.Core.Common;
using MyShop.Core.Interfaces.Repositories;
using MyShop.Shared.Models;

namespace MyShop.Client.Features.Orders.Handlers;

/// <summary>
/// Handler for GetMyOrdersQuery
/// </summary>
public class GetMyOrdersHandler : IRequestHandler<GetMyOrdersQuery, Result<List<Order>>>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IAuthRepository _authRepository;

    public GetMyOrdersHandler(IOrderRepository orderRepository, IAuthRepository authRepository)
    {
        _orderRepository = orderRepository;
        _authRepository = authRepository;
    }

    public async Task<Result<List<Order>>> Handle(GetMyOrdersQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Get current user ID
            var userResult = await _authRepository.GetCurrentUserAsync();
            if (userResult == null || !userResult.IsSuccess || userResult.Data == null)
            {
                return Result<List<Order>>.Failure("User not authenticated");
            }

            var orders = await _orderRepository.GetByCustomerIdAsync(userResult.Data.Id);
            return Result<List<Order>>.Success(orders.ToList());
        }
        catch (Exception ex)
        {
            return Result<List<Order>>.Failure($"Error loading orders: {ex.Message}");
        }
    }
}
