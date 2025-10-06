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
    /// ViewModel cho trang Đăng ký. Xử lý logic đăng ký người dùng và validation.
    /// Kế thừa từ ObservableValidator để hỗ trợ validation dữ liệu đầy đủ.
    /// </summary>
    /// <remarks>
    /// ViewModel này quản lý quá trình đăng ký người dùng bao gồm:
    /// - Validation form đa trường (tên, email, điện thoại, mật khẩu)
    /// - Kiểm tra khớp mật khẩu xác nhận
    /// - Đăng ký người dùng thông qua IAuthService
    /// - Điều hướng đến các trang phù hợp dựa trên kết quả đăng ký
    /// - Xử lý lỗi toàn diện và phản hồi người dùng
    /// </remarks>
    public partial class RegisterViewModel : ObservableValidator
    {
        #region Private Fields

        /// <summary>
        /// Service để xử lý các hoạt động xác thực và đăng ký
        /// </summary>
        private readonly IAuthService _authService;
        
        /// <summary>
        /// Service để xử lý điều hướng giữa các trang
        /// </summary>
        private readonly INavigationService _navigationService;

        #endregion

        #region Observable Properties - User Information

        /// <summary>
        /// Lấy hoặc đặt tên của người dùng.
        /// Trường bắt buộc để đăng ký với validation.
        /// </summary>
        /// <value>Chuỗi tên, mặc định là chuỗi rỗng</value>
        [ObservableProperty]
        [Required(ErrorMessage = "Tên không được để trống")]
        [NotifyDataErrorInfo]
        [NotifyCanExecuteChangedFor(nameof(AttemptRegisterCommand))]
        private string _firstName = string.Empty;

        /// <summary>
        /// Lấy hoặc đặt họ của người dùng.
        /// Trường bắt buộc để đăng ký với validation.
        /// </summary>
        /// <value>Chuỗi họ, mặc định là chuỗi rỗng</value>
        [ObservableProperty]
        [Required(ErrorMessage = "Họ không được để trống")]
        [NotifyDataErrorInfo]
        [NotifyCanExecuteChangedFor(nameof(AttemptRegisterCommand))]
        private string _lastName = string.Empty;

        /// <summary>
        /// Lấy hoặc đặt số điện thoại của người dùng.
        /// Trường bắt buộc với validation định dạng số điện thoại.
        /// </summary>
        /// <value>Chuỗi số điện thoại, mặc định là chuỗi rỗng</value>
        [ObservableProperty]
        [Required(ErrorMessage = "Số điện thoại không được để trống")]
        [Phone(ErrorMessage = "Số điện thoại không đúng định dạng")]
        [NotifyDataErrorInfo]
        [NotifyCanExecuteChangedFor(nameof(AttemptRegisterCommand))]
        private string _phone = string.Empty;

        /// <summary>
        /// Lấy hoặc đặt tên đăng nhập mong muốn của người dùng.
        /// Trường tùy chọn - nếu để trống, sẽ được tạo từ tên và họ.
        /// </summary>
        /// <value>Chuỗi username, mặc định là chuỗi rỗng</value>
        [ObservableProperty]
        private string _username = string.Empty;

        /// <summary>
        /// Lấy hoặc đặt địa chỉ email của người dùng.
        /// Trường bắt buộc với validation định dạng email.
        /// </summary>
        /// <value>Chuỗi email, mặc định là chuỗi rỗng</value>
        [ObservableProperty]
        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        [NotifyDataErrorInfo]
        [NotifyCanExecuteChangedFor(nameof(AttemptRegisterCommand))]
        private string _email = string.Empty;

        #endregion

        #region Observable Properties - Password

        /// <summary>
        /// Lấy hoặc đặt mật khẩu của người dùng.
        /// Trường bắt buộc với validation độ dài tối thiểu.
        /// </summary>
        /// <value>Chuỗi mật khẩu, mặc định là chuỗi rỗng</value>
        [ObservableProperty]
        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        [MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
        [NotifyDataErrorInfo]
        [NotifyCanExecuteChangedFor(nameof(AttemptRegisterCommand))]
        private string _password = string.Empty;

        /// <summary>
        /// Lấy hoặc đặt xác nhận mật khẩu.
        /// Phải khớp với trường mật khẩu để đăng ký thành công.
        /// </summary>
        /// <value>Chuỗi xác nhận mật khẩu, mặc định là chuỗi rỗng</value>
        [ObservableProperty]
        [Required(ErrorMessage = "Xác nhận mật khẩu không được để trống")]
        [NotifyDataErrorInfo]
        [NotifyCanExecuteChangedFor(nameof(AttemptRegisterCommand))]
        private string _confirmPassword = string.Empty;

        #endregion

        #region Observable Properties - UI State

        /// <summary>
        /// Lấy hoặc đặt thông báo lỗi sẽ được hiển thị trên UI khi đăng ký thất bại.
        /// </summary>
        /// <value>Chuỗi thông báo lỗi, mặc định là chuỗi rỗng</value>
        [ObservableProperty]
        private string _errorMessage = string.Empty;

        /// <summary>
        /// Lấy hoặc đặt giá trị cho biết liệu có hoạt động đăng ký đang diễn ra.
        /// Khi true, UI nên hiển thị chỉ báo loading và vô hiệu hóa tương tác người dùng.
        /// </summary>
        /// <value>True nếu đang loading, false nếu không</value>
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(AttemptRegisterCommand))]
        private bool _isLoading = false;

        /// <summary>
        /// Lấy hoặc đặt giá trị cho biết liệu nút đăng ký có nên được bật.
        /// Nút được bật khi tất cả các trường bắt buộc hợp lệ và mật khẩu khớp.
        /// </summary>
        /// <value>True nếu được phép đăng ký, false nếu không</value>
        [ObservableProperty]
        private bool _canRegister = true;

        #endregion

        #region Observable Properties - Field-Specific Errors

        /// <summary>
        /// Lấy hoặc đặt thông báo lỗi cụ thể cho trường tên.
        /// </summary>
        /// <value>Thông báo lỗi validation tên</value>
        [ObservableProperty]
        private string _firstNameError = string.Empty;

        /// <summary>
        /// Lấy hoặc đặt thông báo lỗi cụ thể cho trường họ.
        /// </summary>
        /// <value>Thông báo lỗi validation họ</value>
        [ObservableProperty]
        private string _lastNameError = string.Empty;

        /// <summary>
        /// Lấy hoặc đặt thông báo lỗi cụ thể cho trường điện thoại.
        /// </summary>
        /// <value>Thông báo lỗi validation điện thoại</value>
        [ObservableProperty]
        private string _phoneError = string.Empty;

        /// <summary>
        /// Lấy hoặc đặt thông báo lỗi cụ thể cho trường email.
        /// </summary>
        /// <value>Thông báo lỗi validation email</value>
        [ObservableProperty]
        private string _emailError = string.Empty;

        /// <summary>
        /// Lấy hoặc đặt thông báo lỗi cụ thể cho trường mật khẩu.
        /// </summary>
        /// <value>Thông báo lỗi validation mật khẩu</value>
        [ObservableProperty]
        private string _passwordError = string.Empty;

        /// <summary>
        /// Lấy hoặc đặt thông báo lỗi cụ thể cho trường xác nhận mật khẩu.
        /// </summary>
        /// <value>Thông báo lỗi validation xác nhận mật khẩu</value>
        [ObservableProperty]
        private string _confirmPasswordError = string.Empty;

        #endregion

        #region Constructor

        /// <summary>
        /// Khởi tạo một instance mới của class <see cref="RegisterViewModel"/>.
        /// </summary>
        /// <param name="authService">Service xác thực để xử lý các hoạt động đăng ký</param>
        /// <param name="navigationService">Service để điều hướng giữa các trang</param>
        /// <exception cref="ArgumentNullException">Được throw khi authService hoặc navigationService là null</exception>
        public RegisterViewModel(IAuthService authService, INavigationService navigationService)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            
            // Đăng ký sự kiện để cập nhật trạng thái khi có lỗi validation
            ErrorsChanged += (s, e) => {
                UpdateCanRegister();
                UpdateErrorMessages();
            };
            
            // Đăng ký sự kiện để cập nhật trạng thái khi IsLoading thay đổi
            PropertyChanged += (s, e) => {
                if (e.PropertyName == nameof(IsLoading))
                {
                    UpdateCanRegister();
                }
            };
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Cập nhật thuộc tính CanRegister dựa trên trạng thái validation, trạng thái loading và khớp mật khẩu.
        /// Đảm bảo nút đăng ký chỉ được bật khi tất cả các điều kiện được đáp ứng.
        /// </summary>
        private void UpdateCanRegister()
        {
            CanRegister = !HasErrors && !IsLoading && 
                         !string.IsNullOrWhiteSpace(FirstName) && 
                         !string.IsNullOrWhiteSpace(LastName) &&
                         !string.IsNullOrWhiteSpace(Email) && 
                         !string.IsNullOrWhiteSpace(Password) &&
                         !string.IsNullOrWhiteSpace(ConfirmPassword) &&
                         Password == ConfirmPassword;
        }

        /// <summary>
        /// Cập nhật các thuộc tính thông báo lỗi riêng lẻ cho tất cả các trường form.
        /// Cho phép UI hiển thị thông báo lỗi cụ thể cho từng trường.
        /// </summary>
        private void UpdateErrorMessages()
        {
            FirstNameError = GetErrors(nameof(FirstName)).FirstOrDefault()?.ErrorMessage ?? string.Empty;
            LastNameError = GetErrors(nameof(LastName)).FirstOrDefault()?.ErrorMessage ?? string.Empty;
            PhoneError = GetErrors(nameof(Phone)).FirstOrDefault()?.ErrorMessage ?? string.Empty;
            EmailError = GetErrors(nameof(Email)).FirstOrDefault()?.ErrorMessage ?? string.Empty;
            PasswordError = GetErrors(nameof(Password)).FirstOrDefault()?.ErrorMessage ?? string.Empty;
            ConfirmPasswordError = GetErrors(nameof(ConfirmPassword)).FirstOrDefault()?.ErrorMessage ?? string.Empty;
        }

        /// <summary>
        /// Xác định liệu lệnh đăng ký có thể thực thi được không.
        /// Được sử dụng bởi RelayCommand để bật/tắt chức năng đăng ký.
        /// </summary>
        /// <returns>True nếu có thể thử đăng ký, false nếu không</returns>
        private bool CanAttemptRegister => !HasErrors && !IsLoading && 
                                          !string.IsNullOrWhiteSpace(FirstName) && 
                                          !string.IsNullOrWhiteSpace(LastName) &&
                                          !string.IsNullOrWhiteSpace(Email) && 
                                          !string.IsNullOrWhiteSpace(Password) &&
                                          !string.IsNullOrWhiteSpace(ConfirmPassword) &&
                                          Password == ConfirmPassword;

        #endregion

        #region Commands

        /// <summary>
        /// Thử đăng ký người dùng mới với thông tin được cung cấp một cách bất đồng bộ.
        /// Validates form, kiểm tra xác nhận mật khẩu, gọi service xác thực,
        /// và điều hướng đến dashboard khi thành công.
        /// </summary>
        /// <returns>Một task đại diện cho hoạt động đăng ký bất đồng bộ</returns>
        /// <remarks>
        /// Phương thức này thực hiện các bước sau:
        /// 1. Xóa bất kỳ thông báo lỗi hiện có nào
        /// 2. Đặt trạng thái loading thành true
        /// 3. Validates tất cả các thuộc tính
        /// 4. Kiểm tra khớp xác nhận mật khẩu
        /// 5. Gọi service xác thực để đăng ký
        /// 6. Điều hướng đến dashboard khi thành công hoặc hiển thị thông báo lỗi khi thất bại
        /// 7. Reset trạng thái loading
        /// </remarks>
        [RelayCommand(CanExecute = nameof(CanAttemptRegister))]
        private async Task AttemptRegister()
        {
            ErrorMessage = string.Empty;
            IsLoading = true;

            try
            {
                // Validate tất cả các thuộc tính
                ValidateAllProperties();
                
                // Kiểm tra mật khẩu khớp
                if (Password != ConfirmPassword)
                {
                    ErrorMessage = "Mật khẩu xác nhận không khớp";
                    return;
                }

                if (HasErrors)
                {
                    return;
                }

                // Tạo username nếu không được cung cấp
                var finalUsername = !string.IsNullOrWhiteSpace(Username) 
                    ? Username 
                    : $"{FirstName} {LastName}";

                var result = await _authService.RegisterAsync(finalUsername, Email, Password);
                
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
                ErrorMessage = "Lỗi đăng ký: " + ex.Message;
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Điều hướng trở lại trang đăng nhập.
        /// Lệnh này cho phép người dùng quay lại đăng nhập nếu họ đã có tài khoản.
        /// </summary>
        [RelayCommand]
        private void NavigateToLogin()
        {
            _navigationService.NavigateTo<LoginView>();
        }

        #endregion

        #region Property Change Handlers

        /// <summary>
        /// Được gọi khi thuộc tính FirstName thay đổi.
        /// Validates giá trị tên mới và cập nhật trạng thái nút đăng ký.
        /// </summary>
        /// <param name="value">Giá trị tên mới</param>
        partial void OnFirstNameChanged(string value)
        {
            ValidateProperty(value, nameof(FirstName));
            UpdateCanRegister();
        }

        /// <summary>
        /// Được gọi khi thuộc tính LastName thay đổi.
        /// Validates giá trị họ mới và cập nhật trạng thái nút đăng ký.
        /// </summary>
        /// <param name="value">Giá trị họ mới</param>
        partial void OnLastNameChanged(string value)
        {
            ValidateProperty(value, nameof(LastName));
            UpdateCanRegister();
        }

        /// <summary>
        /// Được gọi khi thuộc tính Phone thay đổi.
        /// Validates giá trị điện thoại mới và cập nhật trạng thái nút đăng ký.
        /// </summary>
        /// <param name="value">Giá trị điện thoại mới</param>
        partial void OnPhoneChanged(string value)
        {
            ValidateProperty(value, nameof(Phone));
            UpdateCanRegister();
        }

        /// <summary>
        /// Được gọi khi thuộc tính Email thay đổi.
        /// Validates giá trị email mới và cập nhật trạng thái nút đăng ký.
        /// </summary>
        /// <param name="value">Giá trị email mới</param>
        partial void OnEmailChanged(string value)
        {
            ValidateProperty(value, nameof(Email));
            UpdateCanRegister();
        }

        /// <summary>
        /// Được gọi khi thuộc tính Password thay đổi.
        /// Validates giá trị mật khẩu mới, kích hoạt re-validation xác nhận mật khẩu nếu cần,
        /// và cập nhật trạng thái nút đăng ký.
        /// </summary>
        /// <param name="value">Giá trị mật khẩu mới</param>
        partial void OnPasswordChanged(string value)
        {
            ValidateProperty(value, nameof(Password));
            UpdateCanRegister();
            
            // Re-validate xác nhận mật khẩu khi mật khẩu thay đổi để kiểm tra khớp
            if (!string.IsNullOrEmpty(ConfirmPassword))
            {
                OnConfirmPasswordChanged(ConfirmPassword);
            }
        }

        /// <summary>
        /// Được gọi khi thuộc tính ConfirmPassword thay đổi.
        /// Validates giá trị xác nhận mật khẩu mới và cập nhật trạng thái nút đăng ký.
        /// Validation khớp mật khẩu được thực hiện trong quá trình thử đăng ký.
        /// </summary>
        /// <param name="value">Giá trị xác nhận mật khẩu mới</param>
        partial void OnConfirmPasswordChanged(string value)
        {
            ValidateProperty(value, nameof(ConfirmPassword));
            UpdateCanRegister();
            
            // Password match validation được kiểm tra trong AttemptRegister
            // để tránh lỗi với SetErrors method trong validation framework
        }

        #endregion
    }
}