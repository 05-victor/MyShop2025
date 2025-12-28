using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MyShop.Client.ViewModels.Shell;
using MyShop.Shared.Models;
using MyShop.Client.Views.SalesAgent;
using MyShop.Client.Views.Shared;
using MyShop.Client.Services;
using MyShop.Client.Extensions;
using MyShop.Core.Interfaces.Services;
using MyShop.Client.Common.Navigation;

namespace MyShop.Client.Views.Shell
{
    public sealed partial class SalesAgentDashboardShell : Page
    {
        private const string ROLE = "Agent";
        
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

            // Populate navigation menu from NavRegistry
            PopulateNavigationMenu();

            // Register the shell's ContentFrame for in-shell navigation
            Loaded += (s, e) => _navigationService.RegisterShellFrame(ContentFrame);
            Unloaded += (s, e) => _navigationService.UnregisterShellFrame();

            // Subscribe to ContentFrame navigation to sync NavigationView selection
            ContentFrame.Navigated += ContentFrame_Navigated;
        }

        private void PopulateNavigationMenu()
        {
            // Get all navigation items for Agent role from registry
            var allItems = NavigationViewHelper.CreateMenuItemsForRole(ROLE, "AgentNavItemForeground").ToList();
            
            // Separate common items (profile, settings) from role-specific items
            var commonTags = new[] { "profile", "settings" };
            var roleSpecificItems = allItems.Where(item => !commonTags.Contains(item.Tag as string)).ToList();
            var commonItems = allItems.Where(item => commonTags.Contains(item.Tag as string)).ToList();

            // Add role-specific items first
            foreach (var item in roleSpecificItems)
            {
                Nav.MenuItems.Add(item);
            }

            // Add separator before common items
            if (commonItems.Count > 0)
            {
                Nav.MenuItems.Add(new NavigationViewItemSeparator());
                
                // Add common items
                foreach (var item in commonItems)
                {
                    Nav.MenuItems.Add(item);
                }
            }
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
            
            // Prevent flash: if clicking already selected item, do nothing
            if (item == _currentContentItem)
                return;

            _currentContentItem = item;

            // Use NavRegistry to resolve page type
            var pageType = NavRegistry.GetPageType(tag, ROLE);
            if (pageType != null)
            {
                NavigateToPage(pageType, ViewModel.CurrentUser);
            }
            else
            {
                LoggingService.Instance.Warning($"No page type found for tag '{tag}' and role '{ROLE}'");
                RestoreSelection();
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
                LoggingService.Instance.Error($"Failed to navigate to {pageType.Name}", ex);
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
            // Scroll the new page to top
            if (e.Content is Page page)
            {
                page.Loaded += (s, args) => page.ScrollToTop();
            }

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
        /// Maps page types to their corresponding NavigationView item tags using NavRegistry
        /// </summary>
        private string? GetNavigationTagForPageType(Type? pageType)
        {
            return NavRegistry.GetTagByPageType(pageType);
        }

        /// <summary>
        /// Handle logout navigation item tap
        /// </summary>
        private async void LogoutItem_Tapped(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            try
            {
                // Show confirmation dialog with theme support
                var dialog = new ContentDialog
                {
                    Title = "Logout",
                    Content = "Are you sure you want to logout?",
                    PrimaryButtonText = "Logout",
                    CloseButtonText = "Cancel",
                    DefaultButton = ContentDialogButton.Close,
                    XamlRoot = this.XamlRoot,
                    RequestedTheme = this.ActualTheme
                };

                var result = await dialog.ShowAsync();
                if (result != ContentDialogResult.Primary)
                {
                    return;
                }

                // Execute logout command
                if (ViewModel.LogoutCommand.CanExecute(null))
                {
                    await ViewModel.LogoutCommand.ExecuteAsync(null);
                }
            }
            catch (Exception ex)
            {
                LoggingService.Instance.Error("Failed to logout", ex);
            }
        }

        /// <summary>
        /// Toggle AI Chat Panel visibility
        /// </summary>
        private void ChatButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Toggle chat panel visibility
                bool isVisible = ChatPanel.Visibility == Visibility.Visible;
                ChatPanel.Visibility = isVisible ? Visibility.Collapsed : Visibility.Visible;
                ChatButton.IsOpen = !isVisible;
                
                LoggingService.Instance.Debug($"Chat panel toggled: {!isVisible}");
            }
            catch (Exception ex)
            {
                LoggingService.Instance.Error("Failed to toggle chat panel", ex);
            }
        }
    }
}
