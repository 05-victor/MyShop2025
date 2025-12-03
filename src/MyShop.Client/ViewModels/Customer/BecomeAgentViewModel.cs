using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Client.ViewModels.Base;
using MyShop.Core.Interfaces.Facades;
using MyShop.Core.Interfaces.Services;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MyShop.Client.ViewModels.Customer;

public partial class BecomeAgentViewModel : BaseViewModel
{
    private readonly IAgentRequestFacade _agentRequestFacade;
    private readonly new INavigationService _navigationService;
    private readonly IToastService _toastService;

    // Personal Information
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanSubmit))]
    private string _fullName = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanSubmit))]
    private string _email = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanSubmit))]
    private string _phoneNumber = string.Empty;

    // Address Information
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanSubmit))]
    private string _streetAddress = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanSubmit))]
    private string _city = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanSubmit))]
    private string _stateProvince = string.Empty;

    // Business Information (Optional)
    [ObservableProperty]
    private string _businessName = string.Empty;

    [ObservableProperty]
    private string _taxId = string.Empty;

    // Experience & Motivation
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanSubmit))]
    private string _salesExperience = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanSubmit))]
    private string _motivation = string.Empty;

    // Terms & Conditions
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanSubmit))]
    private bool _agreeToTerms = false;

    // UI State
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotSubmitting))]
    [NotifyPropertyChangedFor(nameof(SubmitButtonText))]
    private bool _isSubmitting = false;

    public bool IsNotSubmitting => !IsSubmitting;

    public string SubmitButtonText => IsSubmitting ? "Submitting..." : "Submit Application";

    /// <summary>
    /// Validation: All required fields must be filled and terms must be agreed
    /// </summary>
    public bool CanSubmit =>
        !string.IsNullOrWhiteSpace(FullName) &&
        !string.IsNullOrWhiteSpace(Email) &&
        IsValidEmail(Email) &&
        !string.IsNullOrWhiteSpace(PhoneNumber) &&
        !string.IsNullOrWhiteSpace(StreetAddress) &&
        !string.IsNullOrWhiteSpace(City) &&
        !string.IsNullOrWhiteSpace(StateProvince) &&
        !string.IsNullOrWhiteSpace(SalesExperience) &&
        !string.IsNullOrWhiteSpace(Motivation) &&
        AgreeToTerms &&
        !IsSubmitting;

    public BecomeAgentViewModel(
        IAgentRequestFacade agentRequestFacade,
        INavigationService navigationService,
        IToastService toastService) : base(toastService, navigationService)
    {
        _agentRequestFacade = agentRequestFacade;
        _navigationService = navigationService;
        _toastService = toastService;
    }

    public async Task InitializeAsync()
    {
        SetLoadingState(false);
        // Could pre-fill with current user's info if needed
    }

    [RelayCommand]
    private async Task SubmitAsync()
    {
        if (!CanSubmit) return;

        try
        {
            IsSubmitting = true;
            SetLoadingState(true);

            // Build application data
            var applicationData = new
            {
                FullName,
                Email,
                PhoneNumber,
                StreetAddress,
                City,
                StateProvince,
                BusinessName,
                TaxId,
                SalesExperience,
                Motivation
            };

            // Submit to facade
            var result = await _agentRequestFacade.SubmitRequestAsync(
                reason: Motivation,
                experience: SalesExperience,
                fullName: FullName,
                email: Email,
                phoneNumber: PhoneNumber,
                address: $"{StreetAddress}, {City}, {StateProvince}",
                businessName: BusinessName,
                taxId: TaxId
            );

            if (result.IsSuccess)
            {
                _ = _toastService.ShowSuccess(
                    "Application Submitted! Your sales agent application has been submitted successfully. We'll review it and get back to you soon."
                );

                // Navigate back to dashboard
                _ = _navigationService.GoBack();
            }
            else
            {
                _ = _toastService.ShowError(
                    $"Submission Failed: {result.ErrorMessage ?? "Failed to submit application. Please try again."}"
                );
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[BecomeAgentViewModel] Error submitting application: {ex.Message}");
            _ = _toastService.ShowError(
                "Error: An unexpected error occurred. Please try again later."
            );
        }
        finally
        {
            IsSubmitting = false;
            SetLoadingState(false);
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        _navigationService.GoBack();
    }

    private bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            var regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
            return regex.IsMatch(email);
        }
        catch
        {
            return false;
        }
    }
}
