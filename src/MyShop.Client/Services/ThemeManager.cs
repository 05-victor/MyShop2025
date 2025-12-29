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

    /// <summary>
    /// Event raised when the theme changes.
    /// NOTE: With native ThemeDictionaries, manual subscriptions are no longer needed.
    /// Theme resources automatically update via {ThemeResource} binding.
    /// </summary>
    public static event Action<ThemeType>? ThemeChanged;

    /// <summary>
    /// Initializes the theme system with a default theme.
    /// Only sets CurrentTheme - no loading of saved preferences.
    /// Persistence is managed by Settings storage/service.
    /// </summary>
    /// <param name="defaultTheme">The theme to use.</param>
    public static void Initialize(ThemeType defaultTheme = ThemeType.Light)
    {
        CurrentTheme = defaultTheme;
        System.Diagnostics.Debug.WriteLine($"[ThemeManager] Initialized with default theme: {defaultTheme}");
    }

    /// <summary>
    /// Applies the specified theme using WinUI's native RequestedTheme API.
    /// This automatically resolves ThemeResource references in XAML.
    /// Also updates the SystemBackdrop based on theme (Acrylic for Light, Mica for Dark).
    /// Does NOT persist theme - persistence is managed by Settings storage/service.
    /// </summary>
    /// <param name="theme">The theme to apply.</param>
    public static void ApplyTheme(ThemeType theme)
    {
        try
        {
            CurrentTheme = theme;

            // Set RequestedTheme on the main window content (Grid)
            // This triggers WinUI's automatic ThemeDictionaries resolution
            if (App.MainWindow?.Content is FrameworkElement rootElement)
            {
                var elementTheme = theme == ThemeType.Dark ? ElementTheme.Dark : ElementTheme.Light;
                rootElement.RequestedTheme = elementTheme;

                System.Diagnostics.Debug.WriteLine($"[ThemeManager] ✓ Applied {theme} theme via RequestedTheme={elementTheme}");
                System.Diagnostics.Debug.WriteLine($"[ThemeManager] Root element type: {rootElement.GetType().Name}");
                System.Diagnostics.Debug.WriteLine($"[ThemeManager] RequestedTheme is now: {rootElement.RequestedTheme}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[ThemeManager] ⚠ MainWindow.Content not available yet (MainWindow={App.MainWindow}, Content={App.MainWindow?.Content})");
            }

            // Update SystemBackdrop based on theme
            // Light theme: DesktopAcrylicBackdrop for better readability
            // Dark theme: MicaBackdrop for modern look
            UpdateBackdrop(theme);

            // Notify subscribers (for legacy components that still need it)
            ThemeChanged?.Invoke(theme);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ThemeManager] ❌ Failed to apply theme {theme}: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"[ThemeManager] Stack trace: {ex.StackTrace}");
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
            System.Diagnostics.Debug.WriteLine($"[ThemeManager] Set {(theme == ThemeType.Light ? "DesktopAcrylicBackdrop" : "MicaBackdrop")}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ThemeManager] Failed to update backdrop: {ex.Message}");
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
