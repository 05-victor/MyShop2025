using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MyShop.Client.Helpers;
using MyShop.Client.Views;
using WinRT.Interop;
using Microsoft.UI.Windowing;

namespace MyShop.Client {
    public sealed partial class MainWindow : Window {
        public MainWindow() {
            this.InitializeComponent();
            ConfigureWindow();

            // RootFrame is already defined in XAML with x:Name="RootFrame"
            // No need to create it again here

            // Use Activated event instead of Content.Loaded
            this.Activated += MainWindow_Activated;

            // Logic điều hướng ban đầu đã được chuyển sang App.xaml.cs
        }

        private void MainWindow_Activated(object sender, WindowActivatedEventArgs args) {
            // Only initialize ToastHelper once
            if (args.WindowActivationState != WindowActivationState.Deactivated) {
                var toastHelper = App.Current.Services.GetRequiredService<IToastHelper>();
                if (this.Content?.XamlRoot != null) {
                    toastHelper.Initialize(this.Content.XamlRoot);
                }
                
                // Unsubscribe to avoid multiple initializations
                this.Activated -= MainWindow_Activated;
            }
        }

        private void ConfigureWindow() {
            // Cấu hình cửa sổ (kích thước, tiêu đề...)
            var hwnd = WindowNative.GetWindowHandle(this);
            var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hwnd);
            var appWindow = AppWindow.GetFromWindowId(windowId);

            this.Title = "MyShop2025 Sales Management";
            if (appWindow.Presenter is OverlappedPresenter presenter) {
                presenter.IsResizable = true;
                presenter.IsMaximizable = true;
                appWindow.Resize(new Windows.Graphics.SizeInt32(1200, 900));
            }
        }
    }
}