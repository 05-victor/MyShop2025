using MyShop.Shared.DTOs.Requests;
using MyShop.Shared.DTOs.Responses;
using Refit;

namespace MyShop.Plugins.API.Profile;

/// <summary>
/// Refit interface for Profile API endpoints
/// </summary>
[Headers("User-Agent: MyShop-Client/1.0")]
public interface IProfileApi
{
    [Get("/api/v1/profile/me")]
    Task<Refit.ApiResponse<MyShop.Shared.DTOs.Common.ApiResponse<ProfileResponse>>> GetMyProfileAsync();

    [Put("/api/v1/profile/me")]
    Task<Refit.ApiResponse<MyShop.Shared.DTOs.Common.ApiResponse<ProfileResponse>>> UpdateMyProfileAsync([Body] UpdateProfileRequest request);

    [Patch("/api/v1/profiles/updateMyProfile")]
    Task<Refit.ApiResponse<MyShop.Shared.DTOs.Common.ApiResponse<UpdateProfileResponse>>> PatchUpdateMyProfileAsync([Body] UpdateProfileRequest request);

    [Multipart]
    [Post("/api/v1/profiles/uploadAvatar")]
    Task<Refit.ApiResponse<MyShop.Shared.DTOs.Common.ApiResponse<string>>> UploadAvatarAsync([AliasAs("file")] StreamPart file);



    [Post("/api/v1/profile/change-password")]
    Task<Refit.ApiResponse<MyShop.Shared.DTOs.Common.ApiResponse<bool>>> ChangePasswordAsync([Body] ChangePasswordRequest request);
}
