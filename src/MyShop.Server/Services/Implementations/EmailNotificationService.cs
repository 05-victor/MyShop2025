using Microsoft.Extensions.Options;
using MyShop.Server.Configuration;
using MyShop.Server.Services.Interfaces;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace MyShop.Server.Services.Implementations
{
    /// <summary>
    /// Email notification service implementation using Brevo API
    /// </summary>
    public class EmailNotificationService : IEmailNotificationService
    {
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<EmailNotificationService> _logger;
        private readonly HttpClient _httpClient;
        private readonly IWebHostEnvironment _environment;

        public EmailNotificationService(
            IOptions<EmailSettings> emailSettings,
            ILogger<EmailNotificationService> logger,
            HttpClient httpClient,
            IWebHostEnvironment environment)
        {
            _emailSettings = emailSettings.Value;
            _logger = logger;
            _httpClient = httpClient;
            _environment = environment;

            // Configure HttpClient headers with decoded API key
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("accept", "application/json");
            _httpClient.DefaultRequestHeaders.Add("api-key", _emailSettings.GetDecodedApiKey());
        }

        /// <summary>
        /// Send email using template with placeholder replacement
        /// </summary>
        public async Task<bool> SendEmailAsync(string recipientEmail, string recipientName, string subject, string templatePath, string[] placeholderValues)
        {
            try
            {
                // Read and process template
                var htmlContent = await ReadAndProcessTemplateAsync(templatePath, placeholderValues);
                if (string.IsNullOrEmpty(htmlContent))
                {
                    _logger.LogError("Failed to read or process template: {TemplatePath}", templatePath);
                    return false;
                }

                // Send email with processed content
                return await SendEmailAsync(recipientEmail, recipientName, subject, htmlContent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending templated email to {Email}", recipientEmail);
                return false;
            }
        }

        /// <summary>
        /// Send email using template with named placeholder replacement
        /// </summary>
        public async Task<bool> SendEmailAsync(string recipientEmail, string recipientName, string subject, string templatePath, Dictionary<string, string> placeholders)
        {
            try
            {
                // Read and process template with named placeholders
                var htmlContent = await ReadAndProcessTemplateAsync(templatePath, placeholders);
                if (string.IsNullOrEmpty(htmlContent))
                {
                    _logger.LogError("Failed to read or process template: {TemplatePath}", templatePath);
                    return false;
                }

                // Send email with processed content
                return await SendEmailAsync(recipientEmail, recipientName, subject, htmlContent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending templated email to {Email}", recipientEmail);
                return false;
            }
        }

        /// <summary>
        /// Send email with direct HTML content
        /// </summary>
        public async Task<bool> SendEmailAsync(string recipientEmail, string recipientName, string subject, string htmlContent)
        {
            try
            {
                var emailRequest = new
                {
                    sender = new
                    {
                        name = _emailSettings.SenderName,
                        email = _emailSettings.SenderEmail
                    },
                    to = new[]
                    {
                        new
                        {
                            email = recipientEmail,
                            name = recipientName
                        }
                    },
                    htmlContent = htmlContent,
                    subject = subject
                };

                var jsonContent = JsonSerializer.Serialize(emailRequest, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                _logger.LogInformation("Sending email to {Email} with subject: {Subject}", recipientEmail, subject);

                var response = await _httpClient.PostAsync(_emailSettings.GetDecodedApiEndpoint(), content);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Email sent successfully to {Email}", recipientEmail);
                    return true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Failed to send email to {Email}. Status: {StatusCode}, Error: {Error}",
                        recipientEmail, response.StatusCode, errorContent);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email to {Email}", recipientEmail);
                return false;
            }
        }

        /// <summary>
        /// Send bulk emails using template
        /// </summary>
        public async Task<int> SendBulkEmailAsync(List<EmailRecipient> recipients, string subject, string templatePath, string[] placeholderValues)
        {
            try
            {
                // Read and process template once
                var htmlContent = await ReadAndProcessTemplateAsync(templatePath, placeholderValues);
                if (string.IsNullOrEmpty(htmlContent))
                {
                    _logger.LogError("Failed to read or process template: {TemplatePath}", templatePath);
                    return 0;
                }

                int successCount = 0;
                var tasks = recipients.Select(async recipient =>
                {
                    var success = await SendEmailAsync(recipient.Email, recipient.Name, subject, htmlContent);
                    if (success)
                    {
                        Interlocked.Increment(ref successCount);
                    }
                    return success;
                });

                await Task.WhenAll(tasks);

                _logger.LogInformation("Bulk email completed. Sent {SuccessCount}/{TotalCount} emails",
                    successCount, recipients.Count);

                return successCount;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending bulk emails");
                return 0;
            }
        }

        /// <summary>
        /// Read template file and replace placeholders with provided values
        /// </summary>
        private async Task<string> ReadAndProcessTemplateAsync(string templatePath, string[] placeholderValues)
        {
            try
            {
                // Construct full path to template
                var templatesDirectory = System.IO.Path.Combine(_environment.ContentRootPath, _emailSettings.TemplatesPath);
                var fullTemplatePath = System.IO.Path.Combine(templatesDirectory, templatePath);

                // Ensure template file exists
                if (!File.Exists(fullTemplatePath))
                {
                    _logger.LogError("Template file not found: {TemplatePath}", fullTemplatePath);
                    return string.Empty;
                }

                // Read template content
                var templateContent = await File.ReadAllTextAsync(fullTemplatePath);

                // Replace placeholders {{}} with provided values
                var processedContent = ReplacePlaceholders(templateContent, placeholderValues);

                return processedContent;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading template: {TemplatePath}", templatePath);
                return string.Empty;
            }
        }

        /// <summary>
        /// Read template file and replace named placeholders with provided values
        /// </summary>
        private async Task<string> ReadAndProcessTemplateAsync(string templatePath, Dictionary<string, string> placeholders)
        {
            try
            {
                // Construct full path to template
                var templatesDirectory = System.IO.Path.Combine(_environment.ContentRootPath, _emailSettings.TemplatesPath);
                var fullTemplatePath = System.IO.Path.Combine(templatesDirectory, templatePath);

                // Ensure template file exists
                if (!File.Exists(fullTemplatePath))
                {
                    _logger.LogError("Template file not found: {TemplatePath}", fullTemplatePath);
                    return string.Empty;
                }

                // Read template content
                var templateContent = await File.ReadAllTextAsync(fullTemplatePath);

                // Replace named placeholders {{KEY}} with provided values
                var processedContent = ReplaceNamedPlaceholders(templateContent, placeholders);

                return processedContent;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading template: {TemplatePath}", templatePath);
                return string.Empty;
            }
        }

        /// <summary>
        /// Replace {{}} placeholders in template with provided values
        /// </summary>
        private string ReplacePlaceholders(string template, string[] values)
        {
            if (values == null || values.Length == 0)
            {
                return template;
            }

            // Find all {{}} placeholders
            var regex = new Regex(@"\{\{\}\}", RegexOptions.Compiled);
            var matches = regex.Matches(template);

            if (matches.Count == 0)
            {
                _logger.LogWarning("No placeholders found in template");
                return template;
            }

            // Replace placeholders with values in order
            var result = template;
            for (int i = 0; i < Math.Min(matches.Count, values.Length); i++)
            {
                // Replace first occurrence of {{}}
                result = regex.Replace(result, values[i], 1);
            }

            // Log warning if there are more placeholders than values
            if (matches.Count > values.Length)
            {
                _logger.LogWarning("Template has {PlaceholderCount} placeholders but only {ValueCount} values provided",
                    matches.Count, values.Length);
            }

            return result;
        }

        /// <summary>
        /// Replace named {{KEY}} placeholders in template with provided values
        /// </summary>
        private string ReplaceNamedPlaceholders(string template, Dictionary<string, string> placeholders)
        {
            if (placeholders == null || placeholders.Count == 0)
            {
                return template;
            }

            var result = template;

            // Replace each named placeholder
            foreach (var placeholder in placeholders)
            {
                var pattern = $"{{{{{placeholder.Key}}}}}"; // Creates {{KEY}}
                result = result.Replace(pattern, placeholder.Value);
            }

            return result;
        }
    }
}