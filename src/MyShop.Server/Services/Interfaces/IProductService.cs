using MyShop.Data.Entities;
using MyShop.Shared.DTOs.Commons;
using MyShop.Shared.DTOs.Requests;
using MyShop.Shared.DTOs.Responses;

namespace MyShop.Server.Services.Interfaces;

public interface IProductService
{
    Task<PagedResult<ProductResponse>> GetAllAsync(PaginationRequest request);
    Task<ProductResponse> GetByIdAsync(Guid id);
    Task<ProductResponse> CreateAsync(CreateProductRequest createProductRequest);
    Task<ProductResponse> UpdateAsync(Guid id, UpdateProductRequest updateProductRequest);
    Task<bool> DeleteAsync(Guid id);

    /// <summary>
    /// Search products with advanced filtering and sorting
    /// </summary>
    Task<PagedResult<ProductResponse>> SearchAsync(SearchProductsRequest request);

    /// <summary>
    /// Create multiple products at once
    /// </summary>
    Task<BulkCreateProductsResponse> BulkCreateAsync(BulkCreateProductsRequest request);
}