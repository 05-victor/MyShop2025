using MyShop.Core.Common;
using MyShop.Shared.DTOs.Requests;
using MyShop.Shared.Models;

namespace MyShop.Core.Interfaces.Repositories;

/// <summary>
/// Repository interface for user profile and account management operations
/// Separate from IAuthRepository to maintain single responsibility
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Check if any users exist in the system (for first-user setup flow)
    /// </summary>
    Task<Result<bool>> HasAnyUsersAsync();

    /// <summary>
    /// Get all users (for admin management)
    /// </summary>
    Task<Result<IEnumerable<User>>> GetAllAsync();

    /// <summary>
    /// Get paged users with filtering
    /// </summary>
    Task<Result<PagedList<User>>> GetPagedAsync(
        int page = 1,
        int pageSize = Common.PaginationConstants.DefaultPageSize,
        string? role = null,
        string? status = null,
        string? searchQuery = null,
        string sortBy = "createdAt",
        bool sortDescending = true);

    /// <summary>
    /// Create a new user (admin function)
    /// </summary>
    /// <param name="user">User to create</param>
    /// <returns>Created user with assigned ID</returns>
    Task<Result<User>> CreateUserAsync(User user);

    /// <summary>
    /// Update user profile information
    /// </summary>
    /// <param name="request">Profile update data</param>
    /// <returns>Updated user object</returns>
    Task<Result<User>> UpdateProfileAsync(UpdateProfileRequest request);

    /// <summary>
    /// Change user password
    /// </summary>
    /// <param name="request">Current and new password</param>
    /// <returns>Success/failure result</returns>
    Task<Result<bool>> ChangePasswordAsync(ChangePasswordRequest request);

    /// <summary>
    /// Upload user avatar image
    /// </summary>
    /// <param name="imageBytes">Image file bytes</param>
    /// <param name="fileName">Original file name</param>
    /// <param name="progress">Optional progress reporter</param>
    /// <returns>Updated user with new avatar URL</returns>
    Task<Result<User>> UploadAvatarAsync(byte[] imageBytes, string fileName, IProgress<double>? progress = null);
}
