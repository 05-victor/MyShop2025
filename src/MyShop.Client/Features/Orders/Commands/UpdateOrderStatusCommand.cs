using MediatR;
using MyShop.Core.Common;

namespace MyShop.Client.Features.Orders.Commands;

/// <summary>
/// Command to update order status (skeleton - backend not ready)
/// </summary>
public record UpdateOrderStatusCommand(Guid OrderId, string NewStatus) : IRequest<Result<bool>>;
