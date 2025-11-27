using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MyShop.Client.ViewModels.Shell;
using MyShop.Shared.Models;
using MyShop.Client.Views.Admin;
using MyShop.Client.Views.Shared;
using MyShop.Client.Services;
using MyShop.Core.Interfaces.Services;

namespace MyShop.Client.Views.Shell
{
    public sealed partial class AdminDashboardShell : Page
    {
        public DashboardShellViewModel ViewModel { get; }
        private readonly INavigationService _navigationService;
        private NavigationViewItem? _currentContentItem;
        private bool _isRestoringSelection;
        private bool _isInitialized;

        public AdminDashboardShell()
        {
            try
            {
                AppLogger.Enter();
                InitializeComponent();
                ViewModel = App.Current.Services.GetRequiredService<DashboardShellViewModel>();
                _navigationService = App.Current.Services.GetRequiredService<INavigationService>();
                DataContext = ViewModel;

                // Register the shell's ContentFrame for in-shell navigation
                Loaded += (s, e) => _navigationService.RegisterShellFrame(ContentFrame);
                Unloaded += (s, e) => _navigationService.UnregisterShellFrame();

                // Subscribe to ContentFrame navigation to sync NavigationView selection
                ContentFrame.Navigated += ContentFrame_Navigated;

                AppLogger.Exit();
            }
            catch (Exception ex)
            {
                AppLogger.Error("AdminDashboardShell constructor failed", ex);
                throw;
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            try
            {
                AppLogger.Enter();
                base.OnNavigatedTo(e);

                if (e.Parameter is User user)
                {
                    ViewModel.Initialize(user);

                    if (_isInitialized) return;
                    _isInitialized = true;

                    ContentFrame.Navigate(typeof(AdminDashboardPage), user);

                    _currentContentItem = FindNavigationItemByTag("dashboard");
                    if (_currentContentItem != null)
                    {
                        _isRestoringSelection = true;
                        Nav.SelectedItem = _currentContentItem;
                        _isRestoringSelection = false;
                    }
                }
                AppLogger.Exit();
            }
            catch (Exception ex)
            {
                AppLogger.Error("OnNavigatedTo failed", ex);
                throw;
            }
        }

        private NavigationViewItem? FindNavigationItemByTag(string tag)
        {
            return Nav.MenuItems
                      .OfType<NavigationViewItem>()
                      .FirstOrDefault(i => (i.Tag as string) == tag);
        }

        private void Nav_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (_isRestoringSelection || args.SelectedItemContainer is not NavigationViewItem item)
                return;

            var tag = item.Tag as string ?? string.Empty;

            switch (tag)
            {
                case "dashboard":
                    _currentContentItem = item;
                    NavigateToPage(typeof(AdminDashboardPage), ViewModel.CurrentUser);
                    break;
                case "products":
                    _currentContentItem = item;
                    NavigateToPage(typeof(AdminProductsPage), ViewModel.CurrentUser);
                    break;
                case "profile":
                    _currentContentItem = item;
                    NavigateToPage(typeof(ProfilePage), ViewModel.CurrentUser);
                    break;
                case "settings":
                    _currentContentItem = item;
                    NavigateToPage(typeof(SettingsPage), ViewModel.CurrentUser);
                    break;
                case "reports":
                    _currentContentItem = item;
                    NavigateToPage(typeof(AdminReportsPage), ViewModel.CurrentUser);
                    break;
                case "users":
                    _currentContentItem = item;
                    NavigateToPage(typeof(AdminUsersPage), ViewModel.CurrentUser);
                    break;
                case "salesAgents":
                    _currentContentItem = item;
                    NavigateToPage(typeof(AdminAgentRequestsPage), ViewModel.CurrentUser);
                    break;
            }
        }

        private void NavigateToPage(Type pageType, object? parameter = null)
        {
            try
            {
                var cacheSize = ContentFrame.CacheSize;
                ContentFrame.CacheSize = 0;
                
                bool result = parameter != null 
                    ? ContentFrame.Navigate(pageType, parameter)
                    : ContentFrame.Navigate(pageType);
                
                ContentFrame.CacheSize = cacheSize;
            }
            catch (Exception ex)
            {
                AppLogger.Error($"Failed to navigate to {pageType.Name}", ex);
            }
        }

        private void RestoreSelection()
        {
            if (_currentContentItem == null) return;
            _isRestoringSelection = true;
            Nav.SelectedItem = _currentContentItem;
            _isRestoringSelection = false;
        }

        /// <summary>
        /// Handles ContentFrame navigation to automatically update NavigationView selection
        /// This ensures the NavigationView stays in sync regardless of how navigation was triggered
        /// (via NavigationView selection or via IShellNavigationService from ViewModels)
        /// </summary>
        private void ContentFrame_Navigated(object sender, NavigationEventArgs e)
        {
            // Map the navigated page type to its corresponding NavigationView tag
            var tag = GetNavigationTagForPageType(e.SourcePageType);
            
            if (string.IsNullOrEmpty(tag))
                return;

            // Find the corresponding NavigationViewItem
            var item = FindNavigationItemByTag(tag);
            
            // Update the NavigationView selection if we found a matching item
            // Use _isRestoringSelection to prevent re-triggering Nav_SelectionChanged
            if (item != null && !_isRestoringSelection)
            {
                _isRestoringSelection = true;
                Nav.SelectedItem = item;
                _currentContentItem = item;
                _isRestoringSelection = false;
            }
        }

        /// <summary>
        /// Maps page types to their corresponding NavigationView item tags
        /// </summary>
        private string? GetNavigationTagForPageType(Type? pageType)
        {
            if (pageType == null)
                return null;

            // Map each page type to its NavigationView tag
            if (pageType == typeof(AdminDashboardPage))
                return "dashboard";
            else if (pageType == typeof(AdminProductsPage))
                return "products";
            else if (pageType == typeof(AdminReportsPage))
                return "reports";
            else if (pageType == typeof(AdminAgentRequestsPage))
                return "salesAgents";
            else if (pageType == typeof(AdminUsersPage))
                return "users";
            else if (pageType == typeof(ProfilePage))
                return "profile";
            else if (pageType == typeof(SettingsPage))
                return "settings";

            return null;
        }

        /// <summary>
        /// Handle logout navigation item tap
        /// </summary>
        private async void LogoutItem_Tapped(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            try
            {
                // Execute logout command
                if (ViewModel.LogoutCommand.CanExecute(null))
                {
                    await ViewModel.LogoutCommand.ExecuteAsync(null);
                }
            }
            catch (Exception ex)
            {
                AppLogger.Error("Failed to logout", ex);
            }
        }
    }
}
