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
                ViewModel.Initialize(user);
            }
        }

        private async void OnActivationSuccessful(object sender, User upgradedUser)
        {
            // Save hasAdmin flag to localStorage
            // Note: ICredentialStorage only supports tokens
            System.Diagnostics.Debug.WriteLine("[CustomerDashboardPage] Admin flag should be saved");

            // Note: Token saving would happen via backend API
            // _credentialStorage.SaveToken expects a token string, not User object
            System.Diagnostics.Debug.WriteLine("[CustomerDashboardPage] User upgraded to admin");

            // Show success message
            await _toastHelper.ShowSuccess($"🎉 Welcome, {upgradedUser.FullName}! You are now an Admin.");

            // Wait a moment for user to see the toast
            await Task.Delay(1500);

            // Navigate to AdminDashboard
            await _navigationService.NavigateTo(typeof(Shell.AdminDashboardShell).FullName!, upgradedUser);
        }

        private void OnBecomeAgentClick(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            _navigationService.NavigateTo(typeof(BecomeAgentPage).FullName!);
        }
    }
}
