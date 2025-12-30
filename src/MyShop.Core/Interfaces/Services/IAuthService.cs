namespace MyShop.Core.Interfaces.Services;

using MyShop.Core.Common;

/// <summary>
/// Service for authentication operations like password reset.
/// Interface defined in Core layer (UI framework independent).
/// Implementation in Client layer.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Send a password reset code to the specified email.
    /// </summary>
    /// <param name="email">The email address to send the reset code to</param>
    /// <returns>Result indicating success or failure</returns>
    Task<Result<Unit>> SendPasswordResetCodeAsync(string email);

    /// <summary>
    /// Verify a password reset code for the specified email.
    /// </summary>
    /// <param name="email">The email address</param>
    /// <param name="code">The verification code</param>
    /// <returns>Result containing the reset token if successful</returns>
    Task<Result<string>> VerifyPasswordResetCodeAsync(string email, string code);

    /// <summary>
    /// Reset the password using the provided token.
    /// </summary>
    /// <param name="email">The email address</param>
    /// <param name="token">The reset token from verification</param>
    /// <param name="newPassword">The new password</param>
    /// <param name="confirmPassword">The confirmation password</param>
    /// <returns>Result indicating success or failure</returns>
    Task<Result<Unit>> ResetPasswordAsync(string email, string token, string newPassword, string confirmPassword = "");
}
