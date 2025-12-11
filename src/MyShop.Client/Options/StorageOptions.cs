using System.ComponentModel.DataAnnotations;

namespace MyShop.Client.Options
{
    /// <summary>
    /// Storage configuration options.
    /// Maps to "Storage" section in appsettings.json
    /// Controls credential and settings storage mechanisms.
    /// </summary>
    public class StorageOptions : OptionsBase
    {
        public override string SectionName => "Storage";

        /// <summary>
        /// Use secure credential storage (DPAPI encryption).
        /// When false, credentials are stored in plain text (NOT RECOMMENDED for production).
        /// </summary>
        public bool UseSecureCredentialStorage { get; set; } = true;

        /// <summary>
        /// Type of credential storage mechanism.
        /// Valid values: DPAPI, PasswordVault, File
        /// </summary>
        [Required]
        public string CredentialStorageType { get; set; } = "DPAPI";

        /// <summary>
        /// Type of settings storage mechanism.
        /// Valid values: File, Registry, Database
        /// </summary>
        [Required]
        public string SettingsStorageType { get; set; } = "File";

        /// <summary>
        /// Cache expiration time in minutes for in-memory data.
        /// Must be between 1 and 1440 minutes (24 hours).
        /// </summary>
        [Range(1, 1440, ErrorMessage = "Cache expiration must be between 1 and 1440 minutes")]
        public int CacheExpirationMinutes { get; set; } = 30;

        /// <summary>
        /// Gets cache expiration as TimeSpan.
        /// </summary>
        public TimeSpan CacheExpiration => TimeSpan.FromMinutes(CacheExpirationMinutes);

        public override bool Validate()
        {
            // Validate credential storage type
            var validCredentialTypes = new[] { "DPAPI", "PasswordVault", "File" };
            if (!validCredentialTypes.Contains(CredentialStorageType, StringComparer.OrdinalIgnoreCase))
                return false;

            // Validate settings storage type
            var validSettingsTypes = new[] { "File", "Registry", "Database" };
            if (!validSettingsTypes.Contains(SettingsStorageType, StringComparer.OrdinalIgnoreCase))
                return false;

            // Warn if using insecure storage
            if (!UseSecureCredentialStorage && CredentialStorageType == "File")
            {
                System.Diagnostics.Debug.WriteLine("[StorageOptions] WARNING: Using insecure credential storage!");
            }

            return true;
        }

        /// <summary>
        /// Checks if the current storage configuration is secure.
        /// </summary>
        public bool IsSecure()
        {
            return UseSecureCredentialStorage && 
                   (CredentialStorageType == "DPAPI" || CredentialStorageType == "PasswordVault");
        }
    }
}
