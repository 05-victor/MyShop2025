using System.ComponentModel.DataAnnotations;

namespace MyShop.Client.Options
{
    /// <summary>
    /// User preferences configuration options.
    /// Maps to "UserPreferences" section in appsettings.json
    /// These settings can be changed at runtime by the user.
    /// </summary>
    public class UserPreferencesOptions : OptionsBase
    {
        public override string SectionName => "UserPreferences";

        /// <summary>
        /// Application theme.
        /// Valid values: Light (default), Dark
        /// </summary>
        [Required]
        public string Theme { get; set; } = "Light";

        /// <summary>
        /// Application language/culture.
        /// Currently only supports: en-US
        /// </summary>
        [Required]
        public string Language { get; set; } = "en-US";

        /// <summary>
        /// Number of items to display per page in lists.
        /// Must be between 5 and 100.
        /// </summary>
        [Range(5, 100, ErrorMessage = "Items per page must be between 5 and 100")]
        public int ItemsPerPage { get; set; } = 20;

        /// <summary>
        /// Enable desktop notifications for important events.
        /// </summary>
        public bool EnableNotifications { get; set; } = true;

        /// <summary>
        /// Auto-save interval in seconds (0 to disable).
        /// Must be between 0 and 600 seconds (10 minutes).
        /// </summary>
        [Range(0, 600, ErrorMessage = "Auto-save interval must be between 0 and 600 seconds")]
        public int AutoSaveInterval { get; set; } = 300;

        /// <summary>
        /// Gets auto-save interval as TimeSpan (null if disabled).
        /// </summary>
        public TimeSpan? AutoSaveTimeSpan => AutoSaveInterval > 0 
            ? TimeSpan.FromSeconds(AutoSaveInterval) 
            : null;

        public override bool Validate()
        {
            // Validate theme
            var validThemes = new[] { "Light", "Dark" };
            if (!validThemes.Contains(Theme, StringComparer.OrdinalIgnoreCase))
                return false;

            // Validate language is a valid culture code
            try
            {
                var culture = System.Globalization.CultureInfo.GetCultureInfo(Language);
                if (culture == null)
                    return false;
            }
            catch
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Checks if auto-save is enabled.
        /// </summary>
        public bool IsAutoSaveEnabled() => AutoSaveInterval > 0;

        /// <summary>
        /// Gets the culture info for the selected language.
        /// </summary>
        public System.Globalization.CultureInfo GetCultureInfo()
        {
            try
            {
                return System.Globalization.CultureInfo.GetCultureInfo(Language);
            }
            catch
            {
                return System.Globalization.CultureInfo.GetCultureInfo("en-US");
            }
        }
    }
}
