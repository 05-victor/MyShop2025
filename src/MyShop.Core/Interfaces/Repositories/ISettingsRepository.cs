using MyShop.Core.Common;
using MyShop.Shared.DTOs.Requests;
using MyShop.Shared.DTOs.Responses;

namespace MyShop.Core.Interfaces.Repositories;

/// <summary>
/// Repository interface for Settings operations
/// Abstracts API calls and provides business logic for settings management
/// </summary>
public interface ISettingsRepository
{
    /// <summary>
    /// Get current application settings
    /// Accessible to all authenticated users
    /// </summary>
    /// <returns>Current settings with role-based visibility (trial info for Admin only)</returns>
    Task<Result<SettingsResponse>> GetSettingsAsync();

    /// <summary>
    /// Update application settings (Admin only)
    /// Updates general, appearance, and trial settings
    /// Automatically refreshes settings after update
    /// </summary>
    /// <param name="request">Settings update request</param>
    /// <returns>Updated settings from server</returns>
    Task<Result<SettingsResponse>> UpdateSettingsAsync(UpdateSettingsRequest request);

    /// <summary>
    /// Update appearance settings (SalesAgent and User only)
    /// Allows non-admin users to change theme preference
    /// Automatically refreshes settings after update
    /// </summary>
    /// <param name="request">Appearance update request</param>
    /// <returns>Updated settings from server</returns>
    Task<Result<SettingsResponse>> UpdateAppearanceAsync(UpdateAppearanceRequest request);
}
