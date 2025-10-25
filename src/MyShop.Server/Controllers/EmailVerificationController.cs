using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyShop.Server.Services.Implementations;
using MyShop.Server.Services.Interfaces;
using System.Security.Claims;

namespace MyShop.Server.Controllers
{
    /// <summary>
    /// Controller for handling email verification operations.
    /// Provides endpoints for sending verification emails and verifying email addresses.
    /// </summary>
    [ApiController]
    [Route("api/v1/email-verification")]
    public class EmailVerificationController : ControllerBase
    {
        private readonly IEmailVerificationService _emailVerificationService;
        private readonly ILogger<EmailVerificationController> _logger;

        public EmailVerificationController(
            IEmailVerificationService emailVerificationService,
            ILogger<EmailVerificationController> logger)
        {
            _emailVerificationService = emailVerificationService;
            _logger = logger;
        }

        /// <summary>
        /// Sends a verification email to the authenticated user.
        /// </summary>
        /// <returns>ActionResult indicating success or failure</returns>
        /// <response code="200">Verification email sent successfully</response>
        /// <response code="400">Email is already verified or user not found</response>
        /// <response code="401">User is not authenticated</response>
        /// <response code="500">Internal server error</response>
        [HttpPost("send")]
        [Authorize]
        public async Task<ActionResult> SendVerificationEmail(CancellationToken cancellationToken)
        {
            try
            {
                var result = await _emailVerificationService.SendVerificationEmailAsync(cancellationToken);

                if (!result.IsSuccess)
                {
                    return BadRequest(new { message = result.Message });
                }

                return Ok(new { message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SendVerificationEmail endpoint");
                return StatusCode(500, new { message = "An error occurred while sending verification email" });
            }
        }

        /// <summary>
        /// Sends a verification email to a specific user (admin only).
        /// </summary>
        /// <param name="userId">The ID of the user to send verification email to</param>
        /// <returns>ActionResult indicating success or failure</returns>
        /// <response code="200">Verification email sent successfully</response>
        /// <response code="400">Email is already verified or user not found</response>
        /// <response code="401">User is not authenticated</response>
        /// <response code="403">User is not authorized (not an admin)</response>
        /// <response code="500">Internal server error</response>
        [HttpPost("send/{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> SendVerificationEmailToUser(Guid userId, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _emailVerificationService.SendVerificationEmailAsync(cancellationToken);

                if (!result.IsSuccess)
                {
                    return BadRequest(new { message = result.Message });
                }

                return Ok(new { message = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SendVerificationEmailToUser endpoint for user {UserId}", userId);
                return StatusCode(500, new { message = "An error occurred while sending verification email" });
            }
        }

        /// <summary>
        /// Verifies a user's email address using the verification token from the URL.
        /// This endpoint is publicly accessible and called when user clicks the verification link.
        /// </summary>
        /// <param name="token">The verification token from the email URL</param>
        /// <returns>ActionResult indicating success or failure</returns>
        /// <response code="200">Email verified successfully</response>
        /// <response code="400">Invalid or expired token, or user not found</response>
        /// <response code="500">Internal server error</response>
        [HttpPost("verify")]
        [AllowAnonymous]
        public async Task<ActionResult> VerifyEmail([FromQuery] string token, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(token))
                {
                    return BadRequest(new { message = "Verification token is required" });
                }

                var result = await _emailVerificationService.VerifyEmailAsync(token, cancellationToken);

                if (!result.IsSuccess)
                {
                    return BadRequest(new { message = result.Message });
                }

                // Return a success page or redirect to the client app
                return Ok(new 
                { 
                    message = result.Message,
                    success = true 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in VerifyEmail endpoint");
                return StatusCode(500, new { message = "An error occurred while verifying email" });
            }
        }

        /// <summary>
        /// Checks if the current user's email is verified.
        /// </summary>
        /// <returns>ActionResult with verification status</returns>
        /// <response code="200">Returns the verification status</response>
        /// <response code="401">User is not authenticated</response>
        [HttpGet("status")]
        [Authorize]
        public async Task<ActionResult> GetVerificationStatus()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                    ?? User.FindFirst("userId")?.Value;

                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                {
                    return Unauthorized(new { message = "Invalid user authentication" });
                }

                // You would need to add a method to get user verification status
                // For now, return a placeholder
                return Ok(new 
                { 
                    userId = userId,
                    // Add actual verification status retrieval here
                    message = "Verification status endpoint" 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetVerificationStatus endpoint");
                return StatusCode(500, new { message = "An error occurred" });
            }
        }
    }
}
