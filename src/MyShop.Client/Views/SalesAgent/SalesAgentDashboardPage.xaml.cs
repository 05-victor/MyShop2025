using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MyShop.Shared.Models;
using MyShop.Client.ViewModels.SalesAgent;
using MyShop.Client.Services;
using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;

namespace MyShop.Client.Views.SalesAgent
{
    public sealed partial class SalesAgentDashboardPage : Page
    {
        public SalesAgentDashboardViewModel ViewModel { get; }
        private const double DESKTOP_BREAKPOINT = 1280; // Threshold for desktop layout

        public SalesAgentDashboardPage()
        {
            try
            {
                LoggingService.Instance.Debug("SalesAgentDashboardPage constructor started");

                this.InitializeComponent();
                ViewModel = App.Current.Services.GetRequiredService<SalesAgentDashboardViewModel>();
                this.DataContext = ViewModel;

                // Subscribe to window size changes
                if (App.MainWindow != null)
                {
                    App.MainWindow.SizeChanged += Window_SizeChanged;
                }

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

                // Apply initial responsive layout
                if (App.MainWindow != null)
                {
                    UpdateLowStockProductsLayout(App.MainWindow.Bounds.Width);
                }

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

        /// <summary>
        /// Handle window size change to update responsive layout
        /// </summary>
        private void Window_SizeChanged(object sender, WindowSizeChangedEventArgs args)
        {
            try
            {
                double width = args.Size.Width;
                UpdateLowStockProductsLayout(width);
            }
            catch (Exception ex)
            {
                LoggingService.Instance.Error("Failed to handle window size change", ex);
            }
        }

        /// <summary>
        /// Update Low Stock Products layout based on window width
        /// Desktop (width >= 1280): Horizontal layout (Category | Quantity)
        /// Tablet (width < 1280): Vertical layout (Category above Quantity)
        /// </summary>
        private void UpdateLowStockProductsLayout(double windowWidth)
        {
            bool isDesktopLayout = windowWidth >= DESKTOP_BREAKPOINT;

            // Get all DesktopLayout and TabletLayout elements from the ListView items
            var listView = FindName("LowStockProductsListView") as ListView;
            if (listView == null) return;

            for (int i = 0; i < listView.Items.Count; i++)
            {
                var container = listView.ContainerFromIndex(i) as ListViewItem;
                if (container == null) continue;

                // Find the desktop and tablet layouts within the item template
                var desktopLayout = FindInElement(container, "DesktopLayout") as Grid;
                var tabletLayout = FindInElement(container, "TabletLayout") as StackPanel;

                if (desktopLayout != null && tabletLayout != null)
                {
                    desktopLayout.Visibility = isDesktopLayout ? Visibility.Visible : Visibility.Collapsed;
                    tabletLayout.Visibility = isDesktopLayout ? Visibility.Collapsed : Visibility.Visible;
                }
            }
        }

        /// <summary>
        /// Helper method to find a child element by name within a parent element
        /// </summary>
        private FrameworkElement? FindInElement(FrameworkElement? parent, string elementName)
        {
            if (parent == null) return null;

            if (parent.Name == elementName)
                return parent;

            int childCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i) as FrameworkElement;
                var result = FindInElement(child, elementName);
                if (result != null)
                    return result;
            }

            return null;
        }
    }
}
