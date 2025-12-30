namespace MyShop.Client.Services;

/// <summary>
/// Helper class for consistent theme mapping between ThemeManager.ThemeType and app settings strings.
/// Centralizes all theme parsing logic to ensure consistency across the application.
/// </summary>
public static class ThemeMapping
{
    /// <summary>
    /// Converts a theme string from app settings to ThemeManager.ThemeType.
    /// </summary>
    /// <param name="theme">Theme string from settings (null, empty, "Light", "Dark", etc.)</param>
    /// <returns>
    /// Light if theme is null/empty or unrecognized
    /// Dark if theme is "Dark" (case-insensitive)
    /// Light otherwise
    /// </returns>
    public static ThemeManager.ThemeType FromAppSettings(string? theme)
    {
        if (string.IsNullOrWhiteSpace(theme))
        {
            return ThemeManager.ThemeType.Light;
        }

        return theme.Equals("Dark", StringComparison.OrdinalIgnoreCase)
            ? ThemeManager.ThemeType.Dark
            : ThemeManager.ThemeType.Light;
    }

    /// <summary>
    /// Converts a ThemeManager.ThemeType to a theme string for app settings.
    /// </summary>
    /// <param name="theme">The theme type to convert</param>
    /// <returns>"Light" or "Dark"</returns>
    public static string ToAppSettings(ThemeManager.ThemeType theme)
    {
        return theme == ThemeManager.ThemeType.Dark ? "Dark" : "Light";
    }
}
