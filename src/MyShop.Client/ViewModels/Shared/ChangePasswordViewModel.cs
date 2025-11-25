using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Core.Interfaces.Services;
using MyShop.Core.Interfaces.Repositories;
using MyShop.Shared.DTOs.Requests;
using System;
using System.Threading.Tasks;

namespace MyShop.Client.ViewModels.Shared;

/// <summary>
/// ViewModel for Change Password dialog/page
/// Validates password strength and confirmation
/// </summary>
public partial class ChangePasswordViewModel : ObservableObject
{
    private readonly IUserRepository _userRepository;
    private readonly IValidationService _validationService;
    private readonly IToastService _toastHelper;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsFormValid))]
    [NotifyCanExecuteChangedFor(nameof(ChangePasswordCommand))]
    private string _currentPassword = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsFormValid))]
    [NotifyCanExecuteChangedFor(nameof(ChangePasswordCommand))]
    private string _newPassword = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsFormValid))]
    [NotifyCanExecuteChangedFor(nameof(ChangePasswordCommand))]
    private string _confirmPassword = string.Empty;

    // Validation errors
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsFormValid))]
    [NotifyCanExecuteChangedFor(nameof(ChangePasswordCommand))]
    private string _currentPasswordError = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsFormValid))]
    [NotifyCanExecuteChangedFor(nameof(ChangePasswordCommand))]
    private string _newPasswordError = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsFormValid))]
    [NotifyCanExecuteChangedFor(nameof(ChangePasswordCommand))]
    private string _confirmPasswordError = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    private string _errorMessage = string.Empty;
    
    [ObservableProperty] private bool _isLoading = false;

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    // Password visibility toggles
    [ObservableProperty] private bool _showCurrentPassword = false;
    [ObservableProperty] private bool _showNewPassword = false;
    [ObservableProperty] private bool _showConfirmPassword = false;

    // Success flag for dialog result
    [ObservableProperty] private bool _isSuccess = false;

    public bool IsFormValid =>
        string.IsNullOrWhiteSpace(CurrentPasswordError) &&
        string.IsNullOrWhiteSpace(NewPasswordError) &&
        string.IsNullOrWhiteSpace(ConfirmPasswordError) &&
        !string.IsNullOrWhiteSpace(CurrentPassword) &&
        !string.IsNullOrWhiteSpace(NewPassword) &&
        !string.IsNullOrWhiteSpace(ConfirmPassword);

    public bool CanSubmit => IsFormValid && !IsLoading;

    public ChangePasswordViewModel(
        IUserRepository userRepository,
        IValidationService validationService,
        IToastService toastHelper)
    {
        _userRepository = userRepository;
        _validationService = validationService;
        _toastHelper = toastHelper;
    }

    /// <summary>
    /// Toggle current password visibility
    /// </summary>
    [RelayCommand]
    private void ToggleCurrentPasswordVisibility()
    {
        ShowCurrentPassword = !ShowCurrentPassword;
    }

    /// <summary>
    /// Toggle new password visibility
    /// </summary>
    [RelayCommand]
    private void ToggleNewPasswordVisibility()
    {
        ShowNewPassword = !ShowNewPassword;
    }

    /// <summary>
    /// Toggle confirm password visibility
    /// </summary>
    [RelayCommand]
    private void ToggleConfirmPasswordVisibility()
    {
        ShowConfirmPassword = !ShowConfirmPassword;
    }

    /// <summary>
    /// Attempt to change password
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanChangePassword))]
    private async Task ChangePasswordAsync()
    {
        if (!await ValidateAllAsync())
        {
            ErrorMessage = "Please fix validation errors.";
            return;
        }

        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            var request = new ChangePasswordRequest
            {
                CurrentPassword = CurrentPassword,
                NewPassword = NewPassword
            };

            var result = await _userRepository.ChangePasswordAsync(request);

            if (result.IsSuccess)
            {
                await _toastHelper.ShowSuccess("Password changed successfully!");
                IsSuccess = true;
                
                // Clear sensitive data
                CurrentPassword = string.Empty;
                NewPassword = string.Empty;
                ConfirmPassword = string.Empty;
            }
            else
            {
                // Check if error is about current password
                if (result.ErrorMessage?.Contains("current", StringComparison.OrdinalIgnoreCase) == true)
                {
                    CurrentPasswordError = "Current password is incorrect";
                }
                else
                {
                    ErrorMessage = result.ErrorMessage ?? "Failed to change password.";
                }
            }
        }
        catch (Refit.ApiException apiEx)
        {
            if ((int)apiEx.StatusCode == 400)
            {
                if (apiEx.Content?.Contains("current", StringComparison.OrdinalIgnoreCase) == true)
                {
                    CurrentPasswordError = "Current password is incorrect";
                }
                else
                {
                    ErrorMessage = apiEx.Content ?? "Server validation failed.";
                }
            }
            else
            {
                ErrorMessage = "Server error. Please try again.";
            }
        }
        catch (System.Net.Http.HttpRequestException)
        {
            ErrorMessage = "Cannot connect to server. Please check your connection.";
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ChangePasswordViewModel] Error: {ex.Message}");
            ErrorMessage = "An unexpected error occurred.";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private bool CanChangePassword() => IsFormValid && !IsLoading;

    /// <summary>
    /// Validate all password fields
    /// </summary>
    private async Task<bool> ValidateAllAsync()
    {
        ClearErrors();
        var isValid = true;

        // Validate current password
        if (string.IsNullOrWhiteSpace(CurrentPassword))
        {
            CurrentPasswordError = "Current password is required";
            isValid = false;
        }

        // Validate new password
        var newPasswordResult = await _validationService.ValidatePassword(NewPassword);
        if (newPasswordResult.IsSuccess && newPasswordResult.Data != null)
        {
            if (!newPasswordResult.Data.IsValid)
            {
                NewPasswordError = newPasswordResult.Data.ErrorMessage;
                isValid = false;
            }
        }

        // Check new password differs from current
        if (!string.IsNullOrWhiteSpace(CurrentPassword) && 
            !string.IsNullOrWhiteSpace(NewPassword) &&
            string.Equals(CurrentPassword, NewPassword, StringComparison.Ordinal))
        {
            NewPasswordError = "New password must differ from current password";
            isValid = false;
        }

        // Validate confirmation
        var confirmResult = await _validationService.ValidatePasswordConfirmation(NewPassword, ConfirmPassword);
        if (confirmResult.IsSuccess && confirmResult.Data != null)
        {
            if (!confirmResult.Data.IsValid)
            {
                ConfirmPasswordError = confirmResult.Data.ErrorMessage;
                isValid = false;
            }
        }

        return isValid;
    }

    /// <summary>
    /// Clear all validation errors
    /// </summary>
    private void ClearErrors()
    {
        CurrentPasswordError = string.Empty;
        NewPasswordError = string.Empty;
        ConfirmPasswordError = string.Empty;
        ErrorMessage = string.Empty;
    }

    /// <summary>
    /// Real-time validation for new password
    /// </summary>
    async partial void OnNewPasswordChanged(string value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            var result = await _validationService.ValidatePassword(value);
            if (result.IsSuccess && result.Data != null)
            {
                NewPasswordError = result.Data.IsValid ? string.Empty : result.Data.ErrorMessage;
            }

            // Check if differs from current
            if (!string.IsNullOrWhiteSpace(CurrentPassword) &&
                string.Equals(CurrentPassword, value, StringComparison.Ordinal))
            {
                NewPasswordError = "New password must differ from current password";
            }

            // Re-validate confirmation if already entered
            if (!string.IsNullOrWhiteSpace(ConfirmPassword))
            {
                var confirmResult = await _validationService.ValidatePasswordConfirmation(value, ConfirmPassword);
                if (confirmResult.IsSuccess && confirmResult.Data != null)
                {
                    ConfirmPasswordError = confirmResult.Data.IsValid ? string.Empty : confirmResult.Data.ErrorMessage;
                }
            }
        }
        else
        {
            NewPasswordError = string.Empty;
        }
    }

    /// <summary>
    /// Real-time validation for confirm password
    /// </summary>
    async partial void OnConfirmPasswordChanged(string value)
    {
        if (!string.IsNullOrWhiteSpace(value) && !string.IsNullOrWhiteSpace(NewPassword))
        {
            var result = await _validationService.ValidatePasswordConfirmation(NewPassword, value);
            if (result.IsSuccess && result.Data != null)
            {
                ConfirmPasswordError = result.Data.IsValid ? string.Empty : result.Data.ErrorMessage;
            }
        }
        else
        {
            ConfirmPasswordError = string.Empty;
        }
    }
}
