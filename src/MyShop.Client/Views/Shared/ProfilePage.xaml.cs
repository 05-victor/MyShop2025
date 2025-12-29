using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MyShop.Client.ViewModels.Shared;
using MyShop.Client.Views.Dialogs;
using MyShop.Core.Interfaces.Repositories;
using MyShop.Core.Interfaces.Services;

namespace MyShop.Client.Views.Shared;

public sealed partial class ProfilePage : Page
{
    public ProfileViewModel ViewModel { get; }

    public ProfilePage()
    {
        // Wrap InitializeComponent to catch XAML parse errors
        try
        {
            this.InitializeComponent();
        }
        catch (Exception ex)
        {
            Services.LoggingService.Instance.Error($"[ProfilePage] XAML load failed in InitializeComponent", ex);
            // Create minimal fallback UI
            this.Content = new Microsoft.UI.Xaml.Controls.TextBlock
            {
                Text = $"Failed to load ProfilePage.\n\nError: {ex.Message}\n\nCheck logs at: {Services.LoggingService.Instance.GetLogDirectory()}",
                Margin = new Microsoft.UI.Xaml.Thickness(24),
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Red),
                TextWrapping = Microsoft.UI.Xaml.TextWrapping.Wrap
            };
            return;
        }

        // Wrap ViewModel resolution to catch DI errors
        try
        {
            // Get ViewModel from DI
            ViewModel = App.Current.Services.GetRequiredService<ProfileViewModel>();
        }
        catch (Exception ex)
        {
            Services.LoggingService.Instance.Error($"[ProfilePage] Failed to resolve ProfileViewModel", ex);
            throw; // Re-throw to surface the actual DI issue
        }

