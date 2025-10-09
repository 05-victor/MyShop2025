using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MyShop.Client.Services;
using MyShop.Client.Views;
using Microsoft.UI.Windowing;
using Microsoft.UI;
using WinRT.Interop;
using Microsoft.Extensions.DependencyInjection;

namespace MyShop.Client 
{
    /// <summary>
    /// Cửa sổ chính của ứng dụng MyShop Client.
    /// Chứa Frame điều hướng và cấu hình các thuộc tính cửa sổ.
    /// </summary>
    public sealed partial class MainWindow : Window 
    {
        private Frame? _rootFrame;

        /// <summary>
        /// Lấy Frame gốc của cửa sổ.
        /// </summary>
        public Frame RootFrame 
        {
            get
            {
                if (_rootFrame == null)
                {
                    _rootFrame = new Frame();
                    this.Content = _rootFrame;
                }
                return _rootFrame;
            }
        }

        /// <summary>
        /// Khởi tạo một instance mới của MainWindow.
        /// </summary>
        public MainWindow() 
        {
            this.InitializeComponent();
            ConfigureWindow();
            InitializeNavigation();
        }

        /// <summary>
        /// Khởi tạo navigation service và điều hướng đến trang đăng nhập.
        /// </summary>
        private void InitializeNavigation()
        {
            try
            {
                var app = (App)Application.Current;
                var navigationService = app.Services.GetService<INavigationService>();
                
                if (navigationService != null)
                {
                    navigationService.Initialize(RootFrame);
                    navigationService.NavigateTo<LoginView>();
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("NavigationService không được tìm thấy trong DI container");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi khởi tạo navigation: {ex.Message}");
            }
        }

        /// <summary>
        /// Cấu hình các thuộc tính của cửa sổ ứng dụng.
        /// </summary>
        private void ConfigureWindow() 
        {
            try
            {
                // Lấy AppWindow từ Window hiện tại
                var hwnd = WindowNative.GetWindowHandle(this);
                var windowId = Win32Interop.GetWindowIdFromWindow(hwnd);
                var appWindow = AppWindow.GetFromWindowId(windowId);

                if (appWindow != null)
                {
                    // Thiết lập kích thước mặc định
                    appWindow.Resize(new Windows.Graphics.SizeInt32(800, 700));

                    // Thiết lập kích thước tối thiểu (nếu hỗ trợ)
                    if (appWindow.Presenter is OverlappedPresenter presenter)
                    {
                        presenter.IsResizable = true;
                        presenter.IsMaximizable = true;
                        presenter.IsMinimizable = true;
                    }

                    // Đặt tiêu đề cửa sổ
                    this.Title = "MyShop - Ứng dụng quản lý bán hàng";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi cấu hình cửa sổ: {ex.Message}");
            }
        }
    }
}
