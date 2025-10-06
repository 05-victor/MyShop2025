using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Client.Services;
using MyShop.Client.Views;

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
        /// Lấy hoặc đặt giá trị cho biết liệu có hoạt động dài hạn đang diễn ra (ví dụ: API call).
        /// Khi true, UI nên hiển thị chỉ báo loading và vô hiệu hóa tương tác người dùng.
        /// </summary>
        /// <value>True nếu đang loading, false nếu không</value>
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(AttemptLoginCommand))]
        private bool _isLoading = false;

        /// <summary>
        /// Lấy hoặc đặt giá trị cho biết liệu nút đăng nhập có nên được bật.
        /// Button chỉ được bật khi form hợp lệ và không có tác vụ nào đang chạy.
        /// </summary>
        /// <value>True nếu được phép đăng nhập, false nếu không</value>
        [ObservableProperty]
        private bool _canLogin = true;

        /// <summary>
        /// Lấy hoặc đặt thông báo lỗi cụ thể cho trường username.
        /// Được sử dụng để hiển thị lỗi validation cụ thể cho từng trường trong UI.
        /// </summary>
        /// <value>Thông báo lỗi validation username</value>
        [ObservableProperty]
        private string _usernameError = string.Empty;

        /// <summary>
        /// Lấy hoặc đặt thông báo lỗi cụ thể cho trường password.
        /// Được sử dụng để hiển thị lỗi validation cụ thể cho từng trường trong UI.
        /// </summary>
        /// <value>Thông báo lỗi validation password</value>
        [ObservableProperty]
        private string _passwordError = string.Empty;

        #endregion

        #region Constructor

        /// <summary>
        /// Khởi tạo một instance mới của class <see cref="LoginViewModel"/>.
        /// </summary>
        /// <param name="authService">Service xác thực để xử lý các hoạt động đăng nhập</param>
        /// <param name="navigationService">Service để điều hướng giữa các trang</param>
        /// <exception cref="ArgumentNullException">Được throw khi authService hoặc navigationService là null</exception>
        public LoginViewModel(IAuthService authService, INavigationService navigationService)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            
            // Đăng ký sự kiện để cập nhật trạng thái khi có lỗi validation
            ErrorsChanged += (s, e) => {
                UpdateCanLogin();
                UpdateErrorMessages();
            };
            
            // Đăng ký sự kiện để cập nhật trạng thái khi IsLoading thay đổi
            PropertyChanged += (s, e) => {
                if (e.PropertyName == nameof(IsLoading))
                {
                    UpdateCanLogin();
                }
            };
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Cập nhật thuộc tính CanLogin dựa trên trạng thái validation hiện tại, trạng thái loading và giá trị đầu vào.
        /// Phương thức này đảm bảo nút đăng nhập chỉ được bật khi tất cả các điều kiện được đáp ứng.
        /// </summary>
        private void UpdateCanLogin()
        {
            CanLogin = !HasErrors && !IsLoading && 
                       !string.IsNullOrWhiteSpace(Username) && 
                       !string.IsNullOrWhiteSpace(Password);
        }

        /// <summary>
        /// Cập nhật các thuộc tính thông báo lỗi riêng lẻ cho các trường username và password.
        /// Điều này cho phép UI hiển thị thông báo lỗi cụ thể cho từng trường.
        /// </summary>
        private void UpdateErrorMessages()
        {
            UsernameError = GetErrors(nameof(Username)).FirstOrDefault()?.ErrorMessage ?? string.Empty;
            PasswordError = GetErrors(nameof(Password)).FirstOrDefault()?.ErrorMessage ?? string.Empty;
        }

        /// <summary>
        /// Xác định liệu lệnh đăng nhập có thể thực thi được không.
        /// Được sử dụng bởi RelayCommand để bật/tắt chức năng đăng nhập.
        /// </summary>
        /// <returns>True nếu có thể thử đăng nhập, false nếu không</returns>
        private bool CanAttemptLogin => !HasErrors && !IsLoading && 
                                       !string.IsNullOrWhiteSpace(Username) && 
                                       !string.IsNullOrWhiteSpace(Password);

        #endregion

        #region Commands

        /// <summary>
        /// Thử đăng nhập người dùng bằng thông tin đăng nhập được cung cấp một cách bất đồng bộ.
        /// Validates form, gọi service xác thực, và điều hướng đến dashboard khi thành công.
        /// </summary>
        /// <returns>Một task đại diện cho hoạt động đăng nhập bất đồng bộ</returns>
        /// <remarks>
        /// Phương thức này thực hiện các bước sau:
        /// 1. Xóa bất kỳ thông báo lỗi hiện có nào
        /// 2. Đặt trạng thái loading thành true
        /// 3. Validates tất cả các thuộc tính
        /// 4. Gọi service xác thực
        /// 5. Điều hướng đến dashboard khi thành công hoặc hiển thị thông báo lỗi khi thất bại
        /// 6. Reset trạng thái loading
        /// </remarks>
        [RelayCommand(CanExecute = nameof(CanAttemptLogin))]
        private async Task AttemptLogin()
        {
            ErrorMessage = string.Empty;
            IsLoading = true;

            try
            {
                // Validate tất cả các thuộc tính
                ValidateAllProperties();
                if (HasErrors)
                {
                    return;
                }

                var result = await _authService.LoginAsync(Username, Password);
                
                if (result.Success)
                {
                    // TODO: Lưu token và thông tin người dùng
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

        /// <summary>
        /// Điều hướng đến trang đăng ký người dùng.
        /// Lệnh này cho phép người dùng tạo tài khoản mới nếu họ chưa có.
        /// </summary>
        [RelayCommand]
        private void NavigateToRegister()
        {
            _navigationService.NavigateTo<RegisterView>();
        }

        #endregion

        #region Property Change Handlers

        /// <summary>
        /// Được gọi khi thuộc tính Username thay đổi.
        /// Validates giá trị username mới và cập nhật trạng thái nút đăng nhập.
        /// </summary>
        /// <param name="value">Giá trị username mới</param>
        partial void OnUsernameChanged(string value)
        {
            ValidateProperty(value, nameof(Username));
            UpdateCanLogin();
        }

        /// <summary>
        /// Được gọi khi thuộc tính Password thay đổi.
        /// Validates giá trị password mới và cập nhật trạng thái nút đăng nhập.
        /// </summary>
        /// <param name="value">Giá trị password mới</param>
        partial void OnPasswordChanged(string value)
        {
            ValidateProperty(value, nameof(Password));
            UpdateCanLogin();
        }

        #endregion
    }
}