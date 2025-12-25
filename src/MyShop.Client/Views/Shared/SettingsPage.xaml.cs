using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MyShop.Core.Common;
using MyShop.Client.ViewModels.Settings;
using MyShop.Client.Views.Dialogs;
using MyShop.Client.Services;
using System;
using System.Diagnostics;
using Windows.ApplicationModel.DataTransfer;

namespace MyShop.Client.Views.Shared;

public sealed partial class SettingsPage : Page
{
    public SettingsViewModel ViewModel { get; }
    private bool _isDialogOpen = false;
    private bool _isInitializing = true;

    public SettingsPage()
    {
        LoggingService.Instance.Debug("→ SettingsPage()");
        try
        {
            this.InitializeComponent();
            LoggingService.Instance.Debug("InitializeComponent completed");
            
            // Get ViewModel from DI
            ViewModel = App.Current.Services.GetRequiredService<SettingsViewModel>();
            this.DataContext = ViewModel;
            LoggingService.Instance.Debug("ViewModel retrieved from DI and DataContext set");
            
            // Initialize theme toggle based on current theme
            ThemeToggle.IsOn = ThemeManager.CurrentTheme == ThemeManager.ThemeType.Dark;
            
            // Show/hide Developer tab based on EnableDeveloperOptions setting
            var configService = App.Current.Services?.GetService<MyShop.Client.Services.Configuration.IConfigurationService>();
            if (configService != null && configService.FeatureFlags.EnableDeveloperOptions)
            {
                DeveloperTab.Visibility = Visibility.Visible;
                LoggingService.Instance.Debug("Developer tab enabled via FeatureFlags.EnableDeveloperOptions");
            }
            else
            {
                DeveloperTab.Visibility = Visibility.Collapsed;
                LoggingService.Instance.Debug("Developer tab hidden - EnableDeveloperOptions is false");
            }
            
            SetupKeyboardShortcuts();
        }
        catch (Exception ex)
        {
            LoggingService.Instance.Error("SettingsPage constructor failed", ex);
            throw;
        }
        finally
        {
            LoggingService.Instance.Debug("← SettingsPage()");
        }
    }

    private void SetupKeyboardShortcuts()
    {
        // Ctrl+S: Save settings
        var saveShortcut = new Microsoft.UI.Xaml.Input.KeyboardAccelerator 
        { 
            Key = Windows.System.VirtualKey.S, 
            Modifiers = Windows.System.VirtualKeyModifiers.Control 
        };
        saveShortcut.Invoked += async (s, e) => { await ViewModel.SaveCommand.ExecuteAsync(null); e.Handled = true; };
        KeyboardAccelerators.Add(saveShortcut);

        // Ctrl+E: Export settings
        var exportShortcut = new Microsoft.UI.Xaml.Input.KeyboardAccelerator 
        { 
            Key = Windows.System.VirtualKey.E, 
            Modifiers = Windows.System.VirtualKeyModifiers.Control 
        };
        exportShortcut.Invoked += async (s, e) => { await ViewModel.ExportSettingsCommand.ExecuteAsync(null); e.Handled = true; };
        KeyboardAccelerators.Add(exportShortcut);

        // Ctrl+I: Import settings
        var importShortcut = new Microsoft.UI.Xaml.Input.KeyboardAccelerator 
        { 
            Key = Windows.System.VirtualKey.I, 
            Modifiers = Windows.System.VirtualKeyModifiers.Control 
        };
        importShortcut.Invoked += async (s, e) => { await ViewModel.ImportSettingsCommand.ExecuteAsync(null); e.Handled = true; };
        KeyboardAccelerators.Add(importShortcut);
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        LoggingService.Instance.Debug("→ OnNavigatedTo");
        try
        {
            base.OnNavigatedTo(e);
            
            // Load settings and wait for completion
            await ViewModel.LoadCommand.ExecuteAsync(null);
            LoggingService.Instance.Debug("Settings loaded");
            
            // Wait for UI to stabilize before allowing toggle events
            // This prevents the Toggled event from firing due to binding updates
            await System.Threading.Tasks.Task.Delay(100);
        }
        catch (Exception ex)
        {
            LoggingService.Instance.Error("OnNavigatedTo failed", ex);
        }
        finally
        {
            // Ensure toggle reflects current theme after load
            ThemeToggle.IsOn = ThemeManager.CurrentTheme == ThemeManager.ThemeType.Dark;
            
            // Allow toggle events only after load completes and UI stabilizes
            _isInitializing = false;
            LoggingService.Instance.Debug("← OnNavigatedTo");
        }
    }

