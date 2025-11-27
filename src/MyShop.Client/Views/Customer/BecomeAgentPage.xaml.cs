using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;

namespace MyShop.Client.Views.Customer;

public sealed partial class BecomeAgentPage : Page
{
    public ViewModels.Customer.BecomeAgentViewModel ViewModel { get; }

    public BecomeAgentPage()
    {
        this.InitializeComponent();
        ViewModel = App.Current.Services.GetRequiredService<ViewModels.Customer.BecomeAgentViewModel>();
        _ = ViewModel.InitializeAsync();
    }
}
