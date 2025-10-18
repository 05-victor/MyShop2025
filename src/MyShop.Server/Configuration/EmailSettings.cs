using MyShop.Server.Services.Implementations;

namespace MyShop.Server.Configuration
{
    /// <summary>
    /// Configuration settings for email service using Brevo (formerly Sendinblue)
    /// </summary>
    public class EmailSettings
    {
        /// <summary>
        /// Brevo API endpoint for sending emails (can be encoded in Base64)
        /// </summary>
        public string ApiEndpoint { get; set; } = "https://api.brevo.com/v3/smtp/email";

        /// <summary>
        /// Brevo API key for authentication (can be encoded in Base64)
        /// </summary>
        public string ApiKey { get; set; } = string.Empty;

        /// <summary>
        /// Default sender name
        /// </summary>
        public string SenderName { get; set; } = "MyShop";

        /// <summary>
        /// Default sender email address
        /// </summary>
        public string SenderEmail { get; set; } = "mailtacvu05@gmail.com";

        /// <summary>
        /// Directory path where email templates are stored
        /// </summary>
        public string TemplatesPath { get; set; } = "EmailTemplates";

        /// <summary>
        /// Get decoded API endpoint
        /// </summary>
        public string GetDecodedApiEndpoint()
        {
            return ConfigurationEncoder.Decode(ApiEndpoint);
        }

        /// <summary>
        /// Get decoded API key
        /// </summary>
        public string GetDecodedApiKey()
        {
            return ConfigurationEncoder.Decode(ApiKey);
        }
    }
}