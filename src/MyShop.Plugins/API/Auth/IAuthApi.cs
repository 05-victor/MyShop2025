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
    /// Authenticate user and get JWT access + refresh tokens.
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
    [Get("/api/v1/users/me")]
    Task<Refit.ApiResponse<MyShop.Shared.DTOs.Common.ApiResponse<UserInfoResponse>>> GetMeAsync();

    /// <summary>
    /// Refresh access token using refresh token.
    /// Returns new access token and optionally a new refresh token (if rotation is enabled).
    /// </summary>
    [Post("/api/v1/auth/refresh-token")]
    Task<Refit.ApiResponse<MyShop.Shared.DTOs.Common.ApiResponse<RefreshTokenResponse>>> RefreshTokenAsync([Body] RefreshTokenRequest request);

    /// <summary>
    /// Revoke a refresh token (for logout or security purposes).
    /// </summary>
    [Post("/api/v1/auth/revoke-token")]
    Task<Refit.ApiResponse<MyShop.Shared.DTOs.Common.ApiResponse<object>>> RevokeTokenAsync([Body] RefreshTokenRequest request);

    /// <summary>
    /// Send a verification email to the authenticated user's email address.
    /// User must be authenticated via JWT token.
    /// </summary>
    [Post("/api/v1/email-verification/send")]
    Task<Refit.ApiResponse<MyShop.Shared.DTOs.Common.ApiResponse<object>>> SendVerificationEmailAsync();

    /// <summary>
    /// Send a password reset code to the specified email.
    /// User does not need to be authenticated.
    /// </summary>
    [Post("/api/v1/passwordreset/forgot-password")]
    Task<Refit.ApiResponse<MyShop.Shared.DTOs.Common.ApiResponse<object>>> SendPasswordResetCodeAsync([Body] ForgotPasswordRequest request);

    /// <summary>
    /// Reset password using email, reset code, and new password.
    /// User does not need to be authenticated.
    /// </summary>
    [Post("/api/v1/passwordreset/reset-password")]
    Task<Refit.ApiResponse<MyShop.Shared.DTOs.Common.ApiResponse<object>>> ResetPasswordAsync([Body] ResetPasswordRequest request);
}
