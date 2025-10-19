using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Client.ApiServer;
using MyShop.Client.Helpers;
using MyShop.Client.Views;
using MyShop.Shared.DTOs.Common;
using MyShop.Shared.DTOs.Requests;
using Refit;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using ApiResponse = MyShop.Shared.DTOs.Common.ApiResponse<object>;

namespace MyShop.Client.ViewModels {
    /// <summary>
    /// ViewModel cho trang Đăng ký với validation thời gian thực.
    /// Hỗ trợ validation cho từng trường và hiển thị lỗi ngay lập tức.
    /// </summary>
    public partial class RegisterViewModel : ObservableValidator {
        private readonly IAuthApi _authApi;
        private readonly INavigationService _navigationService;
        private readonly IToastHelper _toastHelper;

        #region Observable Properties - User Information

        [ObservableProperty]
        [Required(ErrorMessage = "Name is required")]
        [NotifyDataErrorInfo]
        [NotifyCanExecuteChangedFor(nameof(AttemptRegisterCommand))]
        private string _firstName = string.Empty;

        [ObservableProperty]
        [Required(ErrorMessage = "Last name is required")]
        [NotifyDataErrorInfo]
        [NotifyCanExecuteChangedFor(nameof(AttemptRegisterCommand))]
        private string _lastName = string.Empty;

        [ObservableProperty]
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [NotifyDataErrorInfo]
        [NotifyCanExecuteChangedFor(nameof(AttemptRegisterCommand))]
        private string _email = string.Empty;

        [ObservableProperty]
        [Required(ErrorMessage = "Phone numeber is required")]
        [Phone(ErrorMessage = "Invalid phone number format")]
        [NotifyDataErrorInfo]
        [NotifyCanExecuteChangedFor(nameof(AttemptRegisterCommand))]
        private string _phone = string.Empty;

        /// <summary>
        /// Tên đăng nhập (tùy chọn - sẽ được tạo tự động nếu để trống)
        /// </summary>
        [ObservableProperty]
        private string _username = string.Empty;

        [ObservableProperty]
        [Required(ErrorMessage = "Password is required")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
        [NotifyDataErrorInfo]
        [NotifyCanExecuteChangedFor(nameof(AttemptRegisterCommand))]
        private string _password = string.Empty;

        [ObservableProperty]
        [Required(ErrorMessage = "Confirm password is required")]
        [NotifyDataErrorInfo]
        [NotifyCanExecuteChangedFor(nameof(AttemptRegisterCommand))]
        private string _confirmPassword = string.Empty;

        #endregion

        #region Observable Properties - Field-Specific Errors

        [ObservableProperty]
        private string _firstNameError = string.Empty;

        [ObservableProperty]
        private string _lastNameError = string.Empty;

        [ObservableProperty]
        private string _emailError = string.Empty;

        [ObservableProperty]
        private string _phoneError = string.Empty;

        [ObservableProperty]
        private string _passwordError = string.Empty;

        [ObservableProperty]
        private string _confirmPasswordError = string.Empty;

        #endregion

        #region Observable Properties - UI State

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(AttemptRegisterCommand))]
        private bool _isLoading = false;

        #endregion

        #region Constructor

        public RegisterViewModel(IAuthApi authApi, INavigationService navigationService, IToastHelper toastHelper) {
            _authApi = authApi;
            _navigationService = navigationService;
            _toastHelper = toastHelper;

            // Đăng ký sự kiện để cập nhật error messages khi có validation errors
            ErrorsChanged += (s, e) => {
                if (e.PropertyName != null) {
                    UpdateErrorMessageForProperty(e.PropertyName);
                }
            };
        }

        #endregion

        #region Property Change Handlers (Real-Time Validation)

        /// <summary>
        /// Validate khi FirstName thay đổi
        /// </summary>
        partial void OnFirstNameChanged(string value) {
            ValidateProperty(value, "FirstName");
        }

        /// <summary>
        /// Validate khi LastName thay đổi
        /// </summary>
        partial void OnLastNameChanged(string value) {
            ValidateProperty(value, "LastName");
        }

        /// <summary>
        /// Validate khi Email thay đổi
        /// </summary>
        partial void OnEmailChanged(string value) {
            ValidateProperty(value, "Email");
        }

        /// <summary>
        /// Validate khi Phone thay đổi
        /// </summary>
        partial void OnPhoneChanged(string value) {
            ValidateProperty(value, "Phone");
        }

        /// <summary>
        /// Validate khi Password thay đổi và re-validate ConfirmPassword
        /// </summary>
        partial void OnPasswordChanged(string value) {
            ValidateProperty(value, "Password");
            
            // Re-validate confirm password để kiểm tra khớp
            if (!string.IsNullOrEmpty(ConfirmPassword)) {
                ValidatePasswordMatch();
            }
        }

