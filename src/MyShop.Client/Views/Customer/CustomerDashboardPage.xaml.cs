using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MyShop.Shared.Models;
using MyShop.Client.ViewModels.Customer;
using MyShop.Core.Interfaces.Infrastructure;
using MyShop.Core.Interfaces.Repositories;
using MyShop.Core.Interfaces.Services;
using System.Threading.Tasks;
using System;

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
            _navigationService.NavigateTo(typeof(BecomeAgentPage).FullName!);
        }
    }
}
