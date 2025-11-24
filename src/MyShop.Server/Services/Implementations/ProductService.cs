using AutoMapper;
using MyShop.Data.Entities;
using MyShop.Data.Repositories.Interfaces;
using MyShop.Server.EntityMappings;
using MyShop.Server.Exceptions;
using MyShop.Server.Factories.Implementations;
using MyShop.Server.Factories.Interfaces;
using MyShop.Server.Mappings;
using MyShop.Server.Services.Interfaces;
using MyShop.Shared.DTOs.Requests;
using MyShop.Shared.DTOs.Responses;

namespace MyShop.Server.Services.Implementations;

/// <summary>
/// Service for managing product operations
/// </summary>
public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IProductFactory _productFactory;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<ProductService> _logger;

    public ProductService(
        IProductRepository productRepository, 
        ICategoryRepository categoryRepository, 
        IProductFactory productFactory,
        ICurrentUserService currentUserService,
        ILogger<ProductService> logger)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _productFactory = productFactory;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<IEnumerable<ProductResponse>> GetAllAsync()
    {
        try
        {
            var products = await _productRepository.GetAllAsync();
            return products.Select(p => ProductMapper.ToProductResponse(p));
        }
        catch (Exception ex) when (ex is not BaseApplicationException) // not any of our custom exceptions
        {
            _logger.LogError(ex, "Error retrieving all products");
            throw InfrastructureException.DatabaseError("Failed to retrieve products", ex);
        }
    }

    public async Task<ProductResponse?> GetByIdAsync(Guid id)
    {
        try
        {
            var product = await _productRepository.GetByIdAsync(id);
            return product is null ? null : ProductMapper.ToProductResponse(product);
        }
        catch (Exception ex) when (ex is not BaseApplicationException)
        {
            _logger.LogError(ex, "Error retrieving product {ProductId}", id);
            throw InfrastructureException.DatabaseError($"Failed to retrieve product with ID {id}", ex);
        }
    }

    public async Task<ProductResponse> CreateAsync(CreateProductRequest createProductRequest)
    {
        // Validate category exists
        var category = await _categoryRepository.GetByIdAsync(createProductRequest.CategoryId);
        if (category is null)
        {
            throw NotFoundException.ForEntity("Category", createProductRequest.CategoryId);
        }

        try
        {
            // Create product using factory (factory will throw ValidationException if invalid)
            var product = _productFactory.Create(createProductRequest);

            // Auto-assign current user as sale agent if not specified
            if (!product.SaleAgentId.HasValue)
            {
                var currentUserId = _currentUserService.UserId;
                if (currentUserId.HasValue)
                {
                    product.SaleAgentId = currentUserId.Value;
                    _logger.LogInformation("Auto-assigned sale agent {UserId} to product {ProductName}", 
                        currentUserId.Value, product.Name);
                }
                else
                {
                    _logger.LogWarning("No authenticated user found. Product created without sale agent.");
                }
            }

            var createdProduct = await _productRepository.CreateAsync(product);
            
            _logger.LogInformation("Product {ProductId} created by sale agent {SaleAgentId}", 
                createdProduct.Id, createdProduct.SaleAgentId);

            return ProductMapper.ToProductResponse(createdProduct);
        }
        catch (ArgumentException argEx)
        {
            // Convert ArgumentException from factory to ValidationException
            throw new ValidationException(argEx.Message);
        }
        catch (Exception ex) when (ex is not BaseApplicationException)
        {
            _logger.LogError(ex, "Error creating product");
            throw InfrastructureException.DatabaseError("Failed to create product", ex);
        }
    }

    public async Task<ProductResponse> UpdateAsync(Guid id, UpdateProductRequest updateProductRequest)
    {
        var existingProduct = await _productRepository.GetByIdAsync(id);
        if (existingProduct is null)
        {
            throw NotFoundException.ForEntity("Product", id);
        }

        // Validate category if being updated
        if (updateProductRequest.CategoryId.HasValue)
        {
            var category = await _categoryRepository.GetByIdAsync(updateProductRequest.CategoryId.Value);
            if (category is null)
            {
                throw NotFoundException.ForEntity("Category", updateProductRequest.CategoryId.Value);
            }
        }

        try
        {
            // Apply updates using Patch method
            existingProduct.Patch(updateProductRequest);
            existingProduct.UpdatedAt = DateTime.UtcNow;

            // Update sale agent if specified
            if (updateProductRequest.SaleAgentId.HasValue)
            {
                existingProduct.SaleAgentId = updateProductRequest.SaleAgentId;
                _logger.LogInformation("Sale agent updated to {SaleAgentId} for product {ProductId}", 
                    updateProductRequest.SaleAgentId, id);
            }

            var updatedProduct = await _productRepository.UpdateAsync(existingProduct);
            return ProductMapper.ToProductResponse(updatedProduct);
        }
        catch (Exception ex) when (ex is not BaseApplicationException)
        {
            _logger.LogError(ex, "Error updating product {ProductId}", id);
            throw InfrastructureException.DatabaseError($"Failed to update product with ID {id}", ex);
        }
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var existingProduct = await _productRepository.GetByIdAsync(id);
        if (existingProduct is null)
        {
            return false;
        }

        try
        {
            await _productRepository.DeleteAsync(id);
            _logger.LogInformation("Product {ProductId} deleted", id);
            return true;
        }
        catch (Exception ex) when (ex is not BaseApplicationException)
        {
            _logger.LogError(ex, "Error deleting product {ProductId}", id);
            throw InfrastructureException.DatabaseError($"Failed to delete product with ID {id}", ex);
        }
    }
}