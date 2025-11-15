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
    /// Activate trial account với admin code
    /// </summary>
    Task<Result<User>> ActivateTrialAsync(string adminCode);
}
