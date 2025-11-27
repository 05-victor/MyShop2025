using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MyShop.Client.ViewModels.Admin;

namespace MyShop.Client.Views.Admin;

public sealed partial class AdminUsersPage : Page
{
    public AdminUsersViewModel ViewModel { get; }

    public AdminUsersPage()
    {
        // COPILOT-FIX: Wrap InitializeComponent to catch XAML parse errors
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

        // COPILOT-FIX: Wrap ViewModel resolution to catch DI errors
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

    // COPILOT-FIX: Harden OnNavigatedTo with comprehensive error handling
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

    private async void RefreshContainer_RefreshRequested(RefreshContainer sender, RefreshRequestedEventArgs args)
    {
        using var deferral = args.GetDeferral();
        await ViewModel.RefreshCommand.ExecuteAsync(null);
    }
}
