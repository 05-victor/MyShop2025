using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Shared.Models;
using MyShop.Client.ViewModels.Base;
using MyShop.Client.Services;
using MyShop.Core.Interfaces.Services;
using MyShop.Core.Interfaces.Infrastructure;
using MyShop.Core.Interfaces.Repositories;
using MyShop.Client.Views.Shared;
using System;
using System.Threading.Tasks;

namespace MyShop.Client.ViewModels.Shell
{
    public partial class DashboardShellViewModel : BaseViewModel
    {
        private new readonly INavigationService _navigationService;
        private new readonly IToastService _toastHelper;
        private readonly ICredentialStorage _credentialStorage;
        private readonly ICurrentUserService _currentUserService;
        private readonly ICartRepository _cartRepository;

        // Current user information for display in PaneHeader / PaneFooter
        [ObservableProperty]
        private User? _currentUser;

        [ObservableProperty]
        private string _welcomeMessage = "Welcome back!";

        [ObservableProperty]
        private int _cartCount = 0;

        public DashboardShellViewModel(
            INavigationService navigationService,
            IToastService toastHelper,
            ICredentialStorage credentialStorage,
            ICurrentUserService currentUserService,
            ICartRepository cartRepository)
        {
            _navigationService = navigationService;
            _toastHelper = toastHelper;
            _credentialStorage = credentialStorage;
            _currentUserService = currentUserService;
            _cartRepository = cartRepository;
        }

        /// <summary>
        /// Called when Shell receives User from Login / App startup.
        /// Initializes the dashboard with user information.
        /// </summary>
        /// <param name="user">The authenticated user.</param>
        public async void Initialize(User user)
        {
            CurrentUser = user;
            WelcomeMessage = $"Welcome back, {user.Username}!";

            // Load cart count for customer role
            if (user.GetPrimaryRole().ToString() == "Customer")
            {
                await RefreshCartCountAsync();
            }
        }

        /// <summary>
        /// Refresh cart count from repository
        /// </summary>
        public async Task RefreshCartCountAsync()
        {
            if (CurrentUser == null) return;

            try
            {
                var result = await _cartRepository.GetCartCountAsync(CurrentUser.Id);
                if (result.IsSuccess)
                {
                    CartCount = result.Data;
                    System.Diagnostics.Debug.WriteLine($"[DashboardShellViewModel] Cart count updated: {CartCount}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DashboardShellViewModel] Failed to refresh cart count: {ex.Message}");
            }
        }

        // ====== Logic previously in AdminDashboardViewModel ======

        [RelayCommand]
        private async Task LogoutAsync()
        {
            // Clear cached user
            _currentUserService.ClearCurrentUser();

            // Remove token
            await _credentialStorage.RemoveToken();

            await _toastHelper.ShowInfo("You have been logged out");
            await _navigationService.NavigateTo(typeof(LoginPage).FullName!);
        }

        // Navigation commands removed - Shell handles navigation directly
        // These were causing redundant navigation logic
    }
}
