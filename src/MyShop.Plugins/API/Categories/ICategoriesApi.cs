using MyShop.Shared.DTOs.Responses;
using Refit;

namespace MyShop.Plugins.API.Categories;

/// <summary>
/// Refit interface for Categories API endpoints
/// </summary>
[Headers("User-Agent: MyShop-Client/1.0")]
public interface ICategoriesApi
{
    [Get("/api/v1/categories")]
    Task<Refit.ApiResponse<MyShop.Shared.DTOs.Common.ApiResponse<List<CategoryResponse>>>> GetAllAsync();

    [Get("/api/v1/categories/{id}")]
    Task<Refit.ApiResponse<MyShop.Shared.DTOs.Common.ApiResponse<CategoryResponse>>> GetByIdAsync(Guid id);

    [Post("/api/v1/categories")]
    Task<Refit.ApiResponse<MyShop.Shared.DTOs.Common.ApiResponse<CategoryResponse>>> CreateAsync([Body] object request);

    [Put("/api/v1/categories/{id}")]
    Task<Refit.ApiResponse<MyShop.Shared.DTOs.Common.ApiResponse<CategoryResponse>>> UpdateAsync(Guid id, [Body] object request);

    [Delete("/api/v1/categories/{id}")]
    Task<Refit.ApiResponse<MyShop.Shared.DTOs.Common.ApiResponse<bool>>> DeleteAsync(Guid id);
}
