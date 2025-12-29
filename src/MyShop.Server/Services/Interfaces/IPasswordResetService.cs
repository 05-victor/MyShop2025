using MyShop.Shared.DTOs;

namespace MyShop.Server.Services.Interfaces;

/// <summary>
/// Service interface for password reset functionality.
/// Handles generation and validation of password reset codes.
/// </summary>
public interface IPasswordResetService
{
    /// <summary>
    /// Sends a password reset code to the user's email.
    /// </summary>
    /// <param name="email">Email address of the user</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>ServiceResult indicating success or failure</returns>
    Task<ServiceResult> SendPasswordResetCodeAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates a reset code for a given email.
    /// </summary>
    /// <param name="email">Email address</param>
    /// <param name="resetCode">Reset code to validate</param>
    /// <returns>True if code is valid, false otherwise</returns>
    Task<bool> ValidateResetCodeAsync(string email, string resetCode);

    /// <summary>
    /// Resets password using a valid reset code.
    /// </summary>
    /// <param name="email">Email address</param>
    /// <param name="resetCode">Reset code</param>
    /// <param name="newPassword">New password</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>ServiceResult indicating success or failure</returns>
    Task<ServiceResult> ResetPasswordAsync(string email, string resetCode, string newPassword, CancellationToken cancellationToken = default);
}
