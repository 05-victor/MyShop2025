using MediatR;
using MyShop.Core.Common;
using MyShop.Shared.Models;

namespace MyShop.Client.Features.Products.Queries;

/// <summary>
/// Query to get all products
/// </summary>
public record GetProductsQuery() : IRequest<Result<List<Product>>>;
