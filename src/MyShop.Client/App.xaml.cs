using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using MyShop.Client.Config;
using MyShop.Client.Helpers;
using MyShop.Client.Views.Shared;
using System;
using System.Threading.Tasks;

// ===== NEW NAMESPACES - After Refactor =====
using MyShop.Core.Interfaces.Repositories;
using MyShop.Core.Interfaces.Services;
using MyShop.Core.Interfaces.Infrastructure;
using MyShop.Client.Strategies;
using MyShop.Client.Services;

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
            
            // Add unhandled exception handler for detailed logging
            this.UnhandledException += App_UnhandledException;
            
            _host = Bootstrapper.CreateHost();
        }

        private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
        {
            // CRITICAL: Log exception details BEFORE debugger break
            // WinRT properties can't be evaluated in debugger, must log in-process
            try
            {
                var exceptionMessage = e.Message ?? "No message";
                var exception = e.Exception;
                var exceptionType = exception?.GetType().FullName ?? "Unknown";
                var innerException = exception?.InnerException;
                
                System.Diagnostics.Debug.WriteLine("═══════════════════════════════════════════════════════");
                System.Diagnostics.Debug.WriteLine("❌ [ERROR] [App.xaml.App_UnhandledException] Unhandled exception caught by global handler");
                System.Diagnostics.Debug.WriteLine($"   Exception: {exceptionType}");
                System.Diagnostics.Debug.WriteLine($"   Message: {exceptionMessage}");
                System.Diagnostics.Debug.WriteLine($"   HRESULT: {exception?.HResult:X8}");
                
                if (exception != null)
                {
                    System.Diagnostics.Debug.WriteLine($"   Stack Trace:");
                    System.Diagnostics.Debug.WriteLine($"{exception.StackTrace}");
                }
                
                if (innerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"   Inner Exception: {innerException.GetType().FullName}");
                    System.Diagnostics.Debug.WriteLine($"   Inner Message: {innerException.Message}");
                    System.Diagnostics.Debug.WriteLine($"   Inner Stack:");
                    System.Diagnostics.Debug.WriteLine($"{innerException.StackTrace}");
                }
                
                System.Diagnostics.Debug.WriteLine("═══════════════════════════════════════════════════════");
                
                // Also log via AppLogger for file output
                AppLogger.Separator("UNHANDLED EXCEPTION");
                AppLogger.Error("Unhandled exception caught by global handler", e.Exception);
                AppLogger.Separator();
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
                AppLogger.Separator("APP LAUNCH");
                AppLogger.Custom("🚀", "APP", "Starting MyShop2025...");
                
                AppLogger.Info("Creating MainWindow...");
                MainWindow = new MainWindow();
                AppLogger.Success("MainWindow created");

                // Force Light theme app-wide at runtime
                if (MainWindow.Content is FrameworkElement root)
                {
                    AppLogger.Debug("Setting Light theme");
                    root.RequestedTheme = ElementTheme.Light;
                }

                AppLogger.Info("Initializing NavigationService...");
                var navigationService = Services.GetRequiredService<INavigationService>();
                
                // Cast to concrete type to call Initialize (WinUI-specific method)
                if (navigationService is NavigationService navService)
                {
                    navService.Initialize(MainWindow.RootFrame);
                    AppLogger.Success("NavigationService initialized with Frame");
                }
                else
                {
                    throw new InvalidOperationException("NavigationService implementation must be NavigationService class");
                }
                
                AppLogger.Success("NavigationService ready");

                AppLogger.Info("Checking for saved credentials...");
                var credentialStorage = Services.GetRequiredService<ICredentialStorage>();
                var token = credentialStorage.GetToken();
                bool isLoggedIn = false;

                if (!string.IsNullOrEmpty(token))
                {
                    AppLogger.Debug($"Token found: {token.Substring(0, Math.Min(20, token.Length))}...");
                    try
                    {
                        AppLogger.Info("Validating token...");
                        var authRepository = Services.GetRequiredService<IAuthRepository>();
                        var result = await authRepository.GetCurrentUserAsync();

                        if (result.IsSuccess && result.Data != null)
                        {
                            var user = result.Data;
                            AppLogger.Auth("Auto-login", user.Username, true);
                            AppLogger.Info($"User roles: {string.Join(", ", user.Roles)}");
                            
                            // Use strategy pattern để navigate
                            var roleStrategyFactory = Services.GetRequiredService<IRoleStrategyFactory>();
                            var primaryRole = user.GetPrimaryRole();
                            var strategy = roleStrategyFactory.GetStrategy(primaryRole);
                            var pageType = strategy.GetDashboardPageType();
                            
                            AppLogger.Navigation("Startup", pageType.Name, user);
                            navigationService.NavigateTo(pageType.FullName!, user);
                            AppLogger.Success("Dashboard loaded");
                            isLoggedIn = true;
                        }
                        else
                        {
                            AppLogger.Warning($"Token validation failed: {result.ErrorMessage}");
                            credentialStorage.RemoveToken();
                        }
                    }
                    catch (Exception ex)
                    {
                        AppLogger.Error("Auto-login failed", ex);
                        credentialStorage.RemoveToken();
                    }
                }
                else
                {
                    AppLogger.Info("No saved token found");
                }

                if (!isLoggedIn)
                {
                    AppLogger.Navigation("Startup", "LoginPage");
                    navigationService.NavigateTo(typeof(LoginPage).FullName!);
                }

                AppLogger.Info("Activating MainWindow...");
                MainWindow.Activate();
                AppLogger.Success("App startup complete!");
                AppLogger.Separator();
            }
            catch (Exception ex)
            {
                AppLogger.Separator("CRITICAL STARTUP ERROR");
                AppLogger.Error("OnLaunched failed", ex);
                AppLogger.Separator();
                
                // Show error dialog
                var errorDialog = new Microsoft.UI.Xaml.Controls.ContentDialog
                {
                    Title = "Application Error",
                    Content = $"Failed to start application:\n\n{ex.Message}\n\nCheck Output window for details.",
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