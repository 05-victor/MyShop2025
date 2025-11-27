using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Client.Facades;
using MyShop.Core.Interfaces.Facades;
using System;
using System.Threading.Tasks;

namespace MyShop.Client.ViewModels.Settings;

public partial class TrialActivationViewModel : ObservableObject
{
    private readonly IAuthFacade _authFacade;

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
    [NotifyPropertyChangedFor(nameof(HasError))]
    private string _errorMessage = string.Empty;
    
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanActivate))]
    [NotifyPropertyChangedFor(nameof(CanSubmit))]
    private bool _isLoading = false;
    
    [ObservableProperty] private bool _isSuccess = false;

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    public bool IsFormValid =>
        !string.IsNullOrWhiteSpace(AdminCode);

    public bool CanActivate => !string.IsNullOrWhiteSpace(AdminCode) && !IsLoading;

    public bool CanSubmit => !string.IsNullOrWhiteSpace(AdminCode) && !IsLoading;

    public TrialActivationViewModel(IAuthFacade authFacade)
    {
        _authFacade = authFacade;
    }

    [RelayCommand]
    private async Task ActivateTrialAsync()
    {
        IsLoading = true;
        ErrorMessage = string.Empty;
        AdminCodeError = string.Empty;

        try
        {
            var result = await _authFacade.ActivateTrialAsync(AdminCode);

            if (result.IsSuccess)
            {
                IsSuccess = true;
                AdminCode = string.Empty;
            }
            else
            {
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
