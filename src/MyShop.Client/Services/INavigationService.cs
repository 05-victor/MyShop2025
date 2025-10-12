using Microsoft.UI.Xaml.Controls;

namespace MyShop.Client.Services
{
    /// <summary>
    /// Interface định nghĩa các phương thức điều hướng trong ứng dụng.
    /// </summary>
    public interface INavigationService
    {
        /// <summary>
        /// Khởi tạo navigation service với frame chính.
        /// </summary>
        /// <param name="frame">Frame chính của ứng dụng</param>
        void Initialize(Frame frame);

        /// <summary>
        /// Điều hướng đến trang được chỉ định.
        /// </summary>
        /// <typeparam name="T">Loại trang cần điều hướng đến</typeparam>
        /// <param name="parameter">Tham số truyền cho trang (tùy chọn)</param>
        /// <returns>True nếu điều hướng thành công, false nếu không</returns>
        bool NavigateTo<T>(object? parameter = null) where T : class;

        /// <summary>
        /// Quay lại trang trước đó.
        /// </summary>
        /// <returns>True nếu có thể quay lại, false nếu không</returns>
        bool GoBack();

        /// <summary>
        /// Kiểm tra xem có thể quay lại hay không.
        /// </summary>
        bool CanGoBack { get; }

        /// <summary>
        /// Xóa lịch sử điều hướng.
        /// </summary>
        void ClearHistory();
    }
}