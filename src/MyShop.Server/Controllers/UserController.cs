using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyShop.Data;
using MyShop.Data.Entities;
using MyShop.Server.Services.Implementations;
using MyShop.Server.Services.Interfaces;
using MyShop.Shared.DTOs.Common;
using MyShop.Shared.DTOs.Commons;
using MyShop.Shared.DTOs.Requests;
using MyShop.Shared.DTOs.Responses;
using System.Security.Claims;

namespace MyShop.Server.Controllers;

[ApiController]
[Route("api/v1/users")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UserController> _logger;

    public UserController(IUserService userService, ILogger<UserController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<UserInfoResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<UserInfoResponse>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<UserInfoResponse>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<UserInfoResponse>>> GetMe()
    {
        try
        {
            var user = await _userService.GetMeAsync();

            if (user == null)
            {
                return NotFound(ApiResponse.NotFoundResponse(
                    "User not found or invalid token"));
            }

            return Ok(ApiResponse<UserInfoResponse>.SuccessResponse(
                user,
                "User profile retrieved successfully",
                200));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetMe endpoint");
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse.ServerErrorResponse(
                    "An error occurred while processing your request"));
        }
    }

    [HttpPost("activate")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<ActivateUserResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ActivateUserResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<ActivateUserResponse>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<ActivateUserResponse>>> ActivateMe([FromQuery] string activateCode)
    {
        _logger.LogInformation("ActivateMe called with activateCode: {ActivateCode}", activateCode);
        var result = await _userService.ActivateUserAsync(activateCode);

        if (result.Success)
        {
            return Ok(ApiResponse<ActivateUserResponse>.SuccessResponse(result));
        }
        else
        {
            return BadRequest(ApiResponse.ErrorResponse(result.Message ?? "Activation failed"));
        }
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<UserInfoResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<PagedResult<UserInfoResponse>>>> GetAllAsync([FromQuery] PaginationRequest request)
    {
        try
        {
            var pagedResult = await _userService.GetAllUsersAsync(request);
            return Ok(ApiResponse<PagedResult<UserInfoResponse>>.SuccessResponse(pagedResult));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving users");
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse.ServerErrorResponse("An error occurred while retrieving users"));
        }
    }

    [HttpPost("change-password")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<bool>>> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(ApiResponse<bool>.ErrorResponse(
                    $"Validation failed: {string.Join(", ", errors)}",
                    400));
            }

            var result = await _userService.ChangePasswordAsync(request);

            if (result == null)
            {
                return Unauthorized(ApiResponse<bool>.ErrorResponse(
                    "User not authenticated or not found",
                    401));
            }

            if (result == false)
            {
                return BadRequest(ApiResponse<bool>.ErrorResponse(
                    "Current password is incorrect",
                    400));
            }

            return Ok(ApiResponse<bool>.SuccessResponse(
                true,
                "Password changed successfully",
                200));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing password");
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse.ServerErrorResponse("An error occurred while changing password"));
        }
    }
}
