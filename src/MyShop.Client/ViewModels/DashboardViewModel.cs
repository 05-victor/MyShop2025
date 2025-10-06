using CommunityToolkit.Mvvm.ComponentModel;

namespace MyShop.Client.ViewModels
{
    /// <summary>
    /// ViewModel cho trang Dashboard chính của ứng dụng.
    /// Quản lý trạng thái và dữ liệu hiển thị trên trang dashboard.
    /// </summary>
    /// <remarks>
    /// ViewModel này chứa logic hiển thị trang dashboard sau khi người dùng đăng nhập thành công.
    /// Hiện tại chỉ hiển thị thông báo chào mừng, nhưng có thể mở rộng để hiển thị:
    /// - Thông tin người dùng hiện tại
    /// - Thống kê và biểu đồ
    /// - Danh sách sản phẩm gần đây
    /// - Menu điều hướng đến các chức năng khác
    /// </remarks>
    public partial class DashboardViewModel : ObservableObject
    {
        /// <summary>
        /// Lấy hoặc đặt thông báo chào mừng hiển thị trên dashboard.
        /// </summary>
        /// <value>Chuỗi thông báo chào mừng, mặc định là thông báo MyShop 2025</value>
        [ObservableProperty]
        private string _welcomeMessage = "Chào mừng đến với Dashboard MyShop 2025!";

        /// <summary>
        /// Khởi tạo một instance mới của class <see cref="DashboardViewModel"/>.
        /// </summary>
        /// <remarks>
        /// Constructor này thiết lập trạng thái ban đầu cho dashboard.
        /// Trong tương lai có thể thêm logic để load dữ liệu từ API.
        /// </remarks>
        public DashboardViewModel()
        {
            // Khởi tạo dữ liệu dashboard ở đây
        }
    }
}