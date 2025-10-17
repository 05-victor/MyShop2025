using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using MyShop.Client.ViewModels;
using Windows.System;

namespace MyShop.Client.Views {
    /// <summary>
    /// Trang đăng nhập của ứng dụng.
    /// Trang đăng nhập của ứng dụng với giao diện 2 cột hiện đại.
    /// Code-behind cho LoginView.xaml, xử lý tương tác UI và binding với LoginViewModel.
    /// </summary>
    /// <remarks>
    /// Trang này cung cấp giao diện đăng nhập cho người dùng với:
    /// - Layout 2 cột: Logo và form đăng nhập
    /// - Form nhập username/email và password
    /// - Các tính năng bổ sung: Remember me, Forgot password, Google login
    /// - Xử lý sự kiện keyboard (Enter để đăng nhập)
    /// - Binding với LoginViewModel để xử lý logic nghiệp vụ
    /// - Xử lý sự kiện thay đổi password (do PasswordBox không hỗ trợ binding trực tiếp)
    /// - Validation và loading states
    /// </remarks>
    public sealed partial class LoginView : Page {
        /// <summary>
        /// Lấy instance của LoginViewModel được sử dụng làm DataContext cho trang này.
        /// </summary>
        /// <value>Instance của LoginViewModel từ dependency injection container</value>
        public LoginViewModel ViewModel { get; }

        /// <summary>
        /// Khởi tạo một instance mới của class <see cref="LoginView"/>.
        /// </summary>
        /// <remarks>
        /// Constructor này:
        /// 1. Khởi tạo components UI từ XAML
        /// 2. Lấy LoginViewModel từ dependency injection
        /// 3. Đặt ViewModel làm DataContext cho page
        /// </remarks>
        public LoginView() {
            this.InitializeComponent();
            ViewModel = App.GetService<LoginViewModel>();
            this.DataContext = ViewModel;
        }

        /// <summary>
        /// Xử lý sự kiện thay đổi password trong PasswordBox.
        /// </summary>
        /// <param name="sender">PasswordBox đã thay đổi</param>
        /// <param name="e">Thông tin sự kiện</param>
        /// <remarks>
        /// Phương thức này cần thiết vì PasswordBox không hỗ trợ two-way binding
        /// trực tiếp với Password property vì lý do bảo mật.
        /// Sự kiện này đảm bảo ViewModel được cập nhật khi người dùng nhập mật khẩu.
        /// </remarks>
        private void PasswordInput_PasswordChanged(object sender, RoutedEventArgs e) {
            if (sender is PasswordBox passwordBox) {
                ViewModel.Password = passwordBox.Password;
            }
        }

        /// <summary>
        /// Xử lý sự kiện nhấn phím trong các control input.
        /// </summary>
        /// <param name="sender">Control đã nhận sự kiện phím</param>
        /// <param name="e">Thông tin sự kiện phím</param>
        /// <remarks>
        /// Phương thức này cho phép người dùng nhấn Enter để thực hiện đăng nhập
        /// mà không cần click vào nút đăng nhập. Cải thiện trải nghiệm người dùng.
        /// Chỉ thực hiện đăng nhập nếu command có thể execute (form hợp lệ).
        /// </remarks>
        private void Input_KeyDown(object sender, KeyRoutedEventArgs e) {
            if (e.Key == VirtualKey.Enter) {
                // Thực hiện lệnh đăng nhập nếu có thể
                if (ViewModel.AttemptLoginCommand.CanExecute(null)) {
                    ViewModel.AttemptLoginCommand.Execute(null);
                }
                e.Handled = true;
            }
        }
    }
}