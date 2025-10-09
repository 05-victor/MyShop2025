using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using MyShop.Client.Services;
using MyShop.Client.ViewModels;
using System;

namespace MyShop.Client {
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application {
        private IHost? _host;

        /// <summary>
        /// Gets the current <see cref="App"/> instance in use
        /// </summary>
        public new static App Current => (App)Application.Current;

        /// <summary>
        /// Gets the <see cref="IServiceProvider"/> instance to resolve application services.
        /// </summary>
        public IServiceProvider Services => _host?.Services ?? throw new InvalidOperationException("Services not initialized");

        /// <summary>
        /// Gets a service of the specified type from the dependency injection container.
        /// </summary>
        /// <typeparam name="T">The type of service to retrieve</typeparam>
        /// <returns>The service instance</returns>
        public static T GetService<T>() where T : class
        {
            return (T)Current.Services.GetRequiredService(typeof(T));
        }

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App() {
            this.InitializeComponent();
            ConfigureServices();
        }

        /// <summary>
        /// Configure dependency injection services
        /// </summary>
        private void ConfigureServices() {
            var builder = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) => {
                    // Register HttpClient
                    services.AddHttpClient<IAuthService, AuthService>(client => {
                        client.BaseAddress = new Uri("https://localhost:7051"); // Replace with your API base URL
                        client.DefaultRequestHeaders.Add("User-Agent", "MyShop-Client");
                    });

                    // Register services
                    services.AddSingleton<INavigationService, NavigationService>();
                    
                    // Register ViewModels
                    services.AddTransient<LoginViewModel>();
                    services.AddTransient<RegisterViewModel>();
                    services.AddTransient<DashboardViewModel>();
                });

            _host = builder.Build();
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs args) {
            m_window = new MainWindow();
            m_window.Activate();
        }

        private Window? m_window;
    }
}
