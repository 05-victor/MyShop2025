using System.ComponentModel.DataAnnotations;

namespace MyShop.Shared.DTOs.Requests;

/// <summary>
/// Request DTO for updating settings (Admin only)
/// PUT /api/v1/settings
/// </summary>
public class UpdateSettingsRequest
{
    // General Settings
    [Required]
    [StringLength(100, MinimumLength = 1, ErrorMessage = "Shop name must be between 1 and 100 characters")]
    public string ShopName { get; set; } = string.Empty;
    
    [StringLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
    public string? Address { get; set; }
    
    // App Info
    [Required]
    public string AppName { get; set; } = string.Empty;
    
    [Required]
    public string Version { get; set; } = string.Empty;
    
    [Required]
    public DateTime ReleaseDate { get; set; }
    
    [Required]
    public string License { get; set; } = string.Empty;
    
    [Required]
    [EmailAddress(ErrorMessage = "Support must be a valid email address")]
    public string Support { get; set; } = string.Empty;
    
    // Appearance Settings (affects Admin only)
    [Required]
    public string Theme { get; set; } = "Light"; // "Light" or "Dark"
}
