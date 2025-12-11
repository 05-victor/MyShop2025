using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyShop.Server.Services.Interfaces;
using MyShop.Server.Services.Implementations;
using MyShop.Shared.DTOs.Requests;
using MyShop.Shared.DTOs.Responses;
using MyShop.Shared.DTOs.Common;

namespace MyShop.Server.Controllers;

[ApiController]
[Route("api/v1/profiles")]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly IProfileService _profileService;
    private readonly IFileUploadService _fileUploadService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<ProfileController> _logger;

    public ProfileController(
        IProfileService profileService,
        IFileUploadService fileUploadService,
        ICurrentUserService currentUserService,
        ILogger<ProfileController> logger)
    {
        _profileService = profileService;
        _fileUploadService = fileUploadService;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    [HttpPatch("updateMyProfile")]
    [ProducesResponseType(typeof(ApiResponse<UpdateProfileResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<UpdateProfileResponse>>> UpdateMyProfile([FromBody] UpdateProfileRequest request)
    {
        var updatedProfile = await _profileService.UpdateMyProfileAsync(request);

        if (updatedProfile == null)
        {
            return BadRequest(ApiResponse<UpdateProfileResponse>.ErrorResponse(
                "Failed to update profile. User may not exist or JWT is invalid.",
                400));
        }

        return Ok(ApiResponse<UpdateProfileResponse>.SuccessResponse(
            updatedProfile,
            "Profile updated successfully",
            200));
    }

    [HttpPost("uploadAvatar")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<string>>> UploadAvatar([FromForm] IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse("No file uploaded", 400));
            }

            var userId = _currentUserService.UserId;
            if (!userId.HasValue)
            {
                return Unauthorized(ApiResponse<string>.ErrorResponse("User not authenticated", 401));
            }

            // Upload file to external service
            var imageUrl = await _fileUploadService.UploadImageAsync(file, userId.Value.ToString());

            // Update profile with new avatar URL
            var updateRequest = new UpdateProfileRequest
            {
                Avatar = imageUrl
            };

            var updatedProfile = await _profileService.UpdateMyProfileAsync(updateRequest);
            if (updatedProfile == null)
            {
                return BadRequest(ApiResponse<string>.ErrorResponse("Failed to update profile with new avatar", 400));
            }

            return Ok(ApiResponse<string>.SuccessResponse(
                imageUrl,
                "Avatar uploaded successfully",
                200));
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid file upload attempt");
            return BadRequest(ApiResponse<string>.ErrorResponse(ex.Message, 400));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading avatar");
            return StatusCode(500, ApiResponse<string>.ErrorResponse("Failed to upload avatar", 500));
        }
    }
}
