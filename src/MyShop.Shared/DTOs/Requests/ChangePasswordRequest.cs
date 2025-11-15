using System.ComponentModel.DataAnnotations;

namespace MyShop.Shared.DTOs.Requests;

/// <summary>
/// Request DTO for changing user password
/// </summary>
public class ChangePasswordRequest
{
    /// <summary>
    /// Current password for verification
    /// </summary>
    [Required(ErrorMessage = "Current password is required")]
    public string CurrentPassword { get; set; } = string.Empty;

    /// <summary>
    /// New password to set
    /// </summary>
    [Required(ErrorMessage = "New password is required")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
    public string NewPassword { get; set; } = string.Empty;
}
