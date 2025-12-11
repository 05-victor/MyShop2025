using FluentValidation;

namespace MyShop.Client.Options.Validators
{
    /// <summary>
    /// Validator for LoggingOptions configuration.
    /// </summary>
    public class LoggingOptionsValidator : AbstractValidator<LoggingOptions>
    {
        private static readonly string[] ValidLogLevels = 
            { "Trace", "Debug", "Information", "Warning", "Error", "Critical", "None" };

        public LoggingOptionsValidator()
        {
            RuleFor(x => x.MinimumLevel)
                .NotEmpty()
                .WithMessage("Minimum log level is required")
                .Must(BeValidLogLevel)
                .WithMessage($"Minimum log level must be one of: {string.Join(", ", ValidLogLevels)}");

            RuleFor(x => x.MaxLogFileSizeMB)
                .InclusiveBetween(1, 100)
                .WithMessage("Max log file size must be between 1 and 100 MB");

            RuleFor(x => x.RetainLogDays)
                .InclusiveBetween(1, 365)
                .WithMessage("Retain log days must be between 1 and 365");

            RuleFor(x => x)
                .Must(HaveAtLeastOneLoggingOutput)
                .WithMessage("At least one logging output (console or file) must be enabled");
        }

        private bool BeValidLogLevel(string level)
        {
            return ValidLogLevels.Contains(level, StringComparer.OrdinalIgnoreCase);
        }

        private bool HaveAtLeastOneLoggingOutput(LoggingOptions options)
        {
            return options.EnableConsoleLogging || options.EnableFileLogging;
        }
    }
}
