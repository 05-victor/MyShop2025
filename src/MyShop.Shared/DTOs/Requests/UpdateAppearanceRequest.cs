using System.ComponentModel.DataAnnotations;

namespace MyShop.Shared.DTOs.Requests;

/// <summary>
/// Request DTO for updating appearance settings (SalesAgent/User only)
/// PUT /api/v1/settings/appearance
/// </summary>
public class UpdateAppearanceRequest
{
    [Required]
    public string Theme { get; set; } = "Light"; // "Light" or "Dark"
}
