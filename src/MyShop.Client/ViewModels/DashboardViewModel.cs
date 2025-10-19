using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Client.Helpers;
using MyShop.Client.Views;
using MyShop.Shared.DTOs.Responses;
using System.Linq;

namespace MyShop.Client.ViewModels {
    public partial class DashboardViewModel : ObservableObject {
        private readonly INavigationService _navigationService;

        [ObservableProperty]
        private string _welcomeMessage = "Welcome to your Dashboard!";

        // Thuộc tính để lưu trữ toàn bộ thông tin người dùng
        [ObservableProperty]
        private LoginResponse? _currentUser;

        [ObservableProperty]
        private string _userInitial = "?";

        [ObservableProperty]
        private bool _isAdmin = false;

        public DashboardViewModel(INavigationService navigationService) {
            _navigationService = navigationService;
        }

        // Phương thức này được gọi từ View để khởi tạo dữ liệu
        public void Initialize(LoginResponse userData) {
            CurrentUser = userData;
            WelcomeMessage = $"Welcome back, {userData.Username}!";

            if (!string.IsNullOrEmpty(userData.Username)) {
                UserInitial = userData.Username[0].ToString().ToUpper();
            }

            IsAdmin = userData.RoleNames.Contains("Admin");
            // Tại đây bạn có thể thêm logic để kiểm tra activateTrial, isVerified...
        }

        [RelayCommand]
        private void Logout() {
            // Xóa token đã lưu
            CredentialHelper.RemoveToken();

            // Xóa dữ liệu người dùng hiện tại
            CurrentUser = null;

            // Điều hướng về trang đăng nhập
            _navigationService.NavigateTo(typeof(LoginView));
        }
    }
}