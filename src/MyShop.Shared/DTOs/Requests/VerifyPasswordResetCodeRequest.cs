using System.ComponentModel.DataAnnotations;

namespace MyShop.Shared.DTOs.Requests;

/// <summary>
/// DTO for verifying a password reset code.
/// </summary>
public class VerifyPasswordResetCodeRequest
{
    /// <summary>
    /// Email address associated with the reset request.
    /// </summary>
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Reset code sent to the user's email.
    /// </summary>
    [Required(ErrorMessage = "Code is required")]
    [StringLength(10, MinimumLength = 4, ErrorMessage = "Code must be between 4 and 10 characters")]
    public string Code { get; set; } = string.Empty;
}
