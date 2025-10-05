using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using MyShop.Client.Services;
using MyShop.Client.ViewModels;
using System.Net.Http;

namespace MyShop.Client {
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application {
        private Window? _window;
        public static IHost AppHost { get; private set; } = null!;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App() {
            InitializeComponent();

            // CREATE HANDLER TO BYPASS SSL CERTIFICATE VALIDATION (DEVELOPMENT ONLY!)
            var handler = new HttpClientHandler {
                // Bypass server certificate validation for self-signed certificates
                ServerCertificateCustomValidationCallback =
                    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };

            AppHost = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) => {
                    // Register HttpClient for API calls with SSL bypass handler
                    services.AddHttpClient<IAuthService, AuthService>(client => {
                        client.BaseAddress = new Uri("https://localhost:7120/");
                    })
                    // Add the handler configuration to DI
                    .ConfigurePrimaryHttpMessageHandler(() => handler);

                    // Register ViewModels
                    services.AddTransient<LoginViewModel>();
                    services.AddTransient<RegisterViewModel>();
                    services.AddTransient<DashboardViewModel>();

                    // Register Navigation Service
                    services.AddSingleton<INavigationService, NavigationService>();
                })
                .Build();

            AppHost.Start();
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args) {
            _window = new MainWindow();
            _window.Activate();
        }

        public static T GetService<T>() where T : class {
            return AppHost.Services.GetRequiredService<T>();
        }
    }
}
