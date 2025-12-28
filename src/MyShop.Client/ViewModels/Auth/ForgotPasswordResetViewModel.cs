using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI;
using Microsoft.UI.Xaml.Media;
using MyShop.Client.ViewModels.Base;
using MyShop.Core.Interfaces.Services;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MyShop.Client.ViewModels.Auth;

public partial class ForgotPasswordResetViewModel : BaseViewModel
{
    private readonly IAuthService _authService;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _token = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowChecklist), nameof(MinLengthIcon), nameof(MinLengthColor), 
                              nameof(HasNumberIcon), nameof(HasNumberColor), nameof(HasSymbolIcon), 
                              nameof(HasSymbolColor), nameof(CanResetPassword))]
    private string _newPassword = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PasswordsMatchIcon), nameof(PasswordsMatchColor), nameof(CanResetPassword))]
    private string _confirmPassword = string.Empty;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanResetPassword))]
    private bool _isLoading;

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    public bool ShowChecklist => !string.IsNullOrEmpty(NewPassword);

    // Password validation properties
    public bool MinLengthMet => NewPassword.Length >= 8;
    public bool HasNumber => !string.IsNullOrEmpty(NewPassword) && Regex.IsMatch(NewPassword, @"\d");
    public bool HasSymbol => !string.IsNullOrEmpty(NewPassword) && Regex.IsMatch(NewPassword, @"[!@#$%^&*(),.?""':{}|<>]");
    public bool PasswordsMatch => !string.IsNullOrEmpty(ConfirmPassword) && NewPassword == ConfirmPassword;

    public string MinLengthIcon => MinLengthMet ? "\uE73E" : "\uE711"; // CheckMark : Circle
    public string HasNumberIcon => HasNumber ? "\uE73E" : "\uE711";
    public string HasSymbolIcon => HasSymbol ? "\uE73E" : "\uE711";
    public string PasswordsMatchIcon => PasswordsMatch ? "\uE73E" : "\uE711";

    public SolidColorBrush MinLengthColor => MinLengthMet 
        ? new SolidColorBrush(Colors.Green) 
        : new SolidColorBrush(Colors.Gray);
    public SolidColorBrush HasNumberColor => HasNumber 
        ? new SolidColorBrush(Colors.Green) 
        : new SolidColorBrush(Colors.Gray);
    public SolidColorBrush HasSymbolColor => HasSymbol 
        ? new SolidColorBrush(Colors.Green) 
        : new SolidColorBrush(Colors.Gray);
    public SolidColorBrush PasswordsMatchColor => PasswordsMatch 
        ? new SolidColorBrush(Colors.Green) 
        : new SolidColorBrush(Colors.Gray);

    public bool CanResetPassword => 
        !IsLoading && 
        MinLengthMet && 
        HasNumber && 
        HasSymbol && 
        PasswordsMatch;

    public ForgotPasswordResetViewModel(
        IAuthService authService,
        INavigationService navigationService,
        IToastService toastService)
        : base(toastService, navigationService)
    {
        _authService = authService;
    }

    public void InitializeWithEmailAndToken(string email, string token)
    {
        Email = email;
        Token = token;
    }

    partial void OnNewPasswordChanged(string value)
    {
        ErrorMessage = string.Empty;
    }

    partial void OnConfirmPasswordChanged(string value)
    {
        ErrorMessage = string.Empty;
    }

    [RelayCommand]
    private async Task ResetPasswordAsync()
    {
        try
        {
            ErrorMessage = string.Empty;

            // Validation
            if (string.IsNullOrWhiteSpace(NewPassword))
            {
                ErrorMessage = "Password is required";
                return;
            }

            if (!MinLengthMet)
            {
                ErrorMessage = "Password must be at least 8 characters";
                return;
            }

            if (!HasNumber || !HasSymbol)
            {
                ErrorMessage = "Password is too weak";
                return;
            }

            if (!PasswordsMatch)
            {
                ErrorMessage = "Passwords do not match";
                return;
            }

            IsLoading = true;

            // Call API to reset password
            var result = await _authService.ResetPasswordAsync(Email, Token, NewPassword);

            if (result.IsSuccess)
            {
                // Navigate to success page
                _navigationService?.NavigateTo("ForgotPasswordSuccess", Email);
            }
            else
            {
                ErrorMessage = result.ErrorMessage ?? "Reset failed. Please try again.";
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ForgotPasswordResetViewModel] Error: {ex.Message}");
            ErrorMessage = "An unexpected error occurred. Please try again.";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        _navigationService?.NavigateTo("Login");
    }
}
