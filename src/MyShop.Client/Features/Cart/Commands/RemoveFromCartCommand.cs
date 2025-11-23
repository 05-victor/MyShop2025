using MediatR;
using MyShop.Core.Common;

namespace MyShop.Client.Features.Cart.Commands;

/// <summary>
/// Command to remove item from cart (skeleton - backend not ready)
/// </summary>
public record RemoveFromCartCommand(Guid ProductId) : IRequest<Result<bool>>;
