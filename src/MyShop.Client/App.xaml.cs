using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using MyShop.Client.Config;
using MyShop.Client.Services;
using MyShop.Client.Services.Configuration;
using MyShop.Client.Views.Shared;
using MyShop.Core.Interfaces.Infrastructure;
using MyShop.Core.Interfaces.Services;
using System;
using System.Threading.Tasks;

// ===== NEW NAMESPACES - After Refactor =====
using MyShop.Core.Interfaces.Repositories;
using MyShop.Client.Strategies;

namespace MyShop.Client
{
    public partial class App : Application
    {
        private readonly IHost _host;
        public new static App Current => (App)Application.Current;
        public IServiceProvider Services => _host.Services;
        public static MainWindow? MainWindow { get; private set; }

        public App()
        {
            this.InitializeComponent();
            
            // Initialize Theme System FIRST (before anything else)
            ThemeManager.Initialize(ThemeManager.ThemeType.Light);
            
            // Initialize DI Host (must be done before logging)
            try
            {
                _host = Bootstrapper.CreateHost();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ FAILED TO BUILD HOST:");
                System.Diagnostics.Debug.WriteLine($"Message: {ex.Message}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Inner: {ex.InnerException.Message}");
                    if (ex.InnerException.InnerException != null)
                        System.Diagnostics.Debug.WriteLine($"Inner2: {ex.InnerException.InnerException.Message}");
                }
                throw;
            }
            
            // Initialize Logging Service with ConfigurationService
            try
            {
                var configService = Services.GetRequiredService<IConfigurationService>();
                LoggingService.Initialize(configService);
                LoggingService.Instance.Initialize();
                
                // Log configuration summary in debug mode
                if (configService.IsDevelopment)
                {
                    System.Diagnostics.Debug.WriteLine(configService.GetConfigurationSummary());
                }
            }
            catch (Exception ex)
            {
                // Fallback to Debug if logging init fails
                System.Diagnostics.Debug.WriteLine($"Failed to initialize LoggingService: {ex.Message}");
            }
            
            // Add comprehensive global exception handlers
            // Catch exceptions from non-UI threads and background tasks
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                var exception = e.ExceptionObject as Exception;
                LoggingService.Instance.Fatal("AppDomain Unhandled Exception", exception);
                System.Diagnostics.Debug.WriteLine($"[FATAL] AppDomain Exception: {exception?.Message}");
            };

            TaskScheduler.UnobservedTaskException += (s, e) =>
            {
                LoggingService.Instance.Error("Unobserved Task Exception", e.Exception);
                System.Diagnostics.Debug.WriteLine($"[ERROR] Unobserved Task: {e.Exception.Message}");
                e.SetObserved(); // Prevent app crash
            };
            
            // Initialize Global Exception Handlers
            GlobalExceptionHandler.Initialize();
            
