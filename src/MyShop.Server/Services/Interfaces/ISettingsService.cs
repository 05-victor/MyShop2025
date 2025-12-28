using MyShop.Shared.DTOs.Requests;
using MyShop.Shared.DTOs.Responses;

namespace MyShop.Server.Services.Interfaces;

/// <summary>
/// Service interface for managing application settings
/// </summary>
public interface ISettingsService
{
    /// <summary>
    /// Get all settings (unified response for all roles)
    /// </summary>
    Task<SettingsResponse> GetSettingsAsync();
    
    /// <summary>
    /// Update settings (Admin only)
    /// </summary>
    Task<SettingsResponse> UpdateSettingsAsync(UpdateSettingsRequest request);
    
    /// <summary>
    /// Update appearance settings (SalesAgent/User only)
    /// </summary>
    Task<SettingsResponse> UpdateAppearanceAsync(UpdateAppearanceRequest request);
}
