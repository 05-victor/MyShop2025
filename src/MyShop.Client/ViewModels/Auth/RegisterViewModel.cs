using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Client.ApiServer;
using MyShop.Client.Helpers;
using MyShop.Client.Views.Auth;
using MyShop.Shared.DTOs.Requests;
using Refit;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MyShop.Client.ViewModels.Auth
{
    public partial class RegisterViewModel : ObservableObject
    {
        private readonly IAuthApi _authApi;
        private readonly INavigationService _navigationService;
        private readonly IToastHelper _toastHelper;

        [ObservableProperty]
        private string _username = string.Empty;

        [ObservableProperty]
        private string _email = string.Empty;

        [ObservableProperty]
        private string _phoneNumber = string.Empty;

        [ObservableProperty]
        private string _password = string.Empty;

        [ObservableProperty]
        private string _confirmPassword = string.Empty;

    // Selected role for registration: CUSTOMER or SALEMAN
    [ObservableProperty]
    private string _selectedRole = "CUSTOMER";

        [ObservableProperty]
        private bool _isLoading = false;

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        [ObservableProperty]
        private string _usernameError = string.Empty;

        [ObservableProperty]
        private string _emailError = string.Empty;

        [ObservableProperty]
        private string _phoneError = string.Empty;

        [ObservableProperty]
        private string _passwordError = string.Empty;

        [ObservableProperty]
        private string _confirmPasswordError = string.Empty;

        public RegisterViewModel(
            IAuthApi authApi,
            INavigationService navigationService,
            IToastHelper toastHelper)
        {
            _authApi = authApi;
            _navigationService = navigationService;
            _toastHelper = toastHelper;
        }

        [RelayCommand]
        private async Task AttemptRegisterAsync()
        {
            // Clear previous errors
            ClearErrors();

            // Validate all fields
            if (!ValidateInputs())
            {
                return;
            }

            IsLoading = true;

            try
            {
                var request = new CreateUserRequest
                {
                    Username = Username.Trim(),
                    Email = Email.Trim(),
                    Sdt = PhoneNumber.Trim(),
                    Password = Password,
                    ActivateTrial = true,
                    RoleNames = new List<string> { string.IsNullOrWhiteSpace(SelectedRole) ? "CUSTOMER" : SelectedRole }
                };

                var response = await _authApi.RegisterAsync(request);

                if (response?.Success == true && response.Result != null)
                {
                    _toastHelper.ShowSuccess("Account created successfully! Please login.");
                    
                    // Navigate to login page
                    _navigationService.NavigateTo(typeof(LoginPage));
                }
                else
                {
                    ErrorMessage = response?.Message ?? "Registration failed. Please try again.";
                }
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
            }
        }

        [RelayCommand]
        private void NavigateToLogin()
        {
            _navigationService.NavigateTo(typeof(LoginPage));
        }

        [RelayCommand]
        private async Task GoogleLogin()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;
                System.Diagnostics.Debug.WriteLine("Google Register clicked - TODO: integrate OAuth2 here");
                await Task.Delay(800);
                ErrorMessage = "Google Sign-Up placeholder. Replace this block with OAuth2 flow and account linking on backend.";
                _toastHelper.ShowInfo("Google Sign-Up coming soon.");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private bool ValidateInputs()
        {
            bool isValid = true;

            // Username validation
            if (string.IsNullOrWhiteSpace(Username))
            {
                UsernameError = "Username is required";
                isValid = false;
            }
            else if (Username.Length < 3)
            {
                UsernameError = "Username must be at least 3 characters";
                isValid = false;
            }
            else if (Username.Length > 100)
            {
                UsernameError = "Username must not exceed 100 characters";
                isValid = false;
            }

            // Email validation
            if (string.IsNullOrWhiteSpace(Email))
            {
                EmailError = "Email is required";
                isValid = false;
            }
            else if (!IsValidEmail(Email))
            {
                EmailError = "Please enter a valid email address";
                isValid = false;
            }

            // Phone validation
            if (string.IsNullOrWhiteSpace(PhoneNumber))
            {
                PhoneError = "Phone number is required";
                isValid = false;
            }
            else if (!IsValidPhone(PhoneNumber))
            {
                PhoneError = "Please enter a valid phone number";
                isValid = false;
            }

            // Password validation
            if (string.IsNullOrWhiteSpace(Password))
            {
                PasswordError = "Password is required";
                isValid = false;
            }
            else if (Password.Length < 6)
            {
                PasswordError = "Password must be at least 6 characters";
                isValid = false;
            }

            // Confirm Password validation
            if (string.IsNullOrWhiteSpace(ConfirmPassword))
            {
                ConfirmPasswordError = "Please confirm your password";
                isValid = false;
            }
            else if (Password != ConfirmPassword)
            {
                ConfirmPasswordError = "Passwords do not match";
                isValid = false;
            }

            return isValid;
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
                return Regex.IsMatch(email, emailPattern);
            }
            catch
            {
                return false;
            }
        }

        private bool IsValidPhone(string phone)
        {
            // Accept phone numbers with 10-15 digits, may contain spaces, dashes, or parentheses
            var phonePattern = @"^[\d\s\-\(\)]{10,20}$";
            return Regex.IsMatch(phone, phonePattern);
        }

        private void ClearErrors()
        {
            ErrorMessage = string.Empty;
            UsernameError = string.Empty;
            EmailError = string.Empty;
            PhoneError = string.Empty;
            PasswordError = string.Empty;
            ConfirmPasswordError = string.Empty;
        }
    }
}