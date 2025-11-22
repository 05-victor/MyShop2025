using System;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MyShop.Client.Config;
using MyShop.Client.Helpers;
using MyShop.Client.Strategies;
using MyShop.Client.ViewModels.Admin;
using MyShop.Client.ViewModels.Shell;
using MyShop.Core.Interfaces.Repositories;
using MyShop.Core.Interfaces.Services;
using MyShop.Core.Interfaces.Infrastructure;
using MyShop.Plugins.API.Auth;
using MyShop.Plugins.API.Dashboard;
using MyShop.Plugins.API.Products;
using MyShop.Plugins.API.Orders;
using MyShop.Plugins.API.Categories;
using MyShop.Plugins.API.Users;
using MyShop.Plugins.API.Profile;
using MyShop.Plugins.API.Cart;
using MyShop.Plugins.API.Reports;
using MyShop.Plugins.API.Commission;
using MyShop.Plugins.Repositories.Api;
using MyShop.Plugins.Repositories.Mocks;
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
                        services.AddSingleton<ISettingsStorage, FileSettingsStorage>();
                        
                        // ===== Repositories (Mock - from Plugins) =====
                        services.AddScoped<IAuthRepository, MockAuthRepository>();
                        services.AddSingleton<IDashboardRepository, MockDashboardRepository>();
                        services.AddSingleton<IProfileRepository, MockProfileRepository>();
                        services.AddSingleton<ICategoryRepository, MockCategoryRepository>();
                        services.AddSingleton<IProductRepository, MockProductRepository>();
                        services.AddScoped<IUserRepository, MockUserRepository>();
                        services.AddSingleton<IOrderRepository, MockOrderRepository>();
                        services.AddSingleton<ICommissionRepository, MockCommissionRepository>();
                        services.AddSingleton<IReportRepository, MockReportRepository>();
                        services.AddSingleton<ICartRepository, MockCartRepository>();
                        
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
                        services.AddTransient<MyShop.Plugins.Http.Handlers.AuthHeaderHandler>();

                        services.AddRefitClient<IAuthApi>()
                            .ConfigureHttpClient(client =>
                            {
                                client.BaseAddress = new Uri(AppConfig.Instance.ApiBaseUrl);
                                client.Timeout = TimeSpan.FromSeconds(AppConfig.Instance.RequestTimeoutSeconds);
                            })
                            .AddHttpMessageHandler<MyShop.Plugins.Http.Handlers.AuthHeaderHandler>();

                        services.AddRefitClient<IDashboardApi>()
                            .ConfigureHttpClient(client =>
                            {
                                client.BaseAddress = new Uri(AppConfig.Instance.ApiBaseUrl);
                                client.Timeout = TimeSpan.FromSeconds(AppConfig.Instance.RequestTimeoutSeconds);
                            })
                            .AddHttpMessageHandler<MyShop.Plugins.Http.Handlers.AuthHeaderHandler>();

                        services.AddRefitClient<IProductsApi>()
                            .ConfigureHttpClient(client =>
                            {
                                client.BaseAddress = new Uri(AppConfig.Instance.ApiBaseUrl);
                                client.Timeout = TimeSpan.FromSeconds(AppConfig.Instance.RequestTimeoutSeconds);
                            })
                            .AddHttpMessageHandler<MyShop.Plugins.Http.Handlers.AuthHeaderHandler>();

                        services.AddRefitClient<IOrdersApi>()
                            .ConfigureHttpClient(client =>
                            {
                                client.BaseAddress = new Uri(AppConfig.Instance.ApiBaseUrl);
                                client.Timeout = TimeSpan.FromSeconds(AppConfig.Instance.RequestTimeoutSeconds);
                            })
                            .AddHttpMessageHandler<MyShop.Plugins.Http.Handlers.AuthHeaderHandler>();

                        services.AddRefitClient<ICategoriesApi>()
                            .ConfigureHttpClient(client =>
                            {
                                client.BaseAddress = new Uri(AppConfig.Instance.ApiBaseUrl);
                                client.Timeout = TimeSpan.FromSeconds(AppConfig.Instance.RequestTimeoutSeconds);
                            })
                            .AddHttpMessageHandler<MyShop.Plugins.Http.Handlers.AuthHeaderHandler>();

                        services.AddRefitClient<IUsersApi>()
                            .ConfigureHttpClient(client =>
                            {
                                client.BaseAddress = new Uri(AppConfig.Instance.ApiBaseUrl);
                                client.Timeout = TimeSpan.FromSeconds(AppConfig.Instance.RequestTimeoutSeconds);
                            })
                            .AddHttpMessageHandler<MyShop.Plugins.Http.Handlers.AuthHeaderHandler>();

                        services.AddRefitClient<IProfileApi>()
                            .ConfigureHttpClient(client =>
                            {
                                client.BaseAddress = new Uri(AppConfig.Instance.ApiBaseUrl);
                                client.Timeout = TimeSpan.FromSeconds(AppConfig.Instance.RequestTimeoutSeconds);
                            })
                            .AddHttpMessageHandler<MyShop.Plugins.Http.Handlers.AuthHeaderHandler>();

                        services.AddRefitClient<ICartApi>()
                            .ConfigureHttpClient(client =>
                            {
                                client.BaseAddress = new Uri(AppConfig.Instance.ApiBaseUrl);
                                client.Timeout = TimeSpan.FromSeconds(AppConfig.Instance.RequestTimeoutSeconds);
                            })
                            .AddHttpMessageHandler<MyShop.Plugins.Http.Handlers.AuthHeaderHandler>();

                        services.AddRefitClient<IReportsApi>()
                            .ConfigureHttpClient(client =>
                            {
                                client.BaseAddress = new Uri(AppConfig.Instance.ApiBaseUrl);
                                client.Timeout = TimeSpan.FromSeconds(AppConfig.Instance.RequestTimeoutSeconds);
                            })
                            .AddHttpMessageHandler<MyShop.Plugins.Http.Handlers.AuthHeaderHandler>();

                        services.AddRefitClient<ICommissionApi>()
                            .ConfigureHttpClient(client =>
                            {
                                client.BaseAddress = new Uri(AppConfig.Instance.ApiBaseUrl);
                                client.Timeout = TimeSpan.FromSeconds(AppConfig.Instance.RequestTimeoutSeconds);
                            })
                            .AddHttpMessageHandler<MyShop.Plugins.Http.Handlers.AuthHeaderHandler>();

                        // ===== Storage (Production) =====
                        services.AddSingleton<ISettingsStorage, FileSettingsStorage>();

                        // ===== Repositories (Real - from Plugins) =====
                        services.AddScoped<IAuthRepository, AuthRepository>();
                        services.AddScoped<IDashboardRepository, DashboardRepository>();
                        services.AddScoped<IProductRepository, ProductRepository>();
                        services.AddScoped<IOrderRepository, OrderRepository>();
                        services.AddScoped<ICategoryRepository, CategoryRepository>();
                        services.AddScoped<IUserRepository, UserRepository>();
                        services.AddScoped<IProfileRepository, ProfileRepository>();
                        services.AddScoped<ICartRepository, CartRepository>();
                        services.AddScoped<IReportRepository, ReportRepository>();
                        services.AddScoped<ICommissionRepository, CommissionRepository>();
                    }

                    // ===== Services (from Client.Services) =====
                    services.AddSingleton<INavigationService, MyShop.Client.Services.NavigationService>();
                    services.AddTransient<MyShop.Core.Interfaces.Services.IToastService, Services.ToastService>();
                    services.AddTransient<MyShop.Core.Interfaces.Services.IDialogService, Services.DialogService>();
                    services.AddSingleton<MyShop.Core.Interfaces.Services.IValidationService, Services.ValidationService>();

                    // ===== MediatR (CQRS) =====
                    services.AddMediatR(cfg =>
                    {
                        cfg.RegisterServicesFromAssembly(typeof(App).Assembly);
                        // Add validation pipeline behavior
                        cfg.AddOpenBehavior(typeof(Common.Behaviors.ValidationBehavior<,>));
                    });

                    // ===== FluentValidation =====
                    services.AddValidatorsFromAssembly(typeof(App).Assembly);

                    // ===== Strategies (from Client.Strategies) =====
                    services.AddSingleton<IRoleStrategy, AdminDashboardStrategy>();
                    services.AddSingleton<IRoleStrategy, SalesmanDashboardStrategy>();
                    services.AddSingleton<IRoleStrategy, CustomerDashboardStrategy>();
                    services.AddSingleton<IRoleStrategyFactory, RoleStrategyFactory>();

                    // ===== ViewModels (Client) =====
                    services.AddTransient<ViewModels.Shared.LoginViewModel>();
                    services.AddTransient<ViewModels.Shared.RegisterViewModel>();
                    
                    // Admin ViewModels
                    services.AddTransient<ViewModels.Admin.AdminDashboardViewModel>();
                    services.AddTransient<ViewModels.Admin.AdminProductViewModel>();
                    services.AddTransient<ViewModels.Admin.AdminUsersViewModel>();
                    services.AddTransient<ViewModels.Admin.ReportsPageViewModel>();
                    services.AddTransient<ViewModels.Admin.AdminAgentRequestsViewModel>();
                    
                    // Customer ViewModels
                    services.AddTransient<ViewModels.Customer.CustomerDashboardViewModel>();
                    
                    // SalesAgent ViewModels
                    services.AddTransient<ViewModels.SalesAgent.SalesmanDashboardViewModel>();
                    services.AddTransient<ViewModels.SalesAgent.EarningsViewModel>();
                    services.AddTransient<ViewModels.SalesAgent.SalesAgentProductsViewModel>();
                    services.AddTransient<ViewModels.SalesAgent.SalesAgentReportsViewModel>();
                    services.AddTransient<ViewModels.SalesAgent.SalesOrdersViewModel>();
                    
                    // Shared ViewModels
                    services.AddTransient<ViewModels.Shared.ProductBrowseViewModel>();
                    services.AddTransient<ViewModels.Shared.CartViewModel>();
                    services.AddTransient<ViewModels.Shared.CheckoutViewModel>();
                    services.AddTransient<ViewModels.Shared.PurchaseOrdersViewModel>();
                    services.AddTransient<ViewModels.Shared.ProfileViewModel>();
                    services.AddTransient<ViewModels.Shared.ChangePasswordViewModel>();
                    
                    // Shell & Settings
                    services.AddTransient<ViewModels.Shell.DashboardShellViewModel>();
                    services.AddTransient<ViewModels.Settings.SettingsViewModel>();
                    services.AddTransient<ViewModels.Settings.TrialActivationViewModel>();
                })
                .Build();
        }
    }
}
