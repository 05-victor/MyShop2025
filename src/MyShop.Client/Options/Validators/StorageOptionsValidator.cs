using FluentValidation;

namespace MyShop.Client.Options.Validators
{
    /// <summary>
    /// Validator for StorageOptions configuration.
    /// </summary>
    public class StorageOptionsValidator : AbstractValidator<StorageOptions>
    {
        private static readonly string[] ValidCredentialTypes = { "DPAPI", "PasswordVault", "File" };
        private static readonly string[] ValidSettingsTypes = { "File", "Registry", "Database" };

        public StorageOptionsValidator()
        {
            RuleFor(x => x.CredentialStorageType)
                .NotEmpty()
                .WithMessage("Credential storage type is required")
                .Must(BeValidCredentialType)
                .WithMessage($"Credential storage type must be one of: {string.Join(", ", ValidCredentialTypes)}");

            RuleFor(x => x.SettingsStorageType)
                .NotEmpty()
                .WithMessage("Settings storage type is required")
                .Must(BeValidSettingsType)
                .WithMessage($"Settings storage type must be one of: {string.Join(", ", ValidSettingsTypes)}");

            RuleFor(x => x.CacheExpirationMinutes)
                .InclusiveBetween(1, 1440)
                .WithMessage("Cache expiration must be between 1 and 1440 minutes");

            // Security warning for insecure configurations
            RuleFor(x => x)
                .Must(UseSecureStorage)
                .When(x => IsProductionEnvironment())
                .WithMessage("Secure credential storage must be enabled in production");
        }

        private bool BeValidCredentialType(string type)
        {
            return ValidCredentialTypes.Contains(type, StringComparer.OrdinalIgnoreCase);
        }

        private bool BeValidSettingsType(string type)
        {
            return ValidSettingsTypes.Contains(type, StringComparer.OrdinalIgnoreCase);
        }

        private bool UseSecureStorage(StorageOptions options)
        {
            return options.UseSecureCredentialStorage;
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
