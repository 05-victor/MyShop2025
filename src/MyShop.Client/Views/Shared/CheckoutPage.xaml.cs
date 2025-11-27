using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using MyShop.Client.ViewModels.Shared;

namespace MyShop.Client.Views.Shared;

public sealed partial class CheckoutPage : Page
{
    public CheckoutViewModel ViewModel { get; }

    public CheckoutPage()
    {
        // Resolve ViewModel via DI
        ViewModel = App.Current.Services.GetRequiredService<CheckoutViewModel>();
        this.DataContext = ViewModel;
        InitializeComponent();
    }
}
