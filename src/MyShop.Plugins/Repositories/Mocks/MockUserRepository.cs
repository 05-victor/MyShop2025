using MyShop.Core.Common;
using MyShop.Core.Interfaces.Repositories;
using MyShop.Core.Interfaces.Infrastructure;
using MyShop.Shared.DTOs.Requests;
using MyShop.Shared.Models;
using MyShop.Plugins.Mocks.Data;

namespace MyShop.Plugins.Repositories.Mocks;

/// <summary>
/// Mock repository for user profile and account management
/// Uses MockUserData for data operations while maintaining ICredentialStorage dependency
/// </summary>
public class MockUserRepository : IUserRepository
{
    private readonly ICredentialStorage _credentialStorage;

    public MockUserRepository(ICredentialStorage credentialStorage)
    {
        _credentialStorage = credentialStorage;
    }

    public async Task<Result<bool>> HasAnyUsersAsync()
    {
        try
        {
            await Task.Delay(50); // Simulate database check
            var users = await MockUserData.GetAllAsync();
            var hasUsers = users.Count > 0;
            System.Diagnostics.Debug.WriteLine($"[MockUserRepository] HasAnyUsersAsync returned {hasUsers} ({users.Count} users)");
            return Result<bool>.Success(hasUsers);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockUserRepository] HasAnyUsersAsync error: {ex.Message}");
            return Result<bool>.Failure($"Failed to check users: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<User>>> GetAllAsync()
    {
        try
        {
            var users = await MockUserData.GetAllAsync();
            System.Diagnostics.Debug.WriteLine($"[MockUserRepository] GetAllAsync returned {users.Count} users");
            return Result<IEnumerable<User>>.Success(users);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockUserRepository] GetAllAsync error: {ex.Message}");
            return Result<IEnumerable<User>>.Failure($"Failed to get users: {ex.Message}");
        }
    }

    public async Task<Result<PagedList<User>>> GetPagedAsync(
        int page = 1,
        int pageSize = 20,
        string? role = null,
        string? status = null,
        string? searchQuery = null,
        string sortBy = "createdAt",
        bool sortDescending = true)
    {
        try
        {
            var (users, totalCount) = await MockUserData.GetPagedAsync(
                page, pageSize, role, status, searchQuery, sortBy, sortDescending);

            var pagedList = new PagedList<User>(users, totalCount, page, pageSize);

            System.Diagnostics.Debug.WriteLine($"[MockUserRepository] GetPagedAsync returned page {page}/{pagedList.TotalPages} ({users.Count} items, {totalCount} total)");
            return Result<PagedList<User>>.Success(pagedList);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockUserRepository] GetPagedAsync error: {ex.Message}");
            return Result<PagedList<User>>.Failure($"Failed to get paged users: {ex.Message}");
        }
    }

    public async Task<Result<User>> UpdateProfileAsync(UpdateProfileRequest request)
    {
        try
        {
            var token = _credentialStorage.GetToken();
            if (string.IsNullOrEmpty(token))
            {
                return Result<User>.Failure("Not authenticated. Please login again.");
            }

            // Get current user from token (mock implementation uses first user)
            var users = await MockUserData.GetAllAsync();
            var currentUser = users.FirstOrDefault();
            
            if (currentUser == null)
            {
                return Result<User>.Failure("User not found");
            }

            // Update user properties
            if (!string.IsNullOrWhiteSpace(request.Avatar))
                currentUser.Avatar = request.Avatar;

            if (!string.IsNullOrWhiteSpace(request.FullName))
                currentUser.FullName = request.FullName;

            if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
                currentUser.PhoneNumber = request.PhoneNumber;

            if (!string.IsNullOrWhiteSpace(request.Address))
                currentUser.Address = request.Address;

            var updated = await MockUserData.UpdateAsync(currentUser);
            System.Diagnostics.Debug.WriteLine($"[MockUserRepository] Profile updated for user: {updated.Username}");
            
            return Result<User>.Success(updated);
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
            await Task.Delay(600);

            var token = _credentialStorage.GetToken();
            if (string.IsNullOrEmpty(token))
            {
                return Result<bool>.Failure("Not authenticated. Please login again.");
            }

            // Simulate password verification (mock accepts "password123")
            if (request.CurrentPassword != "password123")
            {
                return Result<bool>.Failure("Current password is incorrect");
            }

            // Password validation
            if (request.NewPassword.Length < 6)
            {
                return Result<bool>.Failure("New password must be at least 6 characters");
            }

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

            var users = await MockUserData.GetAllAsync();
            var currentUser = users.FirstOrDefault();
            
            if (currentUser == null)
            {
                return Result<User>.Failure("User not found");
            }

            // Generate mock avatar URL
            var avatarUrl = $"https://api.myshop.com/avatars/{Guid.NewGuid()}.jpg";
            currentUser.Avatar = avatarUrl;

            var updated = await MockUserData.UpdateAsync(currentUser);
            System.Diagnostics.Debug.WriteLine($"[MockUserRepository] Avatar uploaded: {avatarUrl}");
            
            return Result<User>.Success(updated);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MockUserRepository] UploadAvatar error: {ex.Message}");
            return Result<User>.Failure("Failed to upload avatar", ex);
        }
    }
}
