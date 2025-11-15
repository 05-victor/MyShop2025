using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Shared.Models;
using MyShop.Client.ViewModels.Base;
using MyShop.Client.Views.Auth;
using MyShop.Client.Helpers;
using MyShop.Core.Interfaces.Storage;
using System.Threading.Tasks;

namespace MyShop.Client.ViewModels.Dashboard
{
    public partial class SalesmanDashboardViewModel : BaseViewModel
    {
        private readonly INavigationService _navigationService;
        private readonly IToastHelper _toastHelper;
        private readonly ICredentialStorage _credentialStorage;

        [ObservableProperty]
        private User? _currentUser;

        [ObservableProperty]
        private string _title = "Salesman Dashboard";

        public SalesmanDashboardViewModel(
            INavigationService navigationService,
            IToastHelper toastHelper,
            ICredentialStorage credentialStorage)
        {
            _navigationService = navigationService;
            _toastHelper = toastHelper;
            _credentialStorage = credentialStorage;
        }

        public void Initialize(User user)
        {
            CurrentUser = user;
        }

        [RelayCommand]
        private async Task LogoutAsync()
        {
            _credentialStorage.RemoveToken();
            _toastHelper.ShowInfo("Logged out");
            _navigationService.NavigateTo(typeof(LoginPage));
            await Task.CompletedTask;
        }
    }
}
