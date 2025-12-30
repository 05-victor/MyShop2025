using MyShop.Shared.Models;
using MyShop.Core.Common;

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
    Task<Result<AppSettings>> GetAsync();

    /// <summary>
    /// Save application settings to storage
    /// </summary>
    Task<Result<Unit>> SaveAsync(AppSettings settings);

    /// <summary>
    /// Reset settings to defaults and delete storage
    /// </summary>
    Task<Result<Unit>> ResetAsync();

    /// <summary>
    /// Save theme to session storage (for app startup without user context).
    /// This allows the app to remember the last used theme before login.
    /// Does not affect current user context.
    /// </summary>
    /// <param name="theme">Theme string to save ("Light" or "Dark")</param>
    /// <returns>Success if saved, Failure otherwise</returns>
    Task<Result<Unit>> SaveSessionThemeAsync(string theme);

    /// <summary>
    /// Load theme from session storage (used for app startup without user context).
    /// This retrieves the last used theme before login.
    /// Does not affect current user context.
    /// </summary>
    /// <returns>Theme string ("Light", "Dark") or null if not found/error</returns>
    Task<string?> LoadSessionThemeAsync();
}
