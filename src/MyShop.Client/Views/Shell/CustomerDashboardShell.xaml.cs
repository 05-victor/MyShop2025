using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MyShop.Client.ViewModels.Shell;
using MyShop.Shared.Models;
using MyShop.Client.Views.Customer;
using MyShop.Client.Views.Shared;
using MyShop.Client.Helpers;

namespace MyShop.Client.Views.Shell
{
    public sealed partial class CustomerDashboardShell : Page
    {
        public DashboardShellViewModel ViewModel { get; }
        private NavigationViewItem? _currentContentItem;
        private bool _isRestoringSelection;
        private bool _isInitialized;

        public CustomerDashboardShell()
        {
            InitializeComponent();
            ViewModel = App.Current.Services.GetRequiredService<DashboardShellViewModel>();
            DataContext = ViewModel;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is User user)
            {
                ViewModel.Initialize(user);
                if (_isInitialized) return;
                _isInitialized = true;

                ContentFrame.Navigate(typeof(CustomerDashboardPage), user);
                _currentContentItem = FindNavigationItemByTag("home");
                if (_currentContentItem != null)
                {
                    _isRestoringSelection = true;
                    Nav.SelectedItem = _currentContentItem;
                    _isRestoringSelection = false;
                }
            }
        }

        private NavigationViewItem? FindNavigationItemByTag(string tag)
        {
            return Nav.MenuItems.OfType<NavigationViewItem>()
                      .FirstOrDefault(i => (i.Tag as string) == tag);
        }

        private void Nav_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (_isRestoringSelection || args.SelectedItemContainer is not NavigationViewItem item)
                return;

            var tag = item.Tag as string ?? string.Empty;
            _currentContentItem = item;

            switch (tag)
            {
                case "home":
                    NavigateToPage(typeof(CustomerDashboardPage), ViewModel.CurrentUser);
                    break;
                case "shopping":
                    NavigateToPage(typeof(ProductBrowsePage), ViewModel.CurrentUser);
                    break;
                case "cart":
                    NavigateToPage(typeof(CartPage), ViewModel.CurrentUser);
                    break;
                case "checkout":
                    NavigateToPage(typeof(CheckoutPage), ViewModel.CurrentUser);
                    break;
                case "purchaseOrders":
                    NavigateToPage(typeof(PurchaseOrdersPage), ViewModel.CurrentUser);
                    break;
                case "profile":
                    NavigateToPage(typeof(ProfilePage), ViewModel.CurrentUser);
                    break;
                case "settings":
                    NavigateToPage(typeof(SettingsPage), ViewModel.CurrentUser);
                    break;
                default:
                    AppLogger.Warning($"Customer menu item '{tag}' not implemented yet");
                    RestoreSelection();
                    break;
            }
        }

        private void NavigateToPage(Type pageType, object? parameter = null)
        {
            try
            {
                var cacheSize = ContentFrame.CacheSize;
                ContentFrame.CacheSize = 0;
                ContentFrame.Navigate(pageType, parameter);
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
