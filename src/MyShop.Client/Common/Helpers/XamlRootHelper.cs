using Microsoft.UI.Xaml;

namespace MyShop.Client.Common.Helpers;

/// <summary>
/// Helper for resolving XamlRoot from various sources
/// Centralizes XamlRoot resolution logic to avoid duplication
/// </summary>
public static class XamlRootHelper
{
    /// <summary>
    /// Resolves XamlRoot from provided instance or falls back to MainWindow
    /// </summary>
    /// <param name="xamlRoot">Optional XamlRoot instance</param>
    /// <returns>Resolved XamlRoot or null if not available</returns>
    public static XamlRoot? ResolveXamlRoot(XamlRoot? xamlRoot = null)
    {
        // Use provided XamlRoot if available
        if (xamlRoot != null)
            return xamlRoot;

        // Try to get from App.MainWindow
        var mainWindow = App.MainWindow;
        if (mainWindow?.Content?.XamlRoot != null)
            return mainWindow.Content.XamlRoot;

        System.Diagnostics.Debug.WriteLine("[XamlRootHelper] Could not resolve XamlRoot");
        return null;
    }

    /// <summary>
    /// Checks if XamlRoot is available
    /// </summary>
    /// <param name="xamlRoot">Optional XamlRoot instance</param>
    /// <returns>True if XamlRoot can be resolved</returns>
    public static bool IsXamlRootAvailable(XamlRoot? xamlRoot = null)
    {
        return ResolveXamlRoot(xamlRoot) != null;
    }
}
