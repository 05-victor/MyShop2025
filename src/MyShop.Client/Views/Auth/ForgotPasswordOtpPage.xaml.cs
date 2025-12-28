using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MyShop.Client.ViewModels.Auth;

namespace MyShop.Client.Views.Auth;

public sealed partial class ForgotPasswordOtpPage : Page
{
    public ForgotPasswordOtpViewModel ViewModel { get; }

    public ForgotPasswordOtpPage()
    {
        this.InitializeComponent();
        ViewModel = App.Current.Services.GetRequiredService<ForgotPasswordOtpViewModel>();
        this.DataContext = ViewModel;
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        
        // Get email from navigation parameter
        if (e.Parameter is string email)
        {
            ViewModel.InitializeWithEmail(email);
        }
        
        OtpTextBox.Focus(Microsoft.UI.Xaml.FocusState.Programmatic);
    }
}
