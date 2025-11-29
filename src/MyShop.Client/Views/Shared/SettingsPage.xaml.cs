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
        AppLogger.Enter();
        try
        {
            this.InitializeComponent();
            AppLogger.Success("InitializeComponent completed");
            
            // Get ViewModel from DI
            ViewModel = App.Current.Services.GetRequiredService<SettingsViewModel>();
            this.DataContext = ViewModel;
            AppLogger.Success("ViewModel retrieved from DI and DataContext set");
            
            SetupKeyboardShortcuts();
        }
        catch (Exception ex)
        {
            AppLogger.Error("SettingsPage constructor failed", ex);
            throw;
        }
        finally
        {
            AppLogger.Exit();
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
        AppLogger.Enter();
        try
        {
            base.OnNavigatedTo(e);
            
            // Load settings and wait for completion
            await ViewModel.LoadCommand.ExecuteAsync(null);
            AppLogger.Success("Settings loaded");
        }
        catch (Exception ex)
        {
            AppLogger.Error("OnNavigatedTo failed", ex);
        }
        finally
        {
            // Allow toggle events only after load completes
            _isInitializing = false;
            AppLogger.Exit();
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
            AppLogger.Error("Failed to open server config dialog", ex);
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
            AppLogger.Error("Failed to open logs folder", ex);
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
            AppLogger.Info("Debug info copied to clipboard");
        }
        catch (Exception ex)
        {
            AppLogger.Error("Failed to copy debug info", ex);
        }
    }
}
