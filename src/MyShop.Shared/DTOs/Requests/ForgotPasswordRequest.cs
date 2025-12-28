using System.ComponentModel.DataAnnotations;

namespace MyShop.Shared.DTOs.Requests;

/// <summary>
/// DTO for forgot password request.
/// Sent to initiate password reset process.
/// </summary>
public class ForgotPasswordRequest
{
    /// <summary>
    /// Email address to send password reset code to.
    /// </summary>
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = string.Empty;
}