        /// <summary>
        /// Validate khi ConfirmPassword thay đổi
        /// </summary>
        partial void OnConfirmPasswordChanged(string value) {
            ValidateProperty(value, "ConfirmPassword");
            ValidatePasswordMatch();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Cập nhật error message cho một property cụ thể
        /// </summary>
        private void UpdateErrorMessageForProperty(string propertyName) {
            var errors = GetErrors(propertyName);
            var errorMessage = errors?.Cast<ValidationResult>()
                .FirstOrDefault()?.ErrorMessage ?? string.Empty;

            switch (propertyName) {
                case "FirstName":
                    FirstNameError = errorMessage;
                    break;
                case "LastName":
                    LastNameError = errorMessage;
                    break;
                case "Email":
                    EmailError = errorMessage;
                    break;
                case "Phone":
                    PhoneError = errorMessage;
                    break;
                case "Password":
                    PasswordError = errorMessage;
                    break;
                case "ConfirmPassword":
                    ConfirmPasswordError = errorMessage;
                    break;
            }
        }

        /// <summary>
        /// Validate xem mật khẩu có khớp không
        /// </summary>
        private void ValidatePasswordMatch() {
            if (!string.IsNullOrEmpty(Password) && !string.IsNullOrEmpty(ConfirmPassword)) {
                if (Password != ConfirmPassword) {
                    ConfirmPasswordError = "Passwords do not match";
                } else {
                    // Clear error nếu passwords khớp
                    ClearErrors("ConfirmPassword");
                    ConfirmPasswordError = string.Empty;
                }
            }
        }

        /// <summary>
        /// Kiểm tra có thể thực hiện đăng ký không
        /// </summary>
        private bool CanRegister() {
            return !IsLoading && 
                   !string.IsNullOrWhiteSpace(FirstName) &&
                   !string.IsNullOrWhiteSpace(LastName) &&
                   !string.IsNullOrWhiteSpace(Email) &&
                   !string.IsNullOrWhiteSpace(Phone) &&
                   !string.IsNullOrWhiteSpace(Password) &&
                   !string.IsNullOrWhiteSpace(ConfirmPassword) &&
                   !HasErrors &&
                   Password == ConfirmPassword;
        }

        #endregion

        #region Commands

        [RelayCommand(CanExecute = nameof(CanRegister))]
        private async Task AttemptRegister() {
            // Validate tất cả properties trước khi submit
            ValidateAllProperties();

            // Kiểm tra password match lần cuối
            if (Password != ConfirmPassword) {
                ErrorMessage = "Passwords do not match";
                await _toastHelper.ShowErrorAsync("Passwords do not match");
                return;
            }

            if (HasErrors) {
                ErrorMessage = "Please check all before registering";
                await _toastHelper.ShowErrorAsync("Please check all errors");
                return;
            }

            IsLoading = true;
            ErrorMessage = string.Empty;

            try {
                // Tạo username từ FirstName + LastName nếu không được nhập
                var finalUsername = !string.IsNullOrWhiteSpace(Username) 
                    ? Username 
                    : $"{FirstName} {LastName}";

                var request = new CreateUserRequest {
                    Username = finalUsername,
                    Email = Email,
                    Password = Password,
                    Sdt = Phone,
                    ActivateTrial = false,
                    RoleNames = new() { }
                };

                System.Diagnostics.Debug.WriteLine("=== REGISTER REQUEST ===");
                System.Diagnostics.Debug.WriteLine($"Username: {request.Username}");
                System.Diagnostics.Debug.WriteLine($"Email: {request.Email}");
                System.Diagnostics.Debug.WriteLine($"Phone (Sdt): {request.Sdt}");
                System.Diagnostics.Debug.WriteLine("=======================");

                var response = await _authApi.RegisterAsync(request);

                System.Diagnostics.Debug.WriteLine("=== REGISTER RESPONSE ===");
                System.Diagnostics.Debug.WriteLine($"Code: {response.Code}");
                System.Diagnostics.Debug.WriteLine($"Success: {response.Success}");
                System.Diagnostics.Debug.WriteLine($"Message: {response.Message}");
                System.Diagnostics.Debug.WriteLine("========================");

                if (response.Code >= 200 && response.Code < 300 && response.Success) {
                    await _toastHelper.ShowSuccessAsync("Registration successful! Please login");
                    
                    _navigationService.NavigateTo(typeof(LoginView));
                    ClearForm();
                }
                else {
                    ErrorMessage = response.Message;
                    await _toastHelper.ShowErrorAsync(response.Message);
                }
            }
            catch (ApiException ex) when (ex.Content != null) {
                try {
                    var errorContent = await ex.GetContentAsAsync<ApiResponse>();
                    ErrorMessage = errorContent?.Message ?? ex.Message;
                    await _toastHelper.ShowErrorAsync(ErrorMessage);
                }
                catch {
                    ErrorMessage = ex.Message;
                    await _toastHelper.ShowErrorAsync($"Registration failed: {ex.Message}");
                }
                System.Diagnostics.Debug.WriteLine($"[Register API Error] {ex}");
            }
            catch (Exception ex) {
                ErrorMessage = $"An error occurred: {ex.Message}";
                await _toastHelper.ShowErrorAsync("n unexpected error occurred");
                System.Diagnostics.Debug.WriteLine($"[Register Error] {ex}");
            }
            finally {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private void NavigateToLogin() {
            _navigationService.NavigateTo(typeof(LoginView));
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

        #endregion

        #region Public Methods

        /// <summary>
        /// Clear toàn bộ form và errors
        /// </summary>
        public void ClearForm() {
            FirstName = string.Empty;
            LastName = string.Empty;
            Email = string.Empty;
            Phone = string.Empty;
            Username = string.Empty;
            Password = string.Empty;
            ConfirmPassword = string.Empty;
            ErrorMessage = string.Empty;
            
            // Clear all error messages
            FirstNameError = string.Empty;
            LastNameError = string.Empty;
            EmailError = string.Empty;
            PhoneError = string.Empty;
            PasswordError = string.Empty;
            ConfirmPasswordError = string.Empty;
            
            ClearErrors();
        }

        #endregion
    }
}