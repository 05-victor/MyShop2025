using MyShop.Shared.DTOs.Requests;
using MyShop.Shared.DTOs.Responses;
using Refit;

namespace MyShop.Plugins.ApiClients.Auth;

/// <summary>
/// Refit interface cho Auth API endpoints
/// </summary>
[Headers("User-Agent: MyShop-Client/1.0")]
public interface IAuthApiClient
{
    [Post("/api/v1/auth/login")]
    Task<MyShop.Shared.DTOs.Common.ApiResponse<LoginResponse>> LoginAsync([Body] LoginRequest request);

    [Post("/api/v1/auth/register")]
    Task<MyShop.Shared.DTOs.Common.ApiResponse<CreateUserResponse>> RegisterAsync([Body] CreateUserRequest request);

    [Get("/api/v1/auth/me")]
    Task<MyShop.Shared.DTOs.Common.ApiResponse<UserInfoResponse>> GetMeAsync();
}
