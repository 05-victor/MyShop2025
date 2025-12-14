using System.ComponentModel.DataAnnotations;

namespace MyShop.Client.Options
{
    /// <summary>
    /// API configuration options.
    /// Maps to "Api" section in appsettings.json
    /// </summary>
    public class ApiOptions : OptionsBase
    {
        public override string SectionName => "Api";

        /// <summary>
        /// Base URL for API endpoints (e.g., https://api.myshop2025.com).
        /// Required and must be a valid HTTP/HTTPS URL.
        /// </summary>
        [Required(ErrorMessage = "API Base URL is required")]
        [Url(ErrorMessage = "API Base URL must be a valid URL")]
        public string BaseUrl { get; set; } = "https://localhost:7120";

        /// <summary>
        /// HTTP request timeout in seconds.
        /// Must be between 10 and 300 seconds.
        /// </summary>
        [Range(10, 300, ErrorMessage = "Request timeout must be between 10 and 300 seconds")]
        public int RequestTimeoutSeconds { get; set; } = 30;

        /// <summary>
        /// Retry policy configuration for failed requests.
        /// </summary>
        [Required]
        public RetryPolicyOptions RetryPolicy { get; set; } = new();

        /// <summary>
        /// Gets the timeout as TimeSpan for convenience.
        /// </summary>
        public TimeSpan Timeout => TimeSpan.FromSeconds(RequestTimeoutSeconds);

        public override bool Validate()
        {
            // Custom validation beyond data annotations
            if (string.IsNullOrWhiteSpace(BaseUrl))
                return false;

            if (!Uri.TryCreate(BaseUrl, UriKind.Absolute, out var uri))
                return false;

            if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
                return false;

            return RetryPolicy?.Validate() ?? false;
        }
    }

    /// <summary>
    /// Retry policy configuration for API requests.
    /// </summary>
    public class RetryPolicyOptions
    {
        /// <summary>
        /// Maximum number of retry attempts.
        /// Must be between 0 and 5.
        /// </summary>
        [Range(0, 5, ErrorMessage = "Max retries must be between 0 and 5")]
        public int MaxRetries { get; set; } = 3;

        /// <summary>
        /// Delay between retry attempts in milliseconds.
        /// Must be between 100 and 10000 ms.
        /// </summary>
        [Range(100, 10000, ErrorMessage = "Retry delay must be between 100 and 10000 milliseconds")]
        public int RetryDelayMilliseconds { get; set; } = 1000;

        /// <summary>
        /// Gets the retry delay as TimeSpan for convenience.
        /// </summary>
        public TimeSpan RetryDelay => TimeSpan.FromMilliseconds(RetryDelayMilliseconds);

        public bool Validate()
        {
            return MaxRetries >= 0 && MaxRetries <= 5 &&
                   RetryDelayMilliseconds >= 100 && RetryDelayMilliseconds <= 10000;
        }
    }
}
