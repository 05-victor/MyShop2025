using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Extensions.DependencyInjection;
using MyShop.Client.ViewModels.Admin;

namespace MyShop.Client.Views.Admin
{
    public sealed partial class AdminAgentRequestsPage : Page
    {
        public AdminAgentRequestsViewModel ViewModel { get; }

        public AdminAgentRequestsPage()
        {
            this.InitializeComponent();
            ViewModel = App.Current.Services.GetRequiredService<AdminAgentRequestsViewModel>();
            this.DataContext = ViewModel;
        }
    }
}
