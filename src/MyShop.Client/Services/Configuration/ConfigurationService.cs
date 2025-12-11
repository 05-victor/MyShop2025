using Microsoft.Extensions.Options;
using MyShop.Client.Options;

namespace MyShop.Client.Services.Configuration
{
    /// <summary>
    /// Implementation of centralized configuration service.
    /// Aggregates all Options classes and provides a clean API for configuration access.
    /// 
    /// Supports hot reload for user preferences (IOptionsMonitor).
    /// Immutable for API, FeatureFlags, Logging, and Storage (IOptions).
    /// </summary>
    public class ConfigurationService : IConfigurationService
    {
        private readonly IOptions<ApiOptions> _apiOptions;
        private readonly IOptions<FeatureFlagOptions> _featureFlagOptions;
        private readonly IOptions<LoggingOptions> _loggingOptions;
        private readonly IOptions<StorageOptions> _storageOptions;
        private readonly IOptionsMonitor<UserPreferencesOptions> _userPreferencesMonitor;

        public ConfigurationService(
            IOptions<ApiOptions> apiOptions,
            IOptions<FeatureFlagOptions> featureFlagOptions,
            IOptions<LoggingOptions> loggingOptions,
            IOptions<StorageOptions> storageOptions,
            IOptionsMonitor<UserPreferencesOptions> userPreferencesMonitor)
        {
            _apiOptions = apiOptions ?? throw new ArgumentNullException(nameof(apiOptions));
            _featureFlagOptions = featureFlagOptions ?? throw new ArgumentNullException(nameof(featureFlagOptions));
            _loggingOptions = loggingOptions ?? throw new ArgumentNullException(nameof(loggingOptions));
            _storageOptions = storageOptions ?? throw new ArgumentNullException(nameof(storageOptions));
            _userPreferencesMonitor = userPreferencesMonitor ?? throw new ArgumentNullException(nameof(userPreferencesMonitor));

            // Subscribe to user preferences changes
            _userPreferencesMonitor.OnChange(OnUserPreferencesChanged);

            System.Diagnostics.Debug.WriteLine("[ConfigurationService] Initialized with hot reload support");
        }

        public ApiOptions Api => _apiOptions.Value;

        public FeatureFlagOptions FeatureFlags => _featureFlagOptions.Value;

        public LoggingOptions Logging => _loggingOptions.Value;

        public StorageOptions Storage => _storageOptions.Value;

        public UserPreferencesOptions UserPreferences => _userPreferencesMonitor.CurrentValue;

        public string Environment
        {
            get
            {
                return System.Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
                    ?? System.Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
                    ?? "Production";
            }
        }

        public bool IsDevelopment => Environment.Equals("Development", StringComparison.OrdinalIgnoreCase);

        public bool IsProduction => Environment.Equals("Production", StringComparison.OrdinalIgnoreCase);

        public event EventHandler<ConfigurationChangedEventArgs>? ConfigurationChanged;

        public void Reload()
        {
            System.Diagnostics.Debug.WriteLine("[ConfigurationService] Manual reload requested");
            
            // Note: IOptions<T> doesn't support manual reload, only IOptionsMonitor<T>
            // For now, we only support hot reload for UserPreferences
            // Other options require app restart
            
            OnConfigurationChanged("UserPreferences");
        }

        private void OnUserPreferencesChanged(UserPreferencesOptions options, string? name)
        {
            System.Diagnostics.Debug.WriteLine($"[ConfigurationService] User preferences changed: Theme={options.Theme}, Language={options.Language}");
            OnConfigurationChanged("UserPreferences");
        }

        private void OnConfigurationChanged(string sectionName)
        {
            ConfigurationChanged?.Invoke(this, new ConfigurationChangedEventArgs
            {
                SectionName = sectionName,
                ChangedAt = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Gets a summary of current configuration for debugging.
        /// </summary>
        public string GetConfigurationSummary()
        {
            return $@"
╔══════════════════════════════════════════════════════════
║ MYSHOP2025 CONFIGURATION SUMMARY
╠══════════════════════════════════════════════════════════
║ Environment: {Environment}
║ Mode: {(IsDevelopment ? "DEVELOPMENT" : IsProduction ? "PRODUCTION" : "STAGING")}
╠══════════════════════════════════════════════════════════
║ API
║   BaseUrl: {Api.BaseUrl}
║   Timeout: {Api.RequestTimeoutSeconds}s
║   Retry: {Api.RetryPolicy.MaxRetries} attempts, {Api.RetryPolicy.RetryDelayMilliseconds}ms delay
╠══════════════════════════════════════════════════════════
║ FEATURE FLAGS
║   UseMockData: {FeatureFlags.UseMockData}
║   EnableDeveloperOptions: {FeatureFlags.EnableDeveloperOptions}
║   EnableCaching: {FeatureFlags.EnableCaching}
║   EnableOfflineMode: {FeatureFlags.EnableOfflineMode}
║   Production Ready: {FeatureFlags.IsProductionReady()}
╠══════════════════════════════════════════════════════════
║ LOGGING
║   MinimumLevel: {Logging.MinimumLevel}
║   Console: {Logging.EnableConsoleLogging}
║   File: {Logging.EnableFileLogging}
║   StoreInProject: {Logging.StoreLogsInProject}
║   MaxFileSize: {Logging.MaxLogFileSizeMB}MB
║   RetainDays: {Logging.RetainLogDays}
╠══════════════════════════════════════════════════════════
║ STORAGE
║   CredentialType: {Storage.CredentialStorageType}
║   SettingsType: {Storage.SettingsStorageType}
║   Secure: {Storage.IsSecure()}
║   CacheExpiration: {Storage.CacheExpirationMinutes}min
╠══════════════════════════════════════════════════════════
║ USER PREFERENCES
║   Theme: {UserPreferences.Theme}
║   Language: {UserPreferences.Language}
║   ItemsPerPage: {UserPreferences.ItemsPerPage}
║   Notifications: {UserPreferences.EnableNotifications}
║   AutoSave: {UserPreferences.AutoSaveInterval}s
╚══════════════════════════════════════════════════════════
";
        }
    }
}
