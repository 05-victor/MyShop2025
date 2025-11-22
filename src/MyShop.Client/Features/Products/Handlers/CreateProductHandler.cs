using MediatR;
using MyShop.Client.Features.Products.Commands;
using MyShop.Core.Common;
using MyShop.Core.Interfaces.Repositories;
using MyShop.Shared.Models;

namespace MyShop.Client.Features.Products.Handlers;

/// <summary>
/// Handler for CreateProductCommand
/// </summary>
public class CreateProductHandler : IRequestHandler<CreateProductCommand, Result<Product>>
{
    private readonly IProductRepository _productRepository;

    public CreateProductHandler(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<Result<Product>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var product = new Product
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                SellingPrice = request.Price,
                Quantity = request.Stock,
                CategoryId = request.CategoryId,
                ImageUrl = request.ImageUrl,
                CreatedAt = DateTimeOffset.UtcNow.DateTime,
                UpdatedAt = DateTimeOffset.UtcNow.DateTime
            };

            var created = await _productRepository.CreateAsync(product);
            return Result<Product>.Success(created);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error creating product: {ex.Message}");
            return Result<Product>.Failure($"Failed to create product: {ex.Message}", ex);
        }
    }
}
