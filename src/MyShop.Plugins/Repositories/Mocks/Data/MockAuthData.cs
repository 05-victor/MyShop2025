using MyShop.Core.Common;
using MyShop.Shared.Models;
using MyShop.Shared.Models.Enums;
using System.Text.Json;

namespace MyShop.Plugins.Mocks.Data;

/// <summary>
/// Mock data provider for authentication - loads from JSON file
/// </summary>
public static class MockAuthData
{
    private static List<MockUserData>? _users;
    private static List<MockRoleData>? _roles;
    private static readonly object _lock = new object();
    private static readonly string _jsonFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Mocks", "Data", "Json", "users.json");

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
                    System.Diagnostics.Debug.WriteLine($"Auth JSON file not found at: {_jsonFilePath}");
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
                    _roles = data.Roles ?? new List<MockRoleData>();
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
        // Initialize empty lists - data should be loaded from users.json
        _users = new List<MockUserData>();
        
        // Roles are static and defined in the system
        _roles = new List<MockRoleData>
        {
            new MockRoleData { Name = "ADMIN", Description = "Administrator" },
            new MockRoleData { Name = "SALESAGENT", Description = "Sales Agent" },
            new MockRoleData { Name = "USER", Description = "Customer" }
        };
        
        System.Diagnostics.Debug.WriteLine("[MockAuthData] JSON file not found - initialized with empty user list");
    }

    public static async Task<Result<User>> LoginAsync(string usernameOrEmail, string password)
    {
        EnsureDataLoaded();
        
        // Simulate network delay
        // await Task.Delay(500);

        var user = _users!.FirstOrDefault(u =>
            (u.Username.Equals(usernameOrEmail, StringComparison.OrdinalIgnoreCase) ||
             u.Email.Equals(usernameOrEmail, StringComparison.OrdinalIgnoreCase)) &&
            u.Password == password);

        if (user == null)
        {
            return Result<User>.Failure("Invalid username/email or password");
        }

        // Convert role names to UserRole enum
        var roles = new List<UserRole>();
        foreach (var roleName in user.RoleNames)
        {
            if (roleName.Equals("ADMIN", StringComparison.OrdinalIgnoreCase))
                roles.Add(UserRole.Admin);
            else if (roleName.Equals("SALESAGENT", StringComparison.OrdinalIgnoreCase))
                roles.Add(UserRole.Salesman);
            else if (roleName.Equals("USER", StringComparison.OrdinalIgnoreCase))
                roles.Add(UserRole.Customer);
        }

        if (roles.Count == 0)
            roles.Add(UserRole.Customer);

        var userModel = new User
        {
            Id = Guid.Parse(user.Id),
            Username = user.Username,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            FullName = user.FullName,
            Avatar = user.Avatar,
            Address = user.Address,
            Roles = roles,
            Token = $"mock_token_{user.Id}_{Guid.NewGuid():N}",
            CreatedAt = user.CreatedAt,
            IsTrialActive = user.IsTrialActive,
            TrialStartDate = user.TrialStartDate,
            TrialEndDate = user.TrialEndDate,
            IsEmailVerified = user.IsEmailVerified
        };

        return Result<User>.Success(userModel);
    }

    public static async Task<Result<User>> RegisterAsync(string username, string email, string phoneNumber, string password, string role)
    {
        EnsureDataLoaded();
        
        // Simulate network delay
        // await Task.Delay(800);

        // Check if username exists
        if (_users!.Any(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase)))
        {
            return Result<User>.Failure("Username already exists");
        }

        // Check if email exists
        if (_users?.Any(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase)) == true)
        {
            return Result<User>.Failure("Email already registered");
        }

        // Parse role
        var roleName = role.ToUpper();
        if (roleName != "ADMIN" && roleName != "SALESAGENT" && roleName != "USER")
        {
            roleName = "USER";
        }

        UserRole userRole = roleName switch
        {
            "ADMIN" => UserRole.Admin,
            "SALESAGENT" => UserRole.Salesman,
            _ => UserRole.Customer
        };

        // Create new user
        var newUser = new MockUserData
        {
            Id = Guid.NewGuid().ToString(),
            Username = username,
            Email = email,
            PhoneNumber = phoneNumber,
            Password = password,
            RoleNames = new List<string> { roleName },
            IsEmailVerified = false,
            IsTrialActive = false,
            CreatedAt = DateTime.UtcNow
        };

        _users?.Add(newUser);

        var userModel = new User
        {
            Id = Guid.Parse(newUser.Id),
            Username = newUser.Username,
            Email = newUser.Email,
            PhoneNumber = newUser.PhoneNumber,
            Roles = new List<UserRole> { userRole },
            Token = $"mock_token_{newUser.Id}_{Guid.NewGuid():N}",
            CreatedAt = newUser.CreatedAt,
            IsEmailVerified = false
        };

        return Result<User>.Success(userModel);
    }

    public static async Task<Result<User>> GetCurrentUserAsync(string token)
    {
        EnsureDataLoaded();
        
        // Simulate network delay
        // await Task.Delay(300);

        // Extract user ID from token
        if (!token.StartsWith("mock_token_"))
        {
            return Result<User>.Failure("Invalid token");
        }

        var parts = token.Split('_');
        if (parts.Length < 3)
        {
            return Result<User>.Failure("Invalid token format");
        }

        var userId = parts[2];
        var user = _users!.FirstOrDefault(u => u.Id == userId);

        if (user == null)
        {
            return Result<User>.Failure("User not found");
        }

        // Convert role names to UserRole enum
        var roles = new List<UserRole>();
        foreach (var roleName in user.RoleNames)
        {
            if (roleName.Equals("ADMIN", StringComparison.OrdinalIgnoreCase))
                roles.Add(UserRole.Admin);
            else if (roleName.Equals("SALESAGENT", StringComparison.OrdinalIgnoreCase))
                roles.Add(UserRole.Salesman);
            else if (roleName.Equals("USER", StringComparison.OrdinalIgnoreCase))
                roles.Add(UserRole.Customer);
        }

        if (roles.Count == 0)
            roles.Add(UserRole.Customer);

        var userModel = new User
        {
            Id = Guid.Parse(user.Id),
            Username = user.Username,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            FullName = user.FullName,
            Avatar = user.Avatar,
            Address = user.Address,
            Roles = roles,
            Token = token,
            CreatedAt = user.CreatedAt,
            IsTrialActive = user.IsTrialActive,
            TrialStartDate = user.TrialStartDate,
            TrialEndDate = user.TrialEndDate,
            IsEmailVerified = user.IsEmailVerified
        };

        return Result<User>.Success(userModel);
    }

    public static async Task<Result<User>> ActivateTrialAsync(string token, string adminCode)
    {
        EnsureDataLoaded();
        
        // Simulate network delay
        // await Task.Delay(500);

        // Extract user ID from token
        if (!token.StartsWith("mock_token_"))
        {
            return Result<User>.Failure("Invalid token");
        }

        var parts = token.Split('_');
        if (parts.Length < 3)
        {
            return Result<User>.Failure("Invalid token format");
        }

        var userId = parts[2];
        var user = _users!.FirstOrDefault(u => u.Id == userId);

        if (user == null)
        {
            return Result<User>.Failure("User not found");
        }

        // Validate admin code using MockSystemActivationData
        var validCode = await MockSystemActivationData.ValidateCodeAsync(adminCode);

        if (validCode == null)
        {
            return Result<User>.Failure("Invalid or expired admin code");
        }

        // Mark code as used via ActivateCodeAsync
        await MockSystemActivationData.ActivateCodeAsync(adminCode, userId);

        // Deactivate trial for this user
        user.IsTrialActive = false;
        user.TrialStartDate = null;
        user.TrialEndDate = null;
        user.UpdatedAt = DateTime.UtcNow;

        // Persist changes to JSON file
        await SaveDataToJsonAsync();

        // Return updated user
        var roles = new List<UserRole>();
        foreach (var roleName in user.RoleNames)
        {
            if (roleName.Equals("ADMIN", StringComparison.OrdinalIgnoreCase))
                roles.Add(UserRole.Admin);
            else if (roleName.Equals("SALESAGENT", StringComparison.OrdinalIgnoreCase))
                roles.Add(UserRole.Salesman);
            else if (roleName.Equals("USER", StringComparison.OrdinalIgnoreCase))
                roles.Add(UserRole.Customer);
        }

        if (roles.Count == 0)
            roles.Add(UserRole.Customer);

        var userModel = new User
        {
            Id = Guid.Parse(user.Id),
            Username = user.Username,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            FullName = user.FullName,
            Avatar = user.Avatar,
            Address = user.Address,
            Roles = roles,
            Token = token,
            CreatedAt = user.CreatedAt,
            IsTrialActive = user.IsTrialActive,
            TrialStartDate = user.TrialStartDate,
            TrialEndDate = user.TrialEndDate,
            IsEmailVerified = user.IsEmailVerified
        };

        return Result<User>.Success(userModel);
    }

    private static async Task SaveDataToJsonAsync()
    {
        try
        {
            var container = new UserDataContainer
            {
                Users = _users!,
                Roles = _roles!
            };

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var jsonString = JsonSerializer.Serialize(container, options);
            await File.WriteAllTextAsync(_jsonFilePath, jsonString);
            
            System.Diagnostics.Debug.WriteLine("Successfully saved user data to JSON");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving users.json: {ex.Message}");
        }
    }

    /// <summary>
    /// Get user by ID for verification checks
    /// </summary>
    public static async Task<User?> GetUserByIdAsync(string userId)
    {
        EnsureDataLoaded();
        // await Task.Delay(100); // Simulate network delay

        var user = _users!.FirstOrDefault(u => u.Id.Equals(userId, StringComparison.OrdinalIgnoreCase));
        if (user == null) return null;

        // Convert to User model
        var roles = new List<UserRole>();
        foreach (var roleName in user.RoleNames)
        {
            if (roleName.Equals("ADMIN", StringComparison.OrdinalIgnoreCase))
                roles.Add(UserRole.Admin);
            else if (roleName.Equals("SALESAGENT", StringComparison.OrdinalIgnoreCase))
                roles.Add(UserRole.Salesman);
            else if (roleName.Equals("USER", StringComparison.OrdinalIgnoreCase))
                roles.Add(UserRole.Customer);
        }

        return new User
        {
            Id = Guid.Parse(user.Id),
            Username = user.Username,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            FullName = user.FullName,
            Avatar = user.Avatar,
            CreatedAt = user.CreatedAt,
            IsEmailVerified = user.IsEmailVerified,
            Roles = roles,
            Token = $"mock-token-{user.Id}"
        };
    }

    /// <summary>
    /// Update email verification status for a user
    /// </summary>
    public static async Task UpdateEmailVerificationAsync(string userId, bool isVerified)
    {
        EnsureDataLoaded();
        // await Task.Delay(100);

        var user = _users!.FirstOrDefault(u => u.Id.Equals(userId, StringComparison.OrdinalIgnoreCase));
        if (user != null)
        {
            user.IsEmailVerified = isVerified;
            System.Diagnostics.Debug.WriteLine($"[MockAuthData] Updated user {userId} IsEmailVerified = {isVerified}");
        }
    }

    // Data container classes for JSON deserialization
    private class UserDataContainer
    {
        public List<MockUserData> Users { get; set; } = new();
        public List<MockRoleData> Roles { get; set; } = new();
    }

    private class MockUserData
    {
        public string Id { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Avatar { get; set; }
        public string? FullName { get; set; }
        public string? Address { get; set; }
        public List<string> RoleNames { get; set; } = new();
        public bool IsTrialActive { get; set; }
        public DateTime? TrialStartDate { get; set; }
        public DateTime? TrialEndDate { get; set; }
        public bool IsEmailVerified { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    private class MockRoleData
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
