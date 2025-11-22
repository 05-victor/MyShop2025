using MediatR;
using MyShop.Core.Common;
using MyShop.Shared.Models;

namespace MyShop.Client.Features.Products.Queries;

/// <summary>
/// Query to get a product by ID
/// </summary>
public record GetProductByIdQuery(Guid ProductId) : IRequest<Result<Product>>;
