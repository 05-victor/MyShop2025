using MediatR;
using MyShop.Client.Features.Products.Queries;
using MyShop.Core.Common;
using MyShop.Core.Interfaces.Repositories;
using MyShop.Shared.Models;

namespace MyShop.Client.Features.Products.Handlers;

/// <summary>
/// Handler for GetProductsQuery
/// </summary>
public class GetProductsHandler : IRequestHandler<GetProductsQuery, Result<List<Product>>>
{
    private readonly IProductRepository _productRepository;

    public GetProductsHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<Result<List<Product>>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _productRepository.GetAllAsync();
            if (!result.IsSuccess)
            {
                return Result<List<Product>>.Failure(result.ErrorMessage);
            }
            
            return Result<List<Product>>.Success(result.Data.ToList());
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error fetching products: {ex.Message}");
            return Result<List<Product>>.Failure($"Failed to fetch products: {ex.Message}", ex);
        }
    }
}
