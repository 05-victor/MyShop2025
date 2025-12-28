using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MyShop.Client.ViewModels.Auth;

namespace MyShop.Client.Views.Auth;

public sealed partial class ForgotPasswordSuccessPage : Page
{
    public ForgotPasswordSuccessViewModel ViewModel { get; }

    public ForgotPasswordSuccessPage()
    {
        this.InitializeComponent();
        ViewModel = App.Current.Services.GetRequiredService<ForgotPasswordSuccessViewModel>();
        this.DataContext = ViewModel;
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        
        // Get email from navigation parameter (optional, for prefilling login)
        if (e.Parameter is string email)
        {
            ViewModel.InitializeWithEmail(email);
        }
    }
}
