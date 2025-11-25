using MyShop.Core.Common;
using MyShop.Core.Interfaces.Repositories;
using MyShop.Plugins.API.Users;
using MyShop.Plugins.API.Profile;
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

    public async Task<Result<IEnumerable<User>>> GetAllAsync()
    {
        try
        {
            var response = await _api.GetAllAsync();
            
            if (response.IsSuccessStatusCode && response.Content != null)
            {
                var apiResponse = response.Content;
                if (apiResponse.Success && apiResponse.Result != null)
                {
                    var users = apiResponse.Result.Select(MapToUser).ToList();
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
                    var user = MapProfileToUser(apiResponse.Result);
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
            var response = await _profileApi.ChangePasswordAsync(request);
            
            if (response.IsSuccessStatusCode && response.Content?.Result == true)
            {
                return Result<bool>.Success(true);
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
                    var user = MapProfileToUser(apiResponse.Result);
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

    /// <summary>
    /// Map UserInfoResponse DTO to User domain model
    /// </summary>
    private static User MapToUser(MyShop.Shared.DTOs.Responses.UserInfoResponse dto)
    {
        return new User
        {
            Id = dto.Id,
            Username = dto.Username,
            Email = dto.Email,
            PhoneNumber = dto.PhoneNumber,
            Avatar = dto.Avatar,
            FullName = dto.FullName,
            Address = dto.Address,
            CreatedAt = dto.CreatedAt,
            IsTrialActive = dto.IsTrialActive,
            TrialStartDate = dto.TrialStartDate,
            TrialEndDate = dto.TrialEndDate,
            IsEmailVerified = dto.IsEmailVerified,
            Roles = MapRoles(dto.RoleNames)
        };
    }

    /// <summary>
    /// Map ProfileResponse DTO to User domain model
    /// </summary>
    private static User MapProfileToUser(MyShop.Shared.DTOs.Responses.ProfileResponse dto)
    {
        return new User
        {
            Id = dto.UserId,
            Username = dto.Username,
            Email = dto.Email,
            PhoneNumber = dto.PhoneNumber,
            Avatar = dto.Avatar,
            FullName = dto.FullName,
            Address = dto.Address,
            CreatedAt = dto.CreatedAt,
            IsEmailVerified = dto.IsEmailVerified
        };
    }

    /// <summary>
    /// Map role names to UserRole enum
    /// </summary>
    private static List<UserRole> MapRoles(List<string> roleNames)
    {
        var roles = new List<UserRole>();
        
        foreach (var roleName in roleNames)
        {
            if (Enum.TryParse<UserRole>(roleName, true, out var role))
            {
                roles.Add(role);
            }
        }

        return roles;
    }
}
