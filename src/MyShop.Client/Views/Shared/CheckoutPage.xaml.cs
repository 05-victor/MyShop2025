using Microsoft.UI.Xaml.Controls;
using MyShop.Client.ViewModels.Shared;

namespace MyShop.Client.Views.Shared;

public sealed partial class CheckoutPage : Page
{
    public CheckoutViewModel ViewModel { get; }

    public CheckoutPage()
    {
        ViewModel = new CheckoutViewModel();
        InitializeComponent();
    }
}
