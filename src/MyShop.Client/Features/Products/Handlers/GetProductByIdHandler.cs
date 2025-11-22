using MediatR;
using MyShop.Client.Features.Products.Queries;
using MyShop.Core.Common;
using MyShop.Core.Interfaces.Repositories;
using MyShop.Shared.Models;

namespace MyShop.Client.Features.Products.Handlers;

/// <summary>
/// Handler for GetProductByIdQuery
/// </summary>
public class GetProductByIdHandler : IRequestHandler<GetProductByIdQuery, Result<Product>>
{
    private readonly IProductRepository _productRepository;

    public GetProductByIdHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<Result<Product>> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var product = await _productRepository.GetByIdAsync(request.ProductId);
            
            if (product == null)
            {
                return Result<Product>.Failure($"Product with ID {request.ProductId} not found.");
            }

            return Result<Product>.Success(product);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error fetching product by ID: {ex.Message}");
            return Result<Product>.Failure($"Failed to fetch product: {ex.Message}", ex);
        }
    }
}
