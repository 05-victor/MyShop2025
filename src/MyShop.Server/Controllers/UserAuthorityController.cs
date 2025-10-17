using Microsoft.AspNetCore.Mvc;
using MyShop.Server.Services.Interfaces;
using MyShop.Shared.DTOs.Common;
using MyShop.Shared.DTOs.Requests;
using MyShop.Shared.DTOs.Responses;

namespace MyShop.Server.Controllers;

/// <summary>
/// Controller để quản lý quyền hạn của user.
/// </summary>
[ApiController]
[Route("api/v1/users/{userId}/authorities")]
public class UserAuthorityController : ControllerBase
{
    private readonly IUserAuthorityService _userAuthorityService;
    private readonly ILogger<UserAuthorityController> _logger;

    public UserAuthorityController(
        IUserAuthorityService userAuthorityService,
        ILogger<UserAuthorityController> logger)
    {
        _userAuthorityService = userAuthorityService;
        _logger = logger;
    }

    /// <summary>
    /// Get effective authorities for a user (authorities from roles minus removed authorities)
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>Detailed information about user's effective authorities</returns>
    [HttpGet("effective")]
    [ProducesResponseType(typeof(ApiResponse<EffectiveAuthoritiesResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<EffectiveAuthoritiesResponse>>> GetEffectiveAuthorities(Guid userId)
    {
        try
        {
            var result = await _userAuthorityService.GetEffectiveAuthoritiesDetailAsync(userId);

            if (result == null)
            {
                return NotFound(ApiResponse<EffectiveAuthoritiesResponse>.NotFoundResponse(
                    $"User with ID {userId} not found"));
            }

            return Ok(ApiResponse<EffectiveAuthoritiesResponse>.SuccessResponse(
                result,
                "Effective authorities retrieved successfully",
                200));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting effective authorities for user {UserId}", userId);
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<EffectiveAuthoritiesResponse>.ServerErrorResponse(
                    "An error occurred while processing your request"));
        }
    }

    /// <summary>
    /// Check if a user has a specific authority
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="authorityName">Authority name to check</param>
    /// <returns>Check result with details</returns>
    [HttpGet("check/{authorityName}")]
    [ProducesResponseType(typeof(ApiResponse<CheckAuthorityResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<CheckAuthorityResponse>>> CheckAuthority(
        Guid userId, 
        string authorityName)
    {
        try
        {
            var result = await _userAuthorityService.HasAuthorityAsync(userId, authorityName);

            return Ok(ApiResponse<CheckAuthorityResponse>.SuccessResponse(
                result,
                "Authority check completed",
                200));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking authority {Authority} for user {UserId}", authorityName, userId);
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<CheckAuthorityResponse>.ServerErrorResponse(
                    "An error occurred while processing your request"));
        }
    }

    /// <summary>
    /// Get list of removed authorities for a user
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <returns>List of removed authorities</returns>
    [HttpGet("removed")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<RemovedAuthorityResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<IEnumerable<RemovedAuthorityResponse>>>> GetRemovedAuthorities(
        Guid userId)
    {
        try
        {
            var result = await _userAuthorityService.GetRemovedAuthoritiesAsync(userId);

            return Ok(ApiResponse<IEnumerable<RemovedAuthorityResponse>>.SuccessResponse(
                result,
                "Removed authorities retrieved successfully",
                200));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting removed authorities for user {UserId}", userId);
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<IEnumerable<RemovedAuthorityResponse>>.ServerErrorResponse(
                    "An error occurred while processing your request"));
        }
    }

    /// <summary>
    /// Add an authority to the removed list for a user (restrict authority)
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="request">Authority to remove with optional reason</param>
    /// <returns>Information about the removed authority</returns>
    [HttpPost("removed")]
    [ProducesResponseType(typeof(ApiResponse<RemovedAuthorityResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<RemovedAuthorityResponse>>> AddRemovedAuthority(
        Guid userId,
        [FromBody] AddRemovedAuthorityRequest request)
    {
        if (!ModelState.IsValid)
        {
            var errors = string.Join(", ", ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage));

            return BadRequest(ApiResponse<RemovedAuthorityResponse>.ErrorResponse(
                $"Validation failed: {errors}",
                400));
        }

        try
        {
            var result = await _userAuthorityService.AddRemovedAuthorityAsync(userId, request);

            return Ok(ApiResponse<RemovedAuthorityResponse>.SuccessResponse(
                result,
                $"Authority '{request.AuthorityName}' removed from user successfully",
                200));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to add removed authority for user {UserId}", userId);
            return BadRequest(ApiResponse<RemovedAuthorityResponse>.ErrorResponse(
                ex.Message,
                400));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding removed authority for user {UserId}", userId);
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<RemovedAuthorityResponse>.ServerErrorResponse(
                    "An error occurred while processing your request"));
        }
    }

    /// <summary>
    /// Remove an authority from the removed list (restore authority to user)
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="authorityName">Authority name to restore</param>
    /// <returns>Success or not found</returns>
    [HttpDelete("removed/{authorityName}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse>> RemoveRemovedAuthority(
        Guid userId,
        string authorityName)
    {
        try
        {
            var result = await _userAuthorityService.RemoveRemovedAuthorityAsync(userId, authorityName);

            if (!result)
            {
                return NotFound(ApiResponse.NotFoundResponse(
                    $"Authority '{authorityName}' is not in the removed list for this user"));
            }

            return Ok(ApiResponse.SuccessResponse(
                $"Authority '{authorityName}' restored to user successfully",
                200));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing removed authority for user {UserId}", userId);
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse.ServerErrorResponse(
                    "An error occurred while processing your request"));
        }
    }
}
