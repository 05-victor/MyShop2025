using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MyShop.Client.ViewModels.Shared;

namespace MyShop.Client.Views.Shared;

public sealed partial class CheckoutPage : Page
{
    public CheckoutViewModel ViewModel { get; }

    public CheckoutPage()
    {
        // Resolve ViewModel via DI
        ViewModel = App.Current.Services.GetRequiredService<CheckoutViewModel>();
        this.DataContext = ViewModel;
        InitializeComponent();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        try
        {
            await ViewModel.InitializeAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[CheckoutPage] OnNavigatedTo failed: {ex.Message}");
        }
    }

    private void QrCodeImage_ImageFailed(object sender, ExceptionRoutedEventArgs e)
    {
        // Hide the failed image and show the placeholder
        if (sender is Image img)
        {
            img.Visibility = Visibility.Collapsed;
        }
        QrPlaceholder.Visibility = Visibility.Visible;
    }
}
