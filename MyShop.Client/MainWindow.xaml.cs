using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using MyShop.Client.Services;
using MyShop.Client.Views;
using Microsoft.UI.Windowing;
using Microsoft.UI;
using WinRT.Interop;

// Để tìm hiểu thêm về WinUI, cấu trúc dự án WinUI,
// và thêm về các template dự án của chúng tôi, xem: http://aka.ms/winui-project-info.

namespace MyShop.Client {
    /// <summary>
    /// Cửa sổ chính của ứng dụng MyShop Client.
    /// Chứa Frame điều hướng và cấu hình các thuộc tính cửa sổ.
    /// </summary>
    /// <remarks>
    /// MainWindow là cửa sổ gốc của ứng dụng WinUI 3, chứa:
    /// - Frame điều hướng chính để chuyển đổi giữa các trang
    /// - Cấu hình kích thước và thuộc tính cửa sổ
    /// - Khởi tạo navigation service và điều hướng đến trang đăng nhập
    /// - Thiết lập kích thước tối thiểu và mặc định cho cửa sổ
    /// </remarks>
    public sealed partial class MainWindow : Window {
        /// <summary>
        /// Khởi tạo một instance mới của class <see cref="MainWindow"/>.
        /// </summary>
        /// <remarks>
        /// Constructor này thực hiện:
        /// 1. Khởi tạo components UI từ XAML
        /// 2. Cấu hình thuộc tính cửa sổ (kích thước, resize, v.v.)
        /// 3. Khởi tạo navigation service với RootFrame
        /// 4. Điều hướng đến trang đăng nhập ban đầu
        /// </remarks>
        public MainWindow() {
            this.InitializeComponent();
            ConfigureWindow();

            // Khởi tạo Navigation Service
            var navigationService = App.GetService<INavigationService>();
            navigationService.Initialize(RootFrame);
            navigationService.NavigateTo<LoginView>();
        }

        /// <summary>
        /// Cấu hình các thuộc tính của cửa sổ ứng dụng.
        /// </summary>
        /// <remarks>
        /// Phương thức này thiết lập:
        /// - Kích thước mặc định của cửa sổ (800x700)
        /// - Kích thước tối thiểu (600x850)
        /// - Cho phép resize, maximize, minimize
        /// - Sử dụng Win32 interop để truy cập AppWindow API
        /// </remarks>
        private void ConfigureWindow() {
            // Lấy AppWindow từ Window hiện tại
            IntPtr hWnd = WindowNative.GetWindowHandle(this);
            WindowId windowId = Win32Interop.GetWindowIdFromWindow(hWnd);
            AppWindow appWindow = AppWindow.GetFromWindowId(windowId);

            if (appWindow != null) {
                // Đặt kích thước mặc định khi mở ứng dụng
                appWindow.Resize(new Windows.Graphics.SizeInt32(800, 700));

                // Cấu hình các thuộc tính của cửa sổ
                if (appWindow.Presenter is OverlappedPresenter presenter) {
                    // Cho phép thay đổi kích thước, phóng to, thu nhỏ
                    presenter.IsResizable = true;
                    presenter.IsMaximizable = true;
                    presenter.IsMinimizable = true;

                    // ĐẶT KÍCH THƯỚC TỐI THIỂU Ở ĐÂY (Cách đúng đắn)
                    presenter.PreferredMinimumWidth = 600;
                    presenter.PreferredMinimumHeight = 850;
                }
            }
        }
    }
}
