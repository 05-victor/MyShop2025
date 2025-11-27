using MyShop.Core.Common;
using MyShop.Core.Interfaces.Facades;
using MyShop.Core.Interfaces.Infrastructure;
using MyShop.Core.Interfaces.Repositories;
using MyShop.Core.Interfaces.Services;
using MyShop.Client.Views.Shared;
using MyShop.Shared.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MyShop.Client.Facades;

/// <summary>
/// Implementation of IAuthFacade - aggregates auth-related services
/// Follows Facade pattern to simplify complex subsystem interactions
/// </summary>
public class AuthFacade : IAuthFacade
{
    private readonly IAuthRepository _authRepository;
    private readonly IUserRepository _userRepository;
    private readonly ICredentialStorage _credentialStorage;
    private readonly IValidationService _validationService;
    private readonly INavigationService _navigationService;
    private readonly IToastService _toastService;

    public AuthFacade(
        IAuthRepository authRepository,
        IUserRepository userRepository,
        ICredentialStorage credentialStorage,
        IValidationService validationService,
        INavigationService navigationService,
        IToastService toastService)
    {
        _authRepository = authRepository ?? throw new ArgumentNullException(nameof(authRepository));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _credentialStorage = credentialStorage ?? throw new ArgumentNullException(nameof(credentialStorage));
        _validationService = validationService ?? throw new ArgumentNullException(nameof(validationService));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _toastService = toastService ?? throw new ArgumentNullException(nameof(toastService));
    }

