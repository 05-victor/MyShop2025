using MyShop.Core.Common;
using MyShop.Core.Interfaces.Facades;
using MyShop.Core.Interfaces.Infrastructure;
using MyShop.Core.Interfaces.Repositories;
using MyShop.Core.Interfaces.Services;
using MyShop.Client.Services;
using MyShop.Client.Views.Shared;
using MyShop.Plugins.Infrastructure;
using MyShop.Shared.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MyShop.Client.Facades;

/// <summary>
/// Implementation of IAuthFacade - aggregates auth-related services
/// Follows Facade pattern to simplify complex subsystem interactions
/// 
/// Storage Integration:
/// - On login: Sets current user ID for per-user storage
/// - On logout: Clears current user and credentials
/// </summary>
public class AuthFacade : IAuthFacade
{
    private readonly IAuthRepository _authRepository;
    private readonly IUserRepository _userRepository;
    private readonly ICredentialStorage _credentialStorage;
    private readonly ISettingsStorage _settingsStorage;
    private readonly IValidationService _validationService;
    private readonly INavigationService _navigationService;
    private readonly IToastService _toastService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ISystemActivationRepository _activationRepository;

    public AuthFacade(
        IAuthRepository authRepository,
        IUserRepository userRepository,
        ICredentialStorage credentialStorage,
        ISettingsStorage settingsStorage,
        IValidationService validationService,
        INavigationService navigationService,
        IToastService toastService,
        ICurrentUserService currentUserService,
        ISystemActivationRepository activationRepository)
    {
        _authRepository = authRepository ?? throw new ArgumentNullException(nameof(authRepository));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _credentialStorage = credentialStorage ?? throw new ArgumentNullException(nameof(credentialStorage));
        _settingsStorage = settingsStorage ?? throw new ArgumentNullException(nameof(settingsStorage));
        _validationService = validationService ?? throw new ArgumentNullException(nameof(validationService));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _toastService = toastService ?? throw new ArgumentNullException(nameof(toastService));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        _activationRepository = activationRepository ?? throw new ArgumentNullException(nameof(activationRepository));
    }

