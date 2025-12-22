using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyShop.Client.Services;

/// <summary>
/// Manages application-wide theme switching between Light and Dark modes.
/// Supports runtime theme changes with persistent storage of user preferences.
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

    private const string LightThemePath = "ms-appx:///Themes/LightTheme.xaml";
    private const string DarkThemePath = "ms-appx:///Themes/DarkTheme.xaml";
    private const string ThemePreferenceKey = "UserThemePreference";

    private static readonly List<WeakReference<FrameworkElement>> _registeredRoots = new();

    /// <summary>
    /// Event raised when the theme changes.
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
    /// Applies the specified theme to the application.
    /// </summary>
    /// <param name="theme">The theme to apply.</param>
    public static void ApplyTheme(ThemeType theme)
    {
        try
        {
            var app = Application.Current;
            if (app == null) return;

            // Check if Resources are accessible (may throw during early startup)
            try
            {
                var _ = app.Resources;
            }
            catch (System.Runtime.InteropServices.COMException)
            {
                // Resources not available yet - save theme for later
                CurrentTheme = theme;
                System.Diagnostics.Debug.WriteLine($"ThemeManager: Deferring theme application (Resources not ready)");
                return;
            }

            RemoveThemeDictionaries(app);

            string path = theme switch
            {
                ThemeType.Light => LightThemePath,
                ThemeType.Dark => DarkThemePath,
                _ => LightThemePath
            };

            var dict = new ResourceDictionary { Source = new Uri(path) };
            app.Resources.MergedDictionaries.Add(dict);
            CurrentTheme = theme;

            // CRITICAL: Set RequestedTheme on the main window content
            // This is required for WinUI ThemeResource to resolve correctly
            try
            {
                if (App.MainWindow?.Content is FrameworkElement rootElement)
                {
                    rootElement.RequestedTheme = theme == ThemeType.Dark 
                        ? ElementTheme.Dark 
                        : ElementTheme.Light;
                    System.Diagnostics.Debug.WriteLine($"ThemeManager: Set RequestedTheme to {rootElement.RequestedTheme}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ThemeManager: Failed to set RequestedTheme: {ex.Message}");
            }

            // Save preference
            SaveThemePreference(theme);

            // Reapply to all registered roots
            ReapplyAll();

            // Notify subscribers
            ThemeChanged?.Invoke(theme);
        }
        catch (System.Runtime.InteropServices.COMException ex)
        {
            // COM exception during early startup - defer theme application
            CurrentTheme = theme;
            System.Diagnostics.Debug.WriteLine($"ThemeManager: COM exception during theme application: {ex.Message}");
        }
        catch (Exception ex)
        {
            // Log error but don't crash app
            System.Diagnostics.Debug.WriteLine($"ThemeManager: Failed to load theme {theme}: {ex.Message}");
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
    /// Applies theme brushes to a specific framework element.
    /// Useful for dynamic content that needs explicit theme application.
    /// </summary>
    /// <param name="root">The root element to apply theme to.</param>
    public static void ApplyTo(FrameworkElement root)
    {
        if (root == null) return;
        var appRes = Application.Current?.Resources;
        if (appRes == null) return;

        ApplyBackgroundBrush(root, appRes);
        ApplyNavigationViewTheme(root, appRes);
        ApplyBordersTheme(root, appRes);
        ApplyTextBlocksTheme(root, appRes);
    }

    /// <summary>
    /// Registers a framework element to receive theme updates.
    /// Weak references are used to prevent memory leaks.
    /// </summary>
    /// <param name="root">The root element to register.</param>
    public static void RegisterRoot(FrameworkElement root)
    {
        if (root == null) return;
        
        lock (_registeredRoots)
        {
            if (!_registeredRoots.Any(wr => wr.TryGetTarget(out var t) && t == root))
                _registeredRoots.Add(new WeakReference<FrameworkElement>(root));
        }

        ApplyTo(root);
    }

    /// <summary>
    /// Saves the current theme preference to local storage.
    /// </summary>
    private static void SaveThemePreference(ThemeType theme)
    {
        try
        {
            // Check if ApplicationData is available
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
            // Check if ApplicationData is available (may not be during early app startup)
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
            // ApplicationData not available yet - this is expected during early startup
            System.Diagnostics.Debug.WriteLine("ThemeManager: ApplicationData not available yet");
        }
        catch (System.Runtime.InteropServices.COMException)
        {
            // COM object not available - this is expected during early startup
            System.Diagnostics.Debug.WriteLine("ThemeManager: COM exception during preference load - using default");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ThemeManager: Failed to load preference: {ex.Message}");
        }

        return null;
    }

    /// <summary>
    /// Removes all theme dictionaries from the application resources.
    /// </summary>
    private static void RemoveThemeDictionaries(Application app)
    {
        var toRemove = app.Resources.MergedDictionaries
            .Where(d => d.Source != null && 
                d.Source.OriginalString.Contains("Theme.xaml", StringComparison.OrdinalIgnoreCase))
            .ToList();

        foreach (var d in toRemove)
            app.Resources.MergedDictionaries.Remove(d);
    }

    /// <summary>
    /// Reapplies theme to all registered roots, cleaning up dead references.
    /// </summary>
    private static void ReapplyAll()
    {
        lock (_registeredRoots)
        {
            for (int i = _registeredRoots.Count - 1; i >= 0; i--)
            {
                if (_registeredRoots[i].TryGetTarget(out var root) && root != null)
                {
                    root.DispatcherQueue.TryEnqueue(() => ApplyTo(root));
                }
                else
                {
                    // Clean up dead references
                    _registeredRoots.RemoveAt(i);
                }
            }
        }
    }

    private static void ApplyBackgroundBrush(FrameworkElement root, ResourceDictionary appRes)
    {
        try
        {
            if (appRes.TryGetValue("BackgroundLightBrush", out var brush) && brush is Brush backgroundBrush)
            {
                if (root is Panel panel)
                    panel.Background = backgroundBrush;
                else if (root is Control control)
                    control.Background = backgroundBrush;
            }
        }
        catch { /* Ignore errors */ }
    }

    private static void ApplyNavigationViewTheme(FrameworkElement root, ResourceDictionary appRes)
    {
        try
        {
            var nav = FindElementByName<NavigationView>(root, "NavigationView");
            if (nav == null)
                nav = FindElementByName<NavigationView>(root, "navView");
            
            if (nav != null)
            {
                if (appRes.TryGetValue("CardBackgroundBrush", out var bgBrush) && bgBrush is Brush navBg)
                    nav.Background = navBg;

                if (appRes.TryGetValue("TextPrimaryBrush", out var fgBrush) && fgBrush is Brush navFg)
                    nav.Foreground = navFg;

                nav.UpdateLayout();
            }
        }
        catch { /* Ignore errors */ }
    }

    private static void ApplyBordersTheme(FrameworkElement root, ResourceDictionary appRes)
    {
        try
        {
            var borders = FindAllDescendants<Border>(root);
            foreach (var border in borders)
            {
                // Apply border brush if border has thickness
                if (border.BorderThickness.Top > 0 || border.BorderThickness.Left > 0)
                {
                    if (appRes.TryGetValue("BorderDefaultBrush", out var borderBrush) && borderBrush is Brush brush)
                        border.BorderBrush = brush;
                }

                // Apply card background to card-like borders
                if (border.Name?.Contains("Card", StringComparison.OrdinalIgnoreCase) == true)
                {
                    if (appRes.TryGetValue("CardBackgroundBrush", out var cardBg) && cardBg is Brush cardBrush)
                        border.Background = cardBrush;
                }
            }
        }
        catch { /* Ignore errors */ }
    }

    private static void ApplyTextBlocksTheme(FrameworkElement root, ResourceDictionary appRes)
    {
        try
        {
            var textBlocks = FindAllDescendants<TextBlock>(root);
            foreach (var tb in textBlocks)
            {
                // Don't override explicitly set foregrounds
                if (tb.ReadLocalValue(TextBlock.ForegroundProperty) == DependencyProperty.UnsetValue)
                {
                    if (appRes.TryGetValue("TextPrimaryBrush", out var textBrush) && textBrush is Brush brush)
                        tb.Foreground = brush;
                }
            }
        }
        catch { /* Ignore errors */ }
    }

    private static T? FindElementByName<T>(DependencyObject parent, string name) where T : FrameworkElement
    {
        if (parent == null) return null;

        try
        {
            if (parent is FrameworkElement element)
            {
                var found = element.FindName(name);
                if (found is T t) return t;
            }
        }
        catch { /* Ignore errors */ }

        return FindDescendantByName<T>(parent, name);
    }

    private static T? FindDescendantByName<T>(DependencyObject parent, string name) where T : FrameworkElement
    {
        if (parent == null) return null;

        int count = VisualTreeHelper.GetChildrenCount(parent);
        for (int i = 0; i < count; i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is T t && (child as FrameworkElement)?.Name == name)
                return t;

            var result = FindDescendantByName<T>(child, name);
            if (result != null) return result;
        }

        return null;
    }

    private static List<T> FindAllDescendants<T>(DependencyObject parent) where T : DependencyObject
    {
        var results = new List<T>();
        if (parent == null) return results;

        try
        {
            int count = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T t)
                    results.Add(t);

                results.AddRange(FindAllDescendants<T>(child));
            }
        }
        catch { /* Ignore errors */ }

        return results;
    }
}
