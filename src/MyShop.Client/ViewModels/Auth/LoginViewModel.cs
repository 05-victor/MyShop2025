using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Client.ApiServer;
using MyShop.Client.Helpers;
using MyShop.Client.Views.Auth;
using MyShop.Client.Views.Dashboard;
using MyShop.Shared.DTOs.Requests;
using Refit;
using System;
using System.Threading.Tasks;

namespace MyShop.Client.ViewModels.Auth
{
    public partial class LoginViewModel : ObservableObject
    {
        private readonly IAuthApi _authApi;
        private readonly INavigationService _navigationService;
        private readonly IToastHelper _toastHelper;

        [ObservableProperty]
        private string _username = string.Empty;

        [ObservableProperty]
        private string _password = string.Empty;

        [ObservableProperty]
        private bool _isRememberMe = true;

        [ObservableProperty]
        private bool _isLoading = false;

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        [ObservableProperty]
        private string _usernameError = string.Empty;

        [ObservableProperty]
        private string _passwordError = string.Empty;

        public LoginViewModel(
            IAuthApi authApi,
            INavigationService navigationService,
            IToastHelper toastHelper)
        {
            _authApi = authApi;
            _navigationService = navigationService;
            _toastHelper = toastHelper;
        }

        [RelayCommand]
        private async Task AttemptLoginAsync()
        {
            // Clear previous errors
            ErrorMessage = string.Empty;
            UsernameError = string.Empty;
            PasswordError = string.Empty;

            // Validation
            bool isValid = true;

            if (string.IsNullOrWhiteSpace(Username))
            {
                UsernameError = "Username or Email is required";
                isValid = false;
            }

            if (string.IsNullOrWhiteSpace(Password))
            {
                PasswordError = "Password is required";
                isValid = false;
            }
            else if (Password.Length < 6)
            {
                PasswordError = "Password must be at least 6 characters";
                isValid = false;
            }

            if (!isValid)
            {
                return;
            }

            IsLoading = true;

            try
            {
                var request = new LoginRequest
                {
                    UsernameOrEmail = Username.Trim(),
                    Password = Password
                };

                var response = await _authApi.LoginAsync(request);

                if (response is not null && response.Success && response.Result != null)
                {
                    var loginData = response.Result;

                    // Save token if remember me is checked
                    if (IsRememberMe)
                    {
                        CredentialHelper.SaveToken(loginData.Token);
                    }

                    // Show success message
                    _toastHelper.ShowSuccess($"Welcome back, {loginData.Username}!");

                    // Navigate to role-specific dashboard
                    var pageType = ChooseDashboardPage(loginData.RoleNames);
                    _navigationService.NavigateTo(pageType, loginData);
                }
                else
                {
                    ErrorMessage = MapLoginError(response?.Code ?? 0, response?.Message);
                }
            }
            catch (ApiException apiEx)
            {
                System.Diagnostics.Debug.WriteLine($"API Error: {apiEx.StatusCode} - {apiEx.Content}");
                
                if (apiEx.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    ErrorMessage = "Invalid username or password. Please try again.";
                }
                else if (apiEx.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    ErrorMessage = "Account not found. Please check your username or email.";
                }
                else if (apiEx.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    ErrorMessage = "Invalid request. Please check your input.";
                }
                else
                {
                    ErrorMessage = "Network error. Please check your connection.";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"General Error: {ex.Message}");
                ErrorMessage = "An unexpected error occurred. Please try again.";
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private void NavigateToRegister()
        {
            _navigationService.NavigateTo(typeof(RegisterPage));
        }

        [RelayCommand]
        private void ForgotPassword()
        {
            _toastHelper.ShowInfo("Password recovery feature coming soon!");
        }

        [RelayCommand]
        private async Task GoogleLogin() {
            try {
                IsLoading = true;
                ErrorMessage = string.Empty;

                // TODO: Implement Google OAuth2 login
                System.Diagnostics.Debug.WriteLine("Google Login clicked - cần implement OAuth2");

                // Tạm thời hiển thị thông báo
                await Task.Delay(1000); // Simulate network call
                ErrorMessage = "Đăng nhập bằng Google sẽ được triển khai sớm.";
            }
            catch (Exception ex) {
                ErrorMessage = $"Lỗi đăng nhập Google: {ex.Message}";
            }
            finally {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task ConfigureServer()
        {
            try {
                IsLoading = true;
                ErrorMessage = string.Empty;

                // TODO: Implement server configuration dialog
                System.Diagnostics.Debug.WriteLine("Cấu hình Server clicked - cần implement chức năng");

                // Tạm thời hiển thị thông báo
                await Task.Delay(1000); // Simulate network call
                ErrorMessage = "Server configuration feature coming soon!";
            }
            catch (Exception ex) {
                ErrorMessage = $"Lỗi đăng nhập Server: {ex.Message}";
            }
            finally {
                IsLoading = false;
            }
        }

        private static System.Type ChooseDashboardPage(System.Collections.Generic.IEnumerable<string> roleNames)
        {
            var roles = new System.Collections.Generic.HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (roleNames != null)
            {
                foreach (var r in roleNames)
                {
                    if (!string.IsNullOrWhiteSpace(r)) roles.Add(r.Trim());
                }
            }

            if (roles.Contains("ADMIN")) return typeof(MyShop.Client.Views.Dashboard.DashboardPage);
            if (roles.Contains("SALEMAN") || roles.Contains("SALESMAN")) return typeof(MyShop.Client.Views.Dashboard.SalesmanDashboardPage);
            return typeof(MyShop.Client.Views.Dashboard.CustomerDashboardPage);
        }

        private static string MapLoginError(int code, string? message)
        {
            if (code == 401) return "Invalid username or password. Please try again.";
            if (code == 404) return "Account not found. Please check your username or email.";
            if (!string.IsNullOrWhiteSpace(message)) return message!;
            return "Login failed. Please try again.";
        }
    }
}
