using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.UI.Composition.SystemBackdrops;

namespace MyShop.Client.Services;

/// <summary>
/// Manages application-wide theme switching using WinUI's native ThemeDictionaries.
/// Simplified version that uses ElementTheme.RequestedTheme instead of manual dictionary loading.
/// </summary>
public static class ThemeManager
{
    /// <summary>
    /// Available theme types for the application.
    /// </summary>
    public enum ThemeType
    {
        Light,
        Dark
    }

    /// <summary>
    /// Gets the currently active theme.
    /// </summary>
    public static ThemeType CurrentTheme { get; private set; } = ThemeType.Light;

    private const string ThemePreferenceKey = "UserThemePreference";

    /// <summary>
    /// Event raised when the theme changes.
    /// NOTE: With native ThemeDictionaries, manual subscriptions are no longer needed.
    /// Theme resources automatically update via {ThemeResource} binding.
    /// </summary>
    public static event Action<ThemeType>? ThemeChanged;

    /// <summary>
    /// Initializes the theme system with a default theme.
    /// Loads saved theme preference if available.
    /// </summary>
    /// <param name="defaultTheme">The theme to use if no preference is saved.</param>
    public static void Initialize(ThemeType defaultTheme = ThemeType.Light)
    {
        // Try to load saved preference
        var savedTheme = LoadThemePreference();
        var themeToApply = savedTheme ?? defaultTheme;
        
        ApplyTheme(themeToApply);
    }

    /// <summary>
    /// Applies the specified theme using WinUI's native RequestedTheme API.
    /// This automatically resolves ThemeResource references in XAML.
    /// Also updates the SystemBackdrop based on theme (Acrylic for Light, Mica for Dark).
    /// </summary>
    /// <param name="theme">The theme to apply.</param>
    public static void ApplyTheme(ThemeType theme)
    {
        try
        {
            CurrentTheme = theme;
            
            // Set RequestedTheme on the main window content
            // This triggers WinUI's automatic ThemeDictionaries resolution
            if (App.MainWindow?.Content is FrameworkElement rootElement)
            {
                rootElement.RequestedTheme = theme == ThemeType.Dark 
                    ? ElementTheme.Dark 
                    : ElementTheme.Light;
                
                System.Diagnostics.Debug.WriteLine($"ThemeManager: Applied {theme} theme via RequestedTheme");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"ThemeManager: MainWindow.Content not available yet");
            }

            // Update SystemBackdrop based on theme
            // Light theme: DesktopAcrylicBackdrop for better readability
            // Dark theme: MicaBackdrop for modern look
            UpdateBackdrop(theme);

            // Save preference
            SaveThemePreference(theme);

            // Notify subscribers (for legacy components that still need it)
            ThemeChanged?.Invoke(theme);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ThemeManager: Failed to apply theme {theme}: {ex.Message}");
        }
    }

    /// <summary>
    /// Updates the window's system backdrop based on the current theme.
    /// Light theme: DesktopAcrylicBackdrop (translucent)
    /// Dark theme: MicaBackdrop (opaque)
    /// </summary>
    private static void UpdateBackdrop(ThemeType theme)
    {
        try
        {
            if (App.MainWindow == null) return;

            SystemBackdrop backdrop = theme == ThemeType.Light 
                ? new DesktopAcrylicBackdrop() 
                : new MicaBackdrop();

            App.MainWindow.SystemBackdrop = backdrop;
            System.Diagnostics.Debug.WriteLine($"ThemeManager: Set {(theme == ThemeType.Light ? "DesktopAcrylicBackdrop" : "MicaBackdrop")}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ThemeManager: Failed to update backdrop: {ex.Message}");
        }
    }

    /// <summary>
    /// Toggles between Light and Dark themes.
    /// </summary>
    public static void ToggleTheme()
    {
        var newTheme = CurrentTheme == ThemeType.Light ? ThemeType.Dark : ThemeType.Light;
        ApplyTheme(newTheme);
    }

    /// <summary>
    /// Saves the current theme preference to local storage.
    /// </summary>
    private static void SaveThemePreference(ThemeType theme)
    {
        try
        {
            var appData = Windows.Storage.ApplicationData.Current;
            if (appData?.LocalSettings == null)
                return;
                
            var localSettings = appData.LocalSettings;
            localSettings.Values[ThemePreferenceKey] = theme.ToString();
        }
        catch (InvalidOperationException)
        {
            // ApplicationData not available yet - this is expected during early startup
        }
        catch (System.Runtime.InteropServices.COMException)
        {
            // COM object not available - this is expected during early startup
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ThemeManager: Failed to save preference: {ex.Message}");
        }
    }

    /// <summary>
    /// Loads the saved theme preference from local storage.
    /// </summary>
    private static ThemeType? LoadThemePreference()
    {
        try
        {
            var appData = Windows.Storage.ApplicationData.Current;
            if (appData?.LocalSettings == null)
                return null;
                
            var localSettings = appData.LocalSettings;
            if (localSettings.Values.TryGetValue(ThemePreferenceKey, out var value) && value is string themeStr)
            {
                if (Enum.TryParse<ThemeType>(themeStr, out var theme))
                    return theme;
            }
        }
        catch (InvalidOperationException)
        {
            // ApplicationData not available yet
            System.Diagnostics.Debug.WriteLine("ThemeManager: ApplicationData not available yet");
        }
        catch (System.Runtime.InteropServices.COMException)
        {
            // COM object not available
            System.Diagnostics.Debug.WriteLine("ThemeManager: COM exception during preference load");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ThemeManager: Failed to load preference: {ex.Message}");
        }

        return null;
    }

    // ============================================
    // LEGACY METHODS (DEPRECATED)
    // ============================================
    // These methods are kept for backward compatibility but no longer needed
    // with native ThemeDictionaries. They can be safely removed after migration.
    
    /// <summary>
    /// [DEPRECATED] Applies theme brushes to a framework element.
    /// With native ThemeDictionaries, this is no longer needed.
    /// ThemeResource bindings automatically update.
    /// </summary>
    [Obsolete("No longer needed with native ThemeDictionaries. Use {ThemeResource} in XAML instead.")]
    public static void ApplyTo(FrameworkElement root)
    {
        // No-op - native ThemeDictionaries handle this automatically
        System.Diagnostics.Debug.WriteLine("ThemeManager.ApplyTo() called but no longer needed with native ThemeDictionaries");
    }

    /// <summary>
    /// [DEPRECATED] Registers a framework element for theme updates.
    /// With native ThemeDictionaries, registration is no longer needed.
    /// </summary>
    [Obsolete("No longer needed with native ThemeDictionaries. Use {ThemeResource} in XAML instead.")]
    public static void RegisterRoot(FrameworkElement root)
    {
        // No-op - native ThemeDictionaries handle this automatically
        System.Diagnostics.Debug.WriteLine("ThemeManager.RegisterRoot() called but no longer needed with native ThemeDictionaries");
    }
}
