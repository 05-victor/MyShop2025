using MyShop.Shared.DTOs.Commons;
using MyShop.Shared.DTOs.Responses;
using Refit;

namespace MyShop.Plugins.API.Users;

/// <summary>
/// Refit interface for Users API endpoints
/// </summary>
[Headers("User-Agent: MyShop-Client/1.0")]
public interface IUsersApi
{
    [Get("/api/v1/users")]
    Task<Refit.ApiResponse<MyShop.Shared.DTOs.Common.ApiResponse<PagedResult<UserInfoResponse>>>> GetAllAsync(
        [Query] int pageNumber = 1, 
        [Query] int pageSize = 10);

    [Get("/api/v1/users/{id}")]
    Task<Refit.ApiResponse<MyShop.Shared.DTOs.Common.ApiResponse<UserInfoResponse>>> GetByIdAsync(Guid id);

    [Post("/api/v1/users")]
    Task<Refit.ApiResponse<MyShop.Shared.DTOs.Common.ApiResponse<object>>> CreateAsync([Body] object request);

    [Put("/api/v1/users/{id}")]
    Task<Refit.ApiResponse<MyShop.Shared.DTOs.Common.ApiResponse<UserInfoResponse>>> UpdateAsync(Guid id, [Body] object request);

    [Delete("/api/v1/users/{id}")]
    Task<Refit.ApiResponse<MyShop.Shared.DTOs.Common.ApiResponse<bool>>> DeleteAsync(Guid id);
}
