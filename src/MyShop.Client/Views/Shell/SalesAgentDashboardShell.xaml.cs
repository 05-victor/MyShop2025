using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MyShop.Client.ViewModels.Shell;
using MyShop.Shared.Models;
using MyShop.Client.Views.SalesAgent;
using MyShop.Client.Views.Shared;
using MyShop.Client.Helpers;
using MyShop.Core.Interfaces.Services;

namespace MyShop.Client.Views.Shell
{
    public sealed partial class SalesAgentDashboardShell : Page
    {
        public DashboardShellViewModel ViewModel { get; }
        private readonly INavigationService _navigationService;
        private NavigationViewItem? _currentContentItem;
        private bool _isRestoringSelection;
        private bool _isInitialized;

        public SalesAgentDashboardShell()
        {
            InitializeComponent();
            ViewModel = App.Current.Services.GetRequiredService<DashboardShellViewModel>();
            _navigationService = App.Current.Services.GetRequiredService<INavigationService>();
            DataContext = ViewModel;

            // Register the shell's ContentFrame for in-shell navigation
            Loaded += (s, e) => _navigationService.RegisterShellFrame(ContentFrame);
            Unloaded += (s, e) => _navigationService.UnregisterShellFrame();

            // Subscribe to ContentFrame navigation to sync NavigationView selection
            ContentFrame.Navigated += ContentFrame_Navigated;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is User user)
            {
                ViewModel.Initialize(user);
                if (_isInitialized) return;
                _isInitialized = true;

                ContentFrame.Navigate(typeof(SalesAgentDashboardPage), user);
                _currentContentItem = FindNavigationItemByTag("dashboard");
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
                case "dashboard":
                    NavigateToPage(typeof(SalesAgentDashboardPage), ViewModel.CurrentUser);
                    break;
                case "myProducts":
                    NavigateToPage(typeof(SalesAgentProductsPage), ViewModel.CurrentUser);
                    break;
                case "salesOrders":
                    NavigateToPage(typeof(SalesOrdersPage), ViewModel.CurrentUser);
                    break;
                case "earnings":
                    NavigateToPage(typeof(EarningsPage), ViewModel.CurrentUser);
                    break;
                case "shopping":
                    NavigateToPage(typeof(ProductBrowsePage), ViewModel.CurrentUser);
                    break;
                case "cart":
                    NavigateToPage(typeof(CartPage), ViewModel.CurrentUser);
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
                case "reports":
                    NavigateToPage(typeof(SalesAgentReportsPage), ViewModel.CurrentUser);
                    break;
                default:
                    AppLogger.Warning($"SalesAgent menu item '{tag}' not implemented yet");
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
            if (pageType == typeof(SalesAgentDashboardPage))
                return "dashboard";
            else if (pageType == typeof(SalesAgentProductsPage))
                return "myProducts";
            else if (pageType == typeof(SalesOrdersPage))
                return "salesOrders";
            else if (pageType == typeof(EarningsPage))
                return "earnings";
            else if (pageType == typeof(SalesAgentReportsPage))
                return "reports";
            else if (pageType == typeof(ProductBrowsePage))
                return "shopping";
            else if (pageType == typeof(CartPage))
                return "cart";
            else if (pageType == typeof(PurchaseOrdersPage))
                return "purchaseOrders";
            else if (pageType == typeof(ProfilePage))
                return "profile";
            else if (pageType == typeof(SettingsPage))
                return "settings";

            return null;
        }
    }
}
