using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.Json;

namespace MyShop.Client.Views.Dialogs
{
    public sealed partial class ServerConfigDialog : ContentDialog
    {
        public string ServerUrl { get; set; } = string.Empty;

        public ServerConfigDialog()
        {
            this.InitializeComponent();
            LoadCurrentServerUrl();
        }

        [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "ApiConfig is a simple POCO")]
        [UnconditionalSuppressMessage("AOT", "IL3050", Justification = "ApiConfig is a simple POCO")]
        private void LoadCurrentServerUrl()
        {
            try
            {
                var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ApiServer", "ApiConfig.json");
                if (File.Exists(configPath))
                {
                    var json = File.ReadAllText(configPath);
                    var config = JsonSerializer.Deserialize<ApiConfig>(json);
                    ServerUrl = config?.BaseUrl ?? "https://localhost:7120";
                }
                else
                {
                    ServerUrl = "https://localhost:7120";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading config: {ex.Message}");
                ServerUrl = "https://localhost:7120";
            }
        }

        [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "ApiConfig is a simple POCO")]
        [UnconditionalSuppressMessage("AOT", "IL3050", Justification = "ApiConfig is a simple POCO")]
        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            // Validate URL
            if (string.IsNullOrWhiteSpace(ServerUrl))
            {
                ValidationMessage.Text = "Server URL cannot be empty";
                ValidationMessage.Visibility = Visibility.Visible;
                args.Cancel = true;
                return;
            }

            if (!Uri.TryCreate(ServerUrl, UriKind.Absolute, out var uri))
            {
                ValidationMessage.Text = "Invalid URL format. Please use a valid URL (e.g., https://localhost:7120)";
                ValidationMessage.Visibility = Visibility.Visible;
                args.Cancel = true;
                return;
            }

            if (uri.Scheme != "http" && uri.Scheme != "https")
            {
                ValidationMessage.Text = "URL must start with http:// or https://";
                ValidationMessage.Visibility = Visibility.Visible;
                args.Cancel = true;
                return;
            }

            // Save to config file
            try
            {
                var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ApiServer", "ApiConfig.json");
                var configDir = Path.GetDirectoryName(configPath);
                
                if (!Directory.Exists(configDir))
                {
                    Directory.CreateDirectory(configDir!);
                }

                var config = new ApiConfig { BaseUrl = ServerUrl.TrimEnd('/') };
                var json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(configPath, json);

                ValidationMessage.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                ValidationMessage.Text = $"Error saving configuration: {ex.Message}";
                ValidationMessage.Visibility = Visibility.Visible;
                args.Cancel = true;
            }
        }

        private class ApiConfig
        {
            public string BaseUrl { get; set; } = string.Empty;
        }
    }
}
