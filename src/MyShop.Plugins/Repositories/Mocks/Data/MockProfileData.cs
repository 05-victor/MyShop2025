using MyShop.Shared.Models;
using System.Text.Json;

namespace MyShop.Plugins.Mocks.Data;

/// <summary>
/// Mock data provider for profiles - loads from JSON file
/// </summary>
public static class MockProfileData
{
    private static List<ProfileDataModel>? _profiles;
    private static readonly object _lock = new object();
    private static readonly string _jsonFilePath = Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory, "Mocks", "Data", "Json", "profiles.json");

    private static void EnsureDataLoaded()
    {
        if (_profiles != null) return;

        lock (_lock)
        {
            if (_profiles != null) return;

            try
            {
                if (!File.Exists(_jsonFilePath))
                {
                    System.Diagnostics.Debug.WriteLine($"Profiles JSON file not found at: {_jsonFilePath}");
                    InitializeDefaultData();
                    return;
                }

                var jsonString = File.ReadAllText(_jsonFilePath);
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var data = JsonSerializer.Deserialize<ProfileDataContainer>(jsonString, options);

                if (data?.Profiles != null)
                {
                    _profiles = data.Profiles;
                    System.Diagnostics.Debug.WriteLine($"Loaded {_profiles.Count} profiles from profiles.json");
                }
                else
                {
                    InitializeDefaultData();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading profiles.json: {ex.Message}");
                InitializeDefaultData();
            }
        }
    }

    private static void InitializeDefaultData()
    {
        _profiles = new List<ProfileDataModel>();
    }

    public static async Task<ProfileData?> GetByUserIdAsync(Guid userId)
    {
        EnsureDataLoaded();

        // Simulate network delay
        await Task.Delay(250);

        var profileData = _profiles!.FirstOrDefault(p => p.UserId == userId.ToString());
        if (profileData == null) return null;

        return new ProfileData
        {
            UserId = Guid.Parse(profileData.UserId),
            Avatar = profileData.Avatar,
            FullName = profileData.FullName,
            PhoneNumber = profileData.PhoneNumber,
            Email = profileData.Email,
            Address = profileData.Address,
            JobTitle = profileData.JobTitle,
            CreatedAt = profileData.CreatedAt,
            UpdatedAt = profileData.UpdatedAt
        };
    }

    public static async Task<ProfileData> CreateAsync(ProfileData profile)
    {
        EnsureDataLoaded();

        // Simulate network delay
        await Task.Delay(400);

        var newProfile = new ProfileDataModel
        {
            UserId = profile.UserId.ToString(),
            Avatar = profile.Avatar,
            FullName = profile.FullName,
            PhoneNumber = profile.PhoneNumber,
            Email = profile.Email,
            Address = profile.Address,
            JobTitle = profile.JobTitle,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null
        };

        _profiles!.Add(newProfile);

        // Persist to JSON
        await SaveDataToJsonAsync();

        return profile;
    }

    public static async Task<ProfileData> UpdateAsync(ProfileData profile)
    {
        EnsureDataLoaded();

        // Simulate network delay
        await Task.Delay(400);

        var existing = _profiles!.FirstOrDefault(p => p.UserId == profile.UserId.ToString());
        
        if (existing == null)
        {
            // Create new profile
            existing = new ProfileDataModel
            {
                UserId = profile.UserId.ToString(),
                CreatedAt = DateTime.UtcNow
            };
            _profiles.Add(existing);
        }

        // Update properties
        existing.Avatar = profile.Avatar;
        existing.FullName = profile.FullName;
        existing.PhoneNumber = profile.PhoneNumber;
        existing.Email = profile.Email;
        existing.Address = profile.Address;
        existing.JobTitle = profile.JobTitle;
        existing.UpdatedAt = DateTime.UtcNow;

        // Persist to JSON
        await SaveDataToJsonAsync();

        return profile;
    }

    public static async Task<bool> DeleteAsync(Guid userId)
    {
        EnsureDataLoaded();

        // Simulate network delay
        await Task.Delay(300);

        var profile = _profiles!.FirstOrDefault(p => p.UserId == userId.ToString());
        if (profile == null) return false;

        _profiles.Remove(profile);

        // Persist to JSON
        await SaveDataToJsonAsync();

        return true;
    }

    private static async Task SaveDataToJsonAsync()
    {
        try
        {
            var container = new ProfileDataContainer
            {
                Profiles = _profiles!
            };

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var jsonString = JsonSerializer.Serialize(container, options);
            await File.WriteAllTextAsync(_jsonFilePath, jsonString);

            System.Diagnostics.Debug.WriteLine("Successfully saved profiles data to JSON");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving profiles.json: {ex.Message}");
        }
    }

    // Data container classes for JSON deserialization
    private class ProfileDataContainer
    {
        public List<ProfileDataModel> Profiles { get; set; } = new();
    }

    private class ProfileDataModel
    {
        public string UserId { get; set; } = string.Empty;
        public string? Avatar { get; set; }
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public string? JobTitle { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
