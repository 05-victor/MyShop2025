using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

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
        
        /// <summary>
        /// Thông tin demo credentials (chỉ dùng trong mock mode)
        /// </summary>
        public DemoCredentials Demo { get; private set; } = new();

        private AppConfig() { }

        public void LoadFromConfiguration(IConfiguration configuration)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            UseMockData = bool.Parse(configuration["UseMockData"] ?? "false");

            if (UseMockData)
            {
                ApiBaseUrl = "mock://localhost";
                
                // Khởi tạo demo credentials
                Demo = new DemoCredentials
                {
                    IsEnabled = true,
                    Accounts = new List<DemoAccount>
                    {
                        new() { Username = "admin", Password = "admin123", Role = "Admin", Description = "Full system access" },
                        new() { Username = "salesman", Password = "sales123", Role = "Salesman", Description = "Sales & commission tracking" },
                        new() { Username = "customer", Password = "customer123", Role = "Customer", Description = "Shopping & orders" }
                    }
                };
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

    /// <summary>
    /// Demo credentials configuration (mock mode only)
    /// </summary>
    public class DemoCredentials
    {
        public bool IsEnabled { get; set; }
        public List<DemoAccount> Accounts { get; set; } = new();
    }

    /// <summary>
    /// Thông tin một tài khoản demo
    /// </summary>
    public class DemoAccount
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
