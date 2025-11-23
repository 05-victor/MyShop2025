using MediatR;
using MyShop.Client.Features.Orders.Commands;
using MyShop.Core.Common;
using MyShop.Shared.Models;

namespace MyShop.Client.Features.Orders.Handlers;

/// <summary>
/// Handler for CreateOrderCommand (skeleton - backend not ready)
/// </summary>
public class CreateOrderHandler : IRequestHandler<CreateOrderCommand, Result<Order>>
{
    public Task<Result<Order>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException("Backend API not ready");
    }
}
