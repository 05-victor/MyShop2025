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

    // Existing Request State
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasNoRequest))]
    [NotifyPropertyChangedFor(nameof(HasPendingRequest))]
    [NotifyPropertyChangedFor(nameof(HasRejectedRequest))]
    [NotifyPropertyChangedFor(nameof(ShowSubmitForm))]
    [NotifyPropertyChangedFor(nameof(ShowRequestStatus))]
    private MyShop.Shared.DTOs.Responses.AgentRequestResponse? _myRequest;

    [ObservableProperty]
    private string? _requestStatusMessage;

    [ObservableProperty]
    private string? _rejectionReason;

    public bool HasNoRequest => MyRequest == null;
    public bool HasPendingRequest => MyRequest != null && MyRequest.Status.Equals("PENDING", StringComparison.OrdinalIgnoreCase);
    public bool HasRejectedRequest => MyRequest != null && MyRequest.Status.Equals("REJECTED", StringComparison.OrdinalIgnoreCase);
    public bool ShowSubmitForm => MyRequest == null || HasRejectedRequest;
    public bool ShowRequestStatus => MyRequest != null;
    public string RequestStatusDisplay => MyRequest?.Status?.ToUpper() ?? "UNKNOWN";
    public string RequestDateDisplay => MyRequest?.RequestedAt.ToLocalTime().ToString("MMM dd, yyyy") ?? string.Empty;

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

    public async Task InitializeAsync(MyShop.Shared.Models.User? user = null)
    {
        SetLoadingState(true);
        
        try
        {
            // Load existing request first
            await LoadMyRequestAsync();

            // Pre-fill with current user's info if provided AND no existing request
            if (user != null && MyRequest == null)
            {
                FullName = user.FullName ?? string.Empty;
                Email = user.Email ?? string.Empty;
                PhoneNumber = user.PhoneNumber ?? string.Empty;
            }
        }
        finally
        {
            SetLoadingState(false);
        }
    }

    [RelayCommand]
    private async Task LoadMyRequestAsync()
    {
        try
        {
            var result = await _agentRequestFacade.GetMyRequestAsync();

            if (result.IsSuccess && result.Data != null)
            {
                MyRequest = result.Data;

                // Set status message based on request status
                if (HasPendingRequest)
                {
                    RequestStatusMessage = "Your application is currently under review. We'll notify you once a decision is made.";
                }
                else if (HasRejectedRequest)
                {
                    RejectionReason = MyRequest.Notes;
                    RequestStatusMessage = "Your application was not approved. You can submit a new application below.";
                }
                else if (MyRequest.Status.Equals("APPROVED", StringComparison.OrdinalIgnoreCase))
                {
                    RequestStatusMessage = "Congratulations! Your application has been approved. You are now a Sales Agent.";
                }

                System.Diagnostics.Debug.WriteLine($"[BecomeAgentViewModel] Loaded existing request: Status={MyRequest.Status}");
            }
            else
            {
                // No existing request - user can submit new one
                MyRequest = null;
                System.Diagnostics.Debug.WriteLine("[BecomeAgentViewModel] No existing request found");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[BecomeAgentViewModel] Error loading request: {ex.Message}");
            MyRequest = null;
        }
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

                // Reload to show the new request status
                await LoadMyRequestAsync();

                // Clear form fields
                ClearForm();
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

    private void ClearForm()
    {
        FullName = string.Empty;
        Email = string.Empty;
        PhoneNumber = string.Empty;
        StreetAddress = string.Empty;
        City = string.Empty;
        StateProvince = string.Empty;
        BusinessName = string.Empty;
        TaxId = string.Empty;
        SalesExperience = string.Empty;
        Motivation = string.Empty;
        AgreeToTerms = false;
    }
}