    private async void MockDataToggle_Toggled(object sender, RoutedEventArgs e)
    {
        // Skip during page initialization
        if (_isInitializing) return;
        
        // Prevent opening multiple dialogs
        if (_isDialogOpen) return;
        
        // Show warning that restart is required
        if (sender is ToggleSwitch toggle)
        {
            try
            {
                _isDialogOpen = true;
                
                var dialog = new ContentDialog
                {
                    Title = "Restart Required",
                    Content = "Changing the data source requires restarting the application. Would you like to save this setting?",
                    PrimaryButtonText = "Save & Restart Later",
                    CloseButtonText = "Cancel",
                    XamlRoot = this.XamlRoot
                };

                var result = await dialog.ShowAsync();
                if (result != ContentDialogResult.Primary)
                {
                    // Revert toggle without triggering event again
                    toggle.Toggled -= MockDataToggle_Toggled;
                    toggle.IsOn = !toggle.IsOn;
                    toggle.Toggled += MockDataToggle_Toggled;
                }
            }
            finally
            {
                _isDialogOpen = false;
            }
        }
    }

    private async void ConfigureServer_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var dialog = new ServerConfigDialog
            {
                XamlRoot = this.XamlRoot
            };
            await dialog.ShowAsync();
        }
        catch (Exception ex)
        {
            LoggingService.Instance.Error("Failed to open server config dialog", ex);
        }
    }

    private void OpenLogsFolder_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var logsPath = StorageConstants.LogsDirectory;
            if (!System.IO.Directory.Exists(logsPath))
            {
                System.IO.Directory.CreateDirectory(logsPath);
            }
            Process.Start(new ProcessStartInfo
            {
                FileName = logsPath,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            LoggingService.Instance.Error("Failed to open logs folder", ex);
        }
    }

    private void OpenExportsFolder_Click(object sender, RoutedEventArgs e)
    {
        StorageConstants.OpenExportsFolder();
    }

    private void CopyDebugInfo_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var debugInfo = ViewModel.GetDebugInfo();
            var dataPackage = new DataPackage();
            dataPackage.SetText(debugInfo);
            Clipboard.SetContent(dataPackage);
            
            // Could show a toast notification here
            LoggingService.Instance.Information("Debug info copied to clipboard");
        }
        catch (Exception ex)
        {
            LoggingService.Instance.Error("Failed to copy debug info", ex);
        }
    }

    private void ThemeToggle_Toggled(object sender, RoutedEventArgs e)
    {
        // Skip during page initialization
        if (_isInitializing) return;

        if (sender is ToggleSwitch toggle)
        {
            try
            {
                var newTheme = toggle.IsOn ? ThemeManager.ThemeType.Dark : ThemeManager.ThemeType.Light;
                ThemeManager.ApplyTheme(newTheme);
                LoggingService.Instance.Information($"Theme changed to: {newTheme}");
            }
            catch (Exception ex)
            {
                LoggingService.Instance.Error("Failed to change theme", ex);
                // Revert toggle on error
                toggle.Toggled -= ThemeToggle_Toggled;
                toggle.IsOn = !toggle.IsOn;
                toggle.Toggled += ThemeToggle_Toggled;
            }
        }
    }
}
