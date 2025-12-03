namespace MyShop.Shared.Models;

/// <summary>
/// Application settings model for user preferences
/// Persisted across app sessions via ISettingsStorage
/// </summary>
public class AppSettings
{
    /// <summary>
    /// Theme preference: "Light", "Dark", or "System"
    /// </summary>
    public string Theme { get; set; } = "Light";

    /// <summary>
    /// Language preference: "vi-VN" or "en-US"
    /// </summary>
    public string Language { get; set; } = "en-US";

    /// <summary>
    /// Enable toast notifications
    /// </summary>
    public bool NotificationsEnabled { get; set; } = true;

    /// <summary>
    /// Restore last visited page on app startup
    /// </summary>
    public bool RestoreLastPage { get; set; } = true;

    /// <summary>
    /// Pagination settings for all entity types
    /// </summary>
    public PaginationSettings Pagination { get; set; } = new();

    // Extended notification settings
    public bool EnableSoundNotifications { get; set; } = true;
    public bool NotifyOnLowStock { get; set; } = true;
    public bool NotifyOnNewOrders { get; set; } = true;

    // Offline mode
    public bool EnableOfflineMode { get; set; } = false;

    // Shop information
    public string? ShopName { get; set; } = "MyShop 2025";
    public string? Address { get; set; } = string.Empty;
    
    // Timezone
    public int SelectedTimezoneIndex { get; set; } = 0;
}
