using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MyShop.Client.Services;
using Microsoft.Extensions.DependencyInjection;

namespace MyShop.Client.Views.Errors;

/// <summary>
/// 404 Not Found page - displayed when a resource is not found.
/// </summary>
public sealed partial class NotFoundPage : Page
{
    private readonly NavigationService? _navigationService;

    public NotFoundPage()
    {
        this.InitializeComponent();
        _navigationService = App.Current.Services?.GetService<NavigationService>();
    }

    private async void GoBackButton_Click(object sender, RoutedEventArgs e)
    {
        if (_navigationService?.CanGoBack == true)
        {
            await _navigationService.GoBack();
        }
        else
        {
            // Navigate to home if can't go back
            GoHomeButton_Click(sender, e);
        }
    }

    private async void GoHomeButton_Click(object sender, RoutedEventArgs e)
    {
        await _navigationService?.NavigateTo("DashboardPage");
    }
}
