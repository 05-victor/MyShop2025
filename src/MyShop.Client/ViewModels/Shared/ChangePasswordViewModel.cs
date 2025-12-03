using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Client.Facades;
using MyShop.Core.Interfaces.Facades;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MyShop.Client.ViewModels.Shared;

public partial class ChangePasswordViewModel : ObservableObject
{
    private readonly IProfileFacade _profileFacade;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsFormValid), nameof(CurrentPasswordError))]
    private string _currentPassword = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsFormValid), nameof(NewPasswordError), nameof(PasswordStrength), nameof(ConfirmPasswordError))]
    private string _newPassword = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsFormValid), nameof(ConfirmPasswordError))]
    private string _confirmPassword = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanSubmit))]
    private bool _isLoading = false;
    [ObservableProperty] private bool _showCurrentPassword = false;
    [ObservableProperty] private bool _showNewPassword = false;
    [ObservableProperty] private bool _showConfirmPassword = false;
    [ObservableProperty] private bool _isSuccess = false;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    private string _errorMessage = string.Empty;

    // Validation error messages
    public string CurrentPasswordError =>
        string.IsNullOrWhiteSpace(CurrentPassword) ? "Current password is required" : string.Empty;

    public string NewPasswordError
    {
        get
        {
            if (string.IsNullOrWhiteSpace(NewPassword))
                return "New password is required";
            
            if (NewPassword.Length < 8)
                return "Password must be at least 8 characters";
            
            if (!Regex.IsMatch(NewPassword, @"[A-Z]"))
                return "Password must contain at least one uppercase letter";
            
            if (!Regex.IsMatch(NewPassword, @"[a-z]"))
                return "Password must contain at least one lowercase letter";
            
            if (!Regex.IsMatch(NewPassword, @"[0-9]"))
                return "Password must contain at least one number";
            
            if (!Regex.IsMatch(NewPassword, @"[!@#$%^&*(),.?""':{}|<>]"))
                return "Password must contain at least one special character";
            
            if (NewPassword == CurrentPassword)
                return "New password must be different from current password";
            
            return string.Empty;
        }
    }

    public string ConfirmPasswordError
    {
        get
        {
            if (string.IsNullOrWhiteSpace(ConfirmPassword))
                return "Please confirm your password";
            
            if (NewPassword != ConfirmPassword)
                return "Passwords do not match";
            
            return string.Empty;
        }
    }

    // Password strength indicator
    public string PasswordStrength
    {
        get
        {
            if (string.IsNullOrWhiteSpace(NewPassword)) return "None";
            
            int strength = 0;
            if (NewPassword.Length >= 8) strength++;
            if (NewPassword.Length >= 12) strength++;
            if (Regex.IsMatch(NewPassword, @"[A-Z]")) strength++;
            if (Regex.IsMatch(NewPassword, @"[a-z]")) strength++;
            if (Regex.IsMatch(NewPassword, @"[0-9]")) strength++;
            if (Regex.IsMatch(NewPassword, @"[!@#$%^&*(),.?""':{}|<>]")) strength++;
            
            return strength switch
            {
                <= 2 => "Weak",
                <= 4 => "Medium",
                _ => "Strong"
            };
        }
    }

    public bool IsFormValid =>
        !string.IsNullOrWhiteSpace(CurrentPassword) &&
        !string.IsNullOrWhiteSpace(NewPassword) &&
        !string.IsNullOrWhiteSpace(ConfirmPassword) &&
        string.IsNullOrEmpty(NewPasswordError) &&
        string.IsNullOrEmpty(ConfirmPasswordError);

    /// <summary>
    /// Can submit form (form valid and not loading)
    /// </summary>
    public bool CanSubmit => IsFormValid && !IsLoading;

    /// <summary>
    /// Has error message to display
    /// </summary>
    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

    public ChangePasswordViewModel(IProfileFacade profileFacade)
    {
        _profileFacade = profileFacade;
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

    [RelayCommand(CanExecute = nameof(IsFormValid))]
    private async Task ChangePasswordAsync()
    {
        if (!IsFormValid) return;

        IsLoading = true;
        ErrorMessage = string.Empty;
        
        try
        {
            var result = await _profileFacade.ChangePasswordAsync(CurrentPassword, NewPassword, ConfirmPassword);

            if (result.IsSuccess)
            {
                IsSuccess = true;
                CurrentPassword = string.Empty;
                NewPassword = string.Empty;
                ConfirmPassword = string.Empty;
                ErrorMessage = string.Empty;
                
                System.Diagnostics.Debug.WriteLine("[ChangePasswordViewModel] Password changed successfully");
            }
            else
            {
                ErrorMessage = result.ErrorMessage ?? "Failed to change password. Please check your current password.";
                System.Diagnostics.Debug.WriteLine($"[ChangePasswordViewModel] Password change failed: {ErrorMessage}");
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = "An error occurred while changing password. Please try again.";
            System.Diagnostics.Debug.WriteLine($"[ChangePasswordViewModel] Exception: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }
}
