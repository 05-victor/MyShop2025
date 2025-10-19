using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using MyShop.Client.ApiServer;
using MyShop.Client.Helpers;
using MyShop.Client.ViewModels;
using MyShop.Client.Views;
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

                    services.AddSingleton<LoginViewModel>();
                    services.AddSingleton<RegisterViewModel>();
                    services.AddSingleton<DashboardViewModel>();
                })
                .Build();
        }

        protected override async void OnLaunched(LaunchActivatedEventArgs args)
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
                        navigationService.NavigateTo(typeof(DashboardView), loginData);
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
                navigationService.NavigateTo(typeof(LoginView));
            }

            window.Activate();
        }
    }
}