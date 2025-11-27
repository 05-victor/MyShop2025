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
    }
}
