using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Shared.Models;
using MyShop.Client.ViewModels.Base;
using MyShop.Core.Interfaces.Services;
using MyShop.Core.Interfaces.Infrastructure;
using MyShop.Client.Views.Shared;
using System;

namespace MyShop.Client.ViewModels.Shell
{
    public partial class DashboardShellViewModel : BaseViewModel
    {
        private readonly INavigationService _navigationService;
        private readonly IToastService _toastHelper;
        private readonly ICredentialStorage _credentialStorage;

        // Thông tin user hiện tại để hiển thị ở PaneHeader / PaneFooter
        [ObservableProperty]
        private User? _currentUser;

        [ObservableProperty]
        private string _welcomeMessage = "Welcome back!";

        public DashboardShellViewModel(
            INavigationService navigationService,
            IToastService toastHelper,
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
            _navigationService.NavigateTo(typeof(LoginPage).FullName!);
        }

        // Navigation commands removed - Shell handles navigation directly
        // These were causing redundant navigation logic
    }
}
