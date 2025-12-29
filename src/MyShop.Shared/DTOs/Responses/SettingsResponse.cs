namespace MyShop.Shared.DTOs.Responses;

/// <summary>
/// Unified settings response for GET /api/v1/settings
/// Available to all roles (Admin, SalesAgent, Customer/User)
/// </summary>
public class SettingsResponse
{
    // General Settings (visible to all)
    public string ShopName { get; set; } = string.Empty;
    public string? Address { get; set; }
    
    // Appearance Settings (per-user preference)
    public string Theme { get; set; } = "Light"; // "Light" or "Dark"
    
    // App Info (read-only)
    public string AppName { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public DateTime ReleaseDate { get; set; }
    public string License { get; set; } = string.Empty;
    public string Support { get; set; } = string.Empty;
    
    // Trial Settings (only populated for Admin users)
    public bool IsTrialActive { get; set; }
    public DateTime? TrialStartDate { get; set; }
    public DateTime? TrialEndDate { get; set; }
}
