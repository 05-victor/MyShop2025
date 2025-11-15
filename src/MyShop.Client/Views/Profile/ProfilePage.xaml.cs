using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MyShop.Client.ViewModels.Profile;
using MyShop.Client.Views.Dialogs;

namespace MyShop.Client.Views.Profile;

public sealed partial class ProfilePage : Page
{
    public ProfileViewModel ViewModel { get; }

    public ProfilePage()
    {
        this.InitializeComponent();
        
        // Get ViewModel from DI
        ViewModel = App.Current.Services.GetRequiredService<ProfileViewModel>();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        
        // Load profile data
        _ = ViewModel.LoadCommand.ExecuteAsync(null);
    }

    /// <summary>
    /// Helper method for avatar initial
    /// </summary>
    public static string GetInitial(string username)
    {
        return string.IsNullOrEmpty(username) ? "?" : username[0].ToString().ToUpper();
    }

    /// <summary>
    /// Show change password dialog
    /// </summary>
    private async void ChangePasswordButton_Click(object sender, RoutedEventArgs _)
    {
        var dialog = new ChangePasswordDialog
        {
            XamlRoot = this.XamlRoot
        };

        await dialog.ShowAsync();
    }

    /// <summary>
    /// Show trial activation dialog
    /// </summary>
    private async void TrialActivationButton_Click(object sender, RoutedEventArgs _)
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

    /// <summary>
    /// Logout with confirmation
    /// </summary>
    private async void LogoutButton_Click(object sender, RoutedEventArgs _)
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
        await ViewModel.SaveCommand.ExecuteAsync(null);
    }

    /// <summary>
    /// Verify email (demo only - for testing UI)
    /// </summary>
    private void VerifyEmailButton_Click(object sender, RoutedEventArgs e)
    {
        // This is demo UI - actual email verification would come from backend
        // For now, just a placeholder for the demo interface
    }
}
