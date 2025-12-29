using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MyShop.Shared.Models;
using MyShop.Client.ViewModels.Customer;
using MyShop.Core.Interfaces.Infrastructure;
using MyShop.Core.Interfaces.Repositories;
using MyShop.Core.Interfaces.Services;
using System.Threading.Tasks;
using System;
using Windows.UI.Xaml;

namespace MyShop.Client.Views.Customer
{
    public sealed partial class CustomerDashboardPage : Page
    {
        public CustomerDashboardViewModel ViewModel { get; }
        private readonly INavigationService _navigationService;
        private readonly IToastService _toastHelper;
        private readonly ICredentialStorage _credentialStorage;
        private readonly IAuthRepository _authRepository;
        private User? _currentUser;

        public CustomerDashboardPage()
        {
            this.InitializeComponent();
            ViewModel = App.Current.Services.GetRequiredService<CustomerDashboardViewModel>();
            _navigationService = App.Current.Services.GetRequiredService<INavigationService>();
            _toastHelper = App.Current.Services.GetRequiredService<IToastService>();
            _credentialStorage = App.Current.Services.GetRequiredService<ICredentialStorage>();
            _authRepository = App.Current.Services.GetRequiredService<IAuthRepository>();
            this.DataContext = ViewModel;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is User user)
            {
                _currentUser = user;
                ViewModel.Initialize(user);

                // Set current user ID for BecomeAdminBanner
                if (this.FindName("BecomeAdminBannerControl") is Components.BecomeAdminBanner banner)
                {
                    banner.CurrentUserId = user.Id;
                }
            }
        }

        private async void OnActivationSuccessful(object sender, LicenseInfo license)
        {
            System.Diagnostics.Debug.WriteLine($"[CustomerDashboardPage] Activation successful: Type={license.Type}");

            // Show success message
            string message;
            if (license.IsPermanent)
            {
                message = "🎉 You are now a permanent Admin!";
            }
            else
            {
                message = $"🎉 You are now an Admin with {license.RemainingDays} days trial!";
            }
            await _toastHelper.ShowSuccess(message);

            // Reload current user to get updated role
            if (_currentUser != null)
            {
                // Navigate to AdminDashboard with updated user info
                // Note: User role has been updated in the backend, need to refresh
                await _navigationService.NavigateTo(typeof(Shell.AdminDashboardShell).FullName!, _currentUser);
            }
        }

        private void OnBecomeAgentClick(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("[CustomerDashboardPage] OnBecomeAgentClick called");
            var shell = FindParentShell();
            if (shell != null)
            {
                System.Diagnostics.Debug.WriteLine("[CustomerDashboardPage] Shell found, calling NavigateToBecomeAgent()");
                shell.NavigateToBecomeAgent();
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("[CustomerDashboardPage] ERROR: Shell not found!");
            }
        }

        private void OnShopNowClick(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("[CustomerDashboardPage] OnShopNowClick called");
            var shell = FindParentShell();
            if (shell != null)
            {
                System.Diagnostics.Debug.WriteLine("[CustomerDashboardPage] Shell found, calling NavigateToShopping()");
                shell.NavigateToShopping();
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("[CustomerDashboardPage] ERROR: Shell not found!");
            }
        }

        private void ViewAllFeaturedProducts_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("[CustomerDashboardPage] ViewAllFeaturedProducts_Click called");
            var shell = FindParentShell();
            if (shell != null)
            {
                System.Diagnostics.Debug.WriteLine("[CustomerDashboardPage] Shell found, calling NavigateToShopping()");
                shell.NavigateToShopping();
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("[CustomerDashboardPage] ERROR: Shell not found!");
            }
        }

        private void ViewAllRecommendedProducts_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("[CustomerDashboardPage] ViewAllRecommendedProducts_Click called");
            var shell = FindParentShell();
            if (shell != null)
            {
                System.Diagnostics.Debug.WriteLine("[CustomerDashboardPage] Shell found, calling NavigateToShopping()");
                shell.NavigateToShopping();
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("[CustomerDashboardPage] ERROR: Shell not found!");
            }
        }

        private async void ResendVerificationEmail_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            try
            {
                var toastService = App.Current.Services.GetRequiredService<IToastService>();
                var authRepository = App.Current.Services.GetRequiredService<IAuthRepository>();
                var button = sender as Button;

                if (button != null)
                    button.IsEnabled = false;

                // Call API to send verification email
                System.Diagnostics.Debug.WriteLine("[CustomerDashboardPage] Sending verification email...");
                var result = await authRepository.SendVerificationEmailAsync(string.Empty);

                if (result.IsSuccess)
                {
                    toastService.ShowSuccess("Verification email sent! Check your inbox for the link.");
                    System.Diagnostics.Debug.WriteLine("[CustomerDashboardPage] ✅ Verification email sent successfully");

                    // Show 60-second cooldown
                    if (button != null)
                    {
                        var countdown = 60;
                        var timer = new DispatcherTimer();
                        timer.Interval = TimeSpan.FromSeconds(1);
                        timer.Tick += (s, args) =>
                        {
                            countdown--;
                            button.Content = countdown > 0 ? $"Resend in {countdown}s" : "Resend Email";

                            if (countdown <= 0)
                            {
                                timer.Stop();
                                button.IsEnabled = true;
                            }
                        };
                        timer.Start();
                    }
                }
                else
                {
                    toastService.ShowError($"Failed to send verification email: {result.ErrorMessage}");
                    System.Diagnostics.Debug.WriteLine($"[CustomerDashboardPage] ❌ Error: {result.ErrorMessage}");

                    if (button != null)
                        button.IsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                Services.LoggingService.Instance.Error("[CustomerDashboardPage] ResendVerificationEmail_Click failed", ex);
                var toastService = App.Current.Services.GetRequiredService<IToastService>();
                toastService.ShowError("An error occurred while sending verification email");

                if (sender is Button btn)
                    btn.IsEnabled = true;
            }
        }

        private Shell.CustomerDashboardShell? FindParentShell()
        {
            DependencyObject current = this;
            while (current != null)
            {
                if (current is Shell.CustomerDashboardShell shell)
                {
                    return shell;
                }
                current = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetParent(current);
            }
            return null;
        }
    }
}
