using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Client.ApiServer;
using MyShop.Client.Helpers;
using MyShop.Client.Views;
using MyShop.Shared.DTOs.Common;
using MyShop.Shared.DTOs.Requests;
using MyShop.Shared.DTOs.Responses;
using Refit;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MyShop.Client.ViewModels {
    /// <summary>
    /// ViewModel cho trang Đăng nhập. Xử lý logic xác thực người dùng và điều hướng.
    /// Kế thừa từ ObservableValidator để hỗ trợ validation dữ liệu.
    /// </summary>
    public partial class LoginViewModel : ObservableValidator {
        #region Private Fields

        private readonly IAuthApi _authApi;
        private readonly INavigationService _navigationService;
        private readonly IToastHelper _toastHelper;

        #endregion

        #region Observable Properties

        [ObservableProperty]
        [Required(ErrorMessage = "Username or email is required")]
        private string _username = string.Empty;

        [ObservableProperty]
        [Required(ErrorMessage = "Password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        private string _password = string.Empty;

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        [ObservableProperty]
        private bool _isRememberMe = false;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(AttemptLoginCommand))]
        private bool _isLoading = false;

        [ObservableProperty]
        private string _usernameError = string.Empty;

        [ObservableProperty]
        private string _passwordError = string.Empty;

        #endregion

        #region Constructor

        public LoginViewModel(IAuthApi authApi, INavigationService navigationService, IToastHelper toastHelper) {
            _authApi = authApi;
            _navigationService = navigationService;
            _toastHelper = toastHelper;
            
            // Subscribe to error changes to update individual field errors
            ErrorsChanged += (s, e) => {
                UpdateFieldErrors();
            };
        }

        #endregion

        #region Private Methods

        private void UpdateFieldErrors() {
            UsernameError = GetErrors(nameof(Username)).FirstOrDefault()?.ErrorMessage ?? string.Empty;
            PasswordError = GetErrors(nameof(Password)).FirstOrDefault()?.ErrorMessage ?? string.Empty;
        }

        #endregion

        #region Commands

        [RelayCommand(CanExecute = nameof(CanLogin))]
        private async Task AttemptLogin() {
            ValidateAllProperties();
            UpdateFieldErrors();
            if (HasErrors) return;

            IsLoading = true;
            ErrorMessage = string.Empty;

            try {
                var request = new LoginRequest {
                    UsernameOrEmail = Username,
                    Password = Password
                };

                System.Diagnostics.Debug.WriteLine("=== LOGIN REQUEST ===");
                System.Diagnostics.Debug.WriteLine($"UsernameOrEmail: {request.UsernameOrEmail}");
                System.Diagnostics.Debug.WriteLine("====================");

                var response = await _authApi.LoginAsync(request);

                System.Diagnostics.Debug.WriteLine("=== LOGIN RESPONSE ===");
                System.Diagnostics.Debug.WriteLine($"Code: {response.Code}");
                System.Diagnostics.Debug.WriteLine($"Success: {response.Success}");
                System.Diagnostics.Debug.WriteLine($"Message: {response.Message}");
                if (response.Result != null) {
                    System.Diagnostics.Debug.WriteLine($"Token: {response.Result.Token?.Substring(0, Math.Min(20, response.Result.Token?.Length ?? 0))}...");
                    System.Diagnostics.Debug.WriteLine($"Username: {response.Result.Username}");
                    System.Diagnostics.Debug.WriteLine($"Email: {response.Result.Email}");
                    System.Diagnostics.Debug.WriteLine($"Roles: {string.Join(", ", response.Result.RoleNames)}");
                    System.Diagnostics.Debug.WriteLine($"IsVerified: {response.Result.IsVerified}");
                    System.Diagnostics.Debug.WriteLine($"ActivateTrial: {response.Result.ActivateTrial}");
                }
                System.Diagnostics.Debug.WriteLine("=====================");

                // ✅ Check response theo chuẩn: code >= 200 && code < 300 && success == true
                if (response.Code >= 200 && response.Code < 300 && response.Success && response.Result?.Token is not null) {
                    // Save token to Windows Credential Locker
                    CredentialHelper.SaveToken(response.Result.Token);
                    
                    await _toastHelper.ShowSuccessAsync("Login successful!");

                    // Navigate to dashboard with user data
                    _navigationService.NavigateTo(typeof(DashboardView), response.Result);
                    ClearForm();
                }
                else {
                    // Handle authentication failure
                    var errorMsg = response.Message;
                    
                    // Make error messages more user-friendly
                    if (response.Code == 401) {
                        errorMsg = "Invalid username or password. Please try again.";
                    } else if (string.IsNullOrWhiteSpace(errorMsg)) {
                        errorMsg = "Login failed. Please check your credentials and try again.";
                    }
                    
                    ErrorMessage = errorMsg;
                    await _toastHelper.ShowErrorAsync(errorMsg);
                    
                    System.Diagnostics.Debug.WriteLine($"[Login Failed] Code: {response.Code}, Message: {errorMsg}");
                }
            }
            catch (ApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized) {
                // 401 Unauthorized - Invalid credentials
                ErrorMessage = "Invalid username or password. Please try again.";
                await _toastHelper.ShowErrorAsync(ErrorMessage);
                System.Diagnostics.Debug.WriteLine($"[Login 401 Error] {ex.Message}");
            }
            catch (ApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound) {
                // 404 Not Found - User doesn't exist
                ErrorMessage = "Account not found. Please check your username or email.";
                await _toastHelper.ShowErrorAsync(ErrorMessage);
                System.Diagnostics.Debug.WriteLine($"[Login 404 Error] {ex.Message}");
            }
            catch (ApiException ex) when (ex.Content != null) {
                try {
                    var errorContent = await ex.GetContentAsAsync<MyShop.Shared.DTOs.Common.ApiResponse<object>>();
                    ErrorMessage = errorContent?.Message ?? "Login failed. Please check your credentials.";
                }
                catch {
                    ErrorMessage = "Login failed. Please check your credentials.";
                }
                await _toastHelper.ShowErrorAsync(ErrorMessage);
                System.Diagnostics.Debug.WriteLine($"[Login API Error] {ex}");
            }
            catch (System.Net.Http.HttpRequestException ex) {
                // Network/Connection errors
                ErrorMessage = "Cannot connect to server. Please check your internet connection and try again.";
                await _toastHelper.ShowErrorAsync("Server connection failed. Please ensure the server is running.");
                System.Diagnostics.Debug.WriteLine($"[Login Connection Error] {ex.Message}");
            }
            catch (TaskCanceledException ex) {
                // Timeout errors
                ErrorMessage = "Request timeout. Please try again.";
                await _toastHelper.ShowErrorAsync(ErrorMessage);
                System.Diagnostics.Debug.WriteLine($"[Login Timeout] {ex.Message}");
            }
            catch (Exception ex) {
                ErrorMessage = "An unexpected error occurred. Please try again later.";
                await _toastHelper.ShowErrorAsync("An unexpected error occurred.");
                System.Diagnostics.Debug.WriteLine($"[Login Error] {ex}");
            }
            finally {
                IsLoading = false;
            }
        }

        private bool CanLogin() => !IsLoading;

        [RelayCommand]
        private void NavigateToRegister() {
            _navigationService.NavigateTo(typeof(RegisterView));
        }

        /// <summary>
        /// Google login command - Feature not yet implemented
        /// Requires OAuth2 integration with Google
        /// </summary>
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
        private async Task ForgotPassword() {
            // TODO: Implement forgot password flow
            // 1. Show dialog to enter email
            // 2. Call API to send reset email
            // 3. Navigate to verification page
            
            System.Diagnostics.Debug.WriteLine("Forgot Password clicked - not implemented");
            
            await _toastHelper.ShowErrorAsync(
                "Password recovery is not available yet.\n" +
                "Please contact support."
            );
        }

        public void ClearForm() {
            Username = string.Empty;
            Password = string.Empty;
            ErrorMessage = string.Empty;
            UsernameError = string.Empty;
            PasswordError = string.Empty;
            IsRememberMe = false;
            ClearErrors();
        }

        #endregion
    }
}
