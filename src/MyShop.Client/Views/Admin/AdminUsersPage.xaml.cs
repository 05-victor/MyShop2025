using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MyShop.Client.ViewModels.Admin;
using System.Linq;

namespace MyShop.Client.Views.Admin;

public sealed partial class AdminUsersPage : Page
{
    public AdminUsersViewModel ViewModel { get; }

    public AdminUsersPage()
    {
        // Wrap InitializeComponent to catch XAML parse errors
        try
        {
            this.InitializeComponent();
        }
        catch (Exception ex)
        {
            Services.LoggingService.Instance.Error($"[AdminUsersPage] XAML load failed in InitializeComponent", ex);
            // Create minimal fallback UI
            this.Content = new Microsoft.UI.Xaml.Controls.TextBlock
            {
                Text = $"Failed to load AdminUsersPage.\n\nError: {ex.Message}\n\nCheck logs at: {Services.LoggingService.Instance.GetLogDirectory()}",
                Margin = new Microsoft.UI.Xaml.Thickness(24),
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Red),
                TextWrapping = Microsoft.UI.Xaml.TextWrapping.Wrap
            };
            return;
        }

        // Wrap ViewModel resolution to catch DI errors
        try
        {
            ViewModel = App.Current.Services.GetRequiredService<AdminUsersViewModel>();
        }
        catch (Exception ex)
        {
            Services.LoggingService.Instance.Error($"[AdminUsersPage] Failed to resolve ViewModel", ex);
            throw; // Re-throw to surface the actual DI issue
        }
    }

    // Harden OnNavigatedTo with comprehensive error handling
    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        try
        {
            base.OnNavigatedTo(e);
            Services.NavigationLogger.LogNavigatedTo(nameof(AdminUsersPage), e.Parameter);

            try
            {
                await ViewModel.InitializeAsync();
            }
            catch (Exception ex)
            {
                Services.LoggingService.Instance.Error($"[AdminUsersPage] ViewModel.InitializeAsync failed", ex);
                // Show user-friendly error in page
            }
        }
        catch (Exception ex)
        {
            Services.LoggingService.Instance.Error($"[AdminUsersPage] OnNavigatedTo failed", ex);
        }
    }

    #region Search Card Event Handlers

    private void SearchCard_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
        {
            var query = sender.Text?.ToLower() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(query))
            {
                sender.ItemsSource = null;
                return;
            }

            // Generate suggestions from current users
            var suggestions = ViewModel.Items
                .Where(u => u.FullName.ToLower().Contains(query) ||
                           u.Email.ToLower().Contains(query) ||
                           (u.Phone?.ToLower().Contains(query) ?? false))
                .Select(u => u.FullName)
                .Distinct()
                .Take(8)
                .ToList();

            sender.ItemsSource = suggestions;
        }
    }

    private void SearchCard_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
    {
        sender.Text = args.SelectedItem?.ToString() ?? string.Empty;
    }

    private async void SearchCard_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        if (args.ChosenSuggestion != null)
        {
            ViewModel.PendingSearchQuery = args.ChosenSuggestion.ToString() ?? string.Empty;
        }
        else
        {
            ViewModel.PendingSearchQuery = args.QueryText;
        }

        if (ViewModel.ApplyFiltersCommand?.CanExecute(null) == true)
        {
            await ViewModel.ApplyFiltersCommand.ExecuteAsync(null);
        }
    }

    #endregion

    #region Filter Handlers

    private void RoleComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // Guard: ViewModel may be null during page initialization
        if (ViewModel == null) return;

        if (RoleComboBox.SelectedItem is ComboBoxItem item)
        {
            var role = item.Tag?.ToString() ?? "All Roles";
            ViewModel.PendingRole = role;
        }
    }

    private async void ExportButton_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel.ExportUsersCommand.CanExecute(null))
        {
            await ViewModel.ExportUsersCommand.ExecuteAsync(null);
        }
    }

    #endregion

    private async void RefreshContainer_RefreshRequested(RefreshContainer sender, RefreshRequestedEventArgs args)
    {
        using var deferral = args.GetDeferral();
        await ViewModel.RefreshCommand.ExecuteAsync(null);
    }

    /// <summary>
    /// Get initials from full name for avatar
    /// </summary>
    private string GetInitials(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            return "U";

        var parts = fullName.Trim().Split(' ');
        if (parts.Length == 1)
            return parts[0][0].ToString().ToUpper();

        return $"{parts[0][0]}{parts[parts.Length - 1][0]}".ToUpper();
    }

    /// <summary>
    /// Get primary role from roleNames list (Admin > SalesAgent > Customer)
    /// </summary>
    private string GetPrimaryRole(List<string> roleNames)
    {
        if (roleNames == null || roleNames.Count == 0)
            return "Customer";

        foreach (var role in roleNames)
        {
            if (role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
                return "Admin";
            if (role.Equals("SalesAgent", StringComparison.OrdinalIgnoreCase))
                return "SalesAgent";
        }

        return "Customer";
    }

    /// <summary>
    /// View user details button click handler
    /// </summary>
    private async void ViewDetailsButton_Click(object sender, RoutedEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine($"[AdminUsersPage] ViewDetailsButton_Click - Event fired!");
        try
        {
            if (sender is Button button)
            {
                System.Diagnostics.Debug.WriteLine($"[AdminUsersPage] Button sender confirmed");

                // Get UserViewModel from CommandParameter (passed via {x:Bind} in XAML)
                var user = button.CommandParameter as UserViewModel;
                if (user != null)
                {
                    System.Diagnostics.Debug.WriteLine($"[AdminUsersPage] ViewDetailsButton_Click - User: {user.Name}, ID: {user.Id}");

                    // Call ViewModel to fetch user details from API
                    if (ViewModel.ViewUserDetailsCommand != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"[AdminUsersPage] Executing ViewUserDetailsCommand for user: {user.Id}");
                        await ViewModel.ViewUserDetailsCommand.ExecuteAsync(user);

                        // Dialog will be shown by ViewModel with API data
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"[AdminUsersPage] ViewUserDetailsCommand is NULL!");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[AdminUsersPage] User is null - CommandParameter is not UserViewModel");
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[AdminUsersPage] Sender is not a Button!");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AdminUsersPage] ViewDetailsButton_Click error: {ex.Message}\n{ex.StackTrace}");
        }
    }

    /// <summary>
    /// Show user details dialog with API data
    /// </summary>
    public async Task ShowUserDetailsDialog(MyShop.Shared.Models.User userDetails)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"[AdminUsersPage] ShowUserDetailsDialog - START");

            if (userDetails == null)
            {
                System.Diagnostics.Debug.WriteLine($"[AdminUsersPage] ShowUserDetailsDialog - userDetails is null");
                return;
            }

            System.Diagnostics.Debug.WriteLine($"[AdminUsersPage] ShowUserDetailsDialog - Displaying user: {userDetails.Username}");

            // Verify dialog exists
            if (UserDetailsDialog == null)
            {
                System.Diagnostics.Debug.WriteLine($"[AdminUsersPage] ERROR: UserDetailsDialog is null!");
                return;
            }

            System.Diagnostics.Debug.WriteLine($"[AdminUsersPage] Dialog reference found, proceeding...");

            // Populate dialog fields with API response data
            // Handle avatar: use image if available, otherwise show initials
            if (!string.IsNullOrEmpty(userDetails.Avatar))
            {
                System.Diagnostics.Debug.WriteLine($"[AdminUsersPage] User has avatar image: {userDetails.Avatar}");

                // Show avatar image - hide initials
                try
                {
                    var imageBrush = new Microsoft.UI.Xaml.Media.ImageBrush();
                    imageBrush.ImageSource = new Microsoft.UI.Xaml.Media.Imaging.BitmapImage(new Uri(userDetails.Avatar));
                    UserAvatarEllipse.Fill = imageBrush;
                    UserAvatarText.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
                    System.Diagnostics.Debug.WriteLine($"[AdminUsersPage] Avatar image loaded successfully");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[AdminUsersPage] Failed to load avatar image: {ex.Message}, falling back to initials");
                    // Fall back to initials
                    UserAvatarText.Text = GetInitials(userDetails.FullName ?? userDetails.Username);
                    UserAvatarText.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
                    UserAvatarEllipse.Fill = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.CornflowerBlue);
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[AdminUsersPage] User has no avatar, showing initials");

                // Show initials fallback
                UserAvatarText.Text = GetInitials(userDetails.FullName ?? userDetails.Username);
                UserAvatarText.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
                UserAvatarEllipse.Fill = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.CornflowerBlue);
            }

            UserFullNameText.Text = userDetails.FullName ?? userDetails.Username;
            UserUsernameText.Text = userDetails.Username;
            UserEmailText.Text = userDetails.Email;
            UserPhoneText.Text = userDetails.PhoneNumber ?? "N/A";
            UserAddressText.Text = userDetails.Address ?? "N/A";

            // Get primary role from Roles list (Admin > SalesAgent > Customer)
            var roleNames = userDetails.Roles?.Select(r => r.ToString()).ToList() ?? new List<string>();
            var primaryRole = GetPrimaryRole(roleNames);
            UserRoleText.Text = primaryRole;

            System.Diagnostics.Debug.WriteLine($"[AdminUsersPage] Dialog fields populated - Username: {userDetails.Username}, Email: {userDetails.Email}, Phone: {userDetails.PhoneNumber}, Address: {userDetails.Address}, Role: {primaryRole}");

            // Set XamlRoot from Window root (not Page root) to ensure proper bounds after window resize
            // This fixes dialog offset when window is maximized/minimized/restored before opening dialog
            System.Diagnostics.Debug.WriteLine($"[AdminUsersPage] Setting XamlRoot from Window root...");
            try
            {
                var windowRoot = (Microsoft.UI.Xaml.FrameworkElement)App.MainWindow.Content;
                if (windowRoot != null)
                {
                    // Force layout pass to stabilize bounds after any window state changes
                    windowRoot.UpdateLayout();
                    System.Diagnostics.Debug.WriteLine($"[AdminUsersPage] Window root layout updated");

                    UserDetailsDialog.XamlRoot = windowRoot.XamlRoot;
                    System.Diagnostics.Debug.WriteLine($"[AdminUsersPage] XamlRoot set from Window root successfully");
                }
                else
                {
                    // Fallback to page XamlRoot if window root not available
                    System.Diagnostics.Debug.WriteLine($"[AdminUsersPage] Window root is null, falling back to page XamlRoot");
                    UserDetailsDialog.XamlRoot = this.XamlRoot;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AdminUsersPage] Error setting XamlRoot from window root: {ex.Message}, using page XamlRoot");
                UserDetailsDialog.XamlRoot = this.XamlRoot;
            }

            if (UserDetailsDialog.XamlRoot == null)
            {
                System.Diagnostics.Debug.WriteLine($"[AdminUsersPage] WARNING: XamlRoot is null!");
                return;
            }

            // Defer ShowAsync to next UI tick to allow layout to stabilize after window resize
            // This prevents dialog offset when opened after window maximize/minimize/restore
            System.Diagnostics.Debug.WriteLine($"[AdminUsersPage] Deferring ShowAsync to next UI tick...");
            var result = DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Normal, async () =>
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine($"[AdminUsersPage] ShowAsync executing on next UI tick...");
                    await UserDetailsDialog.ShowAsync();
                    System.Diagnostics.Debug.WriteLine($"[AdminUsersPage] ShowAsync() completed successfully, dialog closed");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[AdminUsersPage] Error in ShowAsync: {ex.Message}");
                }
            });

            if (!result)
            {
                System.Diagnostics.Debug.WriteLine($"[AdminUsersPage] WARNING: Failed to enqueue ShowAsync to DispatcherQueue");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AdminUsersPage] ShowUserDetailsDialog EXCEPTION: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"[AdminUsersPage] Stack trace: {ex.StackTrace}");
        }
    }

    /// <summary>
    /// Delete user button click handler
    /// </summary>
    private async void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine($"[AdminUsersPage] DeleteButton_Click - Event fired!");
        try
        {
            if (sender is Button button)
            {
                System.Diagnostics.Debug.WriteLine($"[AdminUsersPage] Button sender confirmed");
                System.Diagnostics.Debug.WriteLine($"[AdminUsersPage] CommandParameter type: {button.CommandParameter?.GetType().Name ?? "null"}");

                // Get UserViewModel from CommandParameter (passed via {x:Bind} in XAML)
                var user = button.CommandParameter as UserViewModel;
                if (user != null)
                {
                    System.Diagnostics.Debug.WriteLine($"[AdminUsersPage] DeleteButton_Click - User: {user.Name}, ID: {user.Id}");

                    // Show confirmation dialog using ContentDialog (WinUI 3 compatible)
                    var confirmDialog = new ContentDialog
                    {
                        Title = "Confirm Delete User",
                        Content = $"Are you sure you want to delete user '{user.Name}'?\n\nThis action cannot be undone.",
                        PrimaryButtonText = "Delete",
                        CloseButtonText = "Cancel",
                        DefaultButton = ContentDialogButton.Close,
                        XamlRoot = this.XamlRoot
                    };

                    var result = await confirmDialog.ShowAsync();
                    System.Diagnostics.Debug.WriteLine($"[AdminUsersPage] Confirmation result: {result}");

                    if (result == ContentDialogResult.Primary)
                    {
                        System.Diagnostics.Debug.WriteLine($"[AdminUsersPage] User confirmed deletion, executing command...");
                        System.Diagnostics.Debug.WriteLine($"[AdminUsersPage] Command exists: {ViewModel.DeleteUserCommand != null}");

                        if (ViewModel.DeleteUserCommand != null)
                        {
                            System.Diagnostics.Debug.WriteLine($"[AdminUsersPage] CanExecute: {ViewModel.DeleteUserCommand.CanExecute(user)}");
                            await ViewModel.DeleteUserCommand.ExecuteAsync(user);
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"[AdminUsersPage] DeleteUserCommand is NULL!");
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"[AdminUsersPage] User cancelled deletion");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[AdminUsersPage] User is null - CommandParameter is not UserViewModel");
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[AdminUsersPage] Sender is not a Button!");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AdminUsersPage] DeleteButton_Click error: {ex.Message}\n{ex.StackTrace}");
        }
    }
}
