using FluentValidation;

namespace MyShop.Client.Options.Validators
{
    /// <summary>
    /// Validator for UserPreferencesOptions configuration.
    /// </summary>
    public class UserPreferencesOptionsValidator : AbstractValidator<UserPreferencesOptions>
    {
        private static readonly string[] ValidThemes = { "Light", "Dark" };

        public UserPreferencesOptionsValidator()
        {
            RuleFor(x => x.Theme)
                .NotEmpty()
                .WithMessage("Theme is required")
                .Must(BeValidTheme)
                .WithMessage($"Theme must be one of: {string.Join(", ", ValidThemes)}");

            RuleFor(x => x.Language)
                .NotEmpty()
                .WithMessage("Language is required")
                .Must(BeValidCultureCode)
                .WithMessage("Language must be en-US (currently only English is supported)");

            RuleFor(x => x.ItemsPerPage)
                .InclusiveBetween(5, 100)
                .WithMessage("Items per page must be between 5 and 100");

            RuleFor(x => x.AutoSaveInterval)
                .InclusiveBetween(0, 600)
                .WithMessage("Auto-save interval must be between 0 and 600 seconds");
        }

        private bool BeValidTheme(string theme)
        {
            return ValidThemes.Contains(theme, StringComparer.OrdinalIgnoreCase);
        }

        private bool BeValidCultureCode(string language)
        {
            try
            {
                var culture = System.Globalization.CultureInfo.GetCultureInfo(language);
                return culture != null;
            }
            catch
            {
                return false;
            }
        }
    }
}
