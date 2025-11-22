using MediatR;
using MyShop.Core.Common;

namespace MyShop.Client.Features.Cart.Commands;

/// <summary>
/// Command to update cart item quantity (skeleton - backend not ready)
/// </summary>
public record UpdateCartItemCommand(Guid ProductId, int NewQuantity) : IRequest<Result<bool>>;
