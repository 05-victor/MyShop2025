using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyShop.Server.Services.Interfaces;
using MyShop.Shared.DTOs.Common;
using MyShop.Shared.DTOs.Requests;
using MyShop.Shared.DTOs.Responses;
using System.Security.Claims;

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
    [ProducesResponseType(typeof(ApiResponse<CreateUserResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<CreateUserResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<CreateUserResponse>>> Register([FromBody] CreateUserRequest request)
    {
        if (!ModelState.IsValid)
        {
            var errors = string.Join(", ", ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage));
            
            return BadRequest(ApiResponse<CreateUserResponse>.ErrorResponse(
                $"Validation failed: {errors}", 
                400));
        }

        try
        {
            var response = await _authService.RegisterAsync(request);

            return Ok(ApiResponse<CreateUserResponse>.SuccessResponse(
                response,
                "User registered successfully",
                200));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Registration failed: {Message}", ex.Message);
            return BadRequest(ApiResponse<CreateUserResponse>.ErrorResponse(
                ex.Message,
                400));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Register endpoint");
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<CreateUserResponse>.ServerErrorResponse(
                    "An error occurred while processing your request"));
        }
    }

    /// <summary>
    /// Login with username and password
    /// </summary>
    /// <param name="request">Login credentials</param>
    /// <returns>Standardized API response with authentication details and token</returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<LoginResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<LoginResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<LoginResponse>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<LoginResponse>>> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
        {
            var errors = string.Join(", ", ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage));
            
            return BadRequest(ApiResponse<LoginResponse>.ErrorResponse(
                $"Validation failed: {errors}", 
                400));
        }

        try
        {
            var response = await _authService.LoginAsync(request);

            return Ok(ApiResponse<LoginResponse>.SuccessResponse(
                response,
                "Login successful",
                200));
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Login failed: {Message}", ex.Message);
            return Unauthorized(ApiResponse<LoginResponse>.UnauthorizedResponse(
                ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Login endpoint");
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<LoginResponse>.ServerErrorResponse(
                    "An error occurred while processing your request"));
        }
    }

    /// <summary>
    /// Get current user profile from JWT token
    /// </summary>
    /// <returns>Standardized API response with current user details</returns>
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
            var user = await _authService.GetMeAsync();

            if (user == null)
            {
                return NotFound(ApiResponse<UserInfoResponse>.NotFoundResponse(
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
                ApiResponse<UserInfoResponse>.ServerErrorResponse(
                    "An error occurred while processing your request"));
        }
    }
}
