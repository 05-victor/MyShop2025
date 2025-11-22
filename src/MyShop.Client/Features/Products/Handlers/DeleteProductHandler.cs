using MediatR;
using MyShop.Client.Features.Products.Commands;
using MyShop.Core.Common;
using MyShop.Core.Interfaces.Repositories;

namespace MyShop.Client.Features.Products.Handlers;

/// <summary>
/// Handler for DeleteProductCommand
/// </summary>
public class DeleteProductHandler : IRequestHandler<DeleteProductCommand, Result<bool>>
{
    private readonly IProductRepository _productRepository;

    public DeleteProductHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<Result<bool>> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var deleted = await _productRepository.DeleteAsync(request.ProductId);
            return Result<bool>.Success(deleted);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error deleting product: {ex.Message}");
            return Result<bool>.Failure($"Failed to delete product: {ex.Message}", ex);
        }
    }
}
