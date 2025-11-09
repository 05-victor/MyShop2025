using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using MyShop.Client.Config;
using MyShop.Client.Helpers;
using MyShop.Client.Views.Auth;
using System;
using System.Threading.Tasks;

// ===== NEW NAMESPACES - After Refactor =====
using MyShop.Core.Interfaces.Repositories;
using MyShop.Core.Interfaces.Storage;
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
            _host = Bootstrapper.CreateHost();
        }

        protected override async void OnLaunched(LaunchActivatedEventArgs args)
        {
            try
            {
                MainWindow = new MainWindow();

                // Force Light theme app-wide at runtime
                if (MainWindow.Content is FrameworkElement root)
                {
                    root.RequestedTheme = ElementTheme.Light;
                }

                var navigationService = Services.GetRequiredService<INavigationService>();
                navigationService.Initialize(MainWindow.RootFrame);

                var credentialStorage = Services.GetRequiredService<ICredentialStorage>();
                var token = credentialStorage.GetToken();
                bool isLoggedIn = false;

                if (!string.IsNullOrEmpty(token))
                {
                    try
                    {
                        var authRepository = Services.GetRequiredService<IAuthRepository>();
                        var result = await authRepository.GetCurrentUserAsync();

                        if (result.IsSuccess && result.Data != null)
                        {
                            var user = result.Data;
                            
                            // Use strategy pattern để navigate
                            var roleStrategyFactory = Services.GetRequiredService<IRoleStrategyFactory>();
                            var primaryRole = user.GetPrimaryRole();
                            var strategy = roleStrategyFactory.GetStrategy(primaryRole);
                            var pageType = strategy.GetDashboardPageType();
                            
                            navigationService.NavigateTo(pageType, user);
                            isLoggedIn = true;
                        }
                        else
                        {
                            credentialStorage.RemoveToken();
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error on startup: {ex.Message}");
                        credentialStorage.RemoveToken();
                    }
                }

                if (!isLoggedIn)
                {
                    navigationService.NavigateTo(typeof(LoginPage));
                }

                MainWindow.Activate();
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
    }
}