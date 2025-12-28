using System.ComponentModel.DataAnnotations;

namespace MyShop.Shared.DTOs.Requests;

/// <summary>
/// DTO for resetting password with reset code.
/// </summary>
public class ResetPasswordRequest
{
    /// <summary>
    /// Email address of the user requesting password reset.
    /// </summary>
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Reset code sent to user's email.
    /// </summary>
    [Required(ErrorMessage = "Reset code is required")]
    public string ResetCode { get; set; } = string.Empty;

    /// <summary>
    /// New password to set.
    /// </summary>
    [Required(ErrorMessage = "New password is required")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
    public string NewPassword { get; set; } = string.Empty;

    /// <summary>
    /// Confirmation of new password.
    /// </summary>
    [Required(ErrorMessage = "Password confirmation is required")]
    [Compare(nameof(NewPassword), ErrorMessage = "Passwords do not match")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
