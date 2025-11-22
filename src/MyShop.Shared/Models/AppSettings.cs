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
    public string Theme { get; set; } = "System";

    /// <summary>
    /// Language preference: "vi-VN" or "en-US"
    /// </summary>
    public string Language { get; set; } = "vi-VN";

    /// <summary>
    /// Enable toast notifications
    /// </summary>
    public bool NotificationsEnabled { get; set; } = true;

    /// <summary>
    /// Restore last visited page on app startup
    /// </summary>
    public bool RestoreLastPage { get; set; } = true;

    // Page size preferences
    public int ProductsPageSize { get; set; } = 20;
    public int OrdersPageSize { get; set; } = 15;
    public int CustomersPageSize { get; set; } = 20;

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
