using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace MyShop.Client.Views.Dialogs;

/// <summary>
/// Dialog for entering rejection reason for agent requests
/// </summary>
public sealed partial class RejectReasonDialog : ContentDialog
{
    public string RejectionReason { get; set; } = string.Empty;

    public RejectReasonDialog()
    {
        this.InitializeComponent();
        this.PrimaryButtonText = "Reject";
        this.CloseButtonText = "Cancel";
    }

    private void ReasonTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is TextBox textBox)
        {
            RejectionReason = textBox.Text;
            // Enable primary button only if reason is provided
            IsPrimaryButtonEnabled = !string.IsNullOrWhiteSpace(RejectionReason);
        }
    }

    private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        // Validate reason is not empty
        if (string.IsNullOrWhiteSpace(RejectionReason))
        {
            args.Cancel = true;
            return;
        }
    }
}
