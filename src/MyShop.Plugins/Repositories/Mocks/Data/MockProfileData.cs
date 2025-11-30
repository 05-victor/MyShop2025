using MyShop.Shared.Models;
using System.Text.Json;

namespace MyShop.Plugins.Mocks.Data;

/// <summary>
/// Mock data provider for profiles - reads from users.json
/// Profile data is part of user data, not separate
/// </summary>
public static class MockProfileData
{
    private static List<UserDataModel>? _users;
    private static readonly object _lock = new object();
    private static readonly string _jsonFilePath = Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory, "Mocks", "Data", "Json", "users.json");

    private static void EnsureDataLoaded()
    {
        if (_users != null) return;

        lock (_lock)
        {
            if (_users != null) return;

            try
            {
                if (!File.Exists(_jsonFilePath))
                {
                    System.Diagnostics.Debug.WriteLine($"Users JSON file not found at: {_jsonFilePath}");
                    InitializeDefaultData();
                    return;
                }

                var jsonString = File.ReadAllText(_jsonFilePath);
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var data = JsonSerializer.Deserialize<UserDataContainer>(jsonString, options);

                if (data?.Users != null)
                {
                    _users = data.Users;
                    System.Diagnostics.Debug.WriteLine($"Loaded {_users.Count} users from users.json");
                }
                else
                {
                    InitializeDefaultData();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading users.json: {ex.Message}");
                InitializeDefaultData();
            }
        }
    }

    private static void InitializeDefaultData()
    {
        _users = new List<UserDataModel>();
    }

    public static async Task<ProfileData?> GetByUserIdAsync(Guid userId)
    {
        EnsureDataLoaded();
        // await Task.Delay(250); // Simulate network delay

        var user = _users!.FirstOrDefault(u => u.Id == userId.ToString());
        if (user == null) return null;

        return new ProfileData
        {
            UserId = Guid.Parse(user.Id),
            Avatar = user.Avatar,
            FullName = user.FullName,
            PhoneNumber = user.PhoneNumber,
            Email = user.Email,
            Address = user.Address,
            JobTitle = user.JobTitle,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };
    }

    public static async Task<ProfileData> CreateAsync(ProfileData profile)
    {
        EnsureDataLoaded();
        // await Task.Delay(400); // Simulate network delay

        // For profile creation, we need to update existing user or create new one
        var user = _users!.FirstOrDefault(u => u.Id == profile.UserId.ToString());
        
        if (user != null)
        {
            // Update existing user's profile fields
            user.Avatar = profile.Avatar;
            user.FullName = profile.FullName;
            user.PhoneNumber = profile.PhoneNumber;
            user.Email = profile.Email;
            user.Address = profile.Address;
            user.JobTitle = profile.JobTitle;
            user.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            // Should not happen in normal flow, but handle it
            System.Diagnostics.Debug.WriteLine($"[MockProfileData] Warning: User {profile.UserId} not found for profile creation");
        }

        await SaveDataToJsonAsync();
        return profile;
    }

    public static async Task<ProfileData> UpdateAsync(ProfileData profile)
    {
        EnsureDataLoaded();
        // await Task.Delay(400); // Simulate network delay

        var user = _users!.FirstOrDefault(u => u.Id == profile.UserId.ToString());
        
        if (user != null)
        {
            // Update user's profile fields
            user.Avatar = profile.Avatar;
            user.FullName = profile.FullName;
            user.PhoneNumber = profile.PhoneNumber;
            user.Email = profile.Email;
            user.Address = profile.Address;
            user.JobTitle = profile.JobTitle;
            user.UpdatedAt = DateTime.UtcNow;

            await SaveDataToJsonAsync();
        }
        else
        {
            System.Diagnostics.Debug.WriteLine($"[MockProfileData] Warning: User {profile.UserId} not found for profile update");
        }

        return profile;
    }

    public static async Task<bool> DeleteAsync(Guid userId)
    {
        EnsureDataLoaded();
        // await Task.Delay(300); // Simulate network delay

        var user = _users!.FirstOrDefault(u => u.Id == userId.ToString());
        if (user == null) return false;

        // Clear profile fields (don't delete user, just clear profile data)
        user.Avatar = null;
        user.FullName = null;
        user.Address = null;
        user.JobTitle = null;
        user.UpdatedAt = DateTime.UtcNow;

        await SaveDataToJsonAsync();
        return true;
    }

    private static async Task SaveDataToJsonAsync()
    {
        try
        {
            // Need to read current data to preserve roles
            var currentJson = await File.ReadAllTextAsync(_jsonFilePath);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            var currentData = JsonSerializer.Deserialize<UserDataContainer>(currentJson, options);

            var container = new UserDataContainer
            {
                Users = _users!,
                Roles = currentData?.Roles ?? new List<RoleDataModel>()
            };

            var writeOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var jsonString = JsonSerializer.Serialize(container, writeOptions);
            await File.WriteAllTextAsync(_jsonFilePath, jsonString);

            System.Diagnostics.Debug.WriteLine("Successfully saved user data to JSON");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving users.json: {ex.Message}");
        }
    }

    // Data container classes for JSON serialization/deserialization
    private class UserDataContainer
    {
        public List<UserDataModel> Users { get; set; } = new();
        public List<RoleDataModel> Roles { get; set; } = new();
    }

    private class UserDataModel
    {
        public string Id { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Password { get; set; }
        public string? PhoneNumber { get; set; }
        public string? FullName { get; set; }
        public string? Avatar { get; set; }
        public string? Address { get; set; }
        public string? JobTitle { get; set; }
        public List<string> RoleNames { get; set; } = new();
        public string? Status { get; set; }
        public bool IsEmailVerified { get; set; }
        public bool IsTrialActive { get; set; }
        public DateTime? TrialStartDate { get; set; }
        public DateTime? TrialEndDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
    }

    private class RoleDataModel
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
