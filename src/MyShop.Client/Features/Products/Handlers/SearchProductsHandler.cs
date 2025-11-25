using MediatR;
using MyShop.Client.Features.Products.Queries;
using MyShop.Core.Common;
using MyShop.Core.Interfaces.Repositories;
using MyShop.Shared.Models;

namespace MyShop.Client.Features.Products.Handlers;

/// <summary>
/// Handler for SearchProductsQuery
/// </summary>
public class SearchProductsHandler : IRequestHandler<SearchProductsQuery, Result<List<Product>>>
{
    private readonly IProductRepository _productRepository;

    public SearchProductsHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<Result<List<Product>>> Handle(SearchProductsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // For now, get all products and filter in-memory
            // Backend API should implement server-side search
            var result = await _productRepository.GetAllAsync();
            
            if (!result.IsSuccess)
            {
                return Result<List<Product>>.Failure(result.ErrorMessage);
            }
            
            if (string.IsNullOrWhiteSpace(request.Keyword))
            {
                return Result<List<Product>>.Success(result.Data.ToList());
            }

            var filtered = result.Data
                .Where(p => p.Name.Contains(request.Keyword, StringComparison.OrdinalIgnoreCase) ||
                           (p.Description?.Contains(request.Keyword, StringComparison.OrdinalIgnoreCase) ?? false))
                .ToList();

            return Result<List<Product>>.Success(filtered);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error searching products: {ex.Message}");
            return Result<List<Product>>.Failure($"Failed to search products: {ex.Message}", ex);
        }
    }
}
