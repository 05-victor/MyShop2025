using MyShop.Core.Common;
using MyShop.Shared.Models;
using MyShop.Shared.Models.Enums;

namespace MyShop.Plugins.Mocks.Data;

/// <summary>
/// Mock data provider for authentication
/// </summary>
public static class MockAuthData
{
    private static readonly List<MockUserData> _users = new()
    {
        new MockUserData
        {
            Id = "00000000-0000-0000-0000-000000000001",
            Username = "admin",
            Email = "admin@myshop.com",
            Password = "admin123",
            PhoneNumber = "0901234567",
            Role = UserRole.Admin,
            CreatedAt = DateTime.Now.AddMonths(-6)
        },
        new MockUserData
        {
            Id = "00000000-0000-0000-0000-000000000002",
            Username = "salesman",
            Email = "salesman@myshop.com",
            Password = "sales123",
            PhoneNumber = "0902345678",
            Role = UserRole.Salesman,
            CreatedAt = DateTime.Now.AddMonths(-3)
        },
        new MockUserData
        {
            Id = "00000000-0000-0000-0000-000000000003",
            Username = "customer",
            Email = "customer@myshop.com",
            Password = "customer123",
            PhoneNumber = "0903456789",
            Role = UserRole.Customer,
            CreatedAt = DateTime.Now.AddMonths(-1)
        },
        new MockUserData
        {
            Id = "00000000-0000-0000-0000-000000000004",
            Username = "johndoe",
            Email = "john.doe@email.com",
            Password = "john123",
            PhoneNumber = "0904567890",
            Role = UserRole.Customer,
            CreatedAt = DateTime.Now.AddDays(-15)
        }
    };

    public static async Task<Result<User>> LoginAsync(string usernameOrEmail, string password)
    {
        // Simulate network delay
        await Task.Delay(500);

        var user = _users.FirstOrDefault(u =>
            (u.Username.Equals(usernameOrEmail, StringComparison.OrdinalIgnoreCase) ||
             u.Email.Equals(usernameOrEmail, StringComparison.OrdinalIgnoreCase)) &&
            u.Password == password);

        if (user == null)
        {
            return Result<User>.Failure("Invalid username/email or password");
        }

        var userModel = new User
        {
            Id = Guid.Parse(user.Id),
            Username = user.Username,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            Roles = new List<UserRole> { user.Role },
            Token = $"mock_token_{user.Id}_{Guid.NewGuid():N}",
            CreatedAt = user.CreatedAt
        };

        return Result<User>.Success(userModel);
    }

    public static async Task<Result<User>> RegisterAsync(string username, string email, string phoneNumber, string password, string role)
    {
        // Simulate network delay
        await Task.Delay(800);

        // Check if username exists
        if (_users.Any(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase)))
        {
            return Result<User>.Failure("Username already exists");
        }

        // Check if email exists
        if (_users.Any(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase)))
        {
            return Result<User>.Failure("Email already registered");
        }

        // Parse role
        if (!Enum.TryParse<UserRole>(role, true, out var userRole))
        {
            userRole = UserRole.Customer;
        }

        // Create new user
        var newUser = new MockUserData
        {
            Id = Guid.NewGuid().ToString(),
            Username = username,
            Email = email,
            PhoneNumber = phoneNumber,
            Password = password,
            Role = userRole,
            CreatedAt = DateTime.Now
        };

        _users.Add(newUser);

        var userModel = new User
        {
            Id = Guid.Parse(newUser.Id),
            Username = newUser.Username,
            Email = newUser.Email,
            PhoneNumber = newUser.PhoneNumber,
            Roles = new List<UserRole> { newUser.Role },
            Token = $"mock_token_{newUser.Id}_{Guid.NewGuid():N}",
            CreatedAt = newUser.CreatedAt
        };

        return Result<User>.Success(userModel);
    }

    public static async Task<Result<User>> GetCurrentUserAsync(string token)
    {
        // Simulate network delay
        await Task.Delay(300);

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
        var user = _users.FirstOrDefault(u => u.Id == userId);

        if (user == null)
        {
            return Result<User>.Failure("User not found");
        }

        var userModel = new User
        {
            Id = Guid.Parse(user.Id),
            Username = user.Username,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            Roles = new List<UserRole> { user.Role },
            Token = token,
            CreatedAt = user.CreatedAt
        };

        return Result<User>.Success(userModel);
    }

    private class MockUserData
    {
        public string Id { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
