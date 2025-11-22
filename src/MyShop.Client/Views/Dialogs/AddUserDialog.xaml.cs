using Microsoft.UI.Xaml.Controls;
using MyShop.Client.ViewModels.Dialogs;

namespace MyShop.Client.Views.Dialogs;

public sealed partial class AddUserDialog : ContentDialog
{
    public AddUserDialogViewModel ViewModel { get; }

    public AddUserDialog()
    {
        ViewModel = new AddUserDialogViewModel();
        this.InitializeComponent();
    }

    private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        // Validate before closing
        if (!ViewModel.IsValid())
        {
            args.Cancel = true;
            
            // Show validation error
            var error = ViewModel.GetValidationError();
            if (!string.IsNullOrEmpty(error))
            {
                // You can show an error message here
                System.Diagnostics.Debug.WriteLine($"[AddUserDialog] Validation error: {error}");
            }
        }
    }
}
