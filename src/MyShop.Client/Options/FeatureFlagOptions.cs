using System.ComponentModel.DataAnnotations;

namespace MyShop.Client.Options
{
    /// <summary>
    /// Feature flags configuration.
    /// Maps to "FeatureFlags" section in appsettings.json
    /// Controls application features and behaviors.
    /// </summary>
    public class FeatureFlagOptions : OptionsBase
    {
        public override string SectionName => "FeatureFlags";

        /// <summary>
        /// Use mock data instead of real API calls.
        /// When true, repositories will use mock implementations.
        /// Recommended: true for Development, false for Staging/Production.
        /// </summary>
        public bool UseMockData { get; set; } = false;

        /// <summary>
        /// Enable developer options UI (server config panel, debug tools).
        /// When false, all developer-specific UI elements are hidden.
        /// Recommended: true for Development/Staging, false for Production.
        /// </summary>
        public bool EnableDeveloperOptions { get; set; } = false;

        /// <summary>
        /// Enable application-wide logging.
        /// </summary>
        public bool EnableLogging { get; set; } = true;

        /// <summary>
        /// Enable caching of API responses and data.
        /// When true, frequently accessed data will be cached in memory.
        /// Recommended: false for Development (to always get fresh data), true for Production.
        /// </summary>
        public bool EnableCaching { get; set; } = true;

        /// <summary>
        /// Enable offline mode support.
        /// When true, app can function with cached data when network is unavailable.
        /// </summary>
        public bool EnableOfflineMode { get; set; } = false;

        public override bool Validate()
        {
            // Feature flags don't have strict validation rules
            // but we can enforce business rules here
            
            // Example: Production should never use mock data
            // This would be checked at runtime based on Environment variable
            
            return true;
        }

        /// <summary>
        /// Checks if the current configuration is suitable for production.
        /// </summary>
        public bool IsProductionReady()
        {
            return !UseMockData && !EnableDeveloperOptions;
        }

        /// <summary>
        /// Checks if this is a development configuration.
        /// </summary>
        public bool IsDevelopmentMode()
        {
            return UseMockData || EnableDeveloperOptions;
        }
    }
}
