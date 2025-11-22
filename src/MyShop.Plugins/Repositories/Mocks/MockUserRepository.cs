using MyShop.Core.Common;
using MyShop.Core.Interfaces.Repositories;
using MyShop.Core.Interfaces.Infrastructure;
using MyShop.Shared.DTOs.Requests;
using MyShop.Shared.Models;

namespace MyShop.Plugins.Repositories.Mocks;

/// <summary>
/// Mock repository for user profile and account management
/// Uses in-memory user data with simulated delays
/// </summary>
public class MockUserRepository : IUserRepository
{
    private readonly ICredentialStorage _credentialStorage;
    private User? _currentUser;

    public MockUserRepository(ICredentialStorage credentialStorage)
    {
        _credentialStorage = credentialStorage;
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        await Task.Delay(300);
        // Return mock users for admin management
        return new List<User>();
    }

    public async Task<Result<User>> UpdateProfileAsync(UpdateProfileRequest request)
    {
        try
        {
            await Task.Delay(500); // Simulate network delay

            var token = _credentialStorage.GetToken();
            if (string.IsNullOrEmpty(token))
            {
                return Result<User>.Failure("Not authenticated. Please login again.");
            }

            // Simulate getting current user from token
            _currentUser ??= GetMockUser();

            // Update user properties
            if (!string.IsNullOrWhiteSpace(request.Avatar))
                _currentUser.Avatar = request.Avatar;

            if (!string.IsNullOrWhiteSpace(request.FullName))
                _currentUser.FullName = request.FullName;

            if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
                _currentUser.PhoneNumber = request.PhoneNumber;

            if (!string.IsNullOrWhiteSpace(request.Address))
                _currentUser.Address = request.Address;

            System.Diagnostics.Debug.WriteLine($"[MockUserRepository] Profile updated for user: {_currentUser.Username}");
            
            return Result<User>.Success(_currentUser);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockUserRepository] UpdateProfile error: {ex.Message}");
            return Result<User>.Failure("Failed to update profile", ex);
        }
    }

    public async Task<Result<bool>> ChangePasswordAsync(ChangePasswordRequest request)
    {
        try
        {
            await Task.Delay(600); // Simulate network delay

            var token = _credentialStorage.GetToken();
            if (string.IsNullOrEmpty(token))
            {
                return Result<bool>.Failure("Not authenticated. Please login again.");
            }

            // Simulate password verification (mock always accepts "password123" as current)
            if (request.CurrentPassword != "password123")
            {
                return Result<bool>.Failure("Current password is incorrect");
            }

            // Simulate password strength validation
            if (request.NewPassword.Length < 6)
            {
                return Result<bool>.Failure("New password must be at least 6 characters");
            }

            // Simulate check: new password must differ from current
            if (request.CurrentPassword == request.NewPassword)
            {
                return Result<bool>.Failure("New password must differ from current password");
            }

            System.Diagnostics.Debug.WriteLine($"[MockUserRepository] Password changed successfully");
            
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockUserRepository] ChangePassword error: {ex.Message}");
            return Result<bool>.Failure("Failed to change password", ex);
        }
    }

    public async Task<Result<User>> UploadAvatarAsync(byte[] imageBytes, string fileName, IProgress<double>? progress = null)
    {
        try
        {
            // Simulate upload with progress
            for (int i = 0; i <= 100; i += 20)
            {
                await Task.Delay(100);
                progress?.Report(i / 100.0);
            }

            var token = _credentialStorage.GetToken();
            if (string.IsNullOrEmpty(token))
            {
                return Result<User>.Failure("Not authenticated. Please login again.");
            }

            // Validate file size (max 5MB)
            if (imageBytes.Length > 5 * 1024 * 1024)
            {
                return Result<User>.Failure("Image file is too large (max 5MB)");
            }

            _currentUser ??= GetMockUser();

            // Simulate avatar URL (in real app, this would be server-generated URL)
            var avatarUrl = $"https://api.myshop.com/avatars/{Guid.NewGuid()}.jpg";
            _currentUser.Avatar = avatarUrl;

            System.Diagnostics.Debug.WriteLine($"[MockUserRepository] Avatar uploaded: {avatarUrl}");
            
            return Result<User>.Success(_currentUser);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockUserRepository] UploadAvatar error: {ex.Message}");
            return Result<User>.Failure("Failed to upload avatar", ex);
        }
    }

    private User GetMockUser()
    {
        return new User
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Username = "admin",
            Email = "admin@example.com",
            PhoneNumber = "0123456789",
            FullName = "Administrator",
            Avatar = "https://via.placeholder.com/150",
            Address = "123 Main St, Hanoi, Vietnam",
            CreatedAt = DateTime.UtcNow.AddMonths(-6),
            IsEmailVerified = true,
            Roles = new List<MyShop.Shared.Models.Enums.UserRole> 
            { 
                MyShop.Shared.Models.Enums.UserRole.Admin 
            },
            Token = _credentialStorage.GetToken() ?? string.Empty
        };
    }
}
