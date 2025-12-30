using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MyShop.Client.ViewModels.Auth;

namespace MyShop.Client.Views.Auth;

public sealed partial class ForgotPasswordRequestPage : Page
{
    public ForgotPasswordRequestViewModel ViewModel { get; }

    public ForgotPasswordRequestPage()
    {
        this.InitializeComponent();
        ViewModel = App.Current.Services.GetRequiredService<ForgotPasswordRequestViewModel>();
        this.DataContext = ViewModel;
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        EmailTextBox.Focus(Microsoft.UI.Xaml.FocusState.Programmatic);
    }
}
