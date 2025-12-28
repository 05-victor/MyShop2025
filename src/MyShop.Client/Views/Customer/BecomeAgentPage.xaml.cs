using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MyShop.Shared.Models;

namespace MyShop.Client.Views.Customer;

public sealed partial class BecomeAgentPage : Page
{
    public ViewModels.Customer.BecomeAgentViewModel ViewModel { get; }

    public BecomeAgentPage()
    {
        this.InitializeComponent();
        ViewModel = App.Current.Services.GetRequiredService<ViewModels.Customer.BecomeAgentViewModel>();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        
        if (e.Parameter is User user)
        {
            await ViewModel.InitializeAsync(user);
        }
        else
        {
            await ViewModel.InitializeAsync();
        }
    }
}
