using MyShop.Shared.Models;

namespace MyShop.Core.Interfaces.Repositories;

/// <summary>
/// Repository interface for user profile management
/// </summary>
public interface IProfileRepository
{
    /// <summary>
    /// Get user profile by user ID
    /// </summary>
    Task<ProfileData?> GetByUserIdAsync(Guid userId);

    /// <summary>
    /// Create new user profile
    /// </summary>
    Task<ProfileData> CreateAsync(ProfileData profile);

    /// <summary>
    /// Update existing user profile
    /// </summary>
    Task<ProfileData> UpdateAsync(ProfileData profile);

    /// <summary>
    /// Delete user profile
    /// </summary>
    Task<bool> DeleteAsync(Guid userId);
}
