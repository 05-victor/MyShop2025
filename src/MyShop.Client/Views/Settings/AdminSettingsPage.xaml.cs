using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MyShop.Client.ViewModels.Settings;
using MyShop.Client.Helpers;
using System;

namespace MyShop.Client.Views.Settings;

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
