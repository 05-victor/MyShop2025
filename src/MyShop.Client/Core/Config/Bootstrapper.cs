using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MyShop.Client.ApiServer;
using Refit;
using MyShop.Client.Core.Config;
using MyShop.Client.Core.Repositories.Implementations;
using MyShop.Client.Core.Repositories.Interfaces;
using MyShop.Client.Core.Services.Implementations;
using MyShop.Client.Core.Services.Interfaces;
using MyShop.Client.Core.Strategies;
using MyShop.Client.Helpers;
using System;

namespace MyShop.Client.Core.Config
{
    /// <summary>
    /// Centralized Dependency Injection configuration
    /// Tách biệt DI logic khỏi App.xaml.cs
    /// </summary>
    public static class Bootstrapper
    {
        public static IHost CreateHost()
        {
            return Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration((context, config) =>
                {
                    config.SetBasePath(AppContext.BaseDirectory);
                    config.AddJsonFile("ApiServer/ApiConfig.json", optional: false, reloadOnChange: true);
                })
                .ConfigureServices((context, services) =>
                {
                    // Load configuration vào AppConfig singleton
                    AppConfig.Instance.LoadFromConfiguration(context.Configuration);

                    // Check if using Mock Data
                    var useMockData = context.Configuration.GetValue<bool>("UseMockData");
                    System.Diagnostics.Debug.WriteLine($"[Bootstrapper] UseMockData={useMockData}");

                    if (useMockData)
                    {
                        // ===== Mock Mode - No HTTP Clients =====
                        System.Diagnostics.Debug.WriteLine("[Bootstrapper] Using MOCK DATA mode");
                        
                        // ===== Repositories (Mock) =====
                        services.AddScoped<IAuthRepository, MockAuthRepository>();
                    }
                    else
                    {
                        // ===== Real API Mode =====
                        System.Diagnostics.Debug.WriteLine("[Bootstrapper] Using REAL API mode");

                        // ===== HTTP & API Clients =====
                        services.AddTransient<AuthHeaderHandler>();

                        services.AddRefitClient<IAuthApi>()
                            .ConfigureHttpClient(client =>
                            {
                                client.BaseAddress = new Uri(AppConfig.Instance.ApiBaseUrl);
                                client.Timeout = TimeSpan.FromSeconds(AppConfig.Instance.RequestTimeoutSeconds);
                            })
                            .AddHttpMessageHandler<AuthHeaderHandler>();

                        // ===== Repositories (Real) =====
                        services.AddScoped<IAuthRepository, AuthRepository>();
                    }

                    // ===== Services =====
                    services.AddSingleton<INavigationService, NavigationService>();
                    services.AddTransient<IToastHelper, ToastHelper>();

                    // ===== Strategies =====
                    services.AddSingleton<IRoleStrategy, AdminDashboardStrategy>();
                    services.AddSingleton<IRoleStrategy, SalesmanDashboardStrategy>();
                    services.AddSingleton<IRoleStrategy, CustomerDashboardStrategy>();
                    services.AddSingleton<IRoleStrategyFactory, RoleStrategyFactory>();

                    // ===== ViewModels =====
                    services.AddTransient<ViewModels.Auth.LoginViewModel>();
                    services.AddTransient<ViewModels.Auth.RegisterViewModel>();
                    services.AddTransient<ViewModels.Dashboard.AdminDashboardViewModel>();
                    services.AddTransient<ViewModels.Dashboard.CustomerDashboardViewModel>();
                    services.AddTransient<ViewModels.Dashboard.SalesmanDashboardViewModel>();
                })
                .Build();
        }
    }
}
