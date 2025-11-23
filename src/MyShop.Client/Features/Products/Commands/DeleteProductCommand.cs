using MediatR;
using MyShop.Core.Common;

namespace MyShop.Client.Features.Products.Commands;

/// <summary>
/// Command to delete a product by ID
/// </summary>
public record DeleteProductCommand(Guid ProductId) : IRequest<Result<bool>>;
