using MediatR;
using MyShop.Core.Common;
using MyShop.Shared.Models;

namespace MyShop.Client.Features.Orders.Queries;

/// <summary>
/// Query to get orders (skeleton - backend not ready)
/// </summary>
public record GetOrdersQuery(Guid? CustomerId = null) : IRequest<Result<List<Order>>>;
