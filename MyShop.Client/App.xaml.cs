using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using MyShop.Client.Services;
using MyShop.Client.ViewModels;
using System.Net.Http;

namespace MyShop.Client {
    /// <summary>
    /// Cung cấp hành vi cụ thể cho ứng dụng để bổ sung cho class Application mặc định.
    /// </summary>
    public partial class App : Application {
        private Window? _window;
        public static IHost AppHost { get; private set; } = null!;

        /// <summary>
        /// Khởi tạo đối tượng ứng dụng singleton. Đây là dòng code đầu tiên được thực thi,
        /// và do đó là tương đương logic của main() hoặc WinMain().
        /// </summary>
        public App() {
            InitializeComponent();

            // TẠO HANDLER ĐỂ BỎ QUA VALIDATION CHỨNG CHỈ SSL (CHỈ DÀNH CHO PHÁT TRIỂN!)
            var handler = new HttpClientHandler {
                // Bỏ qua validation chứng chỉ server cho các chứng chỉ tự ký
                ServerCertificateCustomValidationCallback =
                    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };

            AppHost = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) => {
                    // Đăng ký HttpClient cho API calls với handler bỏ qua SSL
                    services.AddHttpClient<IAuthService, AuthService>(client => {
                        client.BaseAddress = new Uri("https://localhost:7120/");
                    })
                    // Thêm cấu hình handler vào DI
                    .ConfigurePrimaryHttpMessageHandler(() => handler);

                    // Đăng ký ViewModels
                    services.AddTransient<LoginViewModel>();
                    services.AddTransient<RegisterViewModel>();
                    services.AddTransient<DashboardViewModel>();

                    // Đăng ký Navigation Service
                    services.AddSingleton<INavigationService, NavigationService>();
                })
                .Build();

            AppHost.Start();
        }

        /// <summary>
        /// Được gọi khi ứng dụng được khởi chạy.
        /// </summary>
        /// <param name="args">Chi tiết về yêu cầu khởi chạy và quá trình.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args) {
            _window = new MainWindow();
            _window.Activate();
        }

        public static T GetService<T>() where T : class {
            return AppHost.Services.GetRequiredService<T>();
        }
    }
}
