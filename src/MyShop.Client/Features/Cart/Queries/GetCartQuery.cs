using MediatR;
using MyShop.Core.Common;

namespace MyShop.Client.Features.Cart.Queries;

/// <summary>
/// Query to get cart items (skeleton - backend not ready)
/// </summary>
public record GetCartQuery() : IRequest<Result<object>>;
