using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using MyShop.Client.ApiServer;
using MyShop.Client.Helpers;
using MyShop.Client.Views.Auth;
using MyShop.Client.Views.Dashboard;
using MyShop.Shared.DTOs.Common;
using MyShop.Shared.DTOs.Responses;
using Refit;
using System;
using System.Threading.Tasks;

namespace MyShop.Client
{
    public partial class App : Application
    {
        private readonly IHost _host;
        public new static App Current => (App)Application.Current;
        public IServiceProvider Services => _host.Services;

        public App()
        {
            this.InitializeComponent();
            _host = Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration((context, config) => {
                    config.SetBasePath(AppContext.BaseDirectory);
                    config.AddJsonFile("ApiServer/ApiConfig.json", optional: false, reloadOnChange: true);
                })
                .ConfigureServices((context, services) => {
                    services.AddTransient<AuthHeaderHandler>();

                    services.AddRefitClient<IAuthApi>()
                        .ConfigureHttpClient(client => {
                            var baseUrl = context.Configuration["BaseUrl"];
                            if (string.IsNullOrEmpty(baseUrl))
                            {
                                throw new InvalidOperationException("BaseUrl is not configured in ApiConfig.json");
                            }
                            client.BaseAddress = new Uri(baseUrl);
                        })
                        .AddHttpMessageHandler<AuthHeaderHandler>();

                    services.AddSingleton<INavigationService, NavigationService>();
                    services.AddTransient<IToastHelper, ToastHelper>();

                    // ViewModels
                    services.AddTransient<MyShop.Client.ViewModels.Auth.LoginViewModel>();
                    services.AddTransient<MyShop.Client.ViewModels.Auth.RegisterViewModel>();
                    services.AddTransient<MyShop.Client.ViewModels.Dashboard.DashboardViewModel>();
                    services.AddTransient<MyShop.Client.ViewModels.Dashboard.CustomerDashboardViewModel>();
                    services.AddTransient<MyShop.Client.ViewModels.Dashboard.SalesmanDashboardViewModel>();
                })
                .Build();
        }

        protected override async void OnLaunched(LaunchActivatedEventArgs args)
        {
            try
            {
                var window = new MainWindow();

                var navigationService = Services.GetRequiredService<INavigationService>();
                navigationService.Initialize(window.RootFrame);

                var token = CredentialHelper.GetToken();
                bool isLoggedIn = false;

                if (!string.IsNullOrEmpty(token))
                {
                    try
                    {
                        var authApi = Services.GetRequiredService<IAuthApi>();
                        var response = await authApi.GetMeAsync();

                        // Use correct MyShop.Shared.DTOs.Common.ApiResponse<UserInfoResponse> structure
                        if (response is not null && response.Success && response.Result is not null)
                        {
                            var userInfo = response.Result;
                            var loginData = new LoginResponse
                            {
                                Id = userInfo.Id,
                                Username = userInfo.Username,
                                Email = userInfo.Email,
                                Token = token,
                                RoleNames = userInfo.RoleNames,
                                CreatedAt = userInfo.CreatedAt
                            };
                            var pageType = ChooseDashboardPage(userInfo.RoleNames);
                            navigationService.NavigateTo(pageType, loginData);
                            isLoggedIn = true;
                        }
                        else
                        {
                            CredentialHelper.RemoveToken();
                        }
                    }
                    catch (ApiException ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"API Error on startup (token likely expired): {ex.StatusCode}");
                        CredentialHelper.RemoveToken();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"General Error on startup: {ex.Message}");
                        CredentialHelper.RemoveToken();
                    }
                }

                if (!isLoggedIn)
                {
                    navigationService.NavigateTo(typeof(LoginPage));
                }

                window.Activate();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CRITICAL ERROR in OnLaunched: {ex}");
                // Show error dialog
                var errorDialog = new Microsoft.UI.Xaml.Controls.ContentDialog
                {
                    Title = "Application Error",
                    Content = $"Failed to start application:\n\n{ex.Message}\n\n{ex.StackTrace}",
                    CloseButtonText = "Exit"
                };
                
                // We need a window to show the dialog, create a temporary one
                var tempWindow = new MainWindow();
                tempWindow.Activate();
                errorDialog.XamlRoot = tempWindow.Content.XamlRoot;
                await errorDialog.ShowAsync();
                
                Environment.Exit(1);
            }
        }

        private static Type ChooseDashboardPage(System.Collections.Generic.IEnumerable<string> roleNames)
        {
            // Normalize roles to upper-case for comparison
            var roles = new System.Collections.Generic.HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (roleNames != null)
            {
                foreach (var r in roleNames)
                {
                    if (!string.IsNullOrWhiteSpace(r)) roles.Add(r.Trim());
                }
            }

            if (roles.Contains("ADMIN")) return typeof(DashboardPage);
            if (roles.Contains("SALEMAN") || roles.Contains("SALESMAN")) return typeof(SalesmanDashboardPage);
            // default customer
            return typeof(CustomerDashboardPage);
        }
    }
}