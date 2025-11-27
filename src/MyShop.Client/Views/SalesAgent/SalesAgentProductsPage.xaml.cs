using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MyShop.Client.ViewModels.SalesAgent;

namespace MyShop.Client.Views.SalesAgent
{
    public sealed partial class SalesAgentProductsPage : Page
    {
        public SalesAgentProductsViewModel ViewModel { get; }

        public SalesAgentProductsPage()
        {
            this.InitializeComponent();
            ViewModel = App.Current.Services.GetRequiredService<SalesAgentProductsViewModel>();
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
                System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsPage] OnNavigatedTo failed: {ex.Message}");
            }
        }
    }
}
