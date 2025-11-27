using MyShop.Core.Common;
using MyShop.Shared.Models;

namespace MyShop.Core.Interfaces.Facades;

/// <summary>
/// Facade pattern for authentication operations
/// Aggregates multiple services (AuthRepository, CredentialStorage, ValidationService, NavigationService)
/// into a single, simplified API for ViewModels
/// 
/// Benefits:
/// - ViewModel chỉ cần inject 1 facade thay vì 5+ services
/// - Business logic tập trung ở một nơi
/// - Dễ test (mock 1 facade thay vì nhiều services)
/// - Giảm coupling giữa Presentation và Infrastructure
/// </summary>
public interface IAuthFacade
{
    /// <summary>
    /// Login user với validation, credential storage, và navigation
    /// Orchestrates: Validation → Repository.Login → Storage.SaveToken → Navigation
    /// </summary>
    /// <param name="username">Username hoặc email</param>
    /// <param name="password">Password</param>
    /// <param name="rememberMe">Có lưu token không</param>
    /// <returns>Result chứa User nếu thành công</returns>
    Task<Result<User>> LoginAsync(string username, string password, bool rememberMe);

    /// <summary>
    /// Logout user - clear storage và navigate về login page
    /// Orchestrates: Storage.RemoveToken → Toast notification → Navigation.ToLogin
    /// </summary>
    Task<Result<Unit>> LogoutAsync();

    /// <summary>
    /// Check if user đang logged in (có token valid)
    /// </summary>
    Task<bool> IsLoggedInAsync();

    /// <summary>
    /// Get current user từ token/storage
    /// </summary>
    Task<Result<User>> GetCurrentUserAsync();

    /// <summary>
    /// Register new user với validation
    /// Orchestrates: Validation → Repository.Register → Storage.SaveToken (if auto-login)
    /// </summary>
    Task<Result<User>> RegisterAsync(string username, string email, string phoneNumber, string password, string role);

    /// <summary>
    /// Activate trial account với admin code
    /// Orchestrates: Validation → Repository.ActivateTrial → Toast notification
    /// </summary>
    Task<Result<User>> ActivateTrialAsync(string adminCode);

    /// <summary>
    /// Send verification email to current user
    /// </summary>
    Task<Result<Unit>> SendVerificationEmailAsync();

    /// <summary>
    /// Verify email với OTP code
    /// </summary>
    Task<Result<Unit>> VerifyEmailAsync(string otp);

    /// <summary>
    /// Get primary role của current user (for navigation)
    /// </summary>
    Task<Result<string>> GetPrimaryRoleAsync();

    /// <summary>
    /// Check if first-user setup is required (no users exist)
    /// </summary>
    Task<Result<bool>> IsFirstUserSetupRequiredAsync();

    /// <summary>
    /// Validate admin code for first-user setup
    /// Checks: code exists, status Active, not expired, usage limit not reached
    /// </summary>
    Task<Result<bool>> ValidateAdminCodeAsync(string adminCode);
}
