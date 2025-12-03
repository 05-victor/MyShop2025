using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

namespace MyShop.Client.Config
{
    /// <summary>
    /// Singleton pattern cho application configuration
    /// 
    /// Developer Options:
    /// - EnableDeveloperOptions: Show/hide developer tools (server config, mock toggle)
    /// - In Release builds, this is always false (hardcoded endpoints)
    /// - In Debug builds, developers can toggle mock/real API
    /// </summary>
    public sealed class AppConfig
    {
        private static readonly Lazy<AppConfig> _instance = new(() => new AppConfig());
        public static AppConfig Instance => _instance.Value;

        #region API Configuration

        /// <summary>
        /// Base URL for API calls
        /// In Release mode, this is hardcoded and cannot be changed
        /// </summary>
        public string ApiBaseUrl { get; private set; } = string.Empty;

        /// <summary>
        /// Request timeout in seconds
        /// </summary>
        public int RequestTimeoutSeconds { get; private set; } = 30;

        #endregion

        #region Feature Flags

        /// <summary>
        /// Enable application logging
        /// </summary>
        public bool EnableLogging { get; private set; } = true;

        /// <summary>
        /// Use mock data instead of real API (development only)
        /// </summary>
        public bool UseMockData { get; private set; } = false;

        /// <summary>
        /// Store logs in project directory instead of AppData (development only)
        /// </summary>
        public bool StoreLogsInProject { get; private set; } = true;

        #endregion

        #region Developer Options

        /// <summary>
        /// Enable developer options UI (server config panel, mock toggle)
        /// 
        /// When TRUE (Debug/Development):
        /// - Login page shows "Config Server" button
        /// - Settings page shows developer section
        /// - Can switch between Mock/Real API
        /// - Can change API endpoint
        /// 
        /// When FALSE (Release/Production):
        /// - All developer UI is hidden
        /// - API endpoint is hardcoded
        /// - Users cannot modify technical settings
        /// </summary>
#if DEBUG
        public bool EnableDeveloperOptions { get; private set; } = true;
#else
        public bool EnableDeveloperOptions { get; private set; } = false;
#endif

        #endregion

        #region Release Hardcoded Values

        /// <summary>
        /// Production API endpoint (used when EnableDeveloperOptions = false)
        /// Change this before deploying to production
        /// </summary>
        private const string PRODUCTION_API_URL = "https://api.myshop2025.com";

        /// <summary>
        /// Staging API endpoint (for QA testing)
        /// </summary>
        private const string STAGING_API_URL = "https://staging-api.myshop2025.com";

        #endregion

        private AppConfig() { }

        public void LoadFromConfiguration(IConfiguration configuration)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

#if RELEASE
            // ===== RELEASE MODE: Hardcoded values, ignore config file =====
            ApiBaseUrl = PRODUCTION_API_URL;
            UseMockData = false;
            EnableDeveloperOptions = false;
            StoreLogsInProject = false;
            EnableLogging = true;
            RequestTimeoutSeconds = 30;
            
            System.Diagnostics.Debug.WriteLine("[AppConfig] ===== RELEASE MODE =====");
            System.Diagnostics.Debug.WriteLine($"[AppConfig] API: {ApiBaseUrl}");
            System.Diagnostics.Debug.WriteLine("[AppConfig] Developer options: DISABLED");
#else
            // ===== DEBUG MODE: Load from config file =====
            
            // Developer options flag
            EnableDeveloperOptions = bool.Parse(configuration["EnableDeveloperOptions"] ?? "true");
            
            // Mock data toggle
            UseMockData = bool.Parse(configuration["UseMockData"] ?? "false");

            // Log storage location
            StoreLogsInProject = bool.Parse(configuration["StoreLogsInProject"] ?? "true");

            if (UseMockData)
            {
                ApiBaseUrl = "mock://localhost";
                System.Diagnostics.Debug.WriteLine("[AppConfig] ===== MOCK MODE ENABLED =====");
                System.Diagnostics.Debug.WriteLine("[AppConfig] Authentication uses data from: Mocks/Data/Json/auth.json");
                System.Diagnostics.Debug.WriteLine("[AppConfig] Demo accounts: admin/admin123, salesman/sales123, customer/customer123");
            }
            else
            {
                // Allow empty BaseUrl and use default fallback
                ApiBaseUrl = configuration["BaseUrl"] ?? "https://localhost:7120";
                
                // If BaseUrl is empty or whitespace, use default
                if (string.IsNullOrWhiteSpace(ApiBaseUrl))
                {
                    ApiBaseUrl = "https://localhost:7120";
                }
                
                RequestTimeoutSeconds = int.Parse(configuration["RequestTimeout"] ?? "30");
                System.Diagnostics.Debug.WriteLine($"[AppConfig] Loaded: BaseUrl={ApiBaseUrl}");
            }

            EnableLogging = bool.Parse(configuration["EnableLogging"] ?? "true");
            
            System.Diagnostics.Debug.WriteLine($"[AppConfig] Developer options: {(EnableDeveloperOptions ? "ENABLED" : "DISABLED")}");
            System.Diagnostics.Debug.WriteLine($"[AppConfig] Logs in project: {StoreLogsInProject}");
#endif
        }

        #region Runtime Configuration (Dev Only)

        /// <summary>
        /// Switch to mock data mode at runtime (development only)
        /// </summary>
        public void SetMockMode(bool useMock)
        {
            if (!EnableDeveloperOptions)
            {
                System.Diagnostics.Debug.WriteLine("[AppConfig] Cannot change mock mode - developer options disabled");
                return;
            }

            UseMockData = useMock;
            if (useMock)
            {
                ApiBaseUrl = "mock://localhost";
            }
            System.Diagnostics.Debug.WriteLine($"[AppConfig] Mock mode changed to: {useMock}");
        }

        /// <summary>
        /// Change API endpoint at runtime (development only)
        /// </summary>
        public void SetApiEndpoint(string endpoint)
        {
            if (!EnableDeveloperOptions)
            {
                System.Diagnostics.Debug.WriteLine("[AppConfig] Cannot change endpoint - developer options disabled");
                return;
            }

            if (!string.IsNullOrWhiteSpace(endpoint))
            {
                ApiBaseUrl = endpoint;
                System.Diagnostics.Debug.WriteLine($"[AppConfig] API endpoint changed to: {endpoint}");
            }
        }

        #endregion
    }
}
