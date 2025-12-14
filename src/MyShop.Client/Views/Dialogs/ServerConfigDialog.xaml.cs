using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
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

        [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "AppSettings is a simple POCO")]
        [UnconditionalSuppressMessage("AOT", "IL3050", Justification = "AppSettings is a simple POCO")]
        private void LoadCurrentConfiguration()
        {
            try
            {
                var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.Development.json");
                if (File.Exists(configPath))
                {
                    var json = File.ReadAllText(configPath);
                    
                    // Use case-insensitive deserialization to support both PascalCase and camelCase
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true // Allow both PascalCase (original) and camelCase (if previously saved)
                    };
                    var appSettings = JsonSerializer.Deserialize<AppSettingsJson>(json, options);

                    // Read from Api section
                    var baseUrl = appSettings?.Api?.BaseUrl ?? DefaultServerUrl;
                    ServerUrl = string.IsNullOrWhiteSpace(baseUrl) ? DefaultServerUrl : baseUrl;

                    // Read from FeatureFlags section
                    UseMockData = appSettings?.FeatureFlags?.UseMockData ?? false;

                    System.Diagnostics.Debug.WriteLine($"[ServerConfig] Loaded config from appsettings.Development.json - UseMockData: {UseMockData}, BaseUrl: {ServerUrl}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[ServerConfig] appsettings.Development.json not found, using defaults");
                    // ServerUrl already has default value from property initialization
                    UseMockData = false;
                }

                // Set initial radio button selection
                var selectedIndex = UseMockData ? 1 : 0;
                System.Diagnostics.Debug.WriteLine($"[ServerConfig] Setting SelectedIndex to {selectedIndex} (UseMockData={UseMockData})");
                if (ApiModeRadioButtons.Items.Count > selectedIndex)
                {
                    // Explicitly set both index and item so UI reflects the stored mode
                    ApiModeRadioButtons.SelectedIndex = selectedIndex;
                    ApiModeRadioButtons.SelectedItem = ApiModeRadioButtons.Items[selectedIndex];
                }

                // Explicitly call UpdatePanelVisibility to ensure UI is synced
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
            // Prefer SelectedItem tag if available; fallback to SelectedIndex ordering (0=real, 1=mock)
            var selectedRadio = ApiModeRadioButtons.SelectedItem as RadioButton;
            var isMock = selectedRadio?.Tag?.ToString() == "mock" || ApiModeRadioButtons.SelectedIndex == 1;
            UseMockData = isMock;

            System.Diagnostics.Debug.WriteLine($"[ServerConfig] UpdatePanelVisibility - SelectedIndex={ApiModeRadioButtons.SelectedIndex}, Tag={selectedRadio?.Tag}, isMock={isMock}");

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

            // Track if config changed - will be checked in ContentDialog_PrimaryButtonClick
            // Don't set _configChanged here, let the button click handler determine it
        }

        [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "ApiConfig is a simple POCO")]
        [UnconditionalSuppressMessage("AOT", "IL3050", Justification = "ApiConfig is a simple POCO")]
        private async void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            var deferral = args.GetDeferral();

            try
            {
                System.Diagnostics.Debug.WriteLine("[ServerConfig] Primary button clicked");
                System.Diagnostics.Debug.WriteLine($"[ServerConfig] Original UseMockData: {_originalUseMockData}, Current UseMockData: {UseMockData}");
                System.Diagnostics.Debug.WriteLine($"[ServerConfig] Original URL: {_originalServerUrl}, Current URL: {ServerUrl}");

                // Check if UseMockData mode changed
                if (UseMockData != _originalUseMockData)
                {
                    _configChanged = true;
                    System.Diagnostics.Debug.WriteLine($"[ServerConfig] UseMockData changed: {_originalUseMockData} -> {UseMockData}");
                }

                // Check if URL changed (only relevant when in Real API mode)
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
                    // await Task.Delay(100);

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

        [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "AppSettings is a simple POCO")]
        [UnconditionalSuppressMessage("AOT", "IL3050", Justification = "AppSettings is a simple POCO")]
        private void SaveConfiguration()
        {
            try
            {
                var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.Development.json");

                if (!File.Exists(configPath))
                {
                    System.Diagnostics.Debug.WriteLine($"[ServerConfig] File not found: {configPath}");
                    throw new FileNotFoundException($"appsettings.Development.json not found at {configPath}");
                }

                // Read existing appsettings to preserve other settings
                var json = File.ReadAllText(configPath);
                var appSettings = JsonSerializer.Deserialize<AppSettingsJson>(json);

                if (appSettings == null)
                {
                    appSettings = new AppSettingsJson();
                }

                // Ensure Api section exists
                if (appSettings.Api == null)
                {
                    appSettings.Api = new ApiSection();
                }

                // Ensure FeatureFlags section exists
                if (appSettings.FeatureFlags == null)
                {
                    appSettings.FeatureFlags = new FeatureFlagsSection();
                }

                // Update only the relevant fields
                // Note: Always save the actual server URL, regardless of MockData mode
                // This ensures URL is preserved when switching between modes
                appSettings.Api.BaseUrl = ServerUrl.TrimEnd('/');
                appSettings.FeatureFlags.UseMockData = UseMockData;

                // Serialize with pretty formatting using PascalCase (matching original file format)
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = null, // Use default PascalCase to match original file
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
                };
                var updatedJson = JsonSerializer.Serialize(appSettings, options);

                // Write to file with UTF-8 encoding
                File.WriteAllText(configPath, updatedJson, System.Text.Encoding.UTF8);

                System.Diagnostics.Debug.WriteLine($"[ServerConfig] ✅ Config saved successfully!");
                System.Diagnostics.Debug.WriteLine($"[ServerConfig] Path: {configPath}");
                System.Diagnostics.Debug.WriteLine($"[ServerConfig] UseMockData: {appSettings.FeatureFlags.UseMockData}");
                System.Diagnostics.Debug.WriteLine($"[ServerConfig] BaseUrl: {appSettings.Api.BaseUrl}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ServerConfig] ❌ Error saving config: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[ServerConfig] Stack trace: {ex.StackTrace}");
                throw; // Re-throw to let caller handle
            }
        }

        // Helper classes representing appsettings.Development.json structure
        private class AppSettingsJson
        {
            public ApiSection Api { get; set; } = new();
            public FeatureFlagsSection FeatureFlags { get; set; } = new();
            public LoggingSection Logging { get; set; } = new();
            public StorageSection Storage { get; set; } = new();
            public UserPreferencesSection UserPreferences { get; set; } = new();
        }

        private class ApiSection
        {
            public string BaseUrl { get; set; } = DefaultServerUrl;
            public int RequestTimeoutSeconds { get; set; } = 60;
            public RetryPolicySection RetryPolicy { get; set; } = new();
        }

        private class RetryPolicySection
        {
            public int MaxRetries { get; set; } = 2;
            public int RetryDelayMilliseconds { get; set; } = 500;
        }

        private class FeatureFlagsSection
        {
            public bool UseMockData { get; set; }
            public bool EnableDeveloperOptions { get; set; }
            public bool EnableLogging { get; set; } = true;
            public bool EnableCaching { get; set; }
            public bool EnableOfflineMode { get; set; } = true;
        }

        private class LoggingSection
        {
            public string MinimumLevel { get; set; } = "Debug";
            public bool EnableConsoleLogging { get; set; } = true;
            public bool EnableFileLogging { get; set; } = true;
            public bool StoreLogsInProject { get; set; } = true;
            public int MaxLogFileSizeMB { get; set; } = 50;
            public int RetainLogDays { get; set; } = 7;
        }

        private class StorageSection
        {
            public bool UseSecureCredentialStorage { get; set; } = true;
            public string CredentialStorageType { get; set; } = "DPAPI";
            public string SettingsStorageType { get; set; } = "File";
            public int CacheExpirationMinutes { get; set; } = 5;
        }

        private class UserPreferencesSection
        {
            public string Theme { get; set; } = "Light";
            public string Language { get; set; } = "en-US";
            public int ItemsPerPage { get; set; } = 10;
            public bool EnableNotifications { get; set; } = true;
            public int AutoSaveInterval { get; set; } = 60;
        }
    }
}
