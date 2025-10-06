using Microsoft.UI.Xaml.Controls;
using MyShop.Client.ViewModels;

namespace MyShop.Client.Views
{
    /// <summary>
    /// Trang Dashboard chính của ứng dụng.
    /// Code-behind cho DashboardView.xaml, hiển thị nội dung chính sau khi đăng nhập thành công.
    /// </summary>
    /// <remarks>
    /// Đây là trang đích sau khi người dùng đăng nhập hoặc đăng ký thành công.
    /// Trang này sẽ hiển thị:
    /// - Thông tin tổng quan về ứng dụng
    /// - Menu điều hướng đến các chức năng chính
    /// - Thống kê và dữ liệu quan trọng
    /// 
    /// Hiện tại chỉ hiển thị thông báo chào mừng đơn giản nhưng có thể mở rộng
    /// để thành trang dashboard đầy đủ tính năng.
    /// </remarks>
    public sealed partial class DashboardView : Page
    {
        /// <summary>
        /// Lấy instance của DashboardViewModel được sử dụng làm DataContext cho trang này.
        /// </summary>
        /// <value>Instance của DashboardViewModel từ dependency injection container</value>
        public DashboardViewModel ViewModel { get; }

        /// <summary>
        /// Khởi tạo một instance mới của class <see cref="DashboardView"/>.
        /// </summary>
        /// <remarks>
        /// Constructor này:
        /// 1. Khởi tạo components UI từ XAML
        /// 2. Lấy DashboardViewModel từ dependency injection
        /// 3. Đặt ViewModel làm DataContext cho page
        /// </remarks>
        public DashboardView()
        {
            this.InitializeComponent();
            ViewModel = App.GetService<DashboardViewModel>();
            this.DataContext = ViewModel;
        }
    }
}