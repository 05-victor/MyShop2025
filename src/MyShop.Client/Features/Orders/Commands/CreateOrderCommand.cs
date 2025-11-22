using MediatR;
using MyShop.Core.Common;
using MyShop.Shared.Models;

namespace MyShop.Client.Features.Orders.Commands;

/// <summary>
/// Command to create a new order (skeleton - backend not ready)
/// </summary>
public record CreateOrderCommand(
    Guid CustomerId,
    List<OrderItemModel> Items
) : IRequest<Result<Order>>;

public record OrderItemModel(Guid ProductId, int Quantity, decimal Price);