        // Wrap keyboard shortcuts setup
        try
        {
            // Setup keyboard shortcuts
            SetupKeyboardShortcuts();
        }
        catch (Exception ex)
        {
            Services.LoggingService.Instance.Warning($"[ProfilePage] Failed to setup keyboard shortcuts", ex.ToString());
            // Non-critical, continue without shortcuts
        }
    }

    private void SetupKeyboardShortcuts()
    {
        // Ctrl+E: Edit profile
        var editShortcut = new Microsoft.UI.Xaml.Input.KeyboardAccelerator
        {
            Key = Windows.System.VirtualKey.E,
            Modifiers = Windows.System.VirtualKeyModifiers.Control
        };
        editShortcut.Invoked += (s, e) =>
        {
            if (!ViewModel.IsEditing)
            {
                ViewModel.EditCommand.Execute(null);
            }
            e.Handled = true;
        };
        KeyboardAccelerators.Add(editShortcut);

        // Ctrl+S: Save changes
        var saveShortcut = new Microsoft.UI.Xaml.Input.KeyboardAccelerator
        {
            Key = Windows.System.VirtualKey.S,
            Modifiers = Windows.System.VirtualKeyModifiers.Control
        };
        saveShortcut.Invoked += async (s, e) =>
        {
            // Wrap async event handler with try-catch
            try
            {
                if (ViewModel.IsEditing && ViewModel.IsFormValid)
                {
                    await ViewModel.SaveCommand.ExecuteAsync(null);
                }
                e.Handled = true;
            }
            catch (Exception ex)
            {
                Services.LoggingService.Instance.Error("[ProfilePage] Save shortcut failed", ex);
            }
        };
        KeyboardAccelerators.Add(saveShortcut);

        // Escape: Cancel edit
        var cancelShortcut = new Microsoft.UI.Xaml.Input.KeyboardAccelerator
        {
            Key = Windows.System.VirtualKey.Escape
        };
        cancelShortcut.Invoked += async (s, e) =>
        {
            // Wrap async event handler with try-catch
            try
            {
                if (ViewModel.IsEditing)
                {
                    await ViewModel.CancelEditCommand.ExecuteAsync(null);
                }
                e.Handled = true;
            }
            catch (Exception ex)
            {
                Services.LoggingService.Instance.Error("[ProfilePage] Cancel shortcut failed", ex);
            }
        };
        KeyboardAccelerators.Add(cancelShortcut);

        // Ctrl+U: Upload avatar (when file is selected)
        var uploadShortcut = new Microsoft.UI.Xaml.Input.KeyboardAccelerator
        {
            Key = Windows.System.VirtualKey.U,
            Modifiers = Windows.System.VirtualKeyModifiers.Control
        };
        uploadShortcut.Invoked += async (s, e) =>
        {
            // Wrap async event handler with try-catch
            try
            {
                if (ViewModel.SelectedAvatarFile != null && !ViewModel.IsLoading)
                {
                    await ViewModel.UploadAvatarCommand.ExecuteAsync(null);
                }
                e.Handled = true;
            }
            catch (Exception ex)
            {
                Services.LoggingService.Instance.Error("[ProfilePage] Upload shortcut failed", ex);
            }
        };
        KeyboardAccelerators.Add(uploadShortcut);
    }

    // Harden OnNavigatedTo with comprehensive error handling
    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        try
        {
            base.OnNavigatedTo(e);
            Services.NavigationLogger.LogNavigatedTo(nameof(ProfilePage), e.Parameter);

            // Load profile data
            _ = ViewModel.LoadCommand.ExecuteAsync(null);
        }
        catch (Exception ex)
        {
            Services.LoggingService.Instance.Error($"[ProfilePage] OnNavigatedTo failed", ex);
        }
    }

    /// <summary>
    /// Helper method for avatar initial
    /// </summary>
    public string GetInitial(string username)
    {
        return string.IsNullOrEmpty(username) ? "?" : username[0].ToString().ToUpper();
    }

    /// <summary>
    /// Show change password dialog
    /// </summary>
    private async void ChangePasswordButton_Click(object sender, RoutedEventArgs _)
    {
        try
        {
            var dialog = new ChangePasswordDialog
            {
                XamlRoot = this.XamlRoot
            };

            await dialog.ShowAsync();
        }
        catch (Exception ex)
        {
            Services.LoggingService.Instance.Error("[ProfilePage] ChangePasswordButton_Click failed", ex);
        }
    }

    /// <summary>
    /// Show trial activation dialog
    /// </summary>
    private async void TrialActivationButton_Click(object sender, RoutedEventArgs _)
    {
        try
        {
            var dialog = new TrialActivationDialog
            {
                XamlRoot = this.XamlRoot
            };

            var result = await dialog.ShowAsync();

            // If activation succeeded, reload profile to update trial status
            if (result == ContentDialogResult.Primary && dialog.ViewModel.IsSuccess)
            {
                await ViewModel.LoadCommand.ExecuteAsync(null);
            }
        }
        catch (Exception ex)
        {
            Services.LoggingService.Instance.Error("[ProfilePage] TrialActivationButton_Click failed", ex);
        }
    }

    /// <summary>
    /// Logout with confirmation
    /// </summary>
    private async void LogoutButton_Click(object sender, RoutedEventArgs _)
    {
        try
        {
            var dialog = new ContentDialog
            {
                Title = "Logout",
                Content = "Are you sure you want to logout from your account?",
                PrimaryButtonText = "Logout",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = this.XamlRoot
            };

            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                await ViewModel.LogoutCommand.ExecuteAsync(null);
            }
        }
        catch (Exception ex)
        {
            Services.LoggingService.Instance.Error("[ProfilePage] LogoutButton_Click failed", ex);
        }
    }

    /// <summary>
    /// Enable edit mode
    /// </summary>
    private void EditButton_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.EditCommand.Execute(null);
    }

    /// <summary>
    /// Cancel edit mode
    /// </summary>
    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.CancelEditCommand.Execute(null);
    }

    /// <summary>
    /// Save changes
    /// </summary>
    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            await ViewModel.SaveCommand.ExecuteAsync(null);
        }
        catch (Exception ex)
        {
            Services.LoggingService.Instance.Error("[ProfilePage] SaveButton_Click failed", ex);
        }
    }

    /// <summary>
    /// Resend verification email via API
    /// </summary>
    private async void ResendVerificationButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var toastService = App.Current.Services.GetRequiredService<IToastService>();
            var authRepository = App.Current.Services.GetRequiredService<IAuthRepository>();
            var button = sender as HyperlinkButton;

            if (button != null)
                button.IsEnabled = false;

            // Call API to send verification email
            System.Diagnostics.Debug.WriteLine("[ProfilePage] Sending verification email...");
            var result = await authRepository.SendVerificationEmailAsync(string.Empty);

            if (result.IsSuccess)
            {
                toastService.ShowSuccess("Verification email sent! Check your inbox for the link.");
                System.Diagnostics.Debug.WriteLine("[ProfilePage] ✅ Verification email sent successfully");

                // Show 60-second cooldown
                if (button != null)
                {
                    var countdown = 60;
                    var timer = new DispatcherTimer();
                    timer.Interval = TimeSpan.FromSeconds(1);
                    timer.Tick += (s, args) =>
                    {
                        countdown--;
                        button.Content = countdown > 0 ? $"Resend in {countdown}s" : "Resend verification email";

                        if (countdown <= 0)
                        {
                            timer.Stop();
                            button.IsEnabled = true;
                        }
                    };
                    timer.Start();
                }
            }
            else
            {
                toastService.ShowError($"Failed to send verification email: {result.ErrorMessage}");
                System.Diagnostics.Debug.WriteLine($"[ProfilePage] ❌ Error: {result.ErrorMessage}");

                if (button != null)
                    button.IsEnabled = true;
            }
        }
        catch (Exception ex)
        {
            Services.LoggingService.Instance.Error("[ProfilePage] ResendVerificationButton_Click failed", ex);
            var toastService = App.Current.Services.GetRequiredService<IToastService>();
            toastService.ShowError("An error occurred while sending verification email");

            if (sender is HyperlinkButton btn)
                btn.IsEnabled = true;
        }
    }

    /// <summary>
    /// Cancel avatar selection
    /// </summary>
    private void CancelAvatarButton_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.SelectedAvatarFile = null;
    }
}
