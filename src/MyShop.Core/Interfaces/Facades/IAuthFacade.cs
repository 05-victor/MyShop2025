using MyShop.Core.Common;
using MyShop.Shared.Models;

namespace MyShop.Core.Interfaces.Facades;

/// <summary>
/// Facade pattern for authentication operations.
/// Aggregates multiple services (AuthRepository, CredentialStorage, ValidationService, NavigationService)
/// into a single, simplified API for ViewModels.
/// 
/// Benefits:
/// - ViewModel only needs to inject 1 facade instead of 5+ services
/// - Business logic centralized in one place
/// - Easy to test (mock 1 facade instead of many services)
/// - Reduces coupling between Presentation and Infrastructure layers
/// </summary>
public interface IAuthFacade
{
    /// <summary>
    /// Login user with validation, credential storage, and navigation.
    /// Orchestrates: Validation → Repository.Login → Storage.SaveToken → Navigation
    /// </summary>
    /// <param name="username">Username or email</param>
    /// <param name="password">Password</param>
    /// <param name="rememberMe">Whether to save token for persistent login</param>
    /// <returns>Result containing User if successful</returns>
    Task<Result<User>> LoginAsync(string username, string password, bool rememberMe);

    /// <summary>
    /// Logout user - clear storage and navigate to login page.
    /// Orchestrates: Storage.RemoveToken → Toast notification → Navigation.ToLogin
    /// </summary>
    Task<Result<Unit>> LogoutAsync();

    /// <summary>
    /// Check if user is currently logged in (has valid token).
    /// </summary>
    Task<bool> IsLoggedInAsync();

    /// <summary>
    /// Get current user from token/storage.
    /// </summary>
    Task<Result<User>> GetCurrentUserAsync();

    /// <summary>
    /// Register new user with validation.
    /// Orchestrates: Validation → Repository.Register → Storage.SaveToken (if auto-login)
    /// </summary>
    Task<Result<User>> RegisterAsync(string username, string email, string phoneNumber, string password, string role);

    /// <summary>
    /// Activate trial account with admin code.
    /// Orchestrates: Validation → Repository.ActivateTrial → Toast notification
    /// </summary>
    Task<Result<User>> ActivateTrialAsync(string adminCode);

    /// <summary>
    /// Send verification email to current user.
    /// </summary>
    Task<Result<Unit>> SendVerificationEmailAsync();

    /// <summary>
    /// Verify email with OTP code.
    /// </summary>
    Task<Result<Unit>> VerifyEmailAsync(string otp);

    /// <summary>
    /// Get primary role of current user (for navigation).
    /// </summary>
    Task<Result<string>> GetPrimaryRoleAsync();

    /// <summary>
    /// Validate admin code for first-user setup.
    /// Checks: code exists, status Active, not expired, usage limit not reached.
    /// </summary>
    Task<Result<bool>> ValidateAdminCodeAsync(string adminCode);
}
