using System.ComponentModel.DataAnnotations;

namespace MyShop.Shared.DTOs.Requests;

/// <summary>
/// Request DTO for updating settings (Admin only)
/// PUT /api/v1/settings
/// Only includes fields that Admin can actually edit (ShopName, Address, Theme, License, trial dates)
/// </summary>
public class UpdateSettingsRequest
{
    // General Settings - Editable by Admin
    [Required]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Shop name must be between 1 and 100 characters")]
    public string ShopName { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
    public string? Address { get; set; }

    // Appearance Settings - Editable by Admin
    [Required]
    public string Theme { get; set; } = "Light"; // "Light" or "Dark"

    // License Settings - Editable by Admin
    [Required]
    public string License { get; set; } = "Commercial"; // "Trial" or "Commercial"
}