    /// <inheritdoc/>
    public async Task<Result<User>> LoginAsync(string username, string password, bool rememberMe)
    {
        try
        {
            // Step 1: Validate inputs
            var usernameValidation = await _validationService.ValidateUsername(username);
            if (!usernameValidation.IsSuccess || usernameValidation.Data == null || !usernameValidation.Data.IsValid)
            {
                var error = usernameValidation.Data?.ErrorMessage ?? "Invalid username";
                return Result<User>.Failure(error);
            }

            var passwordValidation = await _validationService.ValidatePassword(password);
            if (!passwordValidation.IsSuccess || passwordValidation.Data == null || !passwordValidation.Data.IsValid)
            {
                var error = passwordValidation.Data?.ErrorMessage ?? "Invalid password";
                return Result<User>.Failure(error);
            }

            // Step 2: Call repository to login (DTO → Domain Model via Adapter)
            var loginResult = await _authRepository.LoginAsync(username.Trim(), password);
            if (!loginResult.IsSuccess || loginResult.Data == null)
            {
                return Result<User>.Failure(loginResult.ErrorMessage ?? "Login failed");
            }

            var user = loginResult.Data;

            // Step 3: Save token if remember me
            if (rememberMe && !string.IsNullOrEmpty(user.Token))
            {
                await _credentialStorage.SaveToken(user.Token);
            }

            // Step 4: Show success notification
            await _toastService.ShowSuccess($"Welcome back, {user.Username}!");

            return Result<User>.Success(user);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AuthFacade] LoginAsync failed: {ex.Message}");
            return Result<User>.Failure("An unexpected error occurred during login", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<Result<Unit>> LogoutAsync()
    {
        try
        {
            // Step 1: Clear stored credentials
            await _credentialStorage.RemoveToken();

            // Step 2: Show notification
            await _toastService.ShowInfo("You have been logged out");

            // Step 3: Navigate to login page
            await _navigationService.NavigateTo(typeof(LoginPage).FullName!);

            return Result<Unit>.Success(Unit.Value);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AuthFacade] LogoutAsync failed: {ex.Message}");
            return Result<Unit>.Failure("Logout failed", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<bool> IsLoggedInAsync()
    {
        try
        {
            var token = _credentialStorage.GetToken();
            if (string.IsNullOrEmpty(token))
            {
                return false;
            }

            // Validate token by fetching current user
            var result = await _authRepository.GetCurrentUserAsync();
            return result.IsSuccess && result.Data != null;
        }
        catch
        {
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<Result<User>> GetCurrentUserAsync()
    {
        try
        {
            var result = await _authRepository.GetCurrentUserAsync();
            if (!result.IsSuccess || result.Data == null)
            {
                return Result<User>.Failure(result.ErrorMessage ?? "Failed to get current user");
            }

            return Result<User>.Success(result.Data);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AuthFacade] GetCurrentUserAsync failed: {ex.Message}");
            return Result<User>.Failure("Failed to retrieve user information", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<Result<User>> RegisterAsync(string username, string email, string phoneNumber, string password, string role)
    {
        try
        {
            // Step 1: Validate inputs
            var usernameValidation = await _validationService.ValidateUsername(username);
            if (!usernameValidation.IsSuccess || usernameValidation.Data == null || !usernameValidation.Data.IsValid)
            {
                var error = usernameValidation.Data?.ErrorMessage ?? "Invalid username";
                return Result<User>.Failure(error);
            }

            var emailValidation = await _validationService.ValidateEmail(email);
            if (!emailValidation.IsSuccess || emailValidation.Data == null || !emailValidation.Data.IsValid)
            {
                var error = emailValidation.Data?.ErrorMessage ?? "Invalid email";
                return Result<User>.Failure(error);
            }

            var phoneValidation = await _validationService.ValidatePhoneNumber(phoneNumber);
            if (!phoneValidation.IsSuccess || phoneValidation.Data == null || !phoneValidation.Data.IsValid)
            {
                var error = phoneValidation.Data?.ErrorMessage ?? "Invalid phone number";
                return Result<User>.Failure(error);
            }

            var passwordValidation = await _validationService.ValidatePassword(password);
            if (!passwordValidation.IsSuccess || passwordValidation.Data == null || !passwordValidation.Data.IsValid)
            {
                var error = passwordValidation.Data?.ErrorMessage ?? "Invalid password";
                return Result<User>.Failure(error);
            }

            // Step 2: Call repository
            var registerResult = await _authRepository.RegisterAsync(username, email, phoneNumber, password, role);
            if (!registerResult.IsSuccess || registerResult.Data == null)
            {
                return Result<User>.Failure(registerResult.ErrorMessage ?? "Registration failed");
            }

            // Step 3: Show success notification
            await _toastService.ShowSuccess("Registration successful!");

            return Result<User>.Success(registerResult.Data);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AuthFacade] RegisterAsync failed: {ex.Message}");
            return Result<User>.Failure("An unexpected error occurred during registration", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<Result<User>> ActivateTrialAsync(string adminCode)
    {
        try
        {
            // Step 1: Validate admin code
            if (string.IsNullOrWhiteSpace(adminCode))
            {
                return Result<User>.Failure("Admin code is required");
            }

            // Step 2: Call repository
            var activateResult = await _authRepository.ActivateTrialAsync(adminCode);
            if (!activateResult.IsSuccess || activateResult.Data == null)
            {
                return Result<User>.Failure(activateResult.ErrorMessage ?? "Activation failed");
            }

            // Step 3: Show success notification
            await _toastService.ShowSuccess("Trial activated successfully!");

            return Result<User>.Success(activateResult.Data);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AuthFacade] ActivateTrialAsync failed: {ex.Message}");
            return Result<User>.Failure("An unexpected error occurred during activation", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<Result<Unit>> SendVerificationEmailAsync()
    {
        try
        {
            // Get current user ID
            var userIdResult = await _authRepository.GetCurrentUserIdAsync();
            if (!userIdResult.IsSuccess)
            {
                return Result<Unit>.Failure(userIdResult.ErrorMessage ?? "Failed to get user ID");
            }

            // Send verification email
            var sendResult = await _authRepository.SendVerificationEmailAsync(userIdResult.Data.ToString());
            if (!sendResult.IsSuccess)
            {
                return Result<Unit>.Failure(sendResult.ErrorMessage ?? "Failed to send verification email");
            }

            await _toastService.ShowSuccess("Verification email sent!");
            return Result<Unit>.Success(Unit.Value);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AuthFacade] SendVerificationEmailAsync failed: {ex.Message}");
            return Result<Unit>.Failure("Failed to send verification email", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<Result<Unit>> VerifyEmailAsync(string otp)
    {
        try
        {
            // Validate OTP
            if (string.IsNullOrWhiteSpace(otp) || otp.Length != 6)
            {
                return Result<Unit>.Failure("Please enter a valid 6-digit OTP code");
            }

            // Get current user ID
            var userIdResult = await _authRepository.GetCurrentUserIdAsync();
            if (!userIdResult.IsSuccess)
            {
                return Result<Unit>.Failure(userIdResult.ErrorMessage ?? "Failed to get user ID");
            }

            // Verify email
            var verifyResult = await _authRepository.VerifyEmailAsync(userIdResult.Data.ToString(), otp);
            if (!verifyResult.IsSuccess)
            {
                return Result<Unit>.Failure(verifyResult.ErrorMessage ?? "Email verification failed");
            }

            await _toastService.ShowSuccess("Email verified successfully!");
            return Result<Unit>.Success(Unit.Value);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AuthFacade] VerifyEmailAsync failed: {ex.Message}");
            return Result<Unit>.Failure("Failed to verify email", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<Result<string>> GetPrimaryRoleAsync()
    {
        try
        {
            var userResult = await GetCurrentUserAsync();
            if (!userResult.IsSuccess || userResult.Data == null)
            {
                return Result<string>.Failure("Failed to get user information");
            }

            var primaryRole = userResult.Data.GetPrimaryRole();
            return Result<string>.Success(primaryRole.ToString());
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AuthFacade] GetPrimaryRoleAsync failed: {ex.Message}");
            return Result<string>.Failure("Failed to get user role", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<Result<bool>> IsFirstUserSetupRequiredAsync()
    {
        try
        {
            var hasUsersResult = await _userRepository.HasAnyUsersAsync();
            if (!hasUsersResult.IsSuccess)
            {
                return Result<bool>.Failure(hasUsersResult.ErrorMessage ?? "Failed to check users");
            }

            // First-user setup required if no users exist
            var isRequired = !hasUsersResult.Data;
            System.Diagnostics.Debug.WriteLine($"[AuthFacade] IsFirstUserSetupRequiredAsync: {isRequired}");
            return Result<bool>.Success(isRequired);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AuthFacade] IsFirstUserSetupRequiredAsync failed: {ex.Message}");
            return Result<bool>.Failure("Failed to check first-user setup", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<Result<bool>> ValidateAdminCodeAsync(string adminCode)
    {
        try
        {
            await Task.Delay(300); // Simulate validation delay

            // Load admin-codes.json from mock data
            var adminCodesJson = await System.IO.File.ReadAllTextAsync(
                System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, 
                "Repositories", "Mocks", "Data", "Json", "admin-codes.json"));

            var adminCodesData = System.Text.Json.JsonSerializer.Deserialize<AdminCodesData>(
                adminCodesJson,
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (adminCodesData?.AdminCodes == null)
            {
                return Result<bool>.Failure("Failed to load admin codes");
            }

            var codeEntry = adminCodesData.AdminCodes.FirstOrDefault(c => 
                c.Code.Equals(adminCode, StringComparison.OrdinalIgnoreCase));

            if (codeEntry == null)
            {
                System.Diagnostics.Debug.WriteLine($"[AuthFacade] Admin code not found: {adminCode}");
                return Result<bool>.Success(false);
            }

            // Check status
            if (!codeEntry.Status.Equals("Active", StringComparison.OrdinalIgnoreCase))
            {
                System.Diagnostics.Debug.WriteLine($"[AuthFacade] Admin code status not Active: {codeEntry.Status}");
                return Result<bool>.Success(false);
            }

            // Check expiry
            if (codeEntry.ExpiresAt.HasValue && codeEntry.ExpiresAt.Value < DateTime.UtcNow)
            {
                System.Diagnostics.Debug.WriteLine($"[AuthFacade] Admin code expired: {codeEntry.ExpiresAt}");
                return Result<bool>.Success(false);
            }

            // Check usage limit
            if (codeEntry.CurrentUses >= codeEntry.MaxUses)
            {
                System.Diagnostics.Debug.WriteLine($"[AuthFacade] Admin code usage limit reached: {codeEntry.CurrentUses}/{codeEntry.MaxUses}");
                return Result<bool>.Success(false);
            }

            System.Diagnostics.Debug.WriteLine($"[AuthFacade] Admin code validated successfully: {adminCode}");
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AuthFacade] ValidateAdminCodeAsync failed: {ex.Message}");
            return Result<bool>.Failure("Failed to validate admin code", ex);
        }
    }

    // Helper classes for JSON deserialization
    private class AdminCodesData
    {
        public List<AdminCodeEntry>? AdminCodes { get; set; }
    }

    private class AdminCodeEntry
    {
        public string Code { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime? ExpiresAt { get; set; }
        public int MaxUses { get; set; }
        public int CurrentUses { get; set; }
    }
}
