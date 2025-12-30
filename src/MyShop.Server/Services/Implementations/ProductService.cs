using AutoMapper;
using MyShop.Data.Entities;
using MyShop.Data.Repositories.Interfaces;
using MyShop.Server.EntityMappings;
using MyShop.Server.Exceptions;
using MyShop.Server.Factories.Implementations;
using MyShop.Server.Factories.Interfaces;
using MyShop.Server.Mappings;
using MyShop.Server.Services.Interfaces;
using MyShop.Shared.DTOs.Commons;
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

    public async Task<PagedResult<ProductResponse>> GetAllAsync(PaginationRequest request)
    {
        try
        {
            var pagedProducts = await _productRepository.GetAllAsync(request.PageNumber, request.PageSize);
            
            return new PagedResult<ProductResponse>
            {
                Items = pagedProducts.Items.Select(p => ProductMapper.ToProductResponse(p)).ToList(),
                TotalCount = pagedProducts.TotalCount,
                Page = pagedProducts.Page,
                PageSize = pagedProducts.PageSize
            };
        }
        catch (Exception ex) when (ex is not BaseApplicationException)
        {
            _logger.LogError(ex, "Error retrieving products");
            throw InfrastructureException.DatabaseError("Failed to retrieve products", ex);
        }
    }

    public async Task<ProductResponse> GetByIdAsync(Guid id)
    {
        try
        {
            
            var product = await _productRepository.GetByIdAsync(id);

            if (product is null)
            {
                throw NotFoundException.ForEntity("Product", id);
            }

            return ProductMapper.ToProductResponse(product);
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

    public async Task<PagedResult<ProductResponse>> SearchAsync(SearchProductsRequest request)
    {
        try
        {
            _logger.LogInformation(
                "Searching products with filters: Query={Query}, CategoryId={CategoryId}, " +
                "MinPrice={MinPrice}, MaxPrice={MaxPrice}, InStockOnly={InStockOnly}, " +
                "Manufacturer={Manufacturer}, DeviceType={DeviceType}, Status={Status}, " +
                "SortBy={SortBy}, SortOrder={SortOrder}",
                request.Query, request.CategoryId, request.MinPrice, request.MaxPrice, 
                request.InStockOnly, request.Manufacturer, request.DeviceType, request.Status,
                request.SortBy, request.SortOrder);

            var pagedProducts = await _productRepository.SearchAsync(
                query: request.Query,
                categoryId: request.CategoryId,
                minPrice: request.MinPrice,
                maxPrice: request.MaxPrice,
                inStockOnly: request.InStockOnly,
                manufacturer: request.Manufacturer,
                deviceType: request.DeviceType,
                status: request.Status,
                saleAgentId: request.SaleAgentId,
                minStock: request.MinStock,
                maxStock: request.MaxStock,
                minCommissionRate: request.MinCommissionRate,
                maxCommissionRate: request.MaxCommissionRate,
                sortBy: request.SortBy ?? "createdAt",
                sortOrder: request.SortOrder ?? "desc",
                pageNumber: request.PageNumber,
                pageSize: request.PageSize);

            return new PagedResult<ProductResponse>
            {
                Items = pagedProducts.Items.Select(p => ProductMapper.ToProductResponse(p)).ToList(),
                TotalCount = pagedProducts.TotalCount,
                Page = pagedProducts.Page,
                PageSize = pagedProducts.PageSize
            };
        }
        catch (Exception ex) when (ex is not BaseApplicationException)
        {
            _logger.LogError(ex, "Error searching products");
            throw InfrastructureException.DatabaseError("Failed to search products", ex);
        }
    }

    public async Task<BulkCreateProductsResponse> BulkCreateAsync(BulkCreateProductsRequest request)
    {
        var response = new BulkCreateProductsResponse
        {
            TotalSubmitted = request.Products.Count
        };

        try
        {
            _logger.LogInformation("Starting bulk product creation for {Count} products", request.Products.Count);

            // Validate before insert if requested
            if (request.ValidateBeforeInsert)
            {
                var validationErrors = new List<BulkCreateError>();
                
                for (int i = 0; i < request.Products.Count; i++)
                {
                    var productRequest = request.Products[i];
                    try
                    {
                        // Check if category exists
                        var category = await _categoryRepository.GetByIdAsync(productRequest.CategoryId);
                        if (category == null)
                        {
                            validationErrors.Add(new BulkCreateError
                            {
                                Index = i,
                                ProductIdentifier = productRequest.Name,
                                ErrorMessage = $"Category with ID {productRequest.CategoryId} not found"
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        validationErrors.Add(new BulkCreateError
                        {
                            Index = i,
                            ProductIdentifier = productRequest.Name,
                            ErrorMessage = ex.Message
                        });
                    }
                }

                if (validationErrors.Any())
                {
                    response.FailureCount = validationErrors.Count;
                    response.Errors = validationErrors;
                    _logger.LogWarning("Bulk validation failed with {Count} errors", validationErrors.Count);
                    return response;
                }
            }

            // Process each product
            for (int i = 0; i < request.Products.Count; i++)
            {
                var productRequest = request.Products[i];
                try
                {
                    var createdProduct = await CreateAsync(productRequest);
                    response.CreatedProducts.Add(createdProduct);
                    response.SuccessCount++;
                }
                catch (Exception ex)
                {
                    response.FailureCount++;
                    response.Errors.Add(new BulkCreateError
                    {
                        Index = i,
                        ProductIdentifier = $"{productRequest.Name} ({productRequest.SKU})",
                        ErrorMessage = ex.Message
                    });

                    _logger.LogWarning(ex, "Failed to create product at index {Index}: {Name}", i, productRequest.Name);

                    // Stop if not skipping invalid products
                    if (!request.SkipInvalidProducts)
                    {
                        _logger.LogError("Stopping bulk creation due to error at index {Index}", i);
                        break;
                    }
                }
            }

            _logger.LogInformation(
                "Bulk product creation completed: {Success} succeeded, {Failed} failed", 
                response.SuccessCount, 
                response.FailureCount);

            return response;
        }
        catch (Exception ex) when (ex is not BaseApplicationException)
        {
            _logger.LogError(ex, "Error during bulk product creation");
            throw InfrastructureException.DatabaseError("Failed to perform bulk product creation", ex);
        }
    }
}