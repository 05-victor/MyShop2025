using MyShop.Shared.Models;

namespace MyShop.Core.Interfaces.Infrastructure;

/// <summary>
/// Storage interface for application settings/preferences
/// Provides abstraction over file-based, registry, or cloud storage
/// </summary>
public interface ISettingsStorage
{
    /// <summary>
    /// Load application settings from storage
    /// Returns default settings if none exist
    /// </summary>
    Task<AppSettings> GetAsync();

    /// <summary>
    /// Save application settings to storage
    /// </summary>
    Task SaveAsync(AppSettings settings);

    /// <summary>
    /// Reset settings to defaults and delete storage
    /// </summary>
    Task ResetAsync();
}
