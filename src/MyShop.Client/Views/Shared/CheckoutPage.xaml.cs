using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MyShop.Client.ViewModels.Shared;
using MyShop.Client.Views.Dialogs;

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

    private async void ChoosePaymentButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new PaymentMethodDialog
        {
            XamlRoot = this.XamlRoot
        };

        // Set current selection
        var currentMethod = ViewModel.SelectedPaymentMethod switch
        {
            "QR Code / Banking App" => PaymentMethodDialog.PaymentMethod.QR,
            "Credit / Debit Card" => PaymentMethodDialog.PaymentMethod.Card,
            "Cash on Delivery (COD)" => PaymentMethodDialog.PaymentMethod.COD,
            _ => PaymentMethodDialog.PaymentMethod.QR
        };
        dialog.SetInitialMethod(currentMethod);

        var result = await dialog.ShowAsync();

        if (result == ContentDialogResult.Primary && dialog.WasConfirmed)
        {
            // Update ViewModel with new selection
            ViewModel.UpdatePaymentMethod(dialog.GetMethodDisplayText(), dialog.GetMethodIcon());
            System.Diagnostics.Debug.WriteLine($"[CheckoutPage] Payment method updated: {dialog.GetMethodDisplayText()}");
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
