using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Core.Interfaces.Services;
using MyShop.Client.Views.Shared;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using MyShop.Core.Interfaces.Repositories;
using MyShop.Shared.DTOs.Requests;

namespace MyShop.Client.ViewModels.Shared;

public partial class RegisterViewModel : ObservableObject
{
    private readonly IAuthRepository _authRepository;
    private readonly INavigationService _navigationService;
    private readonly IToastService _toastHelper;
    private readonly IValidationService _validationService;
    private CancellationTokenSource? _registerCancellationTokenSource;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsUsernameValid))]
    [NotifyPropertyChangedFor(nameof(IsFormValid))]
    [NotifyCanExecuteChangedFor(nameof(AttemptRegisterCommand))]
    private string _username = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEmailValid))]
    [NotifyPropertyChangedFor(nameof(IsFormValid))]
    [NotifyCanExecuteChangedFor(nameof(AttemptRegisterCommand))]
    private string _email = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsPhoneValid))]
    [NotifyPropertyChangedFor(nameof(IsFormValid))]
    [NotifyCanExecuteChangedFor(nameof(AttemptRegisterCommand))]
    private string _phoneNumber = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsPasswordValid))]
    [NotifyPropertyChangedFor(nameof(IsFormValid))]
    [NotifyCanExecuteChangedFor(nameof(AttemptRegisterCommand))]
    private string _password = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsConfirmPasswordValid))]
    [NotifyPropertyChangedFor(nameof(IsFormValid))]
    [NotifyCanExecuteChangedFor(nameof(AttemptRegisterCommand))]
    private string _confirmPassword = string.Empty;

    // Default role for registration is always CUSTOMER
    private string _selectedRole = "CUSTOMER";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsFormValid))]
    [NotifyCanExecuteChangedFor(nameof(AttemptRegisterCommand))]
    private bool _isLoading = false;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsUsernameValid))]
    [NotifyPropertyChangedFor(nameof(IsFormValid))]
    [NotifyCanExecuteChangedFor(nameof(AttemptRegisterCommand))]
    private string _usernameError = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEmailValid))]
    [NotifyPropertyChangedFor(nameof(IsFormValid))]
    [NotifyCanExecuteChangedFor(nameof(AttemptRegisterCommand))]
    private string _emailError = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsPhoneValid))]
    [NotifyPropertyChangedFor(nameof(IsFormValid))]
    [NotifyCanExecuteChangedFor(nameof(AttemptRegisterCommand))]
    private string _phoneError = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsPasswordValid))]
    [NotifyPropertyChangedFor(nameof(IsFormValid))]
    [NotifyCanExecuteChangedFor(nameof(AttemptRegisterCommand))]
    private string _passwordError = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsConfirmPasswordValid))]
    [NotifyPropertyChangedFor(nameof(IsFormValid))]
    [NotifyCanExecuteChangedFor(nameof(AttemptRegisterCommand))]
    private string _confirmPasswordError = string.Empty;

    // Computed validation properties
    public bool IsUsernameValid => string.IsNullOrWhiteSpace(UsernameError);
    public bool IsEmailValid => string.IsNullOrWhiteSpace(EmailError);
    public bool IsPhoneValid => string.IsNullOrWhiteSpace(PhoneError);
    public bool IsPasswordValid => string.IsNullOrWhiteSpace(PasswordError);
    public bool IsConfirmPasswordValid => string.IsNullOrWhiteSpace(ConfirmPasswordError);
    
    public bool IsFormValid =>
        IsUsernameValid &&
        IsEmailValid &&
        IsPhoneValid &&
        IsPasswordValid &&
        IsConfirmPasswordValid &&
        !string.IsNullOrWhiteSpace(Username) &&
        !string.IsNullOrWhiteSpace(Email) &&
        !string.IsNullOrWhiteSpace(PhoneNumber) &&
        !string.IsNullOrWhiteSpace(Password) &&
        !string.IsNullOrWhiteSpace(ConfirmPassword) &&
        !IsLoading;

    public RegisterViewModel(
        IAuthRepository authRepository,
        INavigationService navigationService,
        IToastService toastHelper,
        IValidationService validationService)
    {
        _authRepository = authRepository;
        _navigationService = navigationService;
        _toastHelper = toastHelper;
        _validationService = validationService;
    }

    // Real-time validation on property changes
    partial void OnUsernameChanged(string value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            var result = _validationService.ValidateUsername(value);
            UsernameError = result.IsValid ? string.Empty : result.ErrorMessage;
        }
        else
        {
            UsernameError = string.Empty;
        }
    }

    partial void OnEmailChanged(string value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            var result = _validationService.ValidateEmail(value);
            EmailError = result.IsValid ? string.Empty : result.ErrorMessage;
        }
        else
        {
            EmailError = string.Empty;
        }
    }

    partial void OnPhoneNumberChanged(string value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            // Basic phone validation using regex
            if (!IsValidPhone(value))
            {
                PhoneError = "Please enter a valid phone number";
            }
            else
            {
                PhoneError = string.Empty;
            }
        }
        else
        {
            PhoneError = string.Empty;
        }
    }

    partial void OnPasswordChanged(string value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            var result = _validationService.ValidatePassword(value);
            PasswordError = result.IsValid ? string.Empty : result.ErrorMessage;

            // Re-validate confirm password if it has a value
            if (!string.IsNullOrWhiteSpace(ConfirmPassword))
            {
                var confirmResult = _validationService.ValidatePasswordConfirmation(value, ConfirmPassword);
                ConfirmPasswordError = confirmResult.IsValid ? string.Empty : confirmResult.ErrorMessage;
            }
        }
        else
        {
            PasswordError = string.Empty;
        }
    }

    partial void OnConfirmPasswordChanged(string value)
    {
        if (!string.IsNullOrWhiteSpace(value) && !string.IsNullOrWhiteSpace(Password))
        {
            var result = _validationService.ValidatePasswordConfirmation(Password, value);
            ConfirmPasswordError = result.IsValid ? string.Empty : result.ErrorMessage;
        }
        else
        {
            ConfirmPasswordError = string.Empty;
        }
    }

    private bool CanAttemptRegister() => IsFormValid;

    [RelayCommand(CanExecute = nameof(CanAttemptRegister), IncludeCancelCommand = true)]
    private async Task AttemptRegisterAsync(CancellationToken cancellationToken)
    {
        // Cancel any previous registration attempt
        _registerCancellationTokenSource?.Cancel();
        _registerCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        // Clear previous errors
        ErrorMessage = string.Empty;

        IsLoading = true;

        try
        {
            var response = await _authRepository.RegisterAsync(
                Username.Trim(),
                Email.Trim(),
                PhoneNumber.Trim(),
                Password,
                _selectedRole
            );

            // Check for cancellation
            _registerCancellationTokenSource.Token.ThrowIfCancellationRequested();

            if (response.IsSuccess)
            {
                _toastHelper.ShowSuccess("Account created successfully! Please login.");
                
                // Navigate to login page
                _navigationService.NavigateTo(typeof(LoginPage).FullName!);
            }
            else
            {
                ErrorMessage = response.ErrorMessage ?? "Registration failed. Please try again.";
            }
        }
        catch (OperationCanceledException)
        {
            ErrorMessage = "Registration cancelled";
        }
        catch (Refit.ApiException apiEx)
        {
            System.Diagnostics.Debug.WriteLine($"API Error: {apiEx.StatusCode} - {apiEx.Content}");
            
            if (apiEx.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                if (apiEx.Content?.Contains("username") == true)
                {
                    UsernameError = "Username already exists";
                }
                else if (apiEx.Content?.Contains("email") == true)
                {
                    EmailError = "Email already registered";
                }
                else
                {
                    ErrorMessage = "Invalid registration data. Please check your input.";
                }
            }
            else
            {
                ErrorMessage = "Network error. Please check your connection.";
            }
        }
        catch (HttpRequestException httpEx)
        {
            System.Diagnostics.Debug.WriteLine($"Network Error: {httpEx.Message}");
            ErrorMessage = "Cannot connect to server. Please check your network connection and ensure the server is running.";
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"General Error: {ex.Message}");
            ErrorMessage = "An unexpected error occurred. Please try again.";
        }
        finally
        {
            IsLoading = false;
            _registerCancellationTokenSource?.Dispose();
            _registerCancellationTokenSource = null;
        }
    }

    [RelayCommand]
    private void BackToLogin()
    {
        _navigationService.NavigateTo(typeof(LoginPage).FullName!);
    }

    [RelayCommand]
    private async Task GoogleLogin()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;
            System.Diagnostics.Debug.WriteLine("[RegisterViewModel] Google Register requested - OAuth2 integration pending");
            await Task.Delay(800);
            ErrorMessage = "Google Sign-Up placeholder. Replace this block with OAuth2 flow and account linking on backend.";
            _toastHelper.ShowInfo("Google Sign-Up coming soon.");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private bool IsValidPhone(string phone)
    {
        // Accept phone numbers with 10-15 digits, may contain spaces, dashes, or parentheses
        var phonePattern = @"^[\d\s\-\(\)]{10,20}$";
        return Regex.IsMatch(phone, phonePattern);
    }
}