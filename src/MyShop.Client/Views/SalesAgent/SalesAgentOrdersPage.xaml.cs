using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MyShop.Client.ViewModels.SalesAgent;

namespace MyShop.Client.Views.SalesAgent
{
    public sealed partial class SalesOrdersPage : Page
    {
        public SalesAgentOrdersViewModel ViewModel { get; }

        public SalesOrdersPage()
        {
            this.InitializeComponent();
            ViewModel = App.Current.Services.GetRequiredService<SalesAgentOrdersViewModel>();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            await ViewModel.InitializeAsync();
        }
    }
}
