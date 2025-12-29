using MyShop.Core.Common;
using MyShop.Core.Interfaces.Services;
using MyShop.Plugins.API.Auth;
using MyShop.Shared.DTOs.Requests;
using MyShop.Shared.DTOs.Responses;

namespace MyShop.Client.Services;

/// <summary>
/// Implementation of IAuthService for password reset operations.
/// Makes actual API calls to the backend endpoints.
/// </summary>
public class AuthService : IAuthService
{
    private readonly IAuthApi _authApi;

    public AuthService(IAuthApi authApi)
    {
        _authApi = authApi;
    }

    /// <summary>
    /// Send a password reset code to the specified email.
    /// </summary>
    public async Task<Result<Unit>> SendPasswordResetCodeAsync(string email)
    {
        try
        {
            var request = new ForgotPasswordRequest { Email = email };
            var response = await _authApi.SendPasswordResetCodeAsync(request);

            if (response.IsSuccessStatusCode && response.Content?.Success == true)
            {
                return Result<Unit>.Success(Unit.Value);
            }

            var errorMessage = response.Content?.Message ?? "Failed to send password reset code";
            return Result<Unit>.Failure(errorMessage);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AuthService] SendPasswordResetCodeAsync Error: {ex.Message}");
            return Result<Unit>.Failure($"Network error: {ex.Message}");
        }
    }

    /// <summary>
    /// Verify a password reset code for the specified email.
    /// Note: This method validates the code client-side only.
    /// The backend validates it during the reset operation.
    /// </summary>
    public async Task<Result<string>> VerifyPasswordResetCodeAsync(string email, string code)
    {
        try
        {
            // Client-side validation only - the code format is just a string
            if (string.IsNullOrWhiteSpace(code))
            {
                return Result<string>.Failure("Code is required");
            }

            // Return the code as the token - backend will validate it during reset
            await Task.Delay(500); // Simulate network delay for UX consistency
            return Result<string>.Success(code);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AuthService] VerifyPasswordResetCodeAsync Error: {ex.Message}");
            return Result<string>.Failure($"Network error: {ex.Message}");
        }
    }

    /// <summary>
    /// Reset the password using the provided token.
    /// </summary>
    public async Task<Result<Unit>> ResetPasswordAsync(string email, string token, string newPassword, string confirmPassword = "")
    {
        try
        {
            // The backend expects email, resetCode (token), newPassword, and confirmPassword
            var request = new ResetPasswordRequest
            {
                Email = email,
                ResetCode = token, // The token is actually the reset code
                NewPassword = newPassword,
                ConfirmPassword = confirmPassword ?? newPassword // If not provided, use newPassword
            };

            var response = await _authApi.ResetPasswordAsync(request);

            if (response.IsSuccessStatusCode && response.Content?.Success == true)
            {
                return Result<Unit>.Success(Unit.Value);
            }

            var errorMessage = response.Content?.Message ?? "Failed to reset password";
            return Result<Unit>.Failure(errorMessage);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AuthService] ResetPasswordAsync Error: {ex.Message}");
            return Result<Unit>.Failure($"Network error: {ex.Message}");
        }
    }
}
