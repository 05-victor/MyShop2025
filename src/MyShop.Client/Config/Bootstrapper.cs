using System;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using MyShop.Client.Config;
using MyShop.Client.Services;
using MyShop.Client.Strategies;
using MyShop.Client.ViewModels.Admin;
using MyShop.Client.ViewModels.Shell;
using MyShop.Core.Common;
using MyShop.Core.Interfaces.Repositories;
using MyShop.Core.Interfaces.Services;
using MyShop.Core.Interfaces.Infrastructure;
using MyShop.Plugins.API.Auth;
using MyShop.Plugins.API.Dashboard;
using MyShop.Plugins.API.Files;
using MyShop.Plugins.API.Products;
using MyShop.Plugins.API.Orders;
using MyShop.Plugins.API.Categories;
using MyShop.Plugins.API.Users;
using MyShop.Plugins.API.Profile;
using MyShop.Plugins.API.Cart;
using MyShop.Plugins.API.Reports;
using MyShop.Plugins.API.Commission;
using MyShop.Plugins.API.Settings;
using MyShop.Plugins.Repositories.Api;
using MyShop.Plugins.Repositories.Mocks;
using MyShop.Plugins.Infrastructure;
using Refit;

namespace MyShop.Client.Config
{
    /// <summary>
    /// Centralized Dependency Injection configuration.
    /// Separates DI logic from App.xaml.cs.
    /// 
    /// Storage Strategy:
    /// - Uses SecureCredentialStorage (DPAPI encrypted) for all modes
    /// - Uses FileSettingsStorage with per-user support
    /// - Removed WindowsCredentialStorage dependency
    /// </summary>
    public static class Bootstrapper
    {
        public static IHost CreateHost()
        {
            // Ensure base storage directories exist before anything else
            StorageConstants.EnsureBaseDirectoriesExist();

            return Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration((context, config) =>
                {
                    config.SetBasePath(AppContext.BaseDirectory);

                    // Hierarchical configuration with environment-specific overrides
                    var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production";
                    System.Diagnostics.Debug.WriteLine($"[Bootstrapper] Environment: {environment}");

                    // Load embedded appsettings.json from assembly resources
                    var assembly = typeof(App).Assembly;
                    using var resourceStream = assembly.GetManifestResourceStream("MyShop.Client.appsettings.json");
                    if (resourceStream == null)
                        throw new InvalidOperationException("Embedded resource 'appsettings.json' not found in assembly.");

                    // Copy to MemoryStream to keep data available after resourceStream is disposed
                    var memoryStream = new MemoryStream();
                    resourceStream.CopyTo(memoryStream);
                    memoryStream.Position = 0; // Reset to beginning for reading
                    config.AddJsonStream(memoryStream);

                    // Load environment-specific settings from file (optional, only in Debug mode)
                    config.AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true);

                    // Add user secrets in Development
                    if (environment == "Development")
                    {
                        // User secrets ID from csproj: <UserSecretsId>myshop-client-secrets</UserSecretsId>
                        config.AddUserSecrets<App>(optional: true);
                    }

                    // Environment variables override everything
                    config.AddEnvironmentVariables(prefix: "MYSHOP_");
                })
                .ConfigureServices((context, services) =>
                {
                    // ===== OPTIONS PATTERN REGISTRATION =====
                    // Register all configuration options with validation

                    // API Options
                    services.AddOptions<Options.ApiOptions>()
                        .Bind(context.Configuration.GetSection("Api"))
                        .ValidateDataAnnotations()
                        .Validate(options => options.Validate(), "ApiOptions validation failed")
                        .ValidateOnStart();

                    // Feature Flags
                    services.AddOptions<Options.FeatureFlagOptions>()
                        .Bind(context.Configuration.GetSection("FeatureFlags"))
                        .ValidateDataAnnotations()
                        .Validate(options => options.Validate(), "FeatureFlagOptions validation failed")
                        .ValidateOnStart();

                    // Logging Options
                    services.AddOptions<Options.LoggingOptions>()
                        .Bind(context.Configuration.GetSection("Logging"))
                        .ValidateDataAnnotations()
                        .Validate(options => options.Validate(), "LoggingOptions validation failed")
                        .ValidateOnStart();

                    // Storage Options
                    services.AddOptions<Options.StorageOptions>()
                        .Bind(context.Configuration.GetSection("Storage"))
                        .ValidateDataAnnotations()
                        .Validate(options => options.Validate(), "StorageOptions validation failed")
                        .ValidateOnStart();

                    // User Preferences (IOptionsSnapshot for runtime changes)
                    services.AddOptions<Options.UserPreferencesOptions>()
                        .Bind(context.Configuration.GetSection("UserPreferences"))
                        .ValidateDataAnnotations()
                        .Validate(options => options.Validate(), "UserPreferencesOptions validation failed")
                        .ValidateOnStart();

                    System.Diagnostics.Debug.WriteLine("[Bootstrapper] All Options registered with validation");

                    // Check if using Mock Data
                    var useMockData = context.Configuration.GetValue<bool>("FeatureFlags:UseMockData");
                    var enableDeveloperOptions = context.Configuration.GetValue<bool>("FeatureFlags:EnableDeveloperOptions");

                    System.Diagnostics.Debug.WriteLine($"[Bootstrapper] UseMockData={useMockData}");
                    System.Diagnostics.Debug.WriteLine($"[Bootstrapper] EnableDeveloperOptions={enableDeveloperOptions}");

                    // DEBUG: Detailed config logging
                    System.Diagnostics.Debug.WriteLine($"[Bootstrapper] ===== DEBUG CONFIG =====");
                    System.Diagnostics.Debug.WriteLine($"[Bootstrapper] Environment: {Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Not set"}");

                    var allConfigs = context.Configuration.AsEnumerable();
                    var mockDataConfigs = allConfigs.Where(kvp => kvp.Key.Contains("UseMockData", StringComparison.OrdinalIgnoreCase));
                    foreach (var config in mockDataConfigs)
                    {
                        System.Diagnostics.Debug.WriteLine($"[Bootstrapper] Found config: {config.Key} = {config.Value}");
                    }

                    // Check environment variables
                    var envMockData = Environment.GetEnvironmentVariable("MYSHOP_FeatureFlags:UseMockData");
                    if (!string.IsNullOrEmpty(envMockData))
                    {
                        System.Diagnostics.Debug.WriteLine($"[Bootstrapper] ⚠️  Environment Variable Override: MYSHOP_FeatureFlags:UseMockData = {envMockData}");
                    }

                    System.Diagnostics.Debug.WriteLine($"[Bootstrapper] Mode: {(useMockData ? "MOCK DATA" : "REAL API")}");
                    System.Diagnostics.Debug.WriteLine($"[Bootstrapper] Will register: {(useMockData ? "MockAuthRepository" : "AuthRepository")}");
                    System.Diagnostics.Debug.WriteLine($"[Bootstrapper] ====================");

                    // ===== FluentValidation for Options =====
                    services.AddSingleton<FluentValidation.IValidator<Options.ApiOptions>, Options.Validators.ApiOptionsValidator>();
                    services.AddSingleton<FluentValidation.IValidator<Options.FeatureFlagOptions>, Options.Validators.FeatureFlagOptionsValidator>();
                    services.AddSingleton<FluentValidation.IValidator<Options.LoggingOptions>, Options.Validators.LoggingOptionsValidator>();
                    services.AddSingleton<FluentValidation.IValidator<Options.StorageOptions>, Options.Validators.StorageOptionsValidator>();
                    services.AddSingleton<FluentValidation.IValidator<Options.UserPreferencesOptions>, Options.Validators.UserPreferencesOptionsValidator>();
                    System.Diagnostics.Debug.WriteLine("[Bootstrapper] FluentValidation validators registered");

                    // ===== Storage (Unified - Same for Mock and Real) =====
                    // Use SecureCredentialStorage (DPAPI encrypted) for ALL modes
                    // This is more secure than FileCredentialStorage and more flexible than WindowsCredentialStorage
                    services.AddSingleton<ICredentialStorage, SecureCredentialStorage>();
                    services.AddSingleton<ISettingsStorage, FileSettingsStorage>();

                    System.Diagnostics.Debug.WriteLine("[Bootstrapper] Using SecureCredentialStorage (DPAPI encrypted)");
                    System.Diagnostics.Debug.WriteLine("[Bootstrapper] Using FileSettingsStorage (per-user preferences)");

                    if (useMockData)
                    {
                        // ===== Mock Mode - No HTTP Clients =====
                        System.Diagnostics.Debug.WriteLine("[Bootstrapper] Using MOCK DATA mode");

                        // ===== Repositories (Mock - from Plugins) =====
                        // Changed to Transient to allow XAML root provider resolution
                        services.AddTransient<IAuthRepository, MockAuthRepository>();
                        services.AddTransient<IDashboardRepository, MockDashboardRepository>();
                        services.AddTransient<IProfileRepository, MockProfileRepository>();
                        services.AddTransient<ICategoryRepository, MockCategoryRepository>();
                        services.AddTransient<IProductRepository, MockProductRepository>();
                        services.AddTransient<IUserRepository, MockUserRepository>();
                        services.AddTransient<IOrderRepository, MockOrderRepository>();
                        services.AddTransient<ICommissionRepository, MockCommissionRepository>();
                        services.AddTransient<IReportRepository, MockReportRepository>();
                        services.AddTransient<ICartRepository, MockCartRepository>();
                        services.AddTransient<IAgentRequestRepository, MockAgentRequestsRepository>();
                        services.AddTransient<ISystemActivationRepository, MockSystemActivationRepository>();
                        services.AddTransient<IChatService, MockChatRepository>();

                        System.Diagnostics.Debug.WriteLine("[Bootstrapper] All Mock Repositories registered");
                    }
                    else
                    {
                        // ===== Real API Mode =====
                        System.Diagnostics.Debug.WriteLine("[Bootstrapper] Using REAL API mode");

                        // Get API options for Refit client configuration
                        var apiOptions = context.Configuration.GetSection("Api").Get<Options.ApiOptions>()
                            ?? throw new InvalidOperationException("API configuration is missing");

                        System.Diagnostics.Debug.WriteLine($"[Bootstrapper] API BaseUrl: {apiOptions.BaseUrl}");
                        System.Diagnostics.Debug.WriteLine($"[Bootstrapper] API Timeout: {apiOptions.RequestTimeoutSeconds}s");

                        // ===== HTTP & API Clients (from Plugins) =====
                        services.AddTransient<MyShop.Plugins.Http.Handlers.AuthHeaderHandler>();

                        // Helper action for configuring HttpClient with API options
                        void ConfigureApiClient(HttpClient client)
                        {
                            client.BaseAddress = new Uri(apiOptions.BaseUrl);
                            client.Timeout = apiOptions.Timeout;
                        }

                        // IAuthApi NOW uses AuthHeaderHandler - circular dependency solved via lazy IServiceProvider
                        // This ensures /users/me has Authorization header and benefits from token refresh
                        services.AddRefitClient<IAuthApi>()
                            .ConfigureHttpClient(ConfigureApiClient)
                            .AddHttpMessageHandler<MyShop.Plugins.Http.Handlers.AuthHeaderHandler>();

                        services.AddRefitClient<IDashboardApi>()
                            .ConfigureHttpClient(ConfigureApiClient)
                            .AddHttpMessageHandler<MyShop.Plugins.Http.Handlers.AuthHeaderHandler>();

                        services.AddRefitClient<IFilesApi>()
                            .ConfigureHttpClient(ConfigureApiClient)
                            .AddHttpMessageHandler<MyShop.Plugins.Http.Handlers.AuthHeaderHandler>();

                        services.AddRefitClient<IProductsApi>()
                            .ConfigureHttpClient(ConfigureApiClient)
                            .AddHttpMessageHandler<MyShop.Plugins.Http.Handlers.AuthHeaderHandler>();

                        services.AddRefitClient<IOrdersApi>()
                            .ConfigureHttpClient(ConfigureApiClient)
                            .AddHttpMessageHandler<MyShop.Plugins.Http.Handlers.AuthHeaderHandler>();

                        services.AddRefitClient<ICategoriesApi>()
                            .ConfigureHttpClient(ConfigureApiClient)
                            .AddHttpMessageHandler<MyShop.Plugins.Http.Handlers.AuthHeaderHandler>();

                        services.AddRefitClient<IUsersApi>()
                            .ConfigureHttpClient(ConfigureApiClient)
                            .AddHttpMessageHandler<MyShop.Plugins.Http.Handlers.AuthHeaderHandler>();

                        services.AddRefitClient<IProfileApi>()
                            .ConfigureHttpClient(ConfigureApiClient)
                            .AddHttpMessageHandler<MyShop.Plugins.Http.Handlers.AuthHeaderHandler>();

                        services.AddRefitClient<ICartApi>()
                            .ConfigureHttpClient(ConfigureApiClient)
                            .AddHttpMessageHandler<MyShop.Plugins.Http.Handlers.AuthHeaderHandler>();

                        services.AddRefitClient<IReportsApi>()
                            .ConfigureHttpClient(ConfigureApiClient)
                            .AddHttpMessageHandler<MyShop.Plugins.Http.Handlers.AuthHeaderHandler>();

                        services.AddRefitClient<ICommissionApi>()
                            .ConfigureHttpClient(ConfigureApiClient)
                            .AddHttpMessageHandler<MyShop.Plugins.Http.Handlers.AuthHeaderHandler>();

                        services.AddRefitClient<MyShop.Plugins.API.Earnings.IEarningsApi>()
                            .ConfigureHttpClient(ConfigureApiClient)
                            .AddHttpMessageHandler<MyShop.Plugins.Http.Handlers.AuthHeaderHandler>();

                        services.AddRefitClient<MyShop.Plugins.API.Users.IAgentRequestsApi>()
                            .ConfigureHttpClient(ConfigureApiClient)
                            .AddHttpMessageHandler<MyShop.Plugins.Http.Handlers.AuthHeaderHandler>();

                        services.AddRefitClient<ISettingsApi>()
                            .ConfigureHttpClient(ConfigureApiClient)
                            .AddHttpMessageHandler<MyShop.Plugins.Http.Handlers.AuthHeaderHandler>();

                        services.AddRefitClient<MyShop.Plugins.API.Forecasts.IForecastApi>()
                            .ConfigureHttpClient(ConfigureApiClient)
                            .AddHttpMessageHandler<MyShop.Plugins.Http.Handlers.AuthHeaderHandler>();

                        System.Diagnostics.Debug.WriteLine("[Bootstrapper] All Refit API clients registered");

                        // ===== Repositories (Real - from Plugins) =====
                        // Changed to Transient to allow XAML root provider resolution
                        services.AddTransient<IAuthRepository, AuthRepository>();
                        services.AddTransient<IDashboardRepository, DashboardRepository>();
                        services.AddTransient<IProductRepository, ProductRepository>();
                        services.AddTransient<IOrderRepository, OrderRepository>();
                        services.AddTransient<ICategoryRepository, CategoryRepository>();
                        services.AddTransient<IUserRepository, UserRepository>();
                        services.AddTransient<IProfileRepository, ProfileRepository>();
                        services.AddTransient<ICartRepository, CartRepository>();
                        services.AddTransient<IReportRepository, ReportRepository>();
                        services.AddTransient<ICommissionRepository, CommissionRepository>();
                        services.AddTransient<IForecastRepository, MyShop.Plugins.Repositories.Api.ForecastRepository>();
                        services.AddTransient<MyShop.Core.Interfaces.Repositories.IEarningsRepository, MyShop.Plugins.Repositories.Api.EarningsRepository>();
                        services.AddTransient<IAgentRequestRepository, MyShop.Plugins.Repositories.Api.AgentRequestRepository>();
                        services.AddTransient<ISettingsRepository, SettingsRepository>();
                        services.AddSingleton<ISystemActivationRepository, MyShop.Plugins.Repositories.Api.SystemActivationRepository>();
                        services.AddTransient<IChatService, ChatRepository>();
                    }

                    // ===== Services (from Client.Services) =====
                    services.AddSingleton<INavigationService, MyShop.Client.Services.NavigationService>();
                    services.AddTransient<MyShop.Core.Interfaces.Services.IToastService, Services.ToastService>();
                    services.AddTransient<MyShop.Core.Interfaces.Services.IDialogService, Services.DialogService>();
                    services.AddSingleton<MyShop.Core.Interfaces.Services.IValidationService, Services.ValidationService>();
                    services.AddSingleton<MyShop.Core.Interfaces.Services.IExportService, Services.ExportService>();
                    services.AddSingleton<ICurrentUserService, CurrentUserService>();
                    services.AddSingleton<Services.IChartExportService, Services.ChartExportService>();
                    services.AddSingleton<Services.IPdfExportService, Services.PdfExportService>();
                    services.AddTransient<MyShop.Core.Interfaces.Services.IAuthService, Services.AuthService>();

                    // ===== Chatbot Service =====
                    services.AddSingleton<IChatbotService, Services.ChatbotService>();
                    System.Diagnostics.Debug.WriteLine("[Bootstrapper] ChatbotService registered as Singleton");

                    // ===== Import/Export Services =====
                    services.AddSingleton<Services.ProductImportService>();
                    System.Diagnostics.Debug.WriteLine("[Bootstrapper] ProductImportService registered as Singleton");

                    // ===== Performance & Caching Services =====
                    services.AddSingleton<Services.ICacheService, Services.CacheService>();
                    services.AddSingleton<Services.IImageCacheService, Services.ImageCacheService>();
                    System.Diagnostics.Debug.WriteLine("[Bootstrapper] CacheService and ImageCacheService registered as Singletons");

                    // ===== Advanced Features Services (T18) =====
                    services.AddSingleton<Services.IActivityLogService, Services.ActivityLogService>();
                    services.AddSingleton<Services.IBatchOperationService, Services.BatchOperationService>();
                    services.AddSingleton<Services.IAppNotificationService, Services.AppNotificationService>();
                    services.AddSingleton<Services.ISavedSearchService, Services.SavedSearchService>();
                    System.Diagnostics.Debug.WriteLine("[Bootstrapper] Advanced Features services registered (ActivityLog, BatchOps, Notifications, SavedSearch)");

                    // ===== Configuration Service (Centralized Config Access) =====
                    services.AddSingleton<Services.Configuration.IConfigurationService, Services.Configuration.ConfigurationService>();
                    System.Diagnostics.Debug.WriteLine("[Bootstrapper] ConfigurationService registered as Singleton");

                    // ===== Settings Service (User preferences) =====
                    services.AddSingleton<Services.SettingsService>();
                    System.Diagnostics.Debug.WriteLine("[Bootstrapper] SettingsService registered as Singleton");

                    // ===== Pagination Service (Global runtime settings) =====
                    services.AddSingleton<MyShop.Core.Interfaces.Services.IPaginationService, PaginationService>();
                    System.Diagnostics.Debug.WriteLine("[Bootstrapper] PaginationService registered as Singleton");

                    // ===== Facades (Application Core - aggregates multiple services) =====
                    // Changed from Scoped to Transient to allow resolution from root provider (XAML constructor injection)

                    // Authentication & User Management
                    services.AddTransient<MyShop.Core.Interfaces.Facades.IAuthFacade, Facades.AuthFacade>();
                    services.AddTransient<MyShop.Core.Interfaces.Facades.IProfileFacade, Facades.ProfileFacade>();
                    services.AddTransient<MyShop.Core.Interfaces.Facades.IUserFacade, Facades.Users.UserFacade>();

                    // Product & Catalog
                    services.AddTransient<MyShop.Core.Interfaces.Facades.IProductFacade, Facades.ProductFacade>();
                    services.AddTransient<MyShop.Core.Interfaces.Facades.ICategoryFacade, Facades.Products.CategoryFacade>();
                    // Note: CategoriesFacade (old HttpClient-based) is deprecated in favor of CategoryFacade

                    // Shopping & Orders
                    services.AddTransient<MyShop.Core.Interfaces.Facades.ICartFacade, Facades.CartFacade>();
                    services.AddTransient<MyShop.Core.Interfaces.Facades.IOrderFacade, Facades.OrderFacade>();

                    // Dashboard & Reports
                    services.AddTransient<MyShop.Core.Interfaces.Facades.IDashboardFacade, Facades.DashboardFacade>();
                    services.AddTransient<MyShop.Core.Interfaces.Facades.IReportFacade, Facades.Reports.ReportFacade>();

                    // Sales Agent Management
                    services.AddTransient<MyShop.Core.Interfaces.Facades.ICommissionFacade, Facades.Reports.CommissionFacade>();
                    services.AddTransient<MyShop.Core.Interfaces.Facades.IEarningsFacade, Facades.EarningsFacade>();
                    services.AddTransient<MyShop.Core.Interfaces.Facades.IAgentRequestFacade, Facades.Users.AgentRequestFacade>();

                    System.Diagnostics.Debug.WriteLine("[Bootstrapper] All 12 Facades registered successfully");

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
                    services.AddSingleton<IRoleStrategy, SalesAgentDashboardStrategy>();
                    services.AddSingleton<IRoleStrategy, CustomerDashboardStrategy>();
                    services.AddSingleton<IRoleStrategyFactory, RoleStrategyFactory>();

                    // ===== ViewModels (Client) =====
                    services.AddTransient<ViewModels.Shared.LoginViewModel>();
                    services.AddTransient<ViewModels.Shared.RegisterViewModel>();

                    // Auth ViewModels
                    services.AddTransient<ViewModels.Auth.ForgotPasswordRequestViewModel>();
                    services.AddTransient<ViewModels.Auth.ForgotPasswordResetViewModel>();

                    // Admin ViewModels
                    services.AddTransient<ViewModels.Admin.AdminDashboardViewModel>();
                    services.AddTransient<ViewModels.Admin.AdminProductsViewModel>();
                    services.AddTransient<ViewModels.Admin.AdminUsersViewModel>();
                    services.AddTransient<ViewModels.Admin.AdminReportsViewModel>();
                    services.AddTransient<ViewModels.Admin.AdminAgentRequestsViewModel>();

                    // Customer ViewModels
                    services.AddTransient<ViewModels.Customer.CustomerDashboardViewModel>();
                    services.AddTransient<ViewModels.Customer.BecomeAgentViewModel>();

                    // SalesAgent ViewModels
                    services.AddTransient<ViewModels.SalesAgent.SalesAgentDashboardViewModel>();
                    services.AddTransient<ViewModels.SalesAgent.SalesAgentEarningsViewModel>();
                    services.AddTransient<ViewModels.SalesAgent.SalesAgentProductsViewModel>();
                    services.AddTransient<ViewModels.SalesAgent.SalesAgentReportsViewModel>();
                    services.AddTransient<ViewModels.SalesAgent.SalesAgentOrdersViewModel>();

                    // Shared ViewModels
                    services.AddTransient<ViewModels.Shared.ProductBrowseViewModel>();
                    services.AddTransient<ViewModels.Shared.CartViewModel>();
                    services.AddTransient<ViewModels.Shared.CheckoutViewModel>();
                    services.AddTransient<ViewModels.Shared.CardPaymentViewModel>();
                    services.AddTransient<ViewModels.Shared.PurchaseOrdersViewModel>();
                    services.AddTransient<ViewModels.Shared.ProfileViewModel>();
                    services.AddTransient<ViewModels.Shared.ChangePasswordViewModel>();
                    services.AddTransient<ViewModels.Shared.CategoriesViewModel>(); // New Categories management VM

                    // Shell & Settings
                    services.AddTransient<ViewModels.Shell.DashboardShellViewModel>();
                    services.AddTransient<ViewModels.Settings.SettingsViewModel>();
                    services.AddTransient<ViewModels.Settings.TrialActivationViewModel>();
                })
                .Build();
        }
    }
}
