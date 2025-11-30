using MyShop.Shared.DTOs.Requests;
using MyShop.Shared.DTOs.Responses;
using Refit;

namespace MyShop.Plugins.API.Auth;

/// <summary>
/// Refit interface for Auth API endpoints.
/// Defines HTTP methods for authentication operations.
/// </summary>
[Headers("User-Agent: MyShop-Client/1.0")]
public interface IAuthApi
{
    /// <summary>
    /// Authenticate user and get JWT token.
    /// </summary>
    [Post("/api/v1/auth/login")]
    Task<Refit.ApiResponse<MyShop.Shared.DTOs.Common.ApiResponse<LoginResponse>>> LoginAsync([Body] LoginRequest request);

    /// <summary>
    /// Register a new user account.
    /// </summary>
    [Post("/api/v1/auth/register")]
    Task<Refit.ApiResponse<MyShop.Shared.DTOs.Common.ApiResponse<CreateUserResponse>>> RegisterAsync([Body] CreateUserRequest request);

    /// <summary>
    /// Get current authenticated user information.
    /// </summary>
    [Get("/api/v1/auth/me")]
    Task<Refit.ApiResponse<MyShop.Shared.DTOs.Common.ApiResponse<UserInfoResponse>>> GetMeAsync();
}
