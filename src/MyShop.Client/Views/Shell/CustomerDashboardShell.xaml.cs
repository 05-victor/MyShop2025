using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MyShop.Client.ViewModels.Shell;
using MyShop.Shared.Models;
using MyShop.Client.Views.Customer;
using MyShop.Client.Views.Shared;
using MyShop.Client.Services;
using MyShop.Client.Extensions;
using MyShop.Core.Interfaces.Services;
using MyShop.Core.Interfaces.Repositories;

namespace MyShop.Client.Views.Shell
{
    public sealed partial class CustomerDashboardShell : Page
    {
        public DashboardShellViewModel ViewModel { get; }
        private readonly INavigationService _navigationService;
        private readonly ISystemActivationRepository _activationRepository;
        private NavigationViewItem? _currentContentItem;
        private bool _isRestoringSelection;
        private bool _isInitialized;
        private User? _currentUser;

        public CustomerDashboardShell()
        {
            InitializeComponent();
            ViewModel = App.Current.Services.GetRequiredService<DashboardShellViewModel>();
            _navigationService = App.Current.Services.GetRequiredService<INavigationService>();
            _activationRepository = App.Current.Services.GetRequiredService<ISystemActivationRepository>();
            DataContext = ViewModel;

            // Register the shell's ContentFrame for in-shell navigation
            Loaded += (s, e) =>
            {
                _navigationService.RegisterShellFrame(ContentFrame);
                _ = CheckBecomeAdminVisibilityAsync();
            };
            Unloaded += (s, e) => _navigationService.UnregisterShellFrame();

            // Subscribe to ContentFrame navigation to sync NavigationView selection
            ContentFrame.Navigated += ContentFrame_Navigated;
        }

        /// <summary>
        /// Check if "Become Admin" button should be visible
        /// Only show when no admin exists in the system
        /// </summary>
        private async System.Threading.Tasks.Task CheckBecomeAdminVisibilityAsync()
        {
            try
            {
                var result = await _activationRepository.HasAnyAdminAsync();
                var hasAdmin = result.IsSuccess && result.Data;
                var visibility = hasAdmin ? Visibility.Collapsed : Visibility.Visible;
                
                BecomeAdminItem.Visibility = visibility;
                BecomeAdminSeparator.Visibility = visibility;
                
                System.Diagnostics.Debug.WriteLine($"[CustomerDashboardShell] BecomeAdmin visibility: {visibility} (HasAdmin: {hasAdmin})");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[CustomerDashboardShell] Error checking admin: {ex.Message}");
                BecomeAdminItem.Visibility = Visibility.Collapsed;
                BecomeAdminSeparator.Visibility = Visibility.Collapsed;
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is User user)
            {
                _currentUser = user;
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
                    LoggingService.Instance.Warning($"Customer menu item '{tag}' not implemented yet");
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
        /// Maps page types to their corresponding NavigationView item tags
        /// </summary>
        private string? GetNavigationTagForPageType(Type? pageType)
        {
            if (pageType == null)
                return null;

            // Map each page type to its NavigationView tag
            if (pageType == typeof(CustomerDashboardPage))
                return "home";
            else if (pageType == typeof(ProductBrowsePage))
                return "shopping";
            else if (pageType == typeof(CartPage))
                return "cart";
            else if (pageType == typeof(CheckoutPage))
                return "checkout";
            else if (pageType == typeof(PurchaseOrdersPage))
                return "purchaseOrders";
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
                // Show confirmation dialog
                var dialog = new ContentDialog
                {
                    Title = "Logout",
                    Content = "Are you sure you want to logout?",
                    PrimaryButtonText = "Logout",
                    CloseButtonText = "Cancel",
                    DefaultButton = ContentDialogButton.Close,
                    XamlRoot = this.XamlRoot
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
        /// Handle "Become Admin" button tap - show activation code dialog
        /// </summary>
        private async void BecomeAdminItem_Tapped(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            try
            {
                // Create activation dialog
                var activationCodeBox = new TextBox
                {
                    PlaceholderText = "MYSHOP-XXXX-XXXX-XXXX",
                    MaxLength = 22,
                    CharacterCasing = CharacterCasing.Upper,
                    HorizontalAlignment = HorizontalAlignment.Stretch
                };

                var infoText = new TextBlock
                {
                    Text = "Enter your admin activation code to become the system administrator.\n\nDemo codes:\n• MYSHOP-PERM-2025-ADMIN (Permanent)\n• MYSHOP-TRIAL-2025-001 (Trial 14 days)",
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(0, 0, 0, 16),
                    Foreground = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["TextFillColorSecondaryBrush"]
                };

                var contentPanel = new StackPanel { Spacing = 8 };
                contentPanel.Children.Add(infoText);
                contentPanel.Children.Add(activationCodeBox);

                var dialog = new ContentDialog
                {
                    Title = "Become Admin",
                    Content = contentPanel,
                    PrimaryButtonText = "Activate",
                    CloseButtonText = "Cancel",
                    DefaultButton = ContentDialogButton.Primary,
                    XamlRoot = this.XamlRoot
                };

                var result = await dialog.ShowAsync();
                if (result != ContentDialogResult.Primary)
                {
                    return;
                }

                var code = activationCodeBox.Text?.Trim();
                if (string.IsNullOrEmpty(code))
                {
                    await ShowErrorDialogAsync("Please enter an activation code.");
                    return;
                }

                // Validate and activate code
                if (_currentUser == null)
                {
                    await ShowErrorDialogAsync("User information not available.");
                    return;
                }

                var activateResult = await _activationRepository.ActivateCodeAsync(code, _currentUser.Id);
                
                if (activateResult.IsSuccess && activateResult.Data != null)
                {
                    var license = activateResult.Data;
                    var licenseType = license.IsPermanent ? "Permanent" : $"Trial ({license.RemainingDays} days)";
                    
                    // Show success and prompt to re-login
                    var successDialog = new ContentDialog
                    {
                        Title = "Activation Successful! 🎉",
                        Content = $"You are now the system administrator!\n\nLicense Type: {licenseType}\n\nPlease log out and log back in to access admin features.",
                        PrimaryButtonText = "Logout Now",
                        CloseButtonText = "Later",
                        DefaultButton = ContentDialogButton.Primary,
                        XamlRoot = this.XamlRoot
                    };

                    var successResult = await successDialog.ShowAsync();
                    
                    // Hide the Become Admin button
                    BecomeAdminItem.Visibility = Visibility.Collapsed;
                    BecomeAdminSeparator.Visibility = Visibility.Collapsed;
                    
                    if (successResult == ContentDialogResult.Primary)
                    {
                        // Logout
                        if (ViewModel.LogoutCommand.CanExecute(null))
                        {
                            await ViewModel.LogoutCommand.ExecuteAsync(null);
                        }
                    }
                }
                else
                {
                    await ShowErrorDialogAsync(activateResult.ErrorMessage ?? "Invalid activation code.");
                }
            }
            catch (Exception ex)
            {
                LoggingService.Instance.Error("Failed to activate admin code", ex);
                await ShowErrorDialogAsync($"An error occurred: {ex.Message}");
            }
        }

        private async System.Threading.Tasks.Task ShowErrorDialogAsync(string message)
        {
            var errorDialog = new ContentDialog
            {
                Title = "Error",
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };
            await errorDialog.ShowAsync();
        }
    }
}
