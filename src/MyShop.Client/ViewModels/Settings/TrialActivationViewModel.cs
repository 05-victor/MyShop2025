using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Client.Helpers;
using MyShop.Core.Interfaces.Repositories;
using System;
using System.Threading.Tasks;

namespace MyShop.Client.ViewModels.Profile;

/// <summary>
/// ViewModel for Trial Activation dialog
/// Validates and submits admin activation code
/// </summary>
public partial class TrialActivationViewModel : ObservableObject
{
    private readonly IAuthRepository _authRepository;
    private readonly IToastHelper _toastHelper;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsFormValid))]
    [NotifyPropertyChangedFor(nameof(CanActivate))]
    [NotifyPropertyChangedFor(nameof(CanSubmit))]
    [NotifyCanExecuteChangedFor(nameof(ActivateTrialCommand))]
    private string _adminCode = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsFormValid))]
    [NotifyPropertyChangedFor(nameof(CanActivate))]
    [NotifyPropertyChangedFor(nameof(CanSubmit))]
    [NotifyCanExecuteChangedFor(nameof(ActivateTrialCommand))]
    private string _adminCodeError = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanActivate))]
    [NotifyPropertyChangedFor(nameof(CanSubmit))]
    private string _errorMessage = string.Empty;
    
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanActivate))]
    [NotifyPropertyChangedFor(nameof(CanSubmit))]
    private bool _isLoading = false;
    
    [ObservableProperty] private bool _isSuccess = false;

    public bool IsFormValid =>
        !string.IsNullOrWhiteSpace(AdminCode);

    public bool CanActivate => !string.IsNullOrWhiteSpace(AdminCode) && !IsLoading;

    public bool CanSubmit => !string.IsNullOrWhiteSpace(AdminCode) && !IsLoading;

    public TrialActivationViewModel(
        IAuthRepository authRepository,
        IToastHelper toastHelper)
    {
        _authRepository = authRepository;
        _toastHelper = toastHelper;
    }

    /// <summary>
    /// Activate trial with admin code
    /// </summary>
    [RelayCommand]
    private async Task ActivateTrialAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;
            AdminCodeError = string.Empty; // Clear previous errors

            // Call real API
            var result = await _authRepository.ActivateTrialAsync(AdminCode);

            if (result.IsSuccess)
            {
                _toastHelper.ShowSuccess("Trial account activated successfully!");
                IsSuccess = true;
                AdminCode = string.Empty;
            }
            else
            {
                // Show error from API
                AdminCodeError = result.ErrorMessage ?? "Invalid admin code";
                ErrorMessage = result.ErrorMessage ?? "Activation failed. Please verify your admin code.";
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[TrialActivationViewModel] Error: {ex.Message}");
            AdminCodeError = "An unexpected error occurred";
            ErrorMessage = "An unexpected error occurred. Please try again.";
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Validate admin code format (8 characters, XXXX-XXXX format)
    /// Now only validates on submit, not real-time
    /// </summary>
    private bool ValidateAdminCode()
    {
        AdminCodeError = string.Empty;

        if (string.IsNullOrWhiteSpace(AdminCode))
        {
            AdminCodeError = "Admin code is required";
            return false;
        }

        return true;
    }

    /// <summary>
    /// Clear validation errors when user types
    /// </summary>
    partial void OnAdminCodeChanged(string value)
    {
        // Clear errors when user starts typing
        AdminCodeError = string.Empty;
        ErrorMessage = string.Empty;
    }
}
