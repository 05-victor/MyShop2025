using MediatR;
using MyShop.Client.Features.Orders.Commands;
using MyShop.Core.Common;
using MyShop.Core.Interfaces.Repositories;

namespace MyShop.Client.Features.Orders.Handlers;

/// <summary>
/// Handler for UpdateOrderStatusCommand
/// </summary>
public class UpdateOrderStatusHandler : IRequestHandler<UpdateOrderStatusCommand, Result<bool>>
{
    private readonly IOrderRepository _orderRepository;

    public UpdateOrderStatusHandler(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<Result<bool>> Handle(UpdateOrderStatusCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.NewStatus))
            {
                return Result<bool>.Failure("Status cannot be empty");
            }

            var result = await _orderRepository.UpdateStatusAsync(request.OrderId, request.NewStatus);
            
            if (!result.IsSuccess)
            {
                return Result<bool>.Failure(result.ErrorMessage);
            }
            
            return Result<bool>.Success(result.Data);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Error updating order status: {ex.Message}");
        }
    }
}
