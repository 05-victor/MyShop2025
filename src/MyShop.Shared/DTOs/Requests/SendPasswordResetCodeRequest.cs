using System.ComponentModel.DataAnnotations;

namespace MyShop.Shared.DTOs.Requests;

/// <summary>
/// DTO for requesting a password reset code to be sent to an email address.
/// </summary>
public class SendPasswordResetCodeRequest
{
    /// <summary>
    /// Email address to send the reset code to.
    /// </summary>
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = string.Empty;
}
