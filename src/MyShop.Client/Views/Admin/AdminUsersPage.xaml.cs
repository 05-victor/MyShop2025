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

    private void StatusComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // Guard: ViewModel may be null during page initialization
        if (ViewModel == null) return;

        if (StatusComboBox.SelectedItem is ComboBoxItem item)
        {
            var status = item.Tag?.ToString() ?? "All Status";
            ViewModel.PendingStatus = status;
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
                System.Diagnostics.Debug.WriteLine($"[AdminUsersPage] CommandParameter type: {button.CommandParameter?.GetType().Name ?? "null"}");

                // Get UserViewModel from CommandParameter (passed via {x:Bind} in XAML)
                var user = button.CommandParameter as UserViewModel;
                if (user != null)
                {
                    System.Diagnostics.Debug.WriteLine($"[AdminUsersPage] ViewDetailsButton_Click - User: {user.Name}, ID: {user.Id}");
                    System.Diagnostics.Debug.WriteLine($"[AdminUsersPage] Command exists: {ViewModel.ViewUserDetailsCommand != null}");

                    if (ViewModel.ViewUserDetailsCommand != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"[AdminUsersPage] CanExecute: {ViewModel.ViewUserDetailsCommand.CanExecute(user)}");
                        await ViewModel.ViewUserDetailsCommand.ExecuteAsync(user);
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
