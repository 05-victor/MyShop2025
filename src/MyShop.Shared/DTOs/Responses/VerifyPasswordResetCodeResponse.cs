namespace MyShop.Shared.DTOs.Responses;

/// <summary>
/// Response from verifying a password reset code.
/// Contains the reset token needed for password reset.
/// </summary>
public class VerifyPasswordResetCodeResponse
{
    /// <summary>
    /// Reset token that can be used to reset the password.
    /// </summary>
    public string ResetToken { get; set; } = string.Empty;

    /// <summary>
    /// Token expiration time in minutes.
    /// </summary>
    public int ExpirationMinutes { get; set; }
}
