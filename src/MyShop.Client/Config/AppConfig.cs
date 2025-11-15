using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

namespace MyShop.Client.Config
{
    /// <summary>
    /// Singleton pattern cho application configuration
    /// </summary>
    public sealed class AppConfig
    {
        private static readonly Lazy<AppConfig> _instance = new(() => new AppConfig());
        public static AppConfig Instance => _instance.Value;

        public string ApiBaseUrl { get; private set; } = string.Empty;
        public int RequestTimeoutSeconds { get; private set; } = 30;
        public bool EnableLogging { get; private set; } = true;
        public bool UseMockData { get; private set; } = false;
        public bool UseWindowsCredentialStorage { get; private set; } = true;

        private AppConfig() { }

        public void LoadFromConfiguration(IConfiguration configuration)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            UseMockData = bool.Parse(configuration["UseMockData"] ?? "false");

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
                UseWindowsCredentialStorage = bool.Parse(configuration["UseWindowsCredentialStorage"] ?? "true");
                System.Diagnostics.Debug.WriteLine($"[AppConfig] Loaded: BaseUrl={ApiBaseUrl}");
            }

            EnableLogging = bool.Parse(configuration["EnableLogging"] ?? "true");
        }
    }
}
