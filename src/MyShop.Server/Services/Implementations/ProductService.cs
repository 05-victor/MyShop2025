using AutoMapper;
using MyShop.Data.Entities;
using MyShop.Data.Repositories.Interfaces;
using MyShop.Server.EntityMappings;
using MyShop.Server.Factories;
using MyShop.Server.Mappings;
using MyShop.Server.Services.Interfaces;
using MyShop.Shared.DTOs.Requests;
using MyShop.Shared.DTOs.Responses;

namespace MyShop.Server.Services.Implementations;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ProductFactory _productFactory;
    public ProductService(IProductRepository productRepository, ICategoryRepository categoryRepository, ProductFactory productFactory)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _productFactory = productFactory;
    }

    public async Task<IEnumerable<ProductResponse>> GetAllAsync()
    {
        var products = await _productRepository.GetAllAsync();
        return products.Select(p => ProductMapper.ToProductResponse(p));
    }

    public async Task<ProductResponse?> GetByIdAsync(Guid id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        return product is null ? null : ProductMapper.ToProductResponse(product);
    }

    public async Task<ProductResponse> CreateAsync(CreateProductRequest createProductRequest)
    {
        var category = await _categoryRepository.GetByIdAsync(createProductRequest.CategoryId);

        if (category is null)
        {
            throw new System.Collections.Generic.KeyNotFoundException("Category not found");
        }

        var product = _productFactory.Create(createProductRequest);
        var createdProduct = await _productRepository.CreateAsync(product);
        return ProductMapper.ToProductResponse(createdProduct);
    }

    public async Task<ProductResponse> UpdateAsync(Guid id, UpdateProductRequest updateProductRequest)
    {
        var existingProduct = await _productRepository.GetByIdAsync(id);
        if (existingProduct is null)
        {
            throw new System.Collections.Generic.KeyNotFoundException("Product not found");
        }

        // AutoMapper can be used here to map the fields
        existingProduct.Patch(updateProductRequest);
        existingProduct.UpdatedAt = DateTime.UtcNow;

        var updatedProduct = await _productRepository.UpdateAsync(existingProduct);
        return ProductMapper.ToProductResponse(updatedProduct);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var existingProduct = await _productRepository.GetByIdAsync(id);
        if (existingProduct is null)
        {
            return false;
        }
        await _productRepository.DeleteAsync(id);
        return true;
    }
}