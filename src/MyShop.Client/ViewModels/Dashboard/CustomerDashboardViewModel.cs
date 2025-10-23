using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Client.Helpers;
using MyShop.Client.Views.Auth;
using MyShop.Shared.DTOs.Responses;
using System.Threading.Tasks;

namespace MyShop.Client.ViewModels.Dashboard
{
    public partial class CustomerDashboardViewModel : ObservableObject
    {
        private readonly INavigationService _navigationService;
        private readonly IToastHelper _toastHelper;

        [ObservableProperty]
        private LoginResponse? _currentUser;

        [ObservableProperty]
        private string _title = "Customer Dashboard";

        public CustomerDashboardViewModel(INavigationService navigationService, IToastHelper toastHelper)
        {
            _navigationService = navigationService;
            _toastHelper = toastHelper;
        }

        public void Initialize(LoginResponse user)
        {
            CurrentUser = user;
        }

        [RelayCommand]
        private async Task LogoutAsync()
        {
            CredentialHelper.RemoveToken();
            _toastHelper.ShowInfo("Logged out");
            _navigationService.NavigateTo(typeof(LoginPage));
            await Task.CompletedTask;
        }
    }
}
