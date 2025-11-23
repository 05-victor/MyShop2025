using Microsoft.UI.Xaml.Controls;
using MyShop.Client.ViewModels.Shared;

namespace MyShop.Client.Views.Shared;

public sealed partial class CartPage : Page
{
    public CartViewModel ViewModel { get; }

    public CartPage(CartViewModel viewModel)
    {
        ViewModel = viewModel;
        InitializeComponent();
    }
}
