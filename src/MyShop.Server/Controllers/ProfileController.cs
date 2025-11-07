using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyShop.Server.Services.Interfaces;
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

    public ProfileController(IProfileService profileService)
    {
        _profileService = profileService;
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
}
