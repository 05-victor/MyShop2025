using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using MyShop.Client.ViewModels.Profile;

namespace MyShop.Client.Views.Dialogs;

public sealed partial class ChangePasswordDialog : ContentDialog
{
    public ChangePasswordViewModel ViewModel { get; }

    public ChangePasswordDialog()
    {
        this.InitializeComponent();
        
        // Get ViewModel from DI
        ViewModel = App.Current.Services.GetRequiredService<ChangePasswordViewModel>();
    }

    private async void ContentDialog_PrimaryButtonClick(ContentDialog _, ContentDialogButtonClickEventArgs args)
    {
        // Get deferral to allow async work
        var deferral = args.GetDeferral();

        try
        {
            // Execute change password command
            await ViewModel.ChangePasswordCommand.ExecuteAsync(null);

            // If not successful, cancel dialog close
            if (!ViewModel.IsSuccess)
            {
                args.Cancel = true;
            }
        }
        finally
        {
            deferral.Complete();
        }
    }
}
