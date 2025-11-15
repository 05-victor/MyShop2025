using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MyShop.Shared.Models;
using MyShop.Client.ViewModels.Dashboard;

namespace MyShop.Client.Views.Dashboard
{
    public sealed partial class SalesmanDashboardPage : Page
    {
        public SalesmanDashboardViewModel ViewModel { get; }

        public SalesmanDashboardPage()
        {
            this.InitializeComponent();
            ViewModel = App.Current.Services.GetRequiredService<SalesmanDashboardViewModel>();
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
