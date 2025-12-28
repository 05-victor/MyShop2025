using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MyShop.Client.ViewModels.SalesAgent;
using System;

namespace MyShop.Client.Views.SalesAgent;

/// <summary>
/// Reports page for SalesAgent role
/// </summary>
public sealed partial class SalesAgentReportsPage : Page
{
    public SalesAgentReportsViewModel ViewModel { get; }

    public SalesAgentReportsPage()
    {
        // Enhanced constructor with comprehensive error handling
        try
        {
            Services.LoggingService.Instance.Debug("[SalesAgentReportsPage] Constructor start");

            // Resolve ViewModel
            ViewModel = App.Current.Services.GetRequiredService<SalesAgentReportsViewModel>();
            Services.LoggingService.Instance.Debug("[SalesAgentReportsPage] ViewModel resolved");
        }
        catch (Exception ex)
        {
            Services.LoggingService.Instance.Error($"[SalesAgentReportsPage] DI FAILED - ViewModel resolution", ex);
            throw; // Re-throw to surface the actual DI issue
        }

        // STEP 2: Wrap InitializeComponent with detailed logging
        try
        {
            Services.LoggingService.Instance.Debug("[SalesAgentReportsPage] Calling InitializeComponent");
            this.InitializeComponent();
            Services.LoggingService.Instance.Debug("[SalesAgentReportsPage] InitializeComponent SUCCESS");

            // CRITICAL FIX: Set DataContext so {Binding} resolves to ViewModel
            this.DataContext = ViewModel;
        }
        catch (Exception ex)
        {
            Services.LoggingService.Instance.Error($"[SalesAgentReportsPage] XAML PARSE FAILED", ex);
            Services.LoggingService.Instance.Error($"Exception Type: {ex.GetType().FullName}");
            Services.LoggingService.Instance.Error($"Message: {ex.Message}");
            Services.LoggingService.Instance.Error($"StackTrace: {ex.StackTrace}");

            // Create minimal fallback UI
            this.Content = new Microsoft.UI.Xaml.Controls.TextBlock
            {
                Text = $"❌ XAML CRASH in SalesAgentReportsPage\n\nType: {ex.GetType().Name}\nMessage: {ex.Message}\n\nLogs: {Services.LoggingService.Instance.GetLogDirectory()}",
                Margin = new Microsoft.UI.Xaml.Thickness(24),
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Red),
                TextWrapping = Microsoft.UI.Xaml.TextWrapping.Wrap,
                FontFamily = new Microsoft.UI.Xaml.Media.FontFamily("Consolas")
            };
            return;
        }
    }

    // Initialize ViewModel in OnNavigatedTo to avoid constructor crash
    protected override void OnNavigatedTo(Microsoft.UI.Xaml.Navigation.NavigationEventArgs e)
    {
        try
        {
            base.OnNavigatedTo(e);
            Services.NavigationLogger.LogNavigatedTo(nameof(SalesAgentReportsPage), e.Parameter);

            // Initialize ViewModel after page is loaded and UI thread is ready
            _ = ViewModel.InitializeCommand.ExecuteAsync(null);
        }
        catch (Exception ex)
        {
            Services.LoggingService.Instance.Error($"[SalesAgentReportsPage] OnNavigatedTo failed", ex);
        }
    }

    private async void RefreshContainer_RefreshRequested(RefreshContainer sender, RefreshRequestedEventArgs args)
    {
        using var deferral = args.GetDeferral();
        try
        {
            if (ViewModel.RefreshCommand?.CanExecute(null) == true)
            {
                await ViewModel.RefreshCommand.ExecuteAsync(null);
            }
        }
        catch (Exception ex)
        {
            Services.LoggingService.Instance.Error("Failed to refresh reports", ex);
        }
    }
}
