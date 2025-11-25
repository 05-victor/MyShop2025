using MyShop.Core.Interfaces.Repositories;
using MyShop.Plugins.Mocks.Data;
using MyShop.Shared.Models;
using MyShop.Core.Common;

namespace MyShop.Plugins.Repositories.Mocks;

/// <summary>
/// Mock repository for Profile management - delegates to MockProfileData
/// </summary>
public class MockProfileRepository : IProfileRepository
{

    public async Task<Result<ProfileData>> GetByUserIdAsync(Guid userId)
    {
        try
        {
            var profile = await MockProfileData.GetByUserIdAsync(userId);
            System.Diagnostics.Debug.WriteLine($"[MockProfileRepository] GetByUserIdAsync({userId}) - Found: {profile != null}");
            return profile != null
                ? Result<ProfileData>.Success(profile)
                : Result<ProfileData>.Failure($"Profile not found for user {userId}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockProfileRepository] GetByUserIdAsync error: {ex.Message}");
            return Result<ProfileData>.Failure($"Failed to get profile: {ex.Message}");
        }
    }

    public async Task<Result<ProfileData>> CreateAsync(ProfileData profile)
    {
        try
        {
            var created = await MockProfileData.CreateAsync(profile);
            System.Diagnostics.Debug.WriteLine($"[MockProfileRepository] Created profile for user: {created.UserId}");
            return Result<ProfileData>.Success(created);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockProfileRepository] CreateAsync error: {ex.Message}");
            return Result<ProfileData>.Failure($"Failed to create profile: {ex.Message}");
        }
    }

    public async Task<Result<ProfileData>> UpdateAsync(ProfileData profile)
    {
        try
        {
            var updated = await MockProfileData.UpdateAsync(profile);
            System.Diagnostics.Debug.WriteLine($"[MockProfileRepository] Updated profile for user: {updated.UserId}");
            return Result<ProfileData>.Success(updated);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockProfileRepository] UpdateAsync error: {ex.Message}");
            return Result<ProfileData>.Failure($"Failed to update profile: {ex.Message}");
        }
    }

    public async Task<Result<bool>> DeleteAsync(Guid userId)
    {
        try
        {
            var result = await MockProfileData.DeleteAsync(userId);
            System.Diagnostics.Debug.WriteLine($"[MockProfileRepository] DeleteAsync - Success: {result}");
            return result
                ? Result<bool>.Success(true)
                : Result<bool>.Failure($"Failed to delete profile for user {userId}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockProfileRepository] DeleteAsync error: {ex.Message}");
            return Result<bool>.Failure($"Failed to delete profile: {ex.Message}");
        }
    }
}
