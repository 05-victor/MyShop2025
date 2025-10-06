using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using MyShop.Client.ViewModels;
using Windows.System;

namespace MyShop.Client.Views
{
    /// <summary>
    /// Trang đăng ký tài khoản của ứng dụng.
    /// Code-behind cho RegisterView.xaml, xử lý tương tác UI và binding với RegisterViewModel.
    /// </summary>
    /// <remarks>
    /// Trang này cung cấp giao diện đăng ký tài khoản mới với:
    /// - Form nhập thông tin người dùng (tên, họ, email, điện thoại, mật khẩu)
    /// - Xác nhận mật khẩu
    /// - Xử lý sự kiện keyboard (Enter để đăng ký)
    /// - Binding với RegisterViewModel để xử lý logic nghiệp vụ
    /// - Xử lý sự kiện thay đổi password cho cả mật khẩu và xác nhận mật khẩu
    /// </remarks>
    public sealed partial class RegisterView : Page
    {
        /// <summary>
        /// Lấy instance của RegisterViewModel được sử dụng làm DataContext cho trang này.
        /// </summary>
        /// <value>Instance của RegisterViewModel từ dependency injection container</value>
        public RegisterViewModel ViewModel { get; }

        /// <summary>
        /// Khởi tạo một instance mới của class <see cref="RegisterView"/>.
        /// </summary>
        /// <remarks>
        /// Constructor này:
        /// 1. Khởi tạo components UI từ XAML
        /// 2. Lấy RegisterViewModel từ dependency injection
        /// 3. Đặt ViewModel làm DataContext cho page
        /// </remarks>
        public RegisterView()
        {
            this.InitializeComponent();
            ViewModel = App.GetService<RegisterViewModel>();
            this.DataContext = ViewModel;
        }

        /// <summary>
        /// Xử lý sự kiện thay đổi mật khẩu trong PasswordBox chính.
        /// </summary>
        /// <param name="sender">PasswordBox mật khẩu chính đã thay đổi</param>
        /// <param name="e">Thông tin sự kiện</param>
        /// <remarks>
        /// Phương thức này cần thiết vì PasswordBox không hỗ trợ two-way binding
        /// trực tiếp với Password property vì lý do bảo mật.
        /// Cập nhật mật khẩu trong ViewModel khi người dùng nhập.
        /// </remarks>
        private void PasswordInput_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (sender is PasswordBox passwordBox)
            {
                ViewModel.Password = passwordBox.Password;
            }
        }

        /// <summary>
        /// Xử lý sự kiện thay đổi mật khẩu xác nhận trong PasswordBox.
        /// </summary>
        /// <param name="sender">PasswordBox xác nhận mật khẩu đã thay đổi</param>
        /// <param name="e">Thông tin sự kiện</param>
        /// <remarks>
        /// Tương tự như PasswordInput_PasswordChanged, phương thức này cập nhật
        /// mật khẩu xác nhận trong ViewModel. Điều này cho phép ViewModel
        /// thực hiện validation khớp mật khẩu.
        /// </remarks>
        private void ConfirmPasswordInput_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (sender is PasswordBox passwordBox)
            {
                ViewModel.ConfirmPassword = passwordBox.Password;
            }
        }

        /// <summary>
        /// Xử lý sự kiện nhấn phím trong các control input.
        /// </summary>
        /// <param name="sender">Control đã nhận sự kiện phím</param>
        /// <param name="e">Thông tin sự kiện phím</param>
        /// <remarks>
        /// Phương thức này cho phép người dùng nhấn Enter để thực hiện đăng ký
        /// mà không cần click vào nút đăng ký. Cải thiện trải nghiệm người dùng.
        /// Chỉ thực hiện đăng ký nếu command có thể execute (form hợp lệ và mật khẩu khớp).
        /// </remarks>
        private void Input_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter)
            {
                // Thực hiện lệnh đăng ký nếu có thể
                if (ViewModel.AttemptRegisterCommand.CanExecute(null))
                {
                    ViewModel.AttemptRegisterCommand.Execute(null);
                }
                e.Handled = true;
            }
        }
    }
}