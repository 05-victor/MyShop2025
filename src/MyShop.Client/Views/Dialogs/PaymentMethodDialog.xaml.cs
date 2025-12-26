using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace MyShop.Client.Views.Dialogs;

/// <summary>
/// Dialog for selecting payment method (QR/Card/COD)
/// </summary>
public sealed partial class PaymentMethodDialog : ContentDialog
{
    public enum PaymentMethod
    {
        QR,
        Card,
        COD
    }

    public PaymentMethod SelectedMethod { get; private set; } = PaymentMethod.QR;
    public bool WasConfirmed { get; private set; } = false;

    public PaymentMethodDialog()
    {
        this.InitializeComponent();
    }

    /// <summary>
    /// Set the initial selected payment method
    /// </summary>
    public void SetInitialMethod(PaymentMethod method)
    {
        SelectedMethod = method;
        UpdateRadioButtons();
    }

    private void PaymentMethod_Click(object sender, RoutedEventArgs e)
    {
        if (sender == QRRadioButton)
        {
            SelectedMethod = PaymentMethod.QR;
        }
        else if (sender == CardRadioButton)
        {
            SelectedMethod = PaymentMethod.Card;
        }
        else if (sender == CODRadioButton)
        {
            SelectedMethod = PaymentMethod.COD;
        }
    }

    private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        WasConfirmed = true;
        System.Diagnostics.Debug.WriteLine($"[PaymentMethodDialog] User confirmed: {SelectedMethod}");
    }

    private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        WasConfirmed = false;
        System.Diagnostics.Debug.WriteLine("[PaymentMethodDialog] User cancelled");
    }

    private void UpdateRadioButtons()
    {
        QRRadioButton.IsChecked = SelectedMethod == PaymentMethod.QR;
        CardRadioButton.IsChecked = SelectedMethod == PaymentMethod.Card;
        CODRadioButton.IsChecked = SelectedMethod == PaymentMethod.COD;
    }

    /// <summary>
    /// Get display text for the selected payment method
    /// </summary>
    public string GetMethodDisplayText()
    {
        return SelectedMethod switch
        {
            PaymentMethod.QR => "QR Code / Banking App",
            PaymentMethod.Card => "Credit / Debit Card",
            PaymentMethod.COD => "Cash on Delivery (COD)",
            _ => "Unknown"
        };
    }

    /// <summary>
    /// Get icon glyph for the selected payment method
    /// </summary>
    public string GetMethodIcon()
    {
        return SelectedMethod switch
        {
            PaymentMethod.QR => "\uE8A7", // QR code icon
            PaymentMethod.Card => "\uE8C7", // Credit card icon
            PaymentMethod.COD => "\uE8A1", // Cash icon
            _ => "\uE8A1"
        };
    }
}
