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
        private const string DefaultServerUrl = "https://localhost:7120";
        
        public string ServerUrl { get; set; } = DefaultServerUrl;
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
                var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config", "ApiConfig.json");
                if (File.Exists(configPath))
                {
                    var json = File.ReadAllText(configPath);
                    var config = JsonSerializer.Deserialize<ApiConfig>(json);
                    var baseUrl = config?.BaseUrl ?? DefaultServerUrl;
                    
                    // If BaseUrl is "mock", use default URL for display
                    ServerUrl = baseUrl == "mock" ? DefaultServerUrl : baseUrl;
                    UseMockData = config?.UseMockData ?? false;
                }
                else
                {
                    // ServerUrl already has default value from property initialization
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
                // ServerUrl already has default value from property initialization
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
                
                // Auto-fill default URL when switching to Real API mode if URL is empty
                if (!isMock && string.IsNullOrWhiteSpace(ServerUrl))
                {
                    ServerUrl = DefaultServerUrl;
                    ServerUrlTextBox.Text = ServerUrl;
                    System.Diagnostics.Debug.WriteLine($"[ServerConfig] Auto-filled default URL: {ServerUrl}");
                }
                
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
                    // Auto-fill default if empty
                    if (string.IsNullOrWhiteSpace(ServerUrl))
                    {
                        ServerUrl = DefaultServerUrl;
                        ServerUrlTextBox.Text = ServerUrl;
                        System.Diagnostics.Debug.WriteLine("[ServerConfig] Auto-filled empty URL with default");
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
                    // Create a batch script to restart the app
                    // This ensures the old process is fully terminated before starting new one
                    var batchPath = Path.Combine(Path.GetTempPath(), "restart_myshop.bat");
                    var batchContent = $@"@echo off
timeout /t 1 /nobreak > nul
start """" ""{exePath}""
exit";
                    File.WriteAllText(batchPath, batchContent);
                    
                    // Start the batch script
                    var startInfo = new ProcessStartInfo
                    {
                        FileName = batchPath,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        WorkingDirectory = Path.GetDirectoryName(exePath) ?? ""
                    };
                    
                    Process.Start(startInfo);
                    
                    // Exit current instance immediately
                    Application.Current.Exit();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error restarting app: {ex.Message}");
                // Fallback: just exit and user can manually restart
                Application.Current.Exit();
            }
        }

        private void SaveConfiguration()
        {
            try
            {
                var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config", "ApiConfig.json");
                var configDir = Path.GetDirectoryName(configPath);

                if (!Directory.Exists(configDir))
                {
                    Directory.CreateDirectory(configDir!);
                }

                // Read existing config to preserve other settings
                var config = new ApiConfig();
                if (File.Exists(configPath))
                {
                    try
                    {
                        var existingJson = File.ReadAllText(configPath);
                        config = JsonSerializer.Deserialize<ApiConfig>(existingJson) ?? new ApiConfig();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[ServerConfig] Error reading existing config: {ex.Message}");
                    }
                }

                // Update only the fields from dialog
                config.BaseUrl = UseMockData ? "mock" : ServerUrl.TrimEnd('/');
                config.UseMockData = UseMockData;

                // Serialize with pretty formatting using PascalCase (matching file format)
                var options = new JsonSerializerOptions 
                { 
                    WriteIndented = true,
                    PropertyNamingPolicy = null // Use default PascalCase for property names
                };
                var json = JsonSerializer.Serialize(config, options);
                
                // Write to file with UTF-8 encoding
                File.WriteAllText(configPath, json, System.Text.Encoding.UTF8);

                System.Diagnostics.Debug.WriteLine($"[ServerConfigDialog] ✅ Config saved successfully!");
                System.Diagnostics.Debug.WriteLine($"[ServerConfigDialog] Path: {configPath}");
                System.Diagnostics.Debug.WriteLine($"[ServerConfigDialog] UseMockData: {config.UseMockData}");
                System.Diagnostics.Debug.WriteLine($"[ServerConfigDialog] BaseUrl: {config.BaseUrl}");
                System.Diagnostics.Debug.WriteLine($"[ServerConfigDialog] JSON Content:\n{json}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ServerConfig] ❌ Error saving config: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[ServerConfig] Stack trace: {ex.StackTrace}");
                throw; // Re-throw to let caller handle
            }
        }

        private class ApiConfig
        {
            public bool UseMockData { get; set; }
            public string BaseUrl { get; set; } = string.Empty;
            public int RequestTimeout { get; set; } = 30;
            public bool EnableLogging { get; set; } = true;
            public bool UseWindowsCredentialStorage { get; set; } = true;
        }
    }
}
