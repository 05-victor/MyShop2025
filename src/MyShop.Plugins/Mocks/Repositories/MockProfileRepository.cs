using System.Text.Json;
using MyShop.Core.Interfaces.Repositories;

namespace MyShop.Plugins.Mocks.Repositories;

/// <summary>
/// Mock repository for Profile management using JSON data
/// </summary>
public class MockProfileRepository : IProfileRepository
{
    private readonly List<ProfileData> _profiles;
    private readonly string _jsonFilePath;

    public MockProfileRepository()
    {
        _jsonFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Mocks", "Data", "Json", "profiles.json");
        _profiles = LoadProfilesFromJson();
    }

    private List<ProfileData> LoadProfilesFromJson()
    {
        try
        {
            if (!File.Exists(_jsonFilePath))
            {
                System.Diagnostics.Debug.WriteLine($"[MockProfileRepository] JSON file not found: {_jsonFilePath}");
                return new List<ProfileData>();
            }

            var json = File.ReadAllText(_jsonFilePath);
            var jsonDoc = JsonDocument.Parse(json);
            var profilesArray = jsonDoc.RootElement.GetProperty("profiles");

            var profiles = new List<ProfileData>();

            foreach (var item in profilesArray.EnumerateArray())
            {
                var profile = new ProfileData
                {
                    UserId = Guid.Parse(item.GetProperty("userId").GetString()!),
                    Avatar = item.TryGetProperty("avatar", out var avatar) && avatar.ValueKind != JsonValueKind.Null
                        ? avatar.GetString()
                        : null,
                    FullName = item.TryGetProperty("fullName", out var fullName) && fullName.ValueKind != JsonValueKind.Null
                        ? fullName.GetString()
                        : null,
                    PhoneNumber = item.TryGetProperty("phoneNumber", out var phone) && phone.ValueKind != JsonValueKind.Null
                        ? phone.GetString()
                        : null,
                    Email = item.GetProperty("email").GetString()!,
                    Address = item.TryGetProperty("address", out var address) && address.ValueKind != JsonValueKind.Null
                        ? address.GetString()
                        : null,
                    JobTitle = item.TryGetProperty("jobTitle", out var jobTitle) && jobTitle.ValueKind != JsonValueKind.Null
                        ? jobTitle.GetString()
                        : null,
                    CreatedAt = DateTime.Parse(item.GetProperty("createdAt").GetString()!),
                    UpdatedAt = item.TryGetProperty("updatedAt", out var updatedAt) && updatedAt.ValueKind != JsonValueKind.Null
                        ? DateTime.Parse(updatedAt.GetString()!)
                        : null
                };

                profiles.Add(profile);
            }

            System.Diagnostics.Debug.WriteLine($"[MockProfileRepository] Loaded {profiles.Count} profiles from JSON");
            return profiles;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockProfileRepository] Error loading JSON: {ex.Message}");
            return new List<ProfileData>();
        }
    }

    public async Task<ProfileData?> GetByUserIdAsync(Guid userId)
    {
        await Task.Delay(300);
        var profile = _profiles.FirstOrDefault(p => p.UserId == userId);
        System.Diagnostics.Debug.WriteLine($"[MockProfileRepository] GetByUserIdAsync({userId}) - Found: {profile != null}");
        return profile;
    }

    public async Task<ProfileData> CreateAsync(ProfileData profile)
    {
        await Task.Delay(500);
        
        profile.CreatedAt = DateTime.UtcNow;
        profile.UpdatedAt = null;
        
        _profiles.Add(profile);
        
        System.Diagnostics.Debug.WriteLine($"[MockProfileRepository] Created profile for user: {profile.UserId}");
        return profile;
    }

    public async Task<ProfileData> UpdateAsync(ProfileData profile)
    {
        await Task.Delay(400);
        
        var existingProfile = _profiles.FirstOrDefault(p => p.UserId == profile.UserId);
        if (existingProfile == null)
        {
            throw new InvalidOperationException($"Profile for user {profile.UserId} not found");
        }

        existingProfile.Avatar = profile.Avatar;
        existingProfile.FullName = profile.FullName;
        existingProfile.PhoneNumber = profile.PhoneNumber;
        existingProfile.Address = profile.Address;
        existingProfile.JobTitle = profile.JobTitle;
        existingProfile.UpdatedAt = DateTime.UtcNow;

        System.Diagnostics.Debug.WriteLine($"[MockProfileRepository] Updated profile for user: {profile.UserId}");
        return existingProfile;
    }

    public async Task<bool> DeleteAsync(Guid userId)
    {
        await Task.Delay(300);
        
        var profile = _profiles.FirstOrDefault(p => p.UserId == userId);
        if (profile == null) return false;

        _profiles.Remove(profile);
        System.Diagnostics.Debug.WriteLine($"[MockProfileRepository] Deleted profile for user: {userId}");
        return true;
    }
}
