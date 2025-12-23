using MyShop.Shared.Models;
using MyShop.Core.Common;
using MyShop.Shared.DTOs.Requests;

namespace MyShop.Core.Interfaces.Repositories;

/// <summary>
/// Repository interface for user profile management
/// </summary>
public interface IProfileRepository
{
    /// <summary>
    /// Get user profile by user ID
    /// </summary>
    Task<Result<ProfileData>> GetByUserIdAsync(Guid userId);

    /// <summary>
    /// Create new user profile
    /// </summary>
    Task<Result<ProfileData>> CreateAsync(ProfileData profile);

    /// <summary>
    /// Update existing user profile
    /// </summary>
    Task<Result<ProfileData>> UpdateAsync(ProfileData profile);

    /// <summary>
    /// Delete user profile
    /// </summary>
    Task<Result<bool>> DeleteAsync(Guid userId);

    /// <summary>
    /// PATCH Update My Profile - uses the new PATCH endpoint
    /// </summary>
    Task<Result<ProfileData>> PatchUpdateMyProfileAsync(UpdateProfileRequest request);

    /// <summary>
    /// Upload Avatar to backend - streams file to server
    /// </summary>
    Task<Result<string>> UploadAvatarAsync(Stream fileStream, string fileName);
}
