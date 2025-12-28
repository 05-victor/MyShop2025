using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Client.ViewModels.Base;
using MyShop.Core.Interfaces.Services;

namespace MyShop.Client.ViewModels.Auth;

public partial class ForgotPasswordSuccessViewModel : BaseViewModel
{
    [ObservableProperty]
    private string _email = string.Empty;

    public ForgotPasswordSuccessViewModel(
        INavigationService navigationService,
        IToastService toastService)
        : base(toastService, navigationService)
    {
    }

    public void InitializeWithEmail(string email)
    {
        Email = email;
    }

    [RelayCommand]
    private void BackToLogin()
    {
        // Navigate to login page with email prefilled
        _navigationService?.NavigateTo("Login", Email);
    }
}
