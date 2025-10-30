using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using MyShop.Client.Core.Repositories.Interfaces;
using MyShop.Client.Core.Services.Interfaces;
using MyShop.Client.Helpers;
using MyShop.Client.ViewModels.Base;
using MyShop.Client.Views.Auth;
using MyShop.Client.Views.Dialogs;
using System;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace MyShop.Client.ViewModels.Auth
{
    public partial class LoginViewModel : BaseViewModel
    {
        private readonly IAuthRepository _authRepository;
        private readonly INavigationService _navigationService;
        private readonly IToastHelper _toastHelper;
        private readonly IRoleStrategyFactory _roleStrategyFactory;

        [ObservableProperty]
        private string _username = string.Empty;

        [ObservableProperty]
        private string _password = string.Empty;

        [ObservableProperty]
        private bool _isRememberMe = true;

        // IsLoading đã được khai báo trong BaseViewModel, không cần khai báo lại

        [ObservableProperty]
        private string _usernameError = string.Empty;

        [ObservableProperty]
        private string _passwordError = string.Empty;

        public string LoginButtonText => IsLoading ? "Signing in..." : "Sign In";

        public LoginViewModel(
            IAuthRepository authRepository,
            INavigationService navigationService,
            IToastHelper toastHelper,
            IRoleStrategyFactory roleStrategyFactory)
        {
            _authRepository = authRepository;
            _navigationService = navigationService;
            _toastHelper = toastHelper;
            _roleStrategyFactory = roleStrategyFactory;

            // Notify LoginButtonText when IsLoading changes
            PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(IsLoading))
                {
                    OnPropertyChanged(nameof(LoginButtonText));
                }
            };
        }

        [RelayCommand]
        private async Task AttemptLoginAsync()
        {
            // Clear previous errors
            ClearError();
            UsernameError = string.Empty;
            PasswordError = string.Empty;

            // Validation
            if (!ValidateInput())
            {
                return;
            }

            // Hiện loading overlay ngay lập tức
            SetLoadingState(true);

            try
            {

                // Use repository thay vì gọi trực tiếp API
                var result = await _authRepository.LoginAsync(Username.Trim(), Password);

                if (result.IsSuccess && result.Data != null)
                {
                    var user = result.Data;

                    // Save token if remember me is checked
                    if (IsRememberMe)
                    {
                        CredentialHelper.SaveToken(user.Token);
                    }

                    // Show success message
                    _toastHelper.ShowSuccess($"Welcome back, {user.Username}!");

                    // Use strategy pattern để navigate đến đúng dashboard
                    var primaryRole = user.GetPrimaryRole();
                    var strategy = _roleStrategyFactory.GetStrategy(primaryRole);
                    var pageType = strategy.GetDashboardPageType();
                    
                    _navigationService.NavigateTo(pageType, user);
                }
                else
                {
                    // Nếu lỗi mạng (repository đã bắt và đính kèm Exception), hiển thị dialog kết nối
                    if (result.Exception is HttpRequestException || result.Exception is SocketException || result.Exception is TaskCanceledException)
                    {
                        await HandleConnectionErrorAsync();
                    }
                    else
                    {
                        // Repository đã handle hết exceptions và map thành error message
                        SetError(result.ErrorMessage ?? "Login failed. Please try again.");
                    }
                }
            }
            catch (HttpRequestException httpEx)
            {
                System.Diagnostics.Debug.WriteLine($"Network Error: {httpEx.Message}");
                // Show immediate inline error to guarantee UX baseline
                SetError("Cannot connect to server. Please check your network connection and ensure the server is running.");
                // Then offer richer UX dialog if possible
                await HandleConnectionErrorAsync();
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Unexpected Error in LoginViewModel: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                SetError("An unexpected error occurred. Please try again.");
            }
            finally
            {
                SetLoadingState(false);
            }
        }

        private async Task HandleConnectionErrorAsync()
        {
            // Ensure a baseline inline message is visible
            SetError("Cannot connect to server. Please check your network connection and ensure the server is running.");

            var action = await _toastHelper.ShowConnectionErrorAsync(
                "Cannot connect to server. Please check your network connection and ensure the server is running.");

            switch (action)
            {
                case ConnectionErrorAction.Retry:
                    // Retry login
                    await AttemptLoginAsync();
                    break;

                case ConnectionErrorAction.ConfigureServer:
                    // Show server config dialog
                    await ShowServerConfigDialogAsync();
                    break;

                case ConnectionErrorAction.Cancel:
                    // Keep the inline error message visible as a fallback
                    break;
            }
        }

        private async Task ShowServerConfigDialogAsync()
        {
            try
            {
                var dialog = new ServerConfigDialog();
                
                // Get XamlRoot from current window
                var window = App.MainWindow;
                if (window?.Content != null)
                {
                    dialog.XamlRoot = window.Content.XamlRoot;
                    await dialog.ShowAsync();
                    // Note: Dialog handles restart internally if config changed
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error showing server config dialog: {ex.Message}");
                SetError("Failed to open server configuration dialog.");
            }
        }

        private bool ValidateInput()
        {
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

            return isValid;
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
            await ShowServerConfigDialogAsync();
        }
    }
}
