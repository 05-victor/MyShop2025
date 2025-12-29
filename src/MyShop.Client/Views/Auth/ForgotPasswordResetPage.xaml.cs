using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MyShop.Client.ViewModels.Auth;

namespace MyShop.Client.Views.Auth;

public sealed partial class ForgotPasswordResetPage : Page
{
    public ForgotPasswordResetViewModel ViewModel { get; }

    public ForgotPasswordResetPage()
    {
        this.InitializeComponent();
        ViewModel = App.Current.Services.GetRequiredService<ForgotPasswordResetViewModel>();
        this.DataContext = ViewModel;
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        // Initialize with email from navigation parameter
        if (e.Parameter is string email)
        {
            ViewModel.InitializeWithEmail(email);
        }
    }
}
