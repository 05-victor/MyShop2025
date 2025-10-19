using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MyShop.Client.ViewModels;
using MyShop.Shared.DTOs.Responses;
using Microsoft.Extensions.DependencyInjection;

namespace MyShop.Client.Views {
    public sealed partial class DashboardView : Page {
        public DashboardViewModel ViewModel { get; }

        public DashboardView() {
            this.InitializeComponent();
            ViewModel = App.Current.Services.GetRequiredService<DashboardViewModel>();
            this.DataContext = ViewModel;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e) {
            base.OnNavigatedTo(e);
            // Khi trang được điều hướng đến, lấy tham số (dữ liệu người dùng) và khởi tạo ViewModel
            if (e.Parameter is LoginResponse userData) {
                ViewModel.Initialize(userData);
            }
        }
    }
}