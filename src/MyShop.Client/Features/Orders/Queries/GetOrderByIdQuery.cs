using MediatR;
using MyShop.Core.Common;
using MyShop.Shared.Models;

namespace MyShop.Client.Features.Orders.Queries;

/// <summary>
/// Query to get order by ID (skeleton - backend not ready)
/// </summary>
public record GetOrderByIdQuery(Guid OrderId) : IRequest<Result<Order>>;
