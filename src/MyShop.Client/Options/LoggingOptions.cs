using System.ComponentModel.DataAnnotations;

namespace MyShop.Client.Options
{
    /// <summary>
    /// Logging configuration options.
    /// Maps to "Logging" section in appsettings.json
    /// </summary>
    public class LoggingOptions : OptionsBase
    {
        public override string SectionName => "Logging";

        /// <summary>
        /// Minimum log level to capture.
        /// Valid values: Trace, Debug, Information, Warning, Error, Critical, None
        /// </summary>
        [Required]
        public string MinimumLevel { get; set; } = "Information";

        /// <summary>
        /// Enable console logging (writes to Debug output in Visual Studio).
        /// </summary>
        public bool EnableConsoleLogging { get; set; } = true;

        /// <summary>
        /// Enable file logging (writes to log files on disk).
        /// </summary>
        public bool EnableFileLogging { get; set; } = true;

        /// <summary>
        /// Store logs in project directory instead of AppData.
        /// Recommended: true for Development, false for Production.
        /// </summary>
        public bool StoreLogsInProject { get; set; } = false;

        /// <summary>
        /// Maximum log file size in megabytes before rotation.
        /// Must be between 1 and 100 MB.
        /// </summary>
        [Range(1, 100, ErrorMessage = "Max log file size must be between 1 and 100 MB")]
        public int MaxLogFileSizeMB { get; set; } = 10;

        /// <summary>
        /// Number of days to retain log files.
        /// Must be between 1 and 365 days.
        /// </summary>
        [Range(1, 365, ErrorMessage = "Retain log days must be between 1 and 365")]
        public int RetainLogDays { get; set; } = 30;

        /// <summary>
        /// Gets the maximum log file size in bytes.
        /// </summary>
        public long MaxLogFileSizeBytes => MaxLogFileSizeMB * 1024 * 1024;

        public override bool Validate()
        {
            // Validate minimum level
            var validLevels = new[] { "Trace", "Debug", "Information", "Warning", "Error", "Critical", "None" };
            if (!validLevels.Contains(MinimumLevel, StringComparer.OrdinalIgnoreCase))
                return false;

            // At least one logging output must be enabled
            if (!EnableConsoleLogging && !EnableFileLogging)
                return false;

            return true;
        }

        /// <summary>
        /// Checks if the specified log level would be logged.
        /// </summary>
        public bool IsEnabled(string level)
        {
            var levels = new[] { "Trace", "Debug", "Information", "Warning", "Error", "Critical" };
            var minIndex = Array.IndexOf(levels, MinimumLevel);
            var checkIndex = Array.IndexOf(levels, level);
            
            return checkIndex >= minIndex;
        }
    }
}
