using MyShop.Shared.DTOs.Requests;

namespace MyShop.Server.Services.Interfaces
{
    /// <summary>
    /// Interface for email notification service
    /// </summary>
    public interface IEmailNotificationService
    {
        /// <summary>
        /// Send email using template with placeholder replacement
        /// </summary>
        /// <param name="recipientEmail">Recipient email address</param>
        /// <param name="recipientName">Recipient name</param>
        /// <param name="subject">Email subject</param>
        /// <param name="templatePath">Path to HTML template file (relative to templates directory)</param>
        /// <param name="placeholderValues">Array of values to replace {{}} placeholders in template</param>
        /// <returns>True if email sent successfully, false otherwise</returns>
        Task<bool> SendEmailAsync(string recipientEmail, string recipientName, string subject, string templatePath, string[] placeholderValues);

        /// <summary>
        /// Send email using template with named placeholder replacement
        /// </summary>
        /// <param name="recipientEmail">Recipient email address</param>
        /// <param name="recipientName">Recipient name</param>
        /// <param name="subject">Email subject</param>
        /// <param name="templatePath">Path to HTML template file (relative to templates directory)</param>
        /// <param name="placeholders">Dictionary of placeholder names and their values (e.g., {"USERNAME": "John"})</param>
        /// <returns>True if email sent successfully, false otherwise</returns>
        Task<bool> SendEmailAsync(string recipientEmail, string recipientName, string subject, string templatePath, Dictionary<string, string> placeholders);

        /// <summary>
        /// Send email with direct HTML content
        /// </summary>
        /// <param name="recipientEmail">Recipient email address</param>
        /// <param name="recipientName">Recipient name</param>
        /// <param name="subject">Email subject</param>
        /// <param name="htmlContent">Direct HTML content</param>
        /// <returns>True if email sent successfully, false otherwise</returns>
        Task<bool> SendEmailAsync(string recipientEmail, string recipientName, string subject, string htmlContent);

        /// <summary>
        /// Send bulk emails using template
        /// </summary>
        /// <param name="recipients">List of recipients with their details</param>
        /// <param name="subject">Email subject</param>
        /// <param name="templatePath">Path to HTML template file</param>
        /// <param name="placeholderValues">Array of values to replace placeholders</param>
        /// <returns>Number of emails sent successfully</returns>
        Task<int> SendBulkEmailAsync(List<EmailRecipient> recipients, string subject, string templatePath, string[] placeholderValues);
    }

    /// <summary>
    /// Email recipient information
    /// </summary>
    public class EmailRecipient
    {
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }
}