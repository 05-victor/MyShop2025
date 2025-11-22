using Microsoft.UI.Xaml.Controls;
using MyShop.Client.ViewModels.Dialogs;
using MyShop.Shared.Models;

namespace MyShop.Client.Views.Dialogs;

public sealed partial class EditUserDialog : ContentDialog
{
    public EditUserDialogViewModel ViewModel { get; }

    public EditUserDialog(User user)
    {
        ViewModel = new EditUserDialogViewModel();
        ViewModel.LoadUser(user);
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
                System.Diagnostics.Debug.WriteLine($"[EditUserDialog] Validation error: {error}");
            }
        }
    }
}
