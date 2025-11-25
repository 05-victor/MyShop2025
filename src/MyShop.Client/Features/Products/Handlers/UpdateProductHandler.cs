using MediatR;
using MyShop.Client.Features.Products.Commands;
using MyShop.Core.Common;
using MyShop.Core.Interfaces.Repositories;
using MyShop.Shared.Models;

namespace MyShop.Client.Features.Products.Handlers;

/// <summary>
/// Handler for UpdateProductCommand
/// </summary>
public class UpdateProductHandler : IRequestHandler<UpdateProductCommand, Result<Product>>
{
    private readonly IProductRepository _productRepository;

    public UpdateProductHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<Result<Product>> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var product = new Product
            {
                Id = request.Id,
                Name = request.Name,
                Description = request.Description,
                SellingPrice = request.Price,
                Quantity = request.Stock,
                CategoryId = request.CategoryId,
                ImageUrl = request.ImageUrl,
                UpdatedAt = DateTimeOffset.UtcNow.DateTime
            };

            var result = await _productRepository.UpdateAsync(product);
            
            if (!result.IsSuccess)
            {
                return Result<Product>.Failure(result.ErrorMessage);
            }
            
            return Result<Product>.Success(result.Data);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error updating product: {ex.Message}");
            return Result<Product>.Failure($"Failed to update product: {ex.Message}", ex);
        }
    }
}
