using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MyShop.Shared.Models;
using MyShop.Client.ViewModels.Admin;

namespace MyShop.Client.Views.Admin
{
    public sealed partial class AdminDashboardPage : Page
    {
        public AdminDashboardViewModel ViewModel { get; }

        public AdminDashboardPage()
        {
            this.InitializeComponent();
            ViewModel = App.Current.Services.GetRequiredService<AdminDashboardViewModel>();
            this.DataContext = ViewModel;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            System.Diagnostics.Debug.WriteLine($"[AdminDashboardPage] OnNavigatedTo called, Parameter: {e.Parameter?.GetType().Name ?? "null"}");

            try
            {
                if (e.Parameter is User user)
                {
                    System.Diagnostics.Debug.WriteLine($"[AdminDashboardPage] Initializing ViewModel with User: {user.Username}");
                    await ViewModel.InitializeAsync(user);
                    System.Diagnostics.Debug.WriteLine($"[AdminDashboardPage] InitializeAsync completed");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("[AdminDashboardPage] WARNING: No User parameter received!");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AdminDashboardPage] OnNavigatedTo failed: {ex.Message}");
            }
        }

        private async void ViewAllAgents_Click(object sender, RoutedEventArgs e)
        {
            await ViewModel.NavigateToAllAgentsCommand.ExecuteAsync(null);
        }

        private async void ViewAllProducts_Click(object sender, RoutedEventArgs e)
        {
            await ViewModel.NavigateToAllProductsCommand.ExecuteAsync(null);
        }

        private async void ViewAllAgentRequests_Click(object sender, RoutedEventArgs e)
        {
            await ViewModel.NavigateToAgentRequestsCommand.ExecuteAsync(null);
        }
    }
}
