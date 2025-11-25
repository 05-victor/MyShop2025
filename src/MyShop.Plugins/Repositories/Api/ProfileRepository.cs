using MyShop.Core.Common;
using MyShop.Core.Interfaces.Repositories;
using MyShop.Plugins.API.Profile;
using MyShop.Shared.DTOs.Requests;
using MyShop.Shared.Models;
namespace MyShop.Plugins.Repositories.Api;

/// <summary>
/// API-based Profile Repository implementation
/// </summary>
public class ProfileRepository : IProfileRepository
{
    private readonly IProfileApi _api;

    public ProfileRepository(IProfileApi api)
    {
        _api = api;
    }

    public async Task<Result<ProfileData>> GetByUserIdAsync(Guid userId)
    {
        try
        {
            // Note: API uses JWT to identify user, so userId parameter is ignored
            var response = await _api.GetMyProfileAsync();
            
            if (response.IsSuccessStatusCode && response.Content != null)
            {
                var apiResponse = response.Content;
                if (apiResponse.Success && apiResponse.Result != null)
                {
                    var profile = MapToProfileData(apiResponse.Result);
                    return Result<ProfileData>.Success(profile);
                }
            }

            return Result<ProfileData>.Failure("Failed to retrieve profile");
        }
        catch (Exception ex)
        {
            return Result<ProfileData>.Failure($"Error retrieving profile: {ex.Message}");
        }
    }

    public async Task<Result<ProfileData>> CreateAsync(ProfileData profile)
    {
        // Note: Profile creation happens during user registration
        // This method may not be needed for typical flows
        return Result<ProfileData>.Failure("Profile is created automatically during user registration");
    }

    public async Task<Result<ProfileData>> UpdateAsync(ProfileData profile)
    {
        try
        {
            var request = new UpdateProfileRequest
            {
                FullName = profile.FullName,
                PhoneNumber = profile.PhoneNumber,
                Address = profile.Address,
                Avatar = profile.Avatar
            };

            var response = await _api.UpdateMyProfileAsync(request);
            
            if (response.IsSuccessStatusCode && response.Content != null)
            {
                var apiResponse = response.Content;
                if (apiResponse.Success && apiResponse.Result != null)
                {
                    var updatedProfile = MapToProfileData(apiResponse.Result);
                    return Result<ProfileData>.Success(updatedProfile);
                }
            }

            return Result<ProfileData>.Failure("Failed to update profile");
        }
        catch (Exception ex)
        {
            return Result<ProfileData>.Failure($"Error updating profile: {ex.Message}");
        }
    }

    public async Task<Result<bool>> DeleteAsync(Guid userId)
    {
        // Note: Profile deletion typically happens when user account is deleted
        // May need dedicated backend endpoint
        return Result<bool>.Failure("Profile deletion not supported via API");
    }

    /// <summary>
    /// Map ProfileResponse DTO to ProfileData domain model
    /// </summary>
    private static ProfileData MapToProfileData(MyShop.Shared.DTOs.Responses.ProfileResponse dto)
    {
        return new ProfileData
        {
            UserId = dto.UserId,
            Avatar = dto.Avatar,
            FullName = dto.FullName,
            PhoneNumber = dto.PhoneNumber,
            Email = dto.Email,
            Address = dto.Address,
            JobTitle = dto.JobTitle,
            CreatedAt = dto.CreatedAt,
            UpdatedAt = dto.UpdatedAt
        };
    }
}
