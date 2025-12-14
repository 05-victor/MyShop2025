namespace MyShop.Client.Options
{
    /// <summary>
    /// Base class for all configuration options.
    /// Provides common validation and tracking capabilities.
    /// </summary>
    public abstract class OptionsBase
    {
        /// <summary>
        /// Indicates if this options instance has been validated.
        /// </summary>
        public bool IsValidated { get; internal set; }

        /// <summary>
        /// Timestamp when options were last loaded.
        /// </summary>
        public DateTime LoadedAt { get; internal set; } = DateTime.UtcNow;

        /// <summary>
        /// Name of the configuration section this options class is bound to.
        /// </summary>
        public abstract string SectionName { get; }

        /// <summary>
        /// Performs basic validation. Override in derived classes for specific validation.
        /// </summary>
        /// <returns>True if valid, false otherwise.</returns>
        public virtual bool Validate()
        {
            return true;
        }
    }
}
