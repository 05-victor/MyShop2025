using MediatR;
using MyShop.Core.Common;

namespace MyShop.Client.Features.Cart.Commands;

/// <summary>
/// Command to add item to cart (skeleton - backend not ready)
/// </summary>
public record AddToCartCommand(Guid ProductId, int Quantity) : IRequest<Result<bool>>;