            // Add WinUI-specific unhandled exception handler
            this.UnhandledException += App_UnhandledException;
        }
        
        /// <summary>
        /// Initialize PaginationService from saved user settings.
        /// Must be called before any ViewModels that use pagination.
        /// </summary>
        private async Task InitializePaginationServiceAsync()
        {
            try
            {
                LoggingService.Instance.Information("Initializing PaginationService from settings...");
                
                var settingsStorage = Services.GetRequiredService<ISettingsStorage>();
                var paginationService = Services.GetRequiredService<IPaginationService>();
                
                var result = await settingsStorage.GetAsync();
                if (result.IsSuccess && result.Data != null)
                {
                    var settings = result.Data;
                    // Create PaginationSettings from AppSettings.Pagination
                    var paginationSettings = settings.Pagination ?? new MyShop.Shared.Models.PaginationSettings();
                    
                    paginationService.Initialize(paginationSettings);
                    LoggingService.Instance.Information($"PaginationService initialized: Products={paginationSettings.ProductsPageSize}, Orders={paginationSettings.OrdersPageSize}");
                }
                else
                {
                    LoggingService.Instance.Warning("Settings not found, using default pagination values");
                }
            }
            catch (Exception ex)
            {
                LoggingService.Instance.Error("Failed to initialize PaginationService", ex);
                // Continue with default values - service already has defaults
            }
        }

        private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            // CRITICAL: Log exception details BEFORE debugger break
            // WinRT properties can't be evaluated in debugger, must log in-process
            try
            {
                var exceptionMessage = e.Message ?? "No message";
                var exception = e.Exception;
                
                // Log via new LoggingService
                LoggingService.Instance.Fatal("WinUI Unhandled Exception", exception);
                
                if (exception != null)
                {
                    // Log to Debug output as well for immediate visibility
                    System.Diagnostics.Debug.WriteLine("═══════════════════════════════════════════════════════");
                    System.Diagnostics.Debug.WriteLine("❌ [FATAL] WinUI Unhandled Exception");
                    System.Diagnostics.Debug.WriteLine($"   Type: {exception.GetType().FullName}");
                    System.Diagnostics.Debug.WriteLine($"   Message: {exceptionMessage}");
                    System.Diagnostics.Debug.WriteLine($"   HRESULT: 0x{exception.HResult:X8}");
                    System.Diagnostics.Debug.WriteLine("═══════════════════════════════════════════════════════");
                }
            }
            catch (Exception logEx)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to log exception: {logEx.Message}");
            }
            
            // Mark as handled to prevent app crash during debugging
            e.Handled = true;
        }

        protected override async void OnLaunched(LaunchActivatedEventArgs args)
        {
            try
            {
                LoggingService.Instance.Information("═══════════════════════════════════════════════════════");
                LoggingService.Instance.Information("MyShop2025 Client Starting...");
                LoggingService.Instance.Information("═══════════════════════════════════════════════════════");
                
                // ===== Initialize Global Pagination Service from saved settings =====
                await InitializePaginationServiceAsync();
                
                LoggingService.Instance.Information("Creating MainWindow...");
                MainWindow = new MainWindow();
                LoggingService.Instance.Information("MainWindow created successfully");

                // ===== APPLY THEME (after MainWindow is created) =====
                // Re-apply saved theme now that MainWindow.Content is available
                ThemeManager.ApplyTheme(ThemeManager.CurrentTheme);
                LoggingService.Instance.Information($"Theme applied: {ThemeManager.CurrentTheme}");

                // ===== API HEALTH CHECK (only if using real API) =====
                var configService = Services.GetRequiredService<IConfigurationService>();
                if (!configService.FeatureFlags.UseMockData)
                {
                    _ = Task.Run(async () =>
                    {
                        var healthCheck = new Services.ApiHealthCheckService(
                            configService.Api.BaseUrl, 
                            timeoutSeconds: 5
                        );
                        
                        var (isReachable, message) = await healthCheck.CheckHealthAsync();
                        healthCheck.Dispose();
                        
                        if (!isReachable)
                        {
                            // Show warning on UI thread
                            MainWindow.DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Normal, async () =>
                            {
                                // Check if XamlRoot is available before showing dialog
                                if (MainWindow.Content?.XamlRoot == null)
                                {
                                    System.Diagnostics.Debug.WriteLine("[HealthCheck] Cannot show dialog - XamlRoot not available yet");
                                    return;
                                }
                                
                                var dialog = new Microsoft.UI.Xaml.Controls.ContentDialog
                                {
                                    Title = "⚠️ API Server Offline",
                                    Content = $"{message}\n\n" +
                                             $"The application may not work correctly without the API server.\n\n" +
                                             $"Please ensure the server is running and try again.",
                                    PrimaryButtonText = "Continue Anyway",
                                    CloseButtonText = "Exit Application",
                                    DefaultButton = Microsoft.UI.Xaml.Controls.ContentDialogButton.Close,
                                    XamlRoot = MainWindow.Content.XamlRoot
                                };
                                
                                var result = await dialog.ShowAsync();
                                if (result == Microsoft.UI.Xaml.Controls.ContentDialogResult.None)
                                {
                                    // User chose to exit
                                    Application.Current.Exit();
                                }
                            });
                        }
                        else
                        {
                            LoggingService.Instance.Information($"[HealthCheck] ✓ {message}");
                        }
                    });
                }

                LoggingService.Instance.Information("Initializing NavigationService...");
                var navigationService = Services.GetRequiredService<INavigationService>();
                
                // Cast to concrete type to call Initialize (WinUI-specific method)
                if (navigationService is NavigationService navService)
                {
                    navService.Initialize(MainWindow.RootFrame);
                    LoggingService.Instance.Information("NavigationService initialized successfully");
                }
                else
                {
                    throw new InvalidOperationException("NavigationService implementation must be NavigationService class");
                }

                LoggingService.Instance.Information("Checking for saved credentials...");
                var credentialStorage = Services.GetRequiredService<ICredentialStorage>();
                var token = credentialStorage.GetToken();
                bool isLoggedIn = false;

                if (!string.IsNullOrEmpty(token))
                {
                    LoggingService.Instance.Debug($"Token found (length: {token.Length})");
                    try
                    {
                        LoggingService.Instance.Information("Validating saved token...");
                        var authRepository = Services.GetRequiredService<IAuthRepository>();
                        var result = await authRepository.GetCurrentUserAsync();

                        if (result.IsSuccess && result.Data != null)
                        {
                            var user = result.Data;
                            LoggingService.Instance.LogAuth("Auto-login", user.Username, true);
                            LoggingService.Instance.Information($"User roles: {string.Join(", ", user.Roles)}");
                            
                            // Use strategy pattern to navigate
                            var roleStrategyFactory = Services.GetRequiredService<IRoleStrategyFactory>();
                            var primaryRole = user.GetPrimaryRole();
                            var strategy = roleStrategyFactory.GetStrategy(primaryRole);
                            var pageType = strategy.GetDashboardPageType();
                            
                            LoggingService.Instance.LogNavigation("Startup", pageType.Name, user, true);
                            await navigationService.NavigateTo(pageType.FullName!, user);
                            LoggingService.Instance.Information("Dashboard loaded successfully");
                            isLoggedIn = true;
                        }
                        else
                        {
                            LoggingService.Instance.Warning($"Token validation failed: {result.ErrorMessage}");
                            await credentialStorage.RemoveToken();
                        }
                    }
                    catch (Exception ex)
                    {
                        LoggingService.Instance.Error("Auto-login failed", ex);
                        await credentialStorage.RemoveToken();
                    }
                }
                else
                {
                    LoggingService.Instance.Information("No saved token found - showing login page");
                }

                if (!isLoggedIn)
                {
                    LoggingService.Instance.LogNavigation("Startup", "LoginPage", null, true);
                    await navigationService.NavigateTo(typeof(LoginPage).FullName!);
                }

                LoggingService.Instance.Information("Activating MainWindow...");
                MainWindow.Activate();
                LoggingService.Instance.Information("═══════════════════════════════════════════════════════");
                LoggingService.Instance.Information("MyShop2025 Client Startup Complete!");
                LoggingService.Instance.Information("═══════════════════════════════════════════════════════");
            }
            catch (Exception ex)
            {
                LoggingService.Instance.Fatal("Application startup failed", ex);
                
                // Show error dialog
                var errorDialog = new Microsoft.UI.Xaml.Controls.ContentDialog
                {
                    Title = "Application Error",
                    Content = $"Failed to start application:\n\n{ex.Message}\n\nCheck log files in:\n{LoggingService.Instance.GetLogDirectory()}",
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
    }
}