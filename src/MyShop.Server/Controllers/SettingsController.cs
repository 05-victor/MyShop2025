using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyShop.Server.Services.Interfaces;
using MyShop.Shared.DTOs.Common;
using MyShop.Shared.DTOs.Requests;
using MyShop.Shared.DTOs.Responses;

namespace MyShop.Server.Controllers;

/// <summary>
/// Controller for managing application settings
/// Implements simplified Settings API with role-based access
/// </summary>
[ApiController]
[Route("api/v1/settings")]
[Authorize]
public class SettingsController : ControllerBase
{
    private readonly ISettingsService _settingsService;
    private readonly ILogger<SettingsController> _logger;

    public SettingsController(
        ISettingsService settingsService,
        ILogger<SettingsController> logger)
    {
        _settingsService = settingsService;
        _logger = logger;
    }

    /// <summary>
    /// GET /api/v1/settings
    /// Retrieve all settings (unified endpoint for all roles)
    /// </summary>
    /// <returns>Settings data with trial info for Admin users only</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<SettingsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<SettingsResponse>>> GetSettings()
    {
        try
        {
            var settings = await _settingsService.GetSettingsAsync();

            return Ok(ApiResponse<SettingsResponse>.SuccessResponse(
                settings,
                "Settings retrieved successfully",
                200));
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access to settings");
            return Unauthorized(ApiResponse.UnauthorizedResponse("User not authenticated"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving settings");
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse.ServerErrorResponse("An error occurred while retrieving settings"));
        }
    }

    /// <summary>
    /// PUT /api/v1/settings
    /// Update General, Appearance, and Trial settings (Admin only)
    /// </summary>
    /// <param name="request">Settings update request</param>
    /// <returns>Updated settings</returns>
    [HttpPut]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<SettingsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<SettingsResponse>>> UpdateSettings(
        [FromBody] UpdateSettingsRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                
                return BadRequest(ApiResponse<SettingsResponse>.ErrorResponse(
                    $"Validation failed: {string.Join(", ", errors)}",
                    400));
            }

            // Validate theme enum
            if (!new[] { "Light", "Dark" }.Contains(request.Theme))
            {
                return BadRequest(ApiResponse<SettingsResponse>.ErrorResponse(
                    "Invalid theme value. Must be 'Light' or 'Dark'",
                    400));
            }

            // Validate license enum
            if (!new[] { "Commercial", "Trial" }.Contains(request.License))
            {
                return BadRequest(ApiResponse<SettingsResponse>.ErrorResponse(
                    "Invalid license value. Must be 'Commercial' or 'Trial'",
                    400));
            }

            var settings = await _settingsService.UpdateSettingsAsync(request);

            return Ok(ApiResponse<SettingsResponse>.SuccessResponse(
                settings,
                "Settings updated successfully",
                200));
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized settings update attempt");
            return StatusCode(StatusCodes.Status403Forbidden,
                ApiResponse.ErrorResponse("Only Admin can update settings", 403));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating settings");
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse.ServerErrorResponse("An error occurred while updating settings"));
        }
    }

    /// <summary>
    /// PUT /api/v1/settings/appearance
    /// Update Appearance settings (SalesAgent and User roles only)
    /// </summary>
    /// <param name="request">Appearance update request</param>
    /// <returns>Updated settings</returns>
    [HttpPut("appearance")]
    [ProducesResponseType(typeof(ApiResponse<SettingsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<SettingsResponse>>> UpdateAppearance(
        [FromBody] UpdateAppearanceRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                
                return BadRequest(ApiResponse<SettingsResponse>.ErrorResponse(
                    $"Validation failed: {string.Join(", ", errors)}",
                    400));
            }

            // Validate theme enum
            if (!new[] { "Light", "Dark" }.Contains(request.Theme))
            {
                return BadRequest(ApiResponse<SettingsResponse>.ErrorResponse(
                    "Invalid theme value. Must be 'Light' or 'Dark'",
                    400));
            }

            var settings = await _settingsService.UpdateAppearanceAsync(request);

            return Ok(ApiResponse<SettingsResponse>.SuccessResponse(
                settings,
                "Appearance settings updated successfully",
                200));
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Admin attempted to use appearance endpoint");
            return StatusCode(StatusCodes.Status403Forbidden,
                ApiResponse.ErrorResponse("Admin should use main PUT endpoint instead", 403));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating appearance");
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse.ServerErrorResponse("An error occurred while updating appearance"));
        }
    }
}
