using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Client.Services;
using MyShop.Client.Views;

namespace MyShop.Client.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        private readonly IAuthService _authService;
        private readonly INavigationService _navigationService;

        [ObservableProperty]
        private string _email = string.Empty;

        [ObservableProperty]
        private string _password = string.Empty;

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        [ObservableProperty]
        private bool _isLoading = false;

        public LoginViewModel(IAuthService authService, INavigationService navigationService)
        {
            _authService = authService;
            _navigationService = navigationService;
        }

        [RelayCommand]
        private async Task AttemptLogin()
        {
            ErrorMessage = string.Empty;
            IsLoading = true;

            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Vui lòng nhập Email và Mật khẩu.";
                IsLoading = false;
                return;
            }

            try
            {
                var result = await _authService.LoginAsync(Email, Password);
                
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
                ErrorMessage = "Lỗi đăng nhập: " + ex.Message;
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private void NavigateToRegister()
        {
            _navigationService.NavigateTo<RegisterView>();
        }
    }
}