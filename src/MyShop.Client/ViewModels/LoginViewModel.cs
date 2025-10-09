using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Client.Services;
using MyShop.Client.Views;
using MyShop.Shared.DTOs;

namespace MyShop.Client.ViewModels
{
    /// <summary>
    /// ViewModel cho trang Đăng nhập. Xử lý logic xác thực người dùng và điều hướng.
    /// Kế thừa từ ObservableValidator để hỗ trợ validation dữ liệu.
    /// </summary>
    /// <remarks>
    /// ViewModel này quản lý quá trình đăng nhập bao gồm:
    /// - Validation đầu vào của người dùng (username/email và password)
    /// - Xác thực thông qua IAuthService
    /// - Điều hướng đến các trang phù hợp dựa trên kết quả đăng nhập
    /// - Xử lý lỗi và phản hồi người dùng
    /// </remarks>
    public partial class LoginViewModel : ObservableValidator
    {
        #region Private Fields
        
        /// <summary>
        /// Service để xử lý các hoạt động xác thực
        /// </summary>
        private readonly IAuthService _authService;
        
        /// <summary>
        /// Service để xử lý điều hướng giữa các trang
        /// </summary>
        private readonly INavigationService _navigationService;

        #endregion

        #region Observable Properties

        /// <summary>
        /// Lấy hoặc đặt tên đăng nhập hoặc địa chỉ email được nhập bởi người dùng.
        /// Được validate để đảm bảo không trống.
        /// </summary>
        /// <value>Chuỗi username hoặc email, mặc định là chuỗi rỗng</value>
        [ObservableProperty]
        [Required(ErrorMessage = "Email hoặc tên đăng nhập không được để trống")]
        [NotifyDataErrorInfo]
        [NotifyCanExecuteChangedFor(nameof(AttemptLoginCommand))]
        private string _username = string.Empty;

        /// <summary>
        /// Lấy hoặc đặt mật khẩu được nhập bởi người dùng.
        /// Được validate để đảm bảo không trống và có độ dài tối thiểu.
        /// </summary>
        /// <value>Chuỗi mật khẩu, mặc định là chuỗi rỗng</value>
        [ObservableProperty]
        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        [MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
        [NotifyDataErrorInfo]
        [NotifyCanExecuteChangedFor(nameof(AttemptLoginCommand))]
        private string _password = string.Empty;

        /// <summary>
        /// Lấy hoặc đặt thông báo lỗi sẽ được hiển thị trên UI khi đăng nhập thất bại.
        /// </summary>
        /// <value>Chuỗi thông báo lỗi, mặc định là chuỗi rỗng</value>
        [ObservableProperty]
        private string _errorMessage = string.Empty;

        /// <summary>
        /// Lấy hoặc đặt trạng thái loading khi đang thực hiện đăng nhập.
        /// </summary>
        /// <value>True nếu đang loading, false nếu không</value>
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(AttemptLoginCommand))]
        private bool _isLoading = false;

        /// <summary>
        /// Lấy hoặc đặt thông báo lỗi cụ thể cho trường username.
        /// </summary>
        /// <value>Thông báo lỗi validation username</value>
        [ObservableProperty]
        private string _usernameError = string.Empty;

        /// <summary>
        /// Lấy hoặc đặt thông báo lỗi cụ thể cho trường password.
        /// </summary>
        /// <value>Thông báo lỗi validation password</value>
        [ObservableProperty]
        private string _passwordError = string.Empty;

        #endregion

        #region Constructor

        /// <summary>
        /// Khởi tạo một instance mới của LoginViewModel.
        /// </summary>
        /// <param name="authService">Service xử lý xác thực</param>
        /// <param name="navigationService">Service xử lý điều hướng</param>
        public LoginViewModel(IAuthService authService, INavigationService navigationService)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            
            // Đăng ký sự kiện để cập nhật error messages khi có lỗi validation
            ErrorsChanged += (s, e) => UpdateErrorMessages();
        }

        #endregion

        #region Commands

        /// <summary>
        /// Command để thực hiện đăng nhập.
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanAttemptLogin))]
        private async Task AttemptLogin()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                var loginRequest = new LoginRequest
                {
                    UsernameOrEmail = Username,
                    Password = Password
                };

                var result = await _authService.LoginAsync(loginRequest);

                if (result.Success)
                {
                    // Đăng nhập thành công, chuyển đến dashboard
                    _navigationService.NavigateTo<DashboardView>();
                }
                else
                {
                    ErrorMessage = result.Message;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Đã xảy ra lỗi: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Kiểm tra xem có thể thực hiện đăng nhập hay không.
        /// </summary>
        /// <returns>True nếu có thể đăng nhập, false nếu không</returns>
        private bool CanAttemptLogin()
        {
            return !IsLoading && !HasErrors && !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Password);
        }

        /// <summary>
        /// Command để chuyển đến trang đăng ký.
        /// </summary>
        [RelayCommand]
        private void NavigateToRegister()
        {
            _navigationService.NavigateTo<RegisterView>();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Xóa tất cả dữ liệu đầu vào và thông báo lỗi.
        /// </summary>
        public void ClearForm()
        {
            Username = string.Empty;
            Password = string.Empty;
            ErrorMessage = string.Empty;
            ClearErrors();
        }

        /// <summary>
        /// Cập nhật các thông báo lỗi riêng lẻ cho từng trường.
        /// </summary>
        private void UpdateErrorMessages()
        {
            UsernameError = GetErrors(nameof(Username)).FirstOrDefault()?.ErrorMessage ?? string.Empty;
            PasswordError = GetErrors(nameof(Password)).FirstOrDefault()?.ErrorMessage ?? string.Empty;
        }

        #endregion
    }
}