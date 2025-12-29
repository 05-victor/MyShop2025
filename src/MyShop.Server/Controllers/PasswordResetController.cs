using Microsoft.AspNetCore.Mvc;
using MyShop.Server.Services.Interfaces;
using MyShop.Shared.DTOs.Common;
using MyShop.Shared.DTOs.Requests;

namespace MyShop.Server.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class PasswordResetController : ControllerBase
{
    private readonly IPasswordResetService _passwordResetService;
    private readonly ILogger<PasswordResetController> _logger;

    public PasswordResetController(
        IPasswordResetService passwordResetService,
        ILogger<PasswordResetController> logger)
    {
        _passwordResetService = passwordResetService;
        _logger = logger;
    }

    /// <summary>
    /// Request a password reset code to be sent to the user's email.
    /// </summary>
    /// <param name="request">Forgot password request containing email</param>
    /// <returns>Success message (doesn't reveal if email exists)</returns>
    [HttpPost("forgot-password")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<object>>> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    $"Validation failed: {string.Join(", ", errors)}",
                    400));
            }

            var result = await _passwordResetService.SendPasswordResetCodeAsync(request.Email);

            if (!result.IsSuccess)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    result.Message ?? "Failed to process password reset request",
                    400));
            }

            return Ok(ApiResponse<object>.SuccessResponse(
                null,
                result.Message ?? "If the email exists, a reset code has been sent",
                200));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing forgot password request for {Email}", request.Email);
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<object>.ErrorResponse("An error occurred while processing your request", 500));
        }
    }

    /// <summary>
    /// Validate a reset code for a given email.
    /// </summary>
    /// <param name="email">Email address</param>
    /// <param name="code">Reset code to validate</param>
    /// <returns>True if code is valid</returns>
    [HttpGet("validate-code")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<bool>>> ValidateResetCode([FromQuery] string email, [FromQuery] string code)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(code))
            {
                return BadRequest(ApiResponse<bool>.ErrorResponse(
                    "Email and code are required",
                    400));
            }

            var isValid = await _passwordResetService.ValidateResetCodeAsync(email, code);

            return Ok(ApiResponse<bool>.SuccessResponse(
                isValid,
                isValid ? "Reset code is valid" : "Invalid or expired reset code",
                200));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating reset code for {Email}", email);
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<bool>.ErrorResponse("An error occurred while validating reset code", 500));
        }
    }

    /// <summary>
    /// Reset password using a valid reset code.
    /// </summary>
    /// <param name="request">Reset password request</param>
    /// <returns>Success or failure message</returns>
    [HttpPost("reset-password")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<object>>> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    $"Validation failed: {string.Join(", ", errors)}",
                    400));
            }

            var result = await _passwordResetService.ResetPasswordAsync(
                request.Email,
                request.ResetCode,
                request.NewPassword);

            if (!result.IsSuccess)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    result.Message ?? "Failed to reset password",
                    400));
            }

            return Ok(ApiResponse<object>.SuccessResponse(
                null,
                result.Message ?? "Password reset successfully",
                200));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting password for {Email}", request.Email);
            return StatusCode(StatusCodes.Status500InternalServerError,
                ApiResponse<object>.ErrorResponse("An error occurred while resetting password", 500));
        }
    }
}
