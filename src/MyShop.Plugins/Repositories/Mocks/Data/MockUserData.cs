using MyShop.Shared.Models;
using MyShop.Shared.Models.Enums;
using System.Text.Json;

namespace MyShop.Plugins.Mocks.Data;

/// <summary>
/// Mock data provider for users - loads from JSON file
/// </summary>
public static class MockUserData
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
        _users = new List<UserDataModel>
        {
            new UserDataModel
            {
                Id = "00000000-0000-0000-0000-000000000001",
                Username = "admin",
                Email = "admin@myshop.com",
                PhoneNumber = "+84901000001",
                FullName = "Nguyễn Quản Trị",
                Role = "Admin",
                Status = "Active",
                EmailVerified = true,
                CreatedAt = DateTime.Parse("2023-01-15T00:00:00Z")
            }
        };
    }

    public static async Task<List<User>> GetAllAsync()
    {
        EnsureDataLoaded();

        // Simulate network delay
        await Task.Delay(350);

        return _users!.Select(MapToUser).ToList();
    }

    public static async Task<User?> GetByIdAsync(Guid id)
    {
        EnsureDataLoaded();

        // Simulate network delay
        await Task.Delay(200);

        var userData = _users!.FirstOrDefault(u => u.Id == id.ToString());
        if (userData == null) return null;

        return MapToUser(userData);
    }

    public static async Task<User> CreateAsync(User user)
    {
        EnsureDataLoaded();

        // Simulate network delay
        await Task.Delay(500);

        // Check if username exists
        if (_users!.Any(u => u.Username.Equals(user.Username, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException("Username already exists");
        }

        // Check if email exists
        if (_users.Any(u => u.Email.Equals(user.Email, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException("Email already registered");
        }

        var newUserData = new UserDataModel
        {
            Id = user.Id.ToString(),
            Username = user.Username,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            FullName = user.FullName,
            Role = user.Roles?.FirstOrDefault().ToString() ?? "Customer",
            Status = "Active",
            EmailVerified = false,
            AvatarUrl = user.Avatar,
            CreatedAt = DateTime.UtcNow
        };

        _users.Add(newUserData);

        // Persist to JSON
        await SaveDataToJsonAsync();

        return user;
    }

    public static async Task<User> UpdateAsync(User user)
    {
        EnsureDataLoaded();

        // Simulate network delay
        await Task.Delay(400);

        var existing = _users!.FirstOrDefault(u => u.Id == user.Id.ToString());
        if (existing == null)
        {
            throw new InvalidOperationException($"User with ID {user.Id} not found");
        }

        // Update properties
        existing.Email = user.Email;
        existing.PhoneNumber = user.PhoneNumber;
        existing.FullName = user.FullName;
        existing.AvatarUrl = user.Avatar;
        existing.LastLoginAt = user.LastLogin;

        // Persist to JSON
        await SaveDataToJsonAsync();

        return user;
    }

    public static async Task<bool> DeleteAsync(Guid id)
    {
        EnsureDataLoaded();

        // Simulate network delay
        await Task.Delay(350);

        var user = _users!.FirstOrDefault(u => u.Id == id.ToString());
        if (user == null) return false;

        _users.Remove(user);

        // Persist to JSON
        await SaveDataToJsonAsync();

        return true;
    }

    public static async Task<List<User>> GetByRoleAsync(string role)
    {
        EnsureDataLoaded();

        // Simulate network delay
        await Task.Delay(300);

        return _users!
            .Where(u => u.Role.Equals(role, StringComparison.OrdinalIgnoreCase))
            .Select(MapToUser)
            .ToList();
    }

    public static async Task<List<User>> SearchAsync(string query)
    {
        EnsureDataLoaded();

        // Simulate network delay
        await Task.Delay(350);

        if (string.IsNullOrWhiteSpace(query))
        {
            return await GetAllAsync();
        }

        var lowerQuery = query.ToLower();
        return _users!
            .Where(u => u.Username.ToLower().Contains(lowerQuery) ||
                       (u.Email != null && u.Email.ToLower().Contains(lowerQuery)) ||
                       (u.FullName != null && u.FullName.ToLower().Contains(lowerQuery)))
            .Select(MapToUser)
            .ToList();
    }

    private static User MapToUser(UserDataModel data)
    {
        // Parse role
        var role = UserRole.Customer;
        if (data.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            role = UserRole.Admin;
        else if (data.Role.Equals("Salesman", StringComparison.OrdinalIgnoreCase))
            role = UserRole.Salesman;

        return new User
        {
            Id = Guid.Parse(data.Id),
            Username = data.Username,
            Email = data.Email,
            PhoneNumber = data.PhoneNumber,
            FullName = data.FullName,
            Avatar = data.AvatarUrl,
            Roles = new List<UserRole> { role },
            Token = $"mock_token_{data.Id}",
            CreatedAt = data.CreatedAt,
            LastLogin = data.LastLoginAt,
            IsEmailVerified = data.EmailVerified
        };
    }

    private static async Task SaveDataToJsonAsync()
    {
        try
        {
            var container = new UserDataContainer
            {
                Users = _users!
            };

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var jsonString = JsonSerializer.Serialize(container, options);
            await File.WriteAllTextAsync(_jsonFilePath, jsonString);

            System.Diagnostics.Debug.WriteLine("Successfully saved users data to JSON");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving users.json: {ex.Message}");
        }
    }

    // Data container classes for JSON deserialization
    private class UserDataContainer
    {
        public List<UserDataModel> Users { get; set; } = new();
    }

    private class UserDataModel
    {
        public string Id { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? FullName { get; set; }
        public string Role { get; set; } = "Customer";
        public string Status { get; set; } = "Active";
        public bool EmailVerified { get; set; }
        public string? AvatarUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
    }
}
