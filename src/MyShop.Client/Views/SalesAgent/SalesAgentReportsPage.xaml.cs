using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MyShop.Client.ViewModels.SalesAgent;
using MyShop.Core.Interfaces.Repositories;
using System;
using System.Globalization;

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

    private async void ExportPdfButton_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel.ExportReportCommand?.CanExecute(null) == true)
        {
            await ViewModel.ExportReportCommand.ExecuteAsync(null);
        }
    }

    private async void PredictThisWeekButton_Click(object sender, RoutedEventArgs e)
    {
        ContentDialog loadingDialog = null;
        try
        {
            // Show loading dialog
            loadingDialog = new ContentDialog
            {
                Title = "Loading Forecast",
                XamlRoot = this.XamlRoot
            };

            var loadingPanel = new StackPanel { Spacing = 12, Padding = new Thickness(24) };
            loadingPanel.Children.Add(new ProgressRing
            {
                IsActive = true,
                Width = 40,
                Height = 40
            });
            loadingPanel.Children.Add(new TextBlock
            {
                Text = "Predicting this week's revenue...",
                TextAlignment = TextAlignment.Center,
                Foreground = Application.Current.Resources["TextFillColorSecondaryBrush"] as Microsoft.UI.Xaml.Media.Brush
            });

            loadingDialog.Content = loadingPanel;

            // Start showing loading dialog without waiting (so we can call API while it's showing)
            var showLoadingTask = loadingDialog.ShowAsync();

            // Call API to predict revenue
            var forecastRepository = App.Current.Services.GetRequiredService<IForecastRepository>();
            string todayDate = DateTime.Now.ToString("yyyy-MM-dd");

            Services.LoggingService.Instance.Information($"[SalesAgentReportsPage] Calling forecast API for date: {todayDate}");

            var forecastResult = await forecastRepository.PredictRevenueAsync(todayDate);

            // Close loading dialog before showing result
            loadingDialog.Hide();

            if (!forecastResult.IsSuccess)
            {
                Services.LoggingService.Instance.Error($"[SalesAgentReportsPage] Forecast API failed: {forecastResult.ErrorMessage}");

                // Show error dialog
                var errorDialog = new ContentDialog
                {
                    Title = "Forecast Error",
                    XamlRoot = this.XamlRoot,
                    CloseButtonText = "Close"
                };

                var errorPanel = new StackPanel { Spacing = 12 };
                errorPanel.Children.Add(new TextBlock
                {
                    Text = "Failed to predict revenue",
                    FontSize = 16,
                    FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                    Foreground = Application.Current.Resources["TextFillColorPrimaryBrush"] as Microsoft.UI.Xaml.Media.Brush
                });
                errorPanel.Children.Add(new TextBlock
                {
                    Text = forecastResult.ErrorMessage ?? "Unknown error occurred",
                    FontSize = 12,
                    Foreground = Application.Current.Resources["TextFillColorTertiaryBrush"] as Microsoft.UI.Xaml.Media.Brush,
                    TextWrapping = TextWrapping.Wrap
                });

                errorDialog.Content = errorPanel;
                await errorDialog.ShowAsync();
                return;
            }

            // Get predicted weekly sales from API (in USD)
            double predictedWeeklySalesUsd = forecastResult.Data.PredictedWeeklySales;

            // Convert USD to VND using fixed exchange rate
            // Typical rate: 1 USD = 25,000 VND (adjust as needed)
            const double USD_TO_VND_RATE = 25000;
            double predictedWeeklySalesVnd = predictedWeeklySalesUsd * USD_TO_VND_RATE;

            // Format as VND using same format as CurrencyConverter (dot as thousand separator, no decimals)
            var amount = (decimal)predictedWeeklySalesVnd;
            amount = Math.Round(amount, 0, MidpointRounding.AwayFromZero);

            var nfi = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
            nfi.NumberGroupSeparator = ".";
            nfi.NumberDecimalSeparator = ",";

            string formattedVnd = amount.ToString("#,##0", nfi) + "₫";

            Services.LoggingService.Instance.Information(
                $"[SalesAgentReportsPage] Forecast received: ${predictedWeeklySalesUsd:F2} USD → {formattedVnd} VND");


            // Create result dialog
            var resultDialog = new ContentDialog
            {
                Title = "This Week Revenue Forecast",
                XamlRoot = this.XamlRoot,
                CloseButtonText = "Close"
            };

            var contentPanel = new StackPanel { Spacing = 12 };

            // Forecast value with SuccessGreenBrush (same as Price in SalesAgentProducts)
            contentPanel.Children.Add(new TextBlock
            {
                Text = $"Predicted weekly revenue: {formattedVnd}",
                FontSize = 16,
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                Foreground = Application.Current.Resources["SuccessGreenBrush"] as Microsoft.UI.Xaml.Media.Brush
            });

            // Subtext with USD value
            contentPanel.Children.Add(new TextBlock
            {
                Text = $"Conversion: ${predictedWeeklySalesUsd:F2} USD × {USD_TO_VND_RATE:N0} rate",
                FontSize = 11,
                Foreground = Application.Current.Resources["TextFillColorTertiaryBrush"] as Microsoft.UI.Xaml.Media.Brush
            });

            resultDialog.Content = contentPanel;
            await resultDialog.ShowAsync();
        }
        catch (Exception ex)
        {
            Services.LoggingService.Instance.Error("[SalesAgentReportsPage] PredictThisWeekButton_Click failed", ex);

            // Show error dialog
            var errorDialog = new ContentDialog
            {
                Title = "Error",
                XamlRoot = this.XamlRoot,
                CloseButtonText = "Close"
            };

            var errorPanel = new StackPanel { Spacing = 12 };
            errorPanel.Children.Add(new TextBlock
            {
                Text = "An error occurred while predicting revenue",
                FontSize = 14,
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold
            });

            errorDialog.Content = errorPanel;
            await errorDialog.ShowAsync();
        }
    }
}
