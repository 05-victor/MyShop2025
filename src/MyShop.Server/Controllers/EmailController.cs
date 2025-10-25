using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyShop.Server.Services.Interfaces;
using MyShop.Shared.DTOs.Common;
using System.ComponentModel.DataAnnotations;

namespace MyShop.Server.Controllers
{
    /// <summary>
    /// Controller for email notification operations
    /// </summary>
    [ApiController]
    [Route("api/v1/[controller]")]
    [Authorize] // Require authentication for all email operations
    public class EmailController : ControllerBase
    {
        private readonly IEmailNotificationService _emailService;
        private readonly ILogger<EmailController> _logger;

        public EmailController(IEmailNotificationService emailService, ILogger<EmailController> logger)
        {
            _emailService = emailService;
            _logger = logger;
        }

        /*
        /// <summary>
        /// Send email using template with placeholder replacement
        /// </summary>
        /// <param name="request">Email send request details</param>
        /// <returns>API response indicating success or failure</returns>
        [HttpPost("send-template")]
        [ProducesResponseType(typeof(ApiResponse<EmailSendResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<EmailSendResponse>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<EmailSendResponse>>> SendEmailTemplate([FromBody] SendEmailTemplateRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = string.Join(", ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));

                return BadRequest(ApiResponse<EmailSendResponse>.ErrorResponse(
                    $"Validation failed: {errors}",
                    400));
            }

            try
            {
                var success = await _emailService.SendEmailAsync(
                    request.RecipientEmail,
                    request.RecipientName,
                    request.Subject,
                    request.TemplatePath,
                    request.PlaceholderValues ?? Array.Empty<string>());

                var response = new EmailSendResponse
                {
                    Success = success,
                    RecipientEmail = request.RecipientEmail,
                    Subject = request.Subject,
                    SentAt = success ? DateTime.UtcNow : null
                };

                if (success)
                {
                    return Ok(ApiResponse<EmailSendResponse>.SuccessResponse(
                        response,
                        "Email sent successfully",
                        200));
                }
                else
                {
                    return BadRequest(ApiResponse<EmailSendResponse>.ErrorResponse(
                        "Failed to send email",
                        400));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SendEmailTemplate endpoint");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ApiResponse<EmailSendResponse>.ServerErrorResponse(
                        "An error occurred while sending email"));
            }
        }

        /// <summary>
        /// Send email with direct HTML content
        /// </summary>
        /// <param name="request">Direct email send request</param>
        /// <returns>API response indicating success or failure</returns>
        [HttpPost("send-direct")]
        [ProducesResponseType(typeof(ApiResponse<EmailSendResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<EmailSendResponse>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<EmailSendResponse>>> SendEmailDirect([FromBody] SendEmailDirectRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = string.Join(", ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));

                return BadRequest(ApiResponse<EmailSendResponse>.ErrorResponse(
                    $"Validation failed: {errors}",
                    400));
            }

            try
            {
                var success = await _emailService.SendEmailAsync(
                    request.RecipientEmail,
                    request.RecipientName,
                    request.Subject,
                    request.HtmlContent);

                var response = new EmailSendResponse
                {
                    Success = success,
                    RecipientEmail = request.RecipientEmail,
                    Subject = request.Subject,
                    SentAt = success ? DateTime.UtcNow : null
                };

                if (success)
                {
                    return Ok(ApiResponse<EmailSendResponse>.SuccessResponse(
                        response,
                        "Email sent successfully",
                        200));
                }
                else
                {
                    return BadRequest(ApiResponse<EmailSendResponse>.ErrorResponse(
                        "Failed to send email",
                        400));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SendEmailDirect endpoint");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ApiResponse<EmailSendResponse>.ServerErrorResponse(
                        "An error occurred while sending email"));
            }
        }

        /// <summary>
        /// Send bulk emails using template
        /// </summary>
        /// <param name="request">Bulk email send request</param>
        /// <returns>API response with bulk send results</returns>
        [HttpPost("send-bulk")]
        [ProducesResponseType(typeof(ApiResponse<BulkEmailSendResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<BulkEmailSendResponse>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<BulkEmailSendResponse>>> SendBulkEmail([FromBody] SendBulkEmailRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = string.Join(", ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));

                return BadRequest(ApiResponse<BulkEmailSendResponse>.ErrorResponse(
                    $"Validation failed: {errors}",
                    400));
            }

            try
            {
                var successCount = await _emailService.SendBulkEmailAsync(
                    request.Recipients,
                    request.Subject,
                    request.TemplatePath,
                    request.PlaceholderValues ?? Array.Empty<string>());

                var response = new BulkEmailSendResponse
                {
                    TotalRecipients = request.Recipients.Count,
                    SuccessfulSends = successCount,
                    FailedSends = request.Recipients.Count - successCount,
                    Subject = request.Subject,
                    SentAt = DateTime.UtcNow
                };

                return Ok(ApiResponse<BulkEmailSendResponse>.SuccessResponse(
                    response,
                    $"Bulk email completed. {successCount}/{request.Recipients.Count} emails sent successfully",
                    200));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SendBulkEmail endpoint");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ApiResponse<BulkEmailSendResponse>.ServerErrorResponse(
                        "An error occurred while sending bulk emails"));
            }
        }
        */

