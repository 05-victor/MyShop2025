using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MyShop.Client.Config;
using MyShop.Client.Helpers;
using MyShop.Client.Strategies;
using MyShop.Client.ViewModels.Product;
using MyShop.Client.ViewModels.Shell;
// ===== NEW NAMESPACES - After Refactor =====
using MyShop.Core.Interfaces.Repositories;
using MyShop.Core.Interfaces.Services;
using MyShop.Core.Interfaces.Storage;
using MyShop.Core.Services;
using MyShop.Plugins.ApiClients.Auth;
using MyShop.Plugins.ApiClients.Dashboard;
using MyShop.Plugins.Http;
using MyShop.Plugins.Mocks.Repositories;
using MyShop.Plugins.Storage;
using Refit;

namespace MyShop.Client.Config
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
                    config.AddJsonFile("Config/ApiConfig.json", optional: false, reloadOnChange: true);
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
                        
                        // ===== Storage (Mock - Simple File Storage) =====
                        services.AddSingleton<ICredentialStorage, FileCredentialStorage>();
                        
                        // ===== Repositories (Mock - from Plugins) =====
                        services.AddScoped<IAuthRepository, MockAuthRepository>();
                        services.AddSingleton<IDashboardRepository, MockDashboardRepository>();
                        services.AddSingleton<IProfileRepository, MockProfileRepository>();
                        services.AddSingleton<ICategoryRepository, MockCategoryRepository>();
                        services.AddSingleton<IProductRepository, MockProductRepository>();
                        
                        System.Diagnostics.Debug.WriteLine("[Bootstrapper] All Mock Repositories registered");
                    }
                    else
                    {
                        // ===== Real API Mode =====
                        System.Diagnostics.Debug.WriteLine("[Bootstrapper] Using REAL API mode");

                        // ===== Storage (Production - Windows PasswordVault or File) =====
                        var useWindowsStorage = context.Configuration.GetValue<bool>("UseWindowsCredentialStorage");
                        if (useWindowsStorage)
                        {
                            services.AddSingleton<ICredentialStorage, WindowsCredentialStorage>();
                            System.Diagnostics.Debug.WriteLine("[Bootstrapper] Using Windows PasswordVault");
                        }
                        else
                        {
                            services.AddSingleton<ICredentialStorage, FileCredentialStorage>();
                            System.Diagnostics.Debug.WriteLine("[Bootstrapper] Using File Storage");
                        }

                        // ===== HTTP & API Clients (from Plugins) =====
                        services.AddTransient<AuthHeaderHandler>();

                        services.AddRefitClient<IAuthApiClient>()
                            .ConfigureHttpClient(client =>
                            {
                                client.BaseAddress = new Uri(AppConfig.Instance.ApiBaseUrl);
                                client.Timeout = TimeSpan.FromSeconds(AppConfig.Instance.RequestTimeoutSeconds);
                            })
                            .AddHttpMessageHandler<AuthHeaderHandler>();

                        services.AddRefitClient<IDashboardApiClient>()
                            .ConfigureHttpClient(client =>
                            {
                                client.BaseAddress = new Uri(AppConfig.Instance.ApiBaseUrl);
                                client.Timeout = TimeSpan.FromSeconds(AppConfig.Instance.RequestTimeoutSeconds);
                            })
                            .AddHttpMessageHandler<AuthHeaderHandler>();

                        // ===== Repositories (Real - from Plugins) =====
                        services.AddScoped<IAuthRepository, AuthRepository>();
                        services.AddScoped<IDashboardRepository, DashboardRepository>();
                    }

                    // ===== Services (from Client.Helpers + Core.Services) =====
                    services.AddSingleton<INavigationService, NavigationService>();
                    services.AddTransient<IToastHelper, ToastHelper>();
                    services.AddSingleton<MyShop.Core.Interfaces.Services.IValidationService, MyShop.Core.Services.ValidationService>();

                    // ===== Strategies (from Client.Strategies) =====
                    services.AddSingleton<IRoleStrategy, AdminDashboardStrategy>();
                    services.AddSingleton<IRoleStrategy, SalesmanDashboardStrategy>();
                    services.AddSingleton<IRoleStrategy, CustomerDashboardStrategy>();
                    services.AddSingleton<IRoleStrategyFactory, RoleStrategyFactory>();

                    // ===== ViewModels (Client) =====
                    services.AddTransient<ViewModels.Auth.LoginViewModel>();
                    services.AddTransient<ViewModels.Auth.RegisterViewModel>();
                    services.AddTransient<ViewModels.Dashboard.AdminDashboardViewModel>();
                    services.AddTransient<ViewModels.Dashboard.CustomerDashboardViewModel>();
                    services.AddTransient<ViewModels.Dashboard.SalesmanDashboardViewModel>();
                    services.AddTransient<ViewModels.Product.AdminProductViewModel>();
                    services.AddTransient<ViewModels.Shell.DashboardShellViewModel>();
                })
                .Build();
        }
    }
}
