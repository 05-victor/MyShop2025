using MyShop.Core.Interfaces.Repositories;
using MyShop.Plugins.Mocks.Data;
using MyShop.Shared.Models;

namespace MyShop.Plugins.Repositories.Mocks;

/// <summary>
/// Mock repository for Profile management - delegates to MockProfileData
/// </summary>
public class MockProfileRepository : IProfileRepository
{

    public async Task<ProfileData?> GetByUserIdAsync(Guid userId)
    {
        try
        {
            var profile = await MockProfileData.GetByUserIdAsync(userId);
            System.Diagnostics.Debug.WriteLine($"[MockProfileRepository] GetByUserIdAsync({userId}) - Found: {profile != null}");
            return profile;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockProfileRepository] GetByUserIdAsync error: {ex.Message}");
            return null;
        }
    }

    public async Task<ProfileData> CreateAsync(ProfileData profile)
    {
        try
        {
            var created = await MockProfileData.CreateAsync(profile);
            System.Diagnostics.Debug.WriteLine($"[MockProfileRepository] Created profile for user: {created.UserId}");
            return created;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockProfileRepository] CreateAsync error: {ex.Message}");
            throw;
        }
    }

    public async Task<ProfileData> UpdateAsync(ProfileData profile)
    {
        try
        {
            var updated = await MockProfileData.UpdateAsync(profile);
            System.Diagnostics.Debug.WriteLine($"[MockProfileRepository] Updated profile for user: {updated.UserId}");
            return updated;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockProfileRepository] UpdateAsync error: {ex.Message}");
            throw;
        }
    }

    public async Task<bool> DeleteAsync(Guid userId)
    {
        try
        {
            var result = await MockProfileData.DeleteAsync(userId);
            System.Diagnostics.Debug.WriteLine($"[MockProfileRepository] DeleteAsync - Success: {result}");
            return result;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockProfileRepository] DeleteAsync error: {ex.Message}");
            return false;
        }
    }
}