        /// <summary>
        /// Quick send email - Simple API for common email sending scenarios
        /// </summary>
        /// <param name="request">Quick email send request</param>
        /// <returns>API response indicating success or failure</returns>
        [HttpPost("send")]
        [ProducesResponseType(typeof(ApiResponse<EmailSendResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<EmailSendResponse>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<EmailSendResponse>>> QuickSendEmail([FromBody] QuickSendEmailRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = string.Join(", ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));

                return BadRequest(ApiResponse<EmailSendResponse>.ErrorResponse(
                    $"Validation failed: {errors}",
                    400));
            }

            try
            {
                var success = await _emailService.SendEmailAsync(
                    request.RecipientEmail,
                    request.RecipientName,
                    request.Subject,
                    request.TemplatePath,
                    request.Values ?? Array.Empty<string>());

                var response = new EmailSendResponse
                {
                    Success = success,
                    RecipientEmail = request.RecipientEmail,
                    Subject = request.Subject,
                    SentAt = success ? DateTime.UtcNow : null
                };

                if (success)
                {
                    return Ok(ApiResponse<EmailSendResponse>.SuccessResponse(
                        response,
                        "Email sent successfully",
                        200));
                }
                else
                {
                    return BadRequest(ApiResponse<EmailSendResponse>.ErrorResponse(
                        "Failed to send email",
                        400));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in QuickSendEmail endpoint");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    ApiResponse<EmailSendResponse>.ServerErrorResponse(
                        "An error occurred while sending email"));
            }
        }
        
    }

    
    #region Request/Response DTOs

    /// <summary>
    /// Request model for sending email using template
    /// </summary>
    public class SendEmailTemplateRequest
    {
        [Required]
        [EmailAddress]
        public string RecipientEmail { get; set; } = string.Empty;

        [Required]
        public string RecipientName { get; set; } = string.Empty;

        [Required]
        public string Subject { get; set; } = string.Empty;

        [Required]
        public string TemplatePath { get; set; } = string.Empty;

        public string[]? PlaceholderValues { get; set; }
    }

    /// <summary>
    /// Request model for sending email with direct HTML content
    /// </summary>
    public class SendEmailDirectRequest
    {
        [Required]
        [EmailAddress]
        public string RecipientEmail { get; set; } = string.Empty;

        [Required]
        public string RecipientName { get; set; } = string.Empty;

        [Required]
        public string Subject { get; set; } = string.Empty;

        [Required]
        public string HtmlContent { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request model for sending bulk emails
    /// </summary>
    public class SendBulkEmailRequest
    {
        [Required]
        [MinLength(1)]
        public List<EmailRecipient> Recipients { get; set; } = new();

        [Required]
        public string Subject { get; set; } = string.Empty;

        [Required]
        public string TemplatePath { get; set; } = string.Empty;

        public string[]? PlaceholderValues { get; set; }
    }

    /// <summary>
    /// Response model for email send operations
    /// </summary>
    public class EmailSendResponse
    {
        public bool Success { get; set; }
        public string RecipientEmail { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public DateTime? SentAt { get; set; }
    }

    /// <summary>
    /// Response model for bulk email operations
    /// </summary>
    public class BulkEmailSendResponse
    {
        public int TotalRecipients { get; set; }
        public int SuccessfulSends { get; set; }
        public int FailedSends { get; set; }
        public string Subject { get; set; } = string.Empty;
        public DateTime SentAt { get; set; }
    }

    /// <summary>
    /// Quick send email request - Simplified for common scenarios
    /// </summary>
    public class QuickSendEmailRequest
    {
        [Required]
        [EmailAddress]
        public string RecipientEmail { get; set; } = string.Empty;

        [Required]
        public string RecipientName { get; set; } = string.Empty;

        [Required]
        public string Subject { get; set; } = string.Empty;

        [Required]
        public string TemplatePath { get; set; } = string.Empty;

        public string[]? Values { get; set; }
    }

    #endregion
    
}