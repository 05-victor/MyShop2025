using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MyShop.Shared.Models;
using MyShop.Client.ViewModels.SalesAgent;
using MyShop.Client.Services;
using System;

namespace MyShop.Client.Views.SalesAgent
{
    public sealed partial class SalesAgentDashboardPage : Page
    {
        public SalesAgentDashboardViewModel ViewModel { get; }

        public SalesAgentDashboardPage()
        {
            try
            {
                LoggingService.Instance.Debug("SalesAgentDashboardPage constructor started");
                
                this.InitializeComponent();
                ViewModel = App.Current.Services.GetRequiredService<SalesAgentDashboardViewModel>();
                this.DataContext = ViewModel;
                
                LoggingService.Instance.Information("SalesAgentDashboardPage created successfully");
            }
            catch (Exception ex)
            {
                LoggingService.Instance.Error("Failed to create SalesAgentDashboardPage", ex);
                NavigationLogger.LogViewModelInitializationError(nameof(SalesAgentDashboardViewModel), ex);
                throw;
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            try
            {
                base.OnNavigatedTo(e);
                
                NavigationLogger.LogNavigatedTo(nameof(SalesAgentDashboardPage), e.Parameter);
                
                if (e.Parameter is User user)
                {
                    LoggingService.Instance.Information($"Initializing Sales Agent Dashboard for user: {user.Username}");
                    ViewModel.Initialize(user);
                }
                else
                {
                    LoggingService.Instance.Warning("No User parameter provided to SalesAgentDashboardPage");
                }
            }
            catch (Exception ex)
            {
                LoggingService.Instance.Error("Failed in OnNavigatedTo for SalesAgentDashboardPage", ex);
                GlobalExceptionHandler.LogException(ex, "SalesAgentDashboardPage.OnNavigatedTo");
                
                // Show error to user
                _ = new ContentDialog
                {
                    Title = "Error",
                    Content = $"Failed to load dashboard: {ex.Message}",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                }.ShowAsync();
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            NavigationLogger.LogNavigatedFrom(nameof(SalesAgentDashboardPage));
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
                LoggingService.Instance.Error("Failed to refresh dashboard", ex);
            }
        }

        /// <summary>
        /// Handle chart Refresh menu click
        /// </summary>
        private async void LineChart_RefreshRequested(object sender, EventArgs e)
        {
            try
            {
                LoggingService.Instance.Debug("Chart refresh requested");
                if (ViewModel.RefreshCommand?.CanExecute(null) == true)
                {
                    await ViewModel.RefreshCommand.ExecuteAsync(null);
                }
            }
            catch (Exception ex)
            {
                LoggingService.Instance.Error("Failed to refresh chart data", ex);
            }
        }

        /// <summary>
        /// Handle chart Export CSV menu click
        /// </summary>
        private async void LineChart_ExportRequested(object sender, string csvData)
        {
            try
            {
                LoggingService.Instance.Debug("Chart export requested");
                if (ViewModel.ExportCommand?.CanExecute(null) == true)
                {
                    await ViewModel.ExportCommand.ExecuteAsync(null);
                }
            }
            catch (Exception ex)
            {
                LoggingService.Instance.Error("Failed to export chart data", ex);
            }
        }
    }
}