    /// <inheritdoc/>
    public async Task<Result<User>> LoginAsync(string username, string password, bool rememberMe)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"[AuthFacade.LoginAsync] Started login for: {username}");

            // Step 1: Validate inputs
            var usernameValidation = await _validationService.ValidateUsername(username);
            if (!usernameValidation.IsSuccess || usernameValidation.Data == null || !usernameValidation.Data.IsValid)
            {
                var error = usernameValidation.Data?.ErrorMessage ?? "Invalid username";
                System.Diagnostics.Debug.WriteLine($"[AuthFacade.LoginAsync] Username validation failed: {error}");
                return Result<User>.Failure(error);
            }

            var passwordValidation = await _validationService.ValidatePassword(password);
            if (!passwordValidation.IsSuccess || passwordValidation.Data == null || !passwordValidation.Data.IsValid)
            {
                var error = passwordValidation.Data?.ErrorMessage ?? "Invalid password";
                System.Diagnostics.Debug.WriteLine($"[AuthFacade.LoginAsync] Password validation failed: {error}");
                return Result<User>.Failure(error);
            }

            // Step 2: Call repository to login (DTO → Domain Model via Adapter)
            System.Diagnostics.Debug.WriteLine($"[AuthFacade.LoginAsync] Calling _authRepository.LoginAsync()...");
            var loginResult = await _authRepository.LoginAsync(username.Trim(), password);
            if (!loginResult.IsSuccess || loginResult.Data == null)
            {
                System.Diagnostics.Debug.WriteLine($"[AuthFacade.LoginAsync] ✗ Login failed: {loginResult.ErrorMessage}");
                return Result<User>.Failure(loginResult.ErrorMessage ?? "Login failed");
            }

            System.Diagnostics.Debug.WriteLine($"[AuthFacade.LoginAsync] ✓ Login successful for {loginResult.Data.Username}");
            var user = loginResult.Data;

            // Step 3: Set current user for per-user storage
            SetCurrentUserForStorage(user.Id.ToString());

            // Step 4: Save tokens if remember me
            if (rememberMe && !string.IsNullOrEmpty(user.Token))
            {
                // Refresh token is already saved in AuthRepository.LoginAsync()
                // No need to save again here
                System.Diagnostics.Debug.WriteLine($"[AuthFacade.LoginAsync] Tokens already saved by AuthRepository (rememberMe=true)");
            }

            // Step 5: Fetch complete user profile from GetMe endpoint
            System.Diagnostics.Debug.WriteLine($"[AuthFacade.LoginAsync] Calling GetCurrentUserAsync()...");
            var meResult = await _authRepository.GetCurrentUserAsync();

            if (meResult.IsSuccess && meResult.Data != null)
            {
                System.Diagnostics.Debug.WriteLine($"[AuthFacade.LoginAsync] ✓ GetMe successful, using complete profile");
                user = meResult.Data;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[AuthFacade.LoginAsync] ✗ GetMe failed: {meResult.ErrorMessage}, using basic login data");
            }

            // Step 6: Cache user globally
            _currentUserService.SetCurrentUser(user);
            System.Diagnostics.Debug.WriteLine($"[AuthFacade.LoginAsync] User cached via ICurrentUserService");

            // Step 7: Show success notification
            await _toastService.ShowSuccess($"Welcome back, {user.Username}!");

            return Result<User>.Success(user);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AuthFacade.LoginAsync] ✗ Exception: {ex.Message}");
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

            // Step 2: Clear current user from storage services
            ClearCurrentUserFromStorage();

            // Step 3: Show notification
            await _toastService.ShowInfo("You have been logged out");

            // Step 4: Navigate to login page
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

            if (result.IsSuccess && result.Data != null)
            {
                // Ensure storage services know the current user
                SetCurrentUserForStorage(result.Data.Id.ToString());
                return true;
            }

            return false;
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

            // Ensure storage services know the current user
            SetCurrentUserForStorage(result.Data.Id.ToString());

            return Result<User>.Success(result.Data);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AuthFacade] GetCurrentUserAsync failed: {ex.Message}");
            return Result<User>.Failure("Failed to retrieve user information", ex);
        }
    }

    #region Storage User Management

    /// <summary>
    /// Set current user for per-user storage (credentials, settings, exports)
    /// </summary>
    private void SetCurrentUserForStorage(string userId)
    {
        try
        {
            // Set global user ID in StorageConstants (used by ExportService etc.)
            StorageConstants.SetCurrentUser(userId);

            // Set user on SecureCredentialStorage
            if (_credentialStorage is SecureCredentialStorage secureStorage)
            {
                secureStorage.SetCurrentUser(userId);
            }

            // Set user on FileSettingsStorage
            if (_settingsStorage is FileSettingsStorage fileSettingsStorage)
            {
                fileSettingsStorage.SetCurrentUser(userId);
            }

            System.Diagnostics.Debug.WriteLine($"[AuthFacade] Set current user for storage: {userId}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AuthFacade] Failed to set storage user: {ex.Message}");
        }
    }

    /// <summary>
    /// Clear current user from storage services (on logout)
    /// </summary>
    private void ClearCurrentUserFromStorage()
    {
        try
        {
            // Clear global user ID from StorageConstants
            StorageConstants.ClearCurrentUser();

            // Clear from SecureCredentialStorage
            if (_credentialStorage is SecureCredentialStorage secureStorage)
            {
                secureStorage.ClearCurrentUser();
            }

            // Clear from FileSettingsStorage
            if (_settingsStorage is FileSettingsStorage fileSettingsStorage)
            {
                fileSettingsStorage.ClearCurrentUser();
            }

            System.Diagnostics.Debug.WriteLine("[AuthFacade] Cleared current user from storage");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AuthFacade] Failed to clear storage user: {ex.Message}");
        }
    }

    #endregion

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
    public async Task<Result<bool>> ValidateAdminCodeAsync(string adminCode)
    {
        try
        {
            // Use unified ISystemActivationRepository
            var result = await _activationRepository.ValidateCodeAsync(adminCode);

            if (!result.IsSuccess || result.Data == null)
            {
                System.Diagnostics.Debug.WriteLine($"[AuthFacade] Admin code validation failed: {result.ErrorMessage}");
                return Result<bool>.Success(false);
            }

            var codeInfo = result.Data;

            if (!codeInfo.IsValid)
            {
                System.Diagnostics.Debug.WriteLine($"[AuthFacade] Admin code not valid: {adminCode}");
                return Result<bool>.Success(false);
            }

            System.Diagnostics.Debug.WriteLine($"[AuthFacade] Admin code validated successfully: {adminCode}, Type: {codeInfo.Type}");
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AuthFacade] ValidateAdminCodeAsync failed: {ex.Message}");
            return Result<bool>.Failure("Failed to validate admin code", ex);
        }
    }
}
