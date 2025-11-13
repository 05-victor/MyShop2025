using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Shared.Models;
using MyShop.Client.ViewModels.Base;
using MyShop.Client.Helpers;
using MyShop.Core.Interfaces.Storage;
using MyShop.Core.Interfaces.Services;
using MyShop.Client.Views.Auth;
using System;

namespace MyShop.Client.ViewModels.Shell
{
    public partial class DashboardShellViewModel : BaseViewModel
    {
        private readonly INavigationService _navigationService;
        private readonly IToastHelper _toastHelper;
        private readonly ICredentialStorage _credentialStorage;

        // Thông tin user hiện tại để hiển thị ở PaneHeader / PaneFooter
        [ObservableProperty]
        private User? _currentUser;

        [ObservableProperty]
        private string _welcomeMessage = "Welcome back!";

        public DashboardShellViewModel(
            INavigationService navigationService,
            IToastHelper toastHelper,
            ICredentialStorage credentialStorage)
        {
            _navigationService = navigationService;
            _toastHelper = toastHelper;
            _credentialStorage = credentialStorage;
        }

        /// <summary>
        /// Được gọi khi Shell nhận được User từ Login / App startup
        /// </summary>
        public void Initialize(User user)
        {
            CurrentUser = user;
            WelcomeMessage = $"Welcome back, {user.Username}!";
        }

        // ====== Logic trước đây nằm trong AdminDashboardViewModel ======

        [RelayCommand]
        private void Logout()
        {
            _credentialStorage.RemoveToken();
            _toastHelper.ShowInfo("You have been logged out");
            _navigationService.NavigateTo(typeof(LoginPage));
        }

        [RelayCommand]
        private void NavigateToProducts()
        {
            _toastHelper.ShowInfo("Products management coming soon!");
            // Sau này có thể đổi thành: _navigationService.NavigateTo(typeof(ProductsPage));
        }

        [RelayCommand]
        private void NavigateToOrders()
        {
            _toastHelper.ShowInfo("Orders management coming soon!");
        }

        [RelayCommand]
        private void NavigateToReports()
        {
            _toastHelper.ShowInfo("Reports coming soon!");
        }

        [RelayCommand]
        private void NavigateToSettings()
        {
            _toastHelper.ShowInfo("Settings coming soon!");
        }

        [RelayCommand]
        private void NavigateToProfile()
        {
            _toastHelper.ShowInfo("Profile page coming soon!");
        }

        [RelayCommand]
        private void NavigateToUsers()
        {
            _toastHelper.ShowInfo("Users page coming soon!");
        }

        [RelayCommand]
        private void NavigateToAgentRequests()
        {
            _toastHelper.ShowInfo("Agent Requests page coming soon!");
        }
    }
}
