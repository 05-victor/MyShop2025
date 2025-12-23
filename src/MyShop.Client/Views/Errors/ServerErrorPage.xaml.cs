using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MyShop.Client.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace MyShop.Client.Views.Errors;

/// <summary>
/// 500 Server Error page - displayed when a server error occurs.
/// </summary>
public sealed partial class ServerErrorPage : Page
{
    private readonly NavigationService? _navigationService;
    private string? _errorDetails;
    private Func<Task>? _retryAction;

    public ServerErrorPage()
    {
        this.InitializeComponent();
        _navigationService = App.Current.Services?.GetService<NavigationService>();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        
        if (e.Parameter is ServerErrorParameters parameters)
        {
            _errorDetails = parameters.ErrorDetails;
            _retryAction = parameters.RetryAction;
            
            if (!string.IsNullOrEmpty(_errorDetails))
            {
                ErrorDetailsText.Text = _errorDetails;
                ErrorDetailsBorder.Visibility = Visibility.Visible;
            }
        }
    }

    private async void RetryButton_Click(object sender, RoutedEventArgs e)
    {
        if (_retryAction != null)
        {
            RetryButton.IsEnabled = false;

            try
            {
                await _retryAction();
            }
            catch
            {
                // Retry failed, reset button state
                RetryButton.IsEnabled = true;
            }
        }
        else
        {
            // No retry action, just go back
            if (_navigationService?.CanGoBack == true)
            {
                _navigationService.GoBack();
            }
        }
    }

    private void GoHomeButton_Click(object sender, RoutedEventArgs e)
    {
        _navigationService?.NavigateTo("DashboardPage");
    }
}

/// <summary>
/// Parameters for the ServerErrorPage
/// </summary>
public class ServerErrorParameters
{
    public string? ErrorDetails { get; set; }
    public Func<Task>? RetryAction { get; set; }
}
