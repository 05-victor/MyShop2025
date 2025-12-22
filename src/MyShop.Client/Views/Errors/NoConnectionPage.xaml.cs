using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MyShop.Client.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace MyShop.Client.Views.Errors;

/// <summary>
/// No Connection page - displayed when there's no network/server connection.
/// </summary>
public sealed partial class NoConnectionPage : Page
{
    private readonly NavigationService? _navigationService;
    private Func<Task>? _retryAction;

    public NoConnectionPage()
    {
        this.InitializeComponent();
        _navigationService = App.Current.Services?.GetService<NavigationService>();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        
        if (e.Parameter is NoConnectionParameters parameters)
        {
            _retryAction = parameters.RetryAction;
        }
    }

    private async void RetryButton_Click(object sender, RoutedEventArgs e)
    {
        RetryButton.IsEnabled = false;

        try
        {
            if (_retryAction != null)
            {
                await _retryAction();
            }
            else
            {
                // Default: simulate connection check
                await Task.Delay(1500);
                
                // If we get here, go back
                if (_navigationService?.CanGoBack == true)
                {
                    _navigationService.GoBack();
                }
            }
        }
        catch
        {
            // Connection still failed
        }
        finally
        {
            // Reset button state
            RetryButton.IsEnabled = true;
        }
    }

    private void ConfigureButton_Click(object sender, RoutedEventArgs e)
    {
        // Navigate to settings page
        _navigationService?.NavigateTo("SettingsPage");
    }
}

/// <summary>
/// Parameters for the NoConnectionPage
/// </summary>
public class NoConnectionParameters
{
    public Func<Task>? RetryAction { get; set; }
}
