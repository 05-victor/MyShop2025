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
    /// Shop name (Admin can edit, others read-only)
    /// </summary>
    public string? ShopName { get; set; } = "MyShop 2025";

    /// <summary>
    /// Address of the shop
    /// </summary>
    public string? Address { get; set; } = null;
}
