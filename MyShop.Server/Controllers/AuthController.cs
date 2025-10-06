using Microsoft.AspNetCore.Mvc;
using MyShop.Server.Services.Interfaces;
using MyShop.Shared.DTOs.Common;
using MyShop.Shared.DTOs.User;

namespace MyShop.Server.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Register a new user
    /// </summary>
    /// <param name="request">Registration details</param>
    /// <returns>Standardized API response with user details</returns>
    [HttpPost("register")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Register([FromBody] RegisterRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            var errors = string.Join(", ", ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage));
            
            return BadRequest(ApiResponse<AuthResponseDto>.ErrorResponse(
                $"Validation failed: {errors}", 
                400));
        }

        try
        {
            var response = await _authService.RegisterAsync(request);

            if (response.Id == 0)
            {
                return BadRequest(ApiResponse<AuthResponseDto>.ErrorResponse(
                    response.Message, 
                    400));
            }

            return Ok(ApiResponse<AuthResponseDto>.SuccessResponse(
                response,
                "User registered successfully",
                200));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Register endpoint");
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<AuthResponseDto>.ServerErrorResponse(
                    "An error occurred while processing your request"));
        }
    }

    /// <summary>
    /// Login with username and password
    /// </summary>
    /// <param name="request">Login credentials</param>
    /// <returns>Standardized API response with authentication details and token</returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Login([FromBody] LoginRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            var errors = string.Join(", ", ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage));
            
            return BadRequest(ApiResponse<AuthResponseDto>.ErrorResponse(
                $"Validation failed: {errors}", 
                400));
        }

        try
        {
            var response = await _authService.LoginAsync(request);

            if (response.Id == 0)
            {
                return Unauthorized(ApiResponse<AuthResponseDto>.UnauthorizedResponse(
                    response.Message));
            }

            return Ok(ApiResponse<AuthResponseDto>.SuccessResponse(
                response,
                "Login successful",
                200));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Login endpoint");
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<AuthResponseDto>.ServerErrorResponse(
                    "An error occurred while processing your request"));
        }
    }

    /// <summary>
    /// Get current user profile (placeholder for authentication implementation)
    /// </summary>
    /// <returns>Standardized API response with current user details</returns>
    [HttpGet("me")]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetMe()
    {
        // TODO: When authentication is implemented, get userId from JWT token claims
        // For now, this is a placeholder that returns empty/unauthorized

        try
        {
            // Placeholder: In a real implementation, you would extract the userId from the JWT token
            // Example: var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            
            var userId = 0; // Placeholder - will be populated from JWT claims later
            
            if (userId == 0)
            {
                return Unauthorized(ApiResponse<UserDto>.UnauthorizedResponse(
                    "Authentication required. This endpoint will be functional once authentication is implemented."));
            }

            var user = await _authService.GetMeAsync(userId);

            if (user == null)
            {
                return NotFound(ApiResponse<UserDto>.NotFoundResponse(
                    "User not found"));
            }

            return Ok(ApiResponse<UserDto>.SuccessResponse(
                user,
                "User profile retrieved successfully",
                200));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetMe endpoint");
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<UserDto>.ServerErrorResponse(
                    "An error occurred while processing your request"));
        }
    }
}
