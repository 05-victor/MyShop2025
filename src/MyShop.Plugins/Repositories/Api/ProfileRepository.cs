using MyShop.Plugins.Adapters;
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
                    var profile = ProfileAdapter.ToModel(apiResponse.Result);
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
                    var updatedProfile = ProfileAdapter.ToModel(apiResponse.Result);
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
    /// PATCH Update My Profile - uses the new PATCH endpoint
    /// </summary>
    public async Task<Result<ProfileData>> PatchUpdateMyProfileAsync(UpdateProfileRequest request)
    {
        try
        {
            var response = await _api.PatchUpdateMyProfileAsync(request);

            if (response.IsSuccessStatusCode && response.Content != null)
            {
                var apiResponse = response.Content;
                if (apiResponse.Success && apiResponse.Result != null)
                {
                    // UpdateProfileResponse only contains partial data, convert to ProfileData
                    var result = apiResponse.Result;
                    var profile = new ProfileData
                    {
                        Avatar = result.Avatar,
                        FullName = result.FullName,
                        PhoneNumber = result.PhoneNumber,
                        Address = result.Address
                    };
                    return Result<ProfileData>.Success(profile);
                }
            }

            return Result<ProfileData>.Failure("Failed to update profile");
        }
        catch (Exception ex)
        {
            return Result<ProfileData>.Failure($"Error updating profile: {ex.Message}");
        }
    }

    /// <summary>
    /// Upload Avatar to backend - streams file to server
    /// </summary>
    public async Task<Result<string>> UploadAvatarAsync(Stream fileStream, string fileName)
    {
        try
        {
            if (fileStream == null || !fileStream.CanRead)
            {
                return Result<string>.Failure("Invalid file stream");
            }

            // Infer content type from file extension
            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            var contentType = GetContentTypeFromExtension(extension);

            // Create StreamPart with proper content type
            var streamPart = new Refit.StreamPart(fileStream, fileName, contentType);
            var response = await _api.UploadAvatarAsync(streamPart);

            if (response.IsSuccessStatusCode && response.Content != null)
            {
                var apiResponse = response.Content;
                if (apiResponse.Success && apiResponse.Result != null)
                {
                    return Result<string>.Success(apiResponse.Result);
                }
            }

            return Result<string>.Failure("Failed to upload avatar");
        }
        catch (Exception ex)
        {
            return Result<string>.Failure($"Error uploading avatar: {ex.Message}");
        }
    }

    /// <summary>
    /// Helper to infer content type from file extension
    /// </summary>
    private string GetContentTypeFromExtension(string extension)
    {
        return extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            _ => "application/octet-stream"
        };
    }
}