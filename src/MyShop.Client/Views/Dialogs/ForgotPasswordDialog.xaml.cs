using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;

namespace MyShop.Client.Views.Dialogs;

public sealed partial class ForgotPasswordDialog : ContentDialog
{
    public ForgotPasswordDialogViewModel ViewModel { get; }

    public ForgotPasswordDialog()
    {
        this.InitializeComponent();
        
        // Get ViewModel from DI
        ViewModel = App.Current.Services.GetRequiredService<ForgotPasswordDialogViewModel>();
        this.DataContext = ViewModel;

        // Handle success - close dialog
        ViewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(ViewModel.IsSuccess) && ViewModel.IsSuccess)
            {
                _ = Task.Delay(2000).ContinueWith(_ =>
                {
                    DispatcherQueue.TryEnqueue(() =>
                    {
                        Hide();
                    });
                });
            }
        };
    }
}
