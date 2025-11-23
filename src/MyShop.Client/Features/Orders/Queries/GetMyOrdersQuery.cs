using MediatR;
using MyShop.Core.Common;
using MyShop.Shared.Models;

namespace MyShop.Client.Features.Orders.Queries;

/// <summary>
/// Query to get my orders (skeleton - backend not ready)
/// </summary>
public record GetMyOrdersQuery() : IRequest<Result<List<Order>>>;
