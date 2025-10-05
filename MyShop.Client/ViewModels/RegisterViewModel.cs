using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Client.Services;
using MyShop.Client.Views;

namespace MyShop.Client.ViewModels
{
    public partial class RegisterViewModel : ObservableObject
    {
        private readonly IAuthService _authService;
        private readonly INavigationService _navigationService;

        [ObservableProperty]
        private string _username = string.Empty;

        [ObservableProperty]
        private string _email = string.Empty;

        [ObservableProperty]
        private string _password = string.Empty;

        [ObservableProperty]
        private string _confirmPassword = string.Empty;

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        [ObservableProperty]
        private bool _isLoading = false;

        public RegisterViewModel(IAuthService authService, INavigationService navigationService)
        {
            _authService = authService;
            _navigationService = navigationService;
        }

        [RelayCommand]
        private async Task AttemptRegister()
        {
            ErrorMessage = string.Empty;
            IsLoading = true;

            if (string.IsNullOrWhiteSpace(Username) || 
                string.IsNullOrWhiteSpace(Email) || 
                string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Vui lòng điền đầy đủ thông tin.";
                IsLoading = false;
                return;
            }

            if (Password != ConfirmPassword)
            {
                ErrorMessage = "Mật khẩu xác nhận không khớp.";
                IsLoading = false;
                return;
            }

            try
            {
                var result = await _authService.RegisterAsync(Username, Email, Password);
                
                if (result.Success)
                {
                    // TODO: Lưu token
                    _navigationService.NavigateTo<DashboardView>();
                }
                else
                {
                    ErrorMessage = result.Message;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "Lỗi đăng ký: " + ex.Message;
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private void NavigateToLogin()
        {
            _navigationService.NavigateTo<LoginView>();
        }
    }
}