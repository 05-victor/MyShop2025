using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MyShop.Client.ViewModels.Shared;

namespace MyShop.Client.Views.Shared
{
    public sealed partial class PurchaseOrdersPage : Page
    {
        public PurchaseOrdersViewModel ViewModel { get; }

        public PurchaseOrdersPage()
        {
            this.InitializeComponent();
            ViewModel = App.Current.Services.GetRequiredService<PurchaseOrdersViewModel>();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            try
            {
                await ViewModel.InitializeAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[PurchaseOrdersPage] OnNavigatedTo failed: {ex.Message}");
            }
        }
    }
}
