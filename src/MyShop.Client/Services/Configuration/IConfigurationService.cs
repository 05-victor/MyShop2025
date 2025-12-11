using MyShop.Client.Options;

namespace MyShop.Client.Services.Configuration
{
    /// <summary>
    /// Centralized configuration service interface.
    /// Provides easy access to all application configuration options.
    /// 
    /// Use this instead of injecting IOptions<T> directly in ViewModels.
    /// This provides a cleaner API and allows for future enhancements (hot reload events, etc.)
    /// </summary>
    public interface IConfigurationService
    {
        /// <summary>
        /// API configuration options (base URL, timeout, retry policy).
        /// </summary>
        ApiOptions Api { get; }

        /// <summary>
        /// Feature flags (mock data, developer options, caching, etc.)
        /// </summary>
        FeatureFlagOptions FeatureFlags { get; }

        /// <summary>
        /// Logging configuration (level, outputs, file rotation).
        /// </summary>
        LoggingOptions Logging { get; }

        /// <summary>
        /// Storage configuration (credential storage, cache settings).
        /// </summary>
        StorageOptions Storage { get; }

        /// <summary>
        /// User preferences (theme, language, items per page).
        /// These can change at runtime.
        /// </summary>
        UserPreferencesOptions UserPreferences { get; }

        /// <summary>
        /// Gets the current environment name (Development, Staging, Production).
        /// </summary>
        string Environment { get; }

        /// <summary>
        /// Checks if the application is running in development mode.
        /// </summary>
        bool IsDevelopment { get; }

        /// <summary>
        /// Checks if the application is running in production mode.
        /// </summary>
        bool IsProduction { get; }

        /// <summary>
        /// Gets a summary of current configuration for debugging.
        /// </summary>
        string GetConfigurationSummary();

        /// <summary>
        /// Reloads configuration from disk (for hot reload scenarios).
        /// </summary>
        void Reload();

        /// <summary>
        /// Event raised when configuration changes (hot reload).
        /// Subscribe to this to react to configuration changes at runtime.
        /// </summary>
        event EventHandler<ConfigurationChangedEventArgs>? ConfigurationChanged;
    }

    /// <summary>
    /// Event args for configuration change notifications.
    /// </summary>
    public class ConfigurationChangedEventArgs : EventArgs
    {
        public string SectionName { get; init; } = string.Empty;
        public DateTime ChangedAt { get; init; } = DateTime.UtcNow;
    }
}
