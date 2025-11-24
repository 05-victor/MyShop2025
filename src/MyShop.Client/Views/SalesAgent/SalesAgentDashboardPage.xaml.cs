using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MyShop.Shared.Models;
using MyShop.Client.ViewModels.SalesAgent;

namespace MyShop.Client.Views.SalesAgent
{
    public sealed partial class SalesAgentDashboardPage : Page
    {
        public SalesAgentDashboardViewModel ViewModel { get; }

        public SalesAgentDashboardPage()
        {
            this.InitializeComponent();
            ViewModel = App.Current.Services.GetRequiredService<SalesAgentDashboardViewModel>();
            this.DataContext = ViewModel;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is User user)
            {
                ViewModel.Initialize(user);
            }
        }
    }
}
