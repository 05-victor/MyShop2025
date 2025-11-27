using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MyShop.Client.ViewModels.Settings;
using MyShop.Client.Services;
using System;

namespace MyShop.Client.Views.Shared;

public sealed partial class SettingsPage : Page
{
    public SettingsViewModel ViewModel { get; }

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

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        AppLogger.Enter();
        try
        {
            base.OnNavigatedTo(e);
            
            // Load settings
            _ = ViewModel.LoadCommand.ExecuteAsync(null);
            AppLogger.Success("Settings loaded");
        }
        catch (Exception ex)
        {
            AppLogger.Error("OnNavigatedTo failed", ex);
        }
        finally
        {
            AppLogger.Exit();
        }
    }
}
