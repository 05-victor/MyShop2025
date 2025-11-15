using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MyShop.Client.Helpers;
using MyShop.Client.Views;
using WinRT.Interop;
using Microsoft.UI.Windowing;
using System;
using System.Runtime.InteropServices;

namespace MyShop.Client {
    public sealed partial class MainWindow : Window {
        private const int MIN_WIDTH = 1400;
        private const int MIN_HEIGHT = 850;

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
                if (this.Content?.XamlRoot != null && toastHelper is ToastHelper concreteToastHelper) {
                    concreteToastHelper.Initialize(this.Content.XamlRoot);
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
                
                // Set initial window size
                appWindow.Resize(new Windows.Graphics.SizeInt32(1400, 850));
            }

            // Subclass window to enforce minimum size
            SubclassWindow(hwnd);
        }

        #region P/Invoke for Minimum Window Size

        private const int WM_GETMINMAXINFO = 0x0024;

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT {
            public int X;
            public int Y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MINMAXINFO {
            public POINT ptReserved;
            public POINT ptMaxSize;
            public POINT ptMaxPosition;
            public POINT ptMinTrackSize;
            public POINT ptMaxTrackSize;
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        private const int GWL_WNDPROC = -4;
        private IntPtr _oldWndProc = IntPtr.Zero;
        private WndProcDelegate? _newWndProcDelegate; // Keep reference to prevent GC

        private void SubclassWindow(IntPtr hwnd) {
            _newWndProcDelegate = new WndProcDelegate(NewWindowProc);
            var newWndProc = Marshal.GetFunctionPointerForDelegate(_newWndProcDelegate);
            _oldWndProc = SetWindowLongPtr(hwnd, GWL_WNDPROC, newWndProc);
        }

        private delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        private IntPtr NewWindowProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam) {
            switch (msg) {
                case WM_GETMINMAXINFO:
                    var minMaxInfo = Marshal.PtrToStructure<MINMAXINFO>(lParam);
                    minMaxInfo.ptMinTrackSize.X = MIN_WIDTH;
                    minMaxInfo.ptMinTrackSize.Y = MIN_HEIGHT;
                    Marshal.StructureToPtr(minMaxInfo, lParam, true);
                    break;
            }

            return CallWindowProc(_oldWndProc, hWnd, msg, wParam, lParam);
        }

        #endregion
    }
}