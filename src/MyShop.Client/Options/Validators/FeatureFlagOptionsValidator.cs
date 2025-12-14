using FluentValidation;

namespace MyShop.Client.Options.Validators
{
    /// <summary>
    /// Validator for FeatureFlagOptions configuration.
    /// Ensures feature flags are compatible with the environment.
    /// </summary>
    public class FeatureFlagOptionsValidator : AbstractValidator<FeatureFlagOptions>
    {
        public FeatureFlagOptionsValidator()
        {
            // No strict validation rules for feature flags
            // but we can add custom business rules

            // Example: In production, mock data should never be enabled
            RuleFor(x => x)
                .Must(NotUseMockDataInProduction)
                .When(x => IsProductionEnvironment())
                .WithMessage("Mock data cannot be enabled in production environment");

            RuleFor(x => x)
                .Must(NotEnableDeveloperOptionsInProduction)
                .When(x => IsProductionEnvironment())
                .WithMessage("Developer options cannot be enabled in production environment");
        }

        private bool NotUseMockDataInProduction(FeatureFlagOptions options)
        {
            return !options.UseMockData;
        }

        private bool NotEnableDeveloperOptionsInProduction(FeatureFlagOptions options)
        {
            return !options.EnableDeveloperOptions;
        }

        private bool IsProductionEnvironment()
        {
            var env = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") 
                ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") 
                ?? "Production";
            
            return env.Equals("Production", StringComparison.OrdinalIgnoreCase);
        }
    }
}
