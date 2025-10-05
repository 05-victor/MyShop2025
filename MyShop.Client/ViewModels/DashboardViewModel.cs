using CommunityToolkit.Mvvm.ComponentModel;

namespace MyShop.Client.ViewModels
{
    public partial class DashboardViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _welcomeMessage = "Chào mừng đến với Dashboard MyShop 2025!";

        public DashboardViewModel()
        {
            // Initialize dashboard data here
        }
    }
}