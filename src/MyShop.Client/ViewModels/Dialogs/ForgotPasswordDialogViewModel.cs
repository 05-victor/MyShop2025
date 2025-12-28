using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Core.Interfaces.Services;
using MyShop.Shared.DTOs.Requests;
using Refit;
using System;
using System.Threading.Tasks;

namespace MyShop.Client.Views.Dialogs;

public partial class ForgotPasswordDialogViewModel : ObservableObject
{
    private readonly IToastService _toastService;
    private readonly IValidationService _validationService;
    private readonly MyShop.Plugins.API.PasswordReset.IPasswordResetApi _passwordResetApi;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _resetCode = string.Empty;

    [ObservableProperty]
    private string _newPassword = string.Empty;

    [ObservableProperty]
    private string _confirmPassword = string.Empty;

    [ObservableProperty]
    private bool _isLoading = false;

    [ObservableProperty]
    private bool _hasError = false;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _isSuccess = false;

    [ObservableProperty]
    private bool _showEmailStep = true;

    [ObservableProperty]
    private bool _showResetStep = false;

    [ObservableProperty]
    private DateTime _lastCodeSentTime;

    public bool CanSendCode => !IsLoading && !string.IsNullOrWhiteSpace(Email) && !ShowResetStep;
    public bool CanResetPassword => !IsLoading && ShowResetStep && 
                                    !string.IsNullOrWhiteSpace(ResetCode) && 
                                    !string.IsNullOrWhiteSpace(NewPassword) &&
                                    !string.IsNullOrWhiteSpace(ConfirmPassword);
    public bool CanResendCode => !IsLoading && ShowResetStep && 
                                 (DateTime.UtcNow - LastCodeSentTime).TotalSeconds >= 60;

    public string SendCodeButtonText => IsLoading ? "Sending..." : "Send Reset Code";
    public string ResetPasswordButtonText => IsLoading ? "Resetting..." : "Reset Password";

    public ForgotPasswordDialogViewModel(
        IToastService toastService,
        IValidationService validationService,
        MyShop.Plugins.API.PasswordReset.IPasswordResetApi passwordResetApi)
    {
        _toastService = toastService ?? throw new ArgumentNullException(nameof(toastService));
        _validationService = validationService ?? throw new ArgumentNullException(nameof(validationService));
        _passwordResetApi = passwordResetApi ?? throw new ArgumentNullException(nameof(passwordResetApi));
    }

    partial void OnEmailChanged(string value)
    {
        OnPropertyChanged(nameof(CanSendCode));
    }

    partial void OnResetCodeChanged(string value)
    {
        OnPropertyChanged(nameof(CanResetPassword));
    }

    partial void OnNewPasswordChanged(string value)
    {
        OnPropertyChanged(nameof(CanResetPassword));
    }

    partial void OnConfirmPasswordChanged(string value)
    {
        OnPropertyChanged(nameof(CanResetPassword));
    }

    partial void OnIsLoadingChanged(bool value)
    {
        OnPropertyChanged(nameof(CanSendCode));
        OnPropertyChanged(nameof(CanResetPassword));
        OnPropertyChanged(nameof(CanResendCode));
        OnPropertyChanged(nameof(SendCodeButtonText));
        OnPropertyChanged(nameof(ResetPasswordButtonText));
    }

    partial void OnShowResetStepChanged(bool value)
    {
        OnPropertyChanged(nameof(CanSendCode));
        OnPropertyChanged(nameof(CanResendCode));
    }

    partial void OnLastCodeSentTimeChanged(DateTime value)
    {
        OnPropertyChanged(nameof(CanResendCode));
    }

    [RelayCommand]
    private async Task SendResetCodeAsync()
    {
        try
        {
            ClearError();
            IsLoading = true;

            // Validate email
            var emailValidation = await _validationService.ValidateEmail(Email);
            if (!emailValidation.IsSuccess || emailValidation.Data == null || !emailValidation.Data.IsValid)
            {
                SetError(emailValidation.Data?.ErrorMessage ?? "Invalid email address");
                return;
            }

            // Call API
            var request = new ForgotPasswordRequest { Email = Email };
            var response = await _passwordResetApi.ForgotPasswordAsync(request);

            if (response.IsSuccessStatusCode && response.Content != null && response.Content.Success)
            {
                ShowEmailStep = false;
                ShowResetStep = true;
                LastCodeSentTime = DateTime.UtcNow;
                await _toastService.ShowSuccess("Reset code sent to your email");
            }
            else
            {
                SetError(response.Content?.Message ?? "Failed to send reset code");
            }
        }
        catch (ApiException apiEx)
        {
            SetError($"API Error: {apiEx.Message}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ForgotPasswordDialog] Error sending code: {ex.Message}");
            SetError("Failed to send reset code. Please try again.");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task ResetPasswordAsync()
    {
        try
        {
            ClearError();
            IsLoading = true;

            // Validate inputs
            if (string.IsNullOrWhiteSpace(ResetCode) || ResetCode.Length != 6)
            {
                SetError("Please enter a valid 6-digit code");
                return;
            }

            var passwordValidation = await _validationService.ValidatePassword(NewPassword);
            if (!passwordValidation.IsSuccess || passwordValidation.Data == null || !passwordValidation.Data.IsValid)
            {
                SetError(passwordValidation.Data?.ErrorMessage ?? "Invalid password");
                return;
            }

            if (NewPassword != ConfirmPassword)
            {
                SetError("Passwords do not match");
                return;
            }

            // Call API
            var request = new ResetPasswordRequest
            {
                Email = Email,
                ResetCode = ResetCode,
                NewPassword = NewPassword,
                ConfirmPassword = ConfirmPassword
            };

            var response = await _passwordResetApi.ResetPasswordAsync(request);

            if (response.IsSuccessStatusCode && response.Content != null && response.Content.Success)
            {
                IsSuccess = true;
                await _toastService.ShowSuccess("Password reset successfully!");
            }
            else
            {
                SetError(response.Content?.Message ?? "Failed to reset password");
            }
        }
        catch (ApiException apiEx)
        {
            SetError($"API Error: {apiEx.Message}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ForgotPasswordDialog] Error resetting password: {ex.Message}");
            SetError("Failed to reset password. Please try again.");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task ResendCodeAsync()
    {
        await SendResetCodeAsync();
    }

    private void SetError(string message)
    {
        ErrorMessage = message;
        HasError = true;
    }

    private void ClearError()
    {
        ErrorMessage = string.Empty;
        HasError = false;
    }
}
