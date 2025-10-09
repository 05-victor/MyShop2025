using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;

namespace MyShop.Client.Services
{
    /// <summary>
    /// Service thực hiện điều hướng giữa các trang trong ứng dụng.
    /// </summary>
    public class NavigationService : INavigationService
    {
        private Frame? _frame;
        private readonly Dictionary<Type, Type> _pageMapping;

        /// <summary>
        /// Khởi tạo một instance mới của NavigationService.
        /// </summary>
        public NavigationService()
        {
            _pageMapping = new Dictionary<Type, Type>();
            RegisterPages();
        }

        /// <summary>
        /// Đăng ký mapping giữa ViewModel và View.
        /// </summary>
        private void RegisterPages()
        {
            // Register page mappings here
            // Example: _pageMapping[typeof(LoginViewModel)] = typeof(LoginView);
        }

        /// <summary>
        /// Khởi tạo navigation service với frame chính.
        /// </summary>
        /// <param name="frame">Frame chính của ứng dụng</param>
        public void Initialize(Frame frame)
        {
            _frame = frame ?? throw new ArgumentNullException(nameof(frame));
        }

        /// <summary>
        /// Điều hướng đến trang được chỉ định.
        /// </summary>
        /// <typeparam name="T">Loại trang cần điều hướng đến</typeparam>
        /// <param name="parameter">Tham số truyền cho trang (tùy chọn)</param>
        /// <returns>True nếu điều hướng thành công, false nếu không</returns>
        public bool NavigateTo<T>(object? parameter = null) where T : class
        {
            if (_frame == null)
            {
                throw new InvalidOperationException("NavigationService chưa được khởi tạo. Gọi Initialize() trước.");
            }

            var pageType = typeof(T);
            
            try
            {
                return _frame.Navigate(pageType, parameter);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi điều hướng: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Quay lại trang trước đó.
        /// </summary>
        /// <returns>True nếu có thể quay lại, false nếu không</returns>
        public bool GoBack()
        {
            if (_frame?.CanGoBack == true)
            {
                _frame.GoBack();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Kiểm tra xem có thể quay lại hay không.
        /// </summary>
        public bool CanGoBack => _frame?.CanGoBack ?? false;

        /// <summary>
        /// Xóa lịch sử điều hướng.
        /// </summary>
        public void ClearHistory()
        {
            if (_frame != null)
            {
                _frame.BackStack.Clear();
            }
        }
    }
}