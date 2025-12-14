using FluentValidation;

namespace MyShop.Client.Options.Validators
{
    /// <summary>
    /// Validator for ApiOptions configuration.
    /// Ensures API settings are valid before application startup.
    /// </summary>
    public class ApiOptionsValidator : AbstractValidator<ApiOptions>
    {
        public ApiOptionsValidator()
        {
            RuleFor(x => x.BaseUrl)
                .NotEmpty()
                .WithMessage("API Base URL is required")
                .Must(BeValidUrl)
                .WithMessage("API Base URL must be a valid HTTP or HTTPS URL");

            RuleFor(x => x.RequestTimeoutSeconds)
                .InclusiveBetween(10, 300)
                .WithMessage("Request timeout must be between 10 and 300 seconds");

            RuleFor(x => x.RetryPolicy)
                .NotNull()
                .WithMessage("Retry policy is required")
                .SetValidator(new RetryPolicyOptionsValidator());
        }

        private bool BeValidUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return false;

            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
                return false;

            return uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps;
        }
    }

    /// <summary>
    /// Validator for RetryPolicyOptions.
    /// </summary>
    public class RetryPolicyOptionsValidator : AbstractValidator<RetryPolicyOptions>
    {
        public RetryPolicyOptionsValidator()
        {
            RuleFor(x => x.MaxRetries)
                .InclusiveBetween(0, 5)
                .WithMessage("Max retries must be between 0 and 5");

            RuleFor(x => x.RetryDelayMilliseconds)
                .InclusiveBetween(100, 10000)
                .WithMessage("Retry delay must be between 100 and 10000 milliseconds");
        }
    }
}
