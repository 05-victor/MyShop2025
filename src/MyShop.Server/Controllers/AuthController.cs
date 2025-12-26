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
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IAuthService authService,
        IRefreshTokenService refreshTokenService,
        ILogger<AuthController> logger)
    {
        _authService = authService;
        _refreshTokenService = refreshTokenService;
        _logger = logger;
    }

    /// <summary>
    /// Register a new user
    /// </summary>
    /// <param name="request">Registration details</param>
    /// <returns>Standardized API response with user details</returns>
    [HttpPost("register")]
    [ProducesResponseType(typeof(ApiResponse<CreateUserResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<CreateUserResponse>>> Register([FromBody] CreateUserRequest request)
    {
        // No need for ModelState validation - service layer handles validation
        var response = await _authService.RegisterAsync(request);

        return Ok(ApiResponse<CreateUserResponse>.SuccessResponse(
            response,
            "User registered successfully",
            200));
    }

    /// <summary>
    /// Login with username and password
    /// Returns both access token and refresh token
    /// </summary>
    /// <param name="request">Login credentials</param>
    /// <returns>Standardized API response with authentication details and token</returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<LoginResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<LoginResponse>>> Login([FromBody] LoginRequest request)
    {
        // No need for try-catch - global exception handler takes care of it
        var response = await _authService.LoginAsync(request);

        return Ok(ApiResponse<LoginResponse>.SuccessResponse(
            response,
            "Login successful",
            200));
    }

    /// <summary>
    /// Refresh access token using refresh token
    /// Implements automatic token rotation for security
    /// </summary>
    /// <param name="request">Refresh token details</param>
    /// <returns>Standardized API response with new access token and refresh token</returns>
    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(ApiResponse<RefreshTokenResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<RefreshTokenResponse>>> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var ipAddress = HttpContext.Connection?.RemoteIpAddress?.ToString();
        var response = await _refreshTokenService.RefreshTokenAsync(request, ipAddress);

        return Ok(ApiResponse<RefreshTokenResponse>.SuccessResponse(
            response,
            "Token refreshed successfully",
            200));
    }

    /// <summary>
    /// Revoke a refresh token (for logout or security purposes)
    /// </summary>
    /// <param name="request">Refresh token details</param>
    /// <returns>Standardized API response confirming token revocation</returns>
    [HttpPost("revoke-token")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<object>>> RevokeToken([FromBody] RefreshTokenRequest request)
    {
        var ipAddress = HttpContext.Connection?.RemoteIpAddress?.ToString();
        var result = await _refreshTokenService.RevokeTokenAsync(request.RefreshToken, ipAddress, "User logout");

        if (!result)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Token not found or already revoked", 400));
        }

        return Ok(ApiResponse<object>.SuccessResponse(null, "Token revoked successfully", 200));
    }
}
