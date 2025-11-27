using MyShop.Core.Common;
using MyShop.Shared.Models;

namespace MyShop.Core.Interfaces.Facades;

/// <summary>
/// Facade pattern for user management (admin)
/// Aggregates: IUserRepository, IAuthRepository, IValidationService, IToastService
/// </summary>
public interface IUserFacade
{
    /// <summary>
    /// Load users với paging and filtering
    /// </summary>
    Task<Result<PagedList<User>>> LoadUsersAsync(
        string? searchQuery = null,
        string? role = null,
        bool? isActive = null,
        int page = 1,
        int pageSize = Common.PaginationConstants.DefaultPageSize);

    /// <summary>
    /// Get user by ID
    /// </summary>
    Task<Result<User>> GetUserByIdAsync(Guid userId);

    /// <summary>
    /// Create new user (admin)
    /// </summary>
    Task<Result<User>> CreateUserAsync(
        string username,
        string email,
        string phoneNumber,
        string password,
        string role);

    /// <summary>
    /// Update user information
    /// </summary>
    Task<Result<User>> UpdateUserAsync(
        Guid userId,
        string fullName,
        string email,
        string phoneNumber,
        string address);

    /// <summary>
    /// Delete user
    /// </summary>
    Task<Result<Unit>> DeleteUserAsync(Guid userId);

    /// <summary>
    /// Toggle user status (active/inactive)
    /// </summary>
    Task<Result<Unit>> ToggleUserStatusAsync(Guid userId);

    /// <summary>
    /// Change user role
    /// </summary>
    Task<Result<Unit>> ChangeUserRoleAsync(Guid userId, string newRole);

    /// <summary>
    /// Reset user password (admin)
    /// </summary>
    Task<Result<Unit>> ResetUserPasswordAsync(Guid userId, string newPassword);

    /// <summary>
    /// Update tax rate for sales agent
    /// </summary>
    Task<Result<Unit>> UpdateTaxRateAsync(Guid userId, decimal taxRate);

    /// <summary>
    /// Export users to CSV
    /// </summary>
    Task<Result<string>> ExportUsersAsync(string? searchQuery = null, string? roleFilter = null);

    /// <summary>
    /// Get user statistics
    /// </summary>
    Task<Result<UserStatistics>> GetUserStatisticsAsync();
}

/// <summary>
/// User statistics for admin dashboard
/// </summary>
public class UserStatistics
{
    public int TotalUsers { get; set; }
    public int TotalCustomers { get; set; }
    public int TotalSalesAgents { get; set; }
    public int TotalAdmins { get; set; }
    public int ActiveUsers { get; set; }
    public int InactiveUsers { get; set; }
}
