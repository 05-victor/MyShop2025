using MyShop.Data.Entities;

namespace MyShop.Data.Repositories.Interfaces;

/// <summary>
/// Repository interface for managing application settings
/// </summary>
public interface ISettingsRepository
{
    /// <summary>
    /// Get global app settings (there should only be one record)
    /// </summary>
    Task<AppSetting?> GetAppSettingsAsync();
    
    /// <summary>
    /// Update global app settings
    /// </summary>
    Task<AppSetting> UpdateAppSettingsAsync(AppSetting appSetting);
    
    /// <summary>
    /// Get user preference settings by user ID
    /// </summary>
    Task<UserPreference?> GetUserPreferenceAsync(Guid userId);
    
    /// <summary>
    /// Create or update user preference settings
    /// </summary>
    Task<UserPreference> UpsertUserPreferenceAsync(UserPreference userPreference);
}
