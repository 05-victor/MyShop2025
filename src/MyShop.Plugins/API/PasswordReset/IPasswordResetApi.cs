using MyShop.Shared.DTOs.Common;
using MyShop.Shared.DTOs.Requests;
using Refit;

namespace MyShop.Plugins.API.PasswordReset;

/// <summary>
/// Refit API interface for password reset endpoints.
/// </summary>
public interface IPasswordResetApi
{
    /// <summary>
    /// Request a password reset code to be sent to email.
    /// </summary>
    [Post("/api/v1/password-reset/forgot-password")]
    Task<ApiResponse<MyShop.Shared.DTOs.Common.ApiResponse<object>>> ForgotPasswordAsync([Body] MyShop.Shared.DTOs.Requests.ForgotPasswordRequest request);

    /// <summary>
    /// Validate a reset code.
    /// </summary>
    [Get("/api/v1/password-reset/validate-code")]
    Task<ApiResponse<MyShop.Shared.DTOs.Common.ApiResponse<bool>>> ValidateResetCodeAsync([Query] string email, [Query] string code);

    /// <summary>
    /// Reset password using a valid reset code.
    /// </summary>
    [Post("/api/v1/password-reset/reset-password")]
    Task<ApiResponse<MyShop.Shared.DTOs.Common.ApiResponse<object>>> ResetPasswordAsync([Body] MyShop.Shared.DTOs.Requests.ResetPasswordRequest request);
}
