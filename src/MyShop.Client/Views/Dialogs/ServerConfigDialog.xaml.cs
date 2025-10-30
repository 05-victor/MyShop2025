using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.Json;
using Windows.ApplicationModel.Core;

namespace MyShop.Client.Views.Dialogs
{
    public sealed partial class ServerConfigDialog : ContentDialog
    {
        public string ServerUrl { get; set; } = string.Empty;
        public bool UseMockData { get; private set; }
        private bool _configChanged = false;
        private bool _originalUseMockData;
        private string _originalServerUrl = string.Empty;

        public ServerConfigDialog()
        {
            this.InitializeComponent();
            LoadCurrentConfiguration();
            
            // Store original values to detect changes
            _originalUseMockData = UseMockData;
            _originalServerUrl = ServerUrl;
        }

        [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "ApiConfig is a simple POCO")]
        [UnconditionalSuppressMessage("AOT", "IL3050", Justification = "ApiConfig is a simple POCO")]
        private void LoadCurrentConfiguration()
        {
            try
            {
                var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ApiServer", "ApiConfig.json");
                if (File.Exists(configPath))
                {
                    var json = File.ReadAllText(configPath);
                    var config = JsonSerializer.Deserialize<ApiConfig>(json);
                    var baseUrl = config?.BaseUrl ?? "https://localhost:7120";
                    
                    // If BaseUrl is "mock", use default URL for display
                    ServerUrl = baseUrl == "mock" ? "https://localhost:7120" : baseUrl;
                    UseMockData = config?.UseMockData ?? false;
                }
                else
                {
                    ServerUrl = "https://localhost:7120";
                    UseMockData = false;
                }

                // Set initial radio button selection
                var selectedIndex = UseMockData ? 1 : 0;
                if (ApiModeRadioButtons.Items.Count > selectedIndex)
                {
                    ApiModeRadioButtons.SelectedIndex = selectedIndex;
                }

                UpdatePanelVisibility();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading config: {ex.Message}");
                ServerUrl = "https://localhost:7120";
                UseMockData = false;
                ApiModeRadioButtons.SelectedIndex = 0;
                UpdatePanelVisibility();
            }
        }

        private void ApiModeRadioButtons_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdatePanelVisibility();
        }

        private void UpdatePanelVisibility()
        {
            if (ApiModeRadioButtons.SelectedItem is RadioButton selectedRadio)
            {
                var isMock = selectedRadio.Tag?.ToString() == "mock";
                UseMockData = isMock;

                RealApiPanel.Visibility = isMock ? Visibility.Collapsed : Visibility.Visible;
                RealApiTipsPanel.Visibility = isMock ? Visibility.Collapsed : Visibility.Visible;
                MockDataPanel.Visibility = isMock ? Visibility.Visible : Visibility.Collapsed;
                
                // Track if config changed
                _configChanged = (UseMockData != _originalUseMockData);
            }
        }

        [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "ApiConfig is a simple POCO")]
        [UnconditionalSuppressMessage("AOT", "IL3050", Justification = "ApiConfig is a simple POCO")]
        private async void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            var deferral = args.GetDeferral();
            
            try
            {
                System.Diagnostics.Debug.WriteLine("[ServerConfig] Primary button clicked");
                
                // Check if URL changed
                if (!UseMockData && ServerUrl != _originalServerUrl)
                {
                    _configChanged = true;
                    System.Diagnostics.Debug.WriteLine($"[ServerConfig] URL changed: {_originalServerUrl} -> {ServerUrl}");
                }
                
                // If not Mock mode, validate URL
                if (!UseMockData)
                {
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
                }

                // Save configuration
                System.Diagnostics.Debug.WriteLine($"[ServerConfig] Saving config: UseMockData={UseMockData}, URL={ServerUrl}");
                SaveConfiguration();
                ValidationMessage.Visibility = Visibility.Collapsed;
                System.Diagnostics.Debug.WriteLine("[ServerConfig] Config saved successfully");
                
                // If config changed, show restart dialog
                // IMPORTANT: Must close current dialog first before showing restart dialog
                // because WinUI only allows one ContentDialog at a time
                if (_configChanged)
                {
                    System.Diagnostics.Debug.WriteLine("[ServerConfig] Config changed, closing this dialog first");
                    
                    // Close this dialog first
                    this.Hide();
                    
                    // Small delay to ensure dialog is fully closed
                    await Task.Delay(100);
                    
                    // Now show restart dialog
                    System.Diagnostics.Debug.WriteLine("[ServerConfig] Showing restart dialog");
                    await ShowRestartDialogAsync();
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("[ServerConfig] No changes, closing dialog");
                    // No changes, allow dialog to close normally
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ServerConfig] ERROR: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[ServerConfig] StackTrace: {ex.StackTrace}");
                ValidationMessage.Text = $"Error: {ex.Message}";
                ValidationMessage.Visibility = Visibility.Visible;
                args.Cancel = true;
            }
            finally
            {
                deferral.Complete();
                System.Diagnostics.Debug.WriteLine("[ServerConfig] Deferral completed");
            }
        }
        
        private async System.Threading.Tasks.Task ShowRestartDialogAsync()
        {
            var restartDialog = new ContentDialog
            {
                Title = "🔄 Restart Required",
                Content = "Configuration has been saved. The application needs to restart for changes to take effect.\n\nRestart now?",
                PrimaryButtonText = "Restart Now",
                CloseButtonText = "Later",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.XamlRoot
            };

            var result = await restartDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                RestartApplication();
            }
            else
            {
                // Close the config dialog
                this.Hide();
            }
        }
        
        private void RestartApplication()
        {
            try
            {
                // Get current executable path
                var exePath = Process.GetCurrentProcess().MainModule?.FileName;
                
                if (!string.IsNullOrEmpty(exePath))
                {
                    // Start new instance
                    Process.Start(exePath);
                    
                    // Exit current instance
                    Application.Current.Exit();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error restarting app: {ex.Message}");
                // Fallback: just exit
                Application.Current.Exit();
            }
        }

        private void SaveConfiguration()
        {
            var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ApiServer", "ApiConfig.json");
            var configDir = Path.GetDirectoryName(configPath);

            if (!Directory.Exists(configDir))
            {
                Directory.CreateDirectory(configDir!);
            }

            var config = new ApiConfig
            {
                BaseUrl = UseMockData ? "mock" : ServerUrl.TrimEnd('/'),
                UseMockData = UseMockData
            };

            var json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(configPath, json);

            System.Diagnostics.Debug.WriteLine($"[ServerConfigDialog] Saved: UseMockData={UseMockData}, BaseUrl={config.BaseUrl}");
        }

        private class ApiConfig
        {
            public string BaseUrl { get; set; } = string.Empty;
            public bool UseMockData { get; set; }
        }
    }
}
