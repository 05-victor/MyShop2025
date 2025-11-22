using MyShop.Shared.DTOs.Requests;
using MyShop.Shared.DTOs.Responses;
using Refit;

namespace MyShop.Plugins.API.Auth;

/// <summary>
/// Refit interface cho Auth API endpoints
/// </summary>
[Headers("User-Agent: MyShop-Client/1.0")]
public interface IAuthApi
{
    [Post("/api/v1/auth/login")]
    Task<Refit.ApiResponse<MyShop.Shared.DTOs.Common.ApiResponse<LoginResponse>>> LoginAsync([Body] LoginRequest request);

    [Post("/api/v1/auth/register")]
    Task<Refit.ApiResponse<MyShop.Shared.DTOs.Common.ApiResponse<CreateUserResponse>>> RegisterAsync([Body] CreateUserRequest request);

    [Get("/api/v1/auth/me")]
    Task<Refit.ApiResponse<MyShop.Shared.DTOs.Common.ApiResponse<UserInfoResponse>>> GetMeAsync();
}
