using System.ComponentModel.DataAnnotations;

namespace MyShop.Shared.DTOs
{
    /// <summary>
    /// Request model for sending email using template with placeholders
    /// </summary>
    public class SendEmailTemplateRequest
    {
        /// <summary>
        /// Recipient email address
        /// </summary>
        [Required]
        [EmailAddress]
        public string To { get; set; } = string.Empty;

        /// <summary>
        /// Email subject
        /// </summary>
        [Required]
        public string Subject { get; set; } = string.Empty;

        /// <summary>
        /// Template file name (e.g., "email-verification.html")
        /// </summary>
        [Required]
        public string TemplateName { get; set; } = string.Empty;

        /// <summary>
        /// Dictionary of placeholder keys and their replacement values
        /// </summary>
        public Dictionary<string, string> Placeholders { get; set; } = new();
    }

    /// <summary>
    /// Response model for email sending operations
    /// </summary>
    public class EmailSendResponse
    {
        /// <summary>
        /// Indicates whether the email was sent successfully
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// Message describing the result
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Optional message ID from the email provider
        /// </summary>
        public string? MessageId { get; set; }
    }
}
