using MediatR;
using MyShop.Core.Common;
using MyShop.Shared.Models;

namespace MyShop.Client.Features.Products.Queries;

/// <summary>
/// Query to search products by keyword
/// </summary>
public record SearchProductsQuery(string? Keyword) : IRequest<Result<List<Product>>>;
