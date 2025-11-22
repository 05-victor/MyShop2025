using MyShop.Core.Common;
using MyShop.Shared.Models;

namespace MyShop.Core.Interfaces.Repositories;

/// <summary>
/// Repository interface cho authentication logic
/// Tách biệt ViewModels khỏi API implementation details
/// </summary>
public interface IAuthRepository
{
    /// <summary>
    /// Login với username hoặc email
    /// </summary>
    Task<Result<User>> LoginAsync(string usernameOrEmail, string password);

    /// <summary>
    /// Register user mới
    /// </summary>
    Task<Result<User>> RegisterAsync(string username, string email, string phoneNumber, string password, string role);

    /// <summary>
    /// Get thông tin user hiện tại từ token
    /// </summary>
    Task<Result<User>> GetCurrentUserAsync();

    /// <summary>
    /// Get current user ID from token/storage
    /// </summary>
    Task<Result<Guid>> GetCurrentUserIdAsync();

    /// <summary>
    /// Activate trial account với admin code
    /// </summary>
    Task<Result<User>> ActivateTrialAsync(string adminCode);

    /// <summary>
    /// Send verification email to user
    /// </summary>
    Task<Result<Unit>> SendVerificationEmailAsync(string userId);

    /// <summary>
    /// Check if user's email is verified
    /// </summary>
    Task<Result<bool>> CheckVerificationStatusAsync(string userId);

    /// <summary>
    /// Verify user's email with verification code
    /// </summary>
    Task<Result<Unit>> VerifyEmailAsync(string userId, string verificationCode);
}
