using Microsoft.Extensions.Configuration;
using System;

namespace MyShop.Client.Core.Config
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

        private AppConfig() { }

        public void LoadFromConfiguration(IConfiguration configuration)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            UseMockData = bool.Parse(configuration["UseMockData"] ?? "false");

            if (UseMockData)
            {
                ApiBaseUrl = "mock://localhost";
                System.Diagnostics.Debug.WriteLine($"[AppConfig] MOCK MODE Enabled");
            }
            else
            {
                ApiBaseUrl = configuration["BaseUrl"] 
                    ?? throw new InvalidOperationException("BaseUrl not configured in ApiConfig.json");
                
                RequestTimeoutSeconds = int.Parse(configuration["RequestTimeout"] ?? "30");
                System.Diagnostics.Debug.WriteLine($"[AppConfig] Loaded: BaseUrl={ApiBaseUrl}");
            }

            EnableLogging = bool.Parse(configuration["EnableLogging"] ?? "true");
        }
    }
}
