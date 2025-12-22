using MyShop.Shared.DTOs.Commons;
using MyShop.Shared.DTOs.Responses;
using Refit;

namespace MyShop.Plugins.API.Products;

/// <summary>
/// Refit interface for Products API endpoints
/// </summary>
[Headers("User-Agent: MyShop-Client/1.0")]
public interface IProductsApi
{
    [Get("/api/v1/products")]
    Task<Refit.ApiResponse<MyShop.Shared.DTOs.Common.ApiResponse<PagedResult<ProductResponse>>>> GetAllAsync(
        [Query] int pageNumber = 1,
        [Query] int pageSize = 10);

    [Get("/api/v1/products/{id}")]
    Task<Refit.ApiResponse<MyShop.Shared.DTOs.Common.ApiResponse<ProductResponse>>> GetByIdAsync(Guid id);

    [Get("/api/v1/products/search")]
    Task<Refit.ApiResponse<MyShop.Shared.DTOs.Common.ApiResponse<List<ProductResponse>>>> SearchAsync([Query] string? query);

    [Post("/api/v1/products")]
    Task<Refit.ApiResponse<MyShop.Shared.DTOs.Common.ApiResponse<ProductResponse>>> CreateAsync([Body] object request);

    [Patch("/api/v1/products/{id}")]
    Task<Refit.ApiResponse<MyShop.Shared.DTOs.Common.ApiResponse<ProductResponse>>> UpdateAsync(Guid id, [Body] object request);

    [Delete("/api/v1/products/{id}")]
    Task<Refit.ApiResponse<MyShop.Shared.DTOs.Common.ApiResponse<bool>>> DeleteAsync(Guid id);
}
