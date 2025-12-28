using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Client.ViewModels.Base;
using MyShop.Core.Interfaces.Services;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MyShop.Client.ViewModels.Auth;

public partial class ForgotPasswordRequestViewModel : BaseViewModel
{
    private readonly IAuthService _authService;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanSendCode))]
    private bool _isLoading;

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    public bool CanSendCode => !IsLoading && IsValidEmail(Email);

    public ForgotPasswordRequestViewModel(
        IAuthService authService,
        INavigationService navigationService,
        IToastService toastService)
        : base(toastService, navigationService)
    {
        _authService = authService;
    }

    partial void OnEmailChanged(string value)
    {
        ErrorMessage = string.Empty;
        OnPropertyChanged(nameof(CanSendCode));
    }

    [RelayCommand]
    private async Task SendCodeAsync()
    {
        try
        {
            ErrorMessage = string.Empty;

            // Validation
            if (string.IsNullOrWhiteSpace(Email))
            {
                ErrorMessage = "Email is required";
                return;
            }

            if (!IsValidEmail(Email))
            {
                ErrorMessage = "Invalid email format";
                return;
            }

            IsLoading = true;

            // Call API to send verification code
            var result = await _authService.SendPasswordResetCodeAsync(Email.Trim().ToLower());

            if (result.IsSuccess)
            {
                // Navigate to OTP page with email parameter
                _navigationService?.NavigateTo("ForgotPasswordOtp", Email.Trim().ToLower());
            }
            else
            {
                // Handle specific error codes
                ErrorMessage = result.ErrorMessage switch
                {
                    var msg when msg?.Contains("not found", StringComparison.OrdinalIgnoreCase) == true
                        => "This email is not registered",
                    var msg when msg?.Contains("network", StringComparison.OrdinalIgnoreCase) == true
                        => "Couldn't send code. Check your connection and try again.",
                    _ => result.ErrorMessage ?? "Failed to send verification code"
                };
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ForgotPasswordRequestViewModel] Error: {ex.Message}");
            ErrorMessage = "An unexpected error occurred. Please try again.";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void BackToLogin()
    {
        _navigationService?.NavigateTo("Login");
    }

    private static bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            // Basic email validation
            var regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase);
            return regex.IsMatch(email);
        }
        catch
        {
            return false;
        }
    }
}
