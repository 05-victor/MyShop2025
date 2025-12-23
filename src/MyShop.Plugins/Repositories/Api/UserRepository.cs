using MyShop.Plugins.Adapters;
using MyShop.Core.Common;
using MyShop.Core.Interfaces.Repositories;
using MyShop.Plugins.API.Users;
using MyShop.Plugins.API.Profile;
using MyShop.Shared.DTOs.Commons;
using MyShop.Shared.DTOs.Requests;
using MyShop.Shared.Models;
using MyShop.Shared.Models.Enums;

namespace MyShop.Plugins.Repositories.Api;

/// <summary>
/// API-based User Repository implementation
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly IUsersApi _api;
    private readonly IProfileApi _profileApi;

    public UserRepository(IUsersApi api, IProfileApi profileApi)
    {
        _api = api;
        _profileApi = profileApi;
    }

    public async Task<Result<bool>> HasAnyUsersAsync()
    {
        try
        {
            var allUsersResult = await GetAllAsync();
            if (!allUsersResult.IsSuccess)
            {
                return Result<bool>.Failure(allUsersResult.ErrorMessage ?? "Failed to check users");
            }

            var hasUsers = allUsersResult.Data?.Any() == true;
            return Result<bool>.Success(hasUsers);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Error checking users: {ex.Message}");
        }
    }

    public async Task<Result<IEnumerable<User>>> GetAllAsync()
    {
        try
        {
            var response = await _api.GetAllAsync(pageNumber: 1, pageSize: int.MaxValue);

            if (response.IsSuccessStatusCode && response.Content != null)
            {
                var apiResponse = response.Content;
                if (apiResponse.Success && apiResponse.Result != null)
                {
                    var users = UserAdapter.ToModelList(apiResponse.Result.Items);
                    return Result<IEnumerable<User>>.Success(users);
                }
            }

            return Result<IEnumerable<User>>.Failure("Failed to retrieve users");
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<User>>.Failure($"Error retrieving users: {ex.Message}");
        }
    }

    public async Task<PagedResult<User>> GetAllAsync(int pageNumber, int pageSize)
    {
        try
        {
            var response = await _api.GetAllAsync(pageNumber, pageSize);

            if (response.IsSuccessStatusCode && response.Content != null)
            {
                var apiResponse = response.Content;
                if (apiResponse.Success && apiResponse.Result != null)
                {
                    return new PagedResult<User>
                    {
                        Items = UserAdapter.ToModelList(apiResponse.Result.Items),
                        TotalCount = apiResponse.Result.TotalCount,
                        Page = apiResponse.Result.Page,
                        PageSize = apiResponse.Result.PageSize
                    };
                }
            }

            return new PagedResult<User>
            {
                Items = new List<User>(),
                TotalCount = 0,
                Page = pageNumber,
                PageSize = pageSize
            };
        }
        catch (Exception)
        {
            return new PagedResult<User>
            {
                Items = new List<User>(),
                TotalCount = 0,
                Page = pageNumber,
                PageSize = pageSize
            };
        }
    }

    public async Task<Result<User>> UpdateProfileAsync(UpdateProfileRequest request)
    {
        try
        {
            var response = await _profileApi.UpdateMyProfileAsync(request);

            if (response.IsSuccessStatusCode && response.Content != null)
            {
                var apiResponse = response.Content;
                if (apiResponse.Success && apiResponse.Result != null)
                {
                    var user = ProfileAdapter.ToUserModel(apiResponse.Result);
                    return Result<User>.Success(user);
                }
            }

            return Result<User>.Failure("Failed to update profile");
        }
        catch (Exception ex)
        {
            return Result<User>.Failure($"Error updating profile: {ex.Message}");
        }
    }

    public async Task<Result<bool>> ChangePasswordAsync(ChangePasswordRequest request)
    {
        try
        {
            var response = await _api.ChangePasswordAsync(request);

            if (response.IsSuccessStatusCode && response.Content != null)
            {
                var apiResponse = response.Content;
                if (apiResponse.Success && apiResponse.Result)
                {
                    return Result<bool>.Success(true);
                }
                
                // If Result is false, it means current password was incorrect
                if (apiResponse.Success && !apiResponse.Result)
                {
                    return Result<bool>.Failure("Current password is incorrect");
                }
            }

            return Result<bool>.Failure("Failed to change password");
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Error changing password: {ex.Message}");
        }
    }

    public async Task<Result<User>> UploadAvatarAsync(byte[] imageBytes, string fileName, IProgress<double>? progress = null)
    {
        try
        {
            // Note: Backend may need dedicated file upload endpoint
            // For now, update avatar URL via profile update
            var base64Image = Convert.ToBase64String(imageBytes);
            var avatarUrl = $"data:image/png;base64,{base64Image}";

            var request = new UpdateProfileRequest
            {
                Avatar = avatarUrl
            };

            var response = await _profileApi.UpdateMyProfileAsync(request);

            if (response.IsSuccessStatusCode && response.Content != null)
            {
                var apiResponse = response.Content;
                if (apiResponse.Success && apiResponse.Result != null)
                {
                    var user = ProfileAdapter.ToUserModel(apiResponse.Result);
                    return Result<User>.Success(user);
                }
            }

            return Result<User>.Failure("Failed to upload avatar");
        }
        catch (Exception ex)
        {
            return Result<User>.Failure($"Error uploading avatar: {ex.Message}");
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
            // Note: Backend API doesn't support server-side paging yet
            // Fallback: fetch all users and apply client-side paging/filtering
            var allUsersResult = await GetAllAsync();
            if (!allUsersResult.IsSuccess || allUsersResult.Data == null)
            {
                return Result<PagedList<User>>.Failure(allUsersResult.ErrorMessage ?? "Failed to retrieve users");
            }

            var query = allUsersResult.Data.AsEnumerable();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                var search = searchQuery.ToLower();
                query = query.Where(u =>
                    u.Username.ToLower().Contains(search) ||
                    u.Email.ToLower().Contains(search) ||
                    (u.FullName != null && u.FullName.ToLower().Contains(search)));
            }

            if (!string.IsNullOrWhiteSpace(role))
            {
                if (Enum.TryParse<UserRole>(role, true, out var userRole))
                {
                    query = query.Where(u => u.HasRole(userRole));
                }
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                // Note: User model doesn't have Status property yet
                // For now, filter based on trial status or skip
                // TODO: Add Status property to User model or use alternative filtering
                var isActive = status.Equals("active", StringComparison.OrdinalIgnoreCase);
                if (isActive)
                {
                    query = query.Where(u => !u.IsTrialActive || (u.TrialEndDate == null || u.TrialEndDate > DateTime.UtcNow));
                }
            }

            // Apply sorting
            query = sortBy.ToLower() switch
            {
                "username" => sortDescending
                    ? query.OrderByDescending(u => u.Username)
                    : query.OrderBy(u => u.Username),
                "email" => sortDescending
                    ? query.OrderByDescending(u => u.Email)
                    : query.OrderBy(u => u.Email),
                "fullname" => sortDescending
                    ? query.OrderByDescending(u => u.FullName)
                    : query.OrderBy(u => u.FullName),
                "role" => sortDescending
                    ? query.OrderByDescending(u => u.GetPrimaryRole())
                    : query.OrderBy(u => u.GetPrimaryRole()),
                "createdat" => sortDescending
                    ? query.OrderByDescending(u => u.CreatedAt)
                    : query.OrderBy(u => u.CreatedAt),
                _ => sortDescending
                    ? query.OrderByDescending(u => u.CreatedAt)
                    : query.OrderBy(u => u.CreatedAt)
            };

            var totalCount = query.Count();
            var items = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            var pagedList = new PagedList<User>(items, totalCount, page, pageSize);
            return Result<PagedList<User>>.Success(pagedList);
        }
        catch (Exception ex)
        {
            return Result<PagedList<User>>.Failure($"Error retrieving paged users: {ex.Message}");
        }
    }

    public async Task<Result<User>> CreateUserAsync(User user)
    {
        try
        {
            // TODO: Call actual backend API when available
            // For now, return failure since API endpoint is not implemented
            // Backend should implement POST /api/users endpoint
            System.Diagnostics.Debug.WriteLine($"[UserRepository] CreateUserAsync called but API not implemented - User: {user.Username}");

            // Set default avatar if not provided
            if (string.IsNullOrEmpty(user.Avatar))
            {
                user.Avatar = "ms-appx:///Assets/Images/user/avatar-placeholder.png";
            }

            return Result<User>.Failure("User creation via API not implemented. Please contact administrator.");
        }
        catch (Exception ex)
        {
            return Result<User>.Failure($"Error creating user: {ex.Message}");
        }
    }
}
