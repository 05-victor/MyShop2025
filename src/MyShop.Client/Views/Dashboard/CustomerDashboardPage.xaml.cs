using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MyShop.Client.Models;
using MyShop.Client.ViewModels.Dashboard;

namespace MyShop.Client.Views.Dashboard
{
    public sealed partial class CustomerDashboardPage : Page
    {
        public CustomerDashboardViewModel ViewModel { get; }

        public CustomerDashboardPage()
        {
            this.InitializeComponent();
            ViewModel = App.Current.Services.GetRequiredService<CustomerDashboardViewModel>();
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
