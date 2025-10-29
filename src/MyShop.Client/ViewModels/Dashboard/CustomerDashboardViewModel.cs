using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Client.Helpers;
using MyShop.Client.Models;
using MyShop.Client.ViewModels.Base;
using MyShop.Client.Views.Auth;
using System.Threading.Tasks;

namespace MyShop.Client.ViewModels.Dashboard
{
    public partial class CustomerDashboardViewModel : BaseViewModel
    {
        private readonly INavigationService _navigationService;
        private readonly IToastHelper _toastHelper;

        [ObservableProperty]
        private User? _currentUser;

        [ObservableProperty]
        private string _title = "Customer Dashboard";

        public CustomerDashboardViewModel(INavigationService navigationService, IToastHelper toastHelper)
        {
            _navigationService = navigationService;
            _toastHelper = toastHelper;
        }

        public void Initialize(User user)
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
