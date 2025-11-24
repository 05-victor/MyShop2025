using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MyShop.Client.ViewModels.Shell;
using MyShop.Shared.Models;
using MyShop.Client.Views.Admin;
using MyShop.Client.Views.Shared;
using MyShop.Client.Helpers;
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
    }
}
