using System.ComponentModel.DataAnnotations;

namespace MyShop.Shared.DTOs.Requests;

/// <summary>
/// DTO for login request payload.
/// Sent to authentication endpoint for user login.
/// </summary>
public class LoginRequest
{
    /// <summary>
    /// Username or email address for authentication.
    /// </summary>
    [Required(ErrorMessage = "Username or email is required")]
    public string UsernameOrEmail { get; set; } = string.Empty;

    /// <summary>
    /// User password.
    /// </summary>
    [Required(ErrorMessage = "Password is required")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
    public string Password { get; set; } = string.Empty;
}
