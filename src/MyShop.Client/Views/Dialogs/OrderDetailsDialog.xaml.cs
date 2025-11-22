using Microsoft.UI.Xaml.Controls;
using MyShop.Client.ViewModels.Shared;

namespace MyShop.Client.Views.Dialogs;

public sealed partial class OrderDetailsDialog : ContentDialog
{
    public OrderViewModel ViewModel { get; }

    public OrderDetailsDialog(OrderViewModel orderViewModel)
    {
        ViewModel = orderViewModel;
        this.InitializeComponent();
    }
}
