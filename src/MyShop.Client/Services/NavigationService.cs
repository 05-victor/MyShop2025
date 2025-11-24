using Microsoft.UI.Xaml.Controls;
using System;
using System.Linq;
using MyShop.Client.Helpers;

namespace MyShop.Client.Services;

/// <summary>
/// NavigationService implementation for WinUI 3
/// Wraps Frame navigation with logging and error handling
/// Supports both root-level navigation (whole app) and shell-scoped navigation (within dashboard)
/// </summary>
public class NavigationService : MyShop.Core.Interfaces.Services.INavigationService
{
    private Frame? _rootFrame;
    private Frame? _shellFrame;

    /// <summary>
    /// Initialize navigation service with WinUI Frame
    /// Must be called before any navigation operations
    /// </summary>
    public void Initialize(Frame frame)
    {
        _rootFrame = frame ?? throw new ArgumentNullException(nameof(frame));
        AppLogger.Debug("NavigationService initialized with root Frame");
    }

    /// <summary>
    /// Register the shell's ContentFrame for in-shell navigation
    /// Called by dashboard shells when they load
    /// </summary>
    public void RegisterShellFrame(object shellFrame)
    {
        if (shellFrame is not Frame frame)
            throw new ArgumentException("Shell frame must be of type Frame", nameof(shellFrame));

        _shellFrame = frame;
        AppLogger.Debug("Shell ContentFrame registered for in-shell navigation");
    }

    /// <summary>
    /// Unregister the shell's ContentFrame
    /// </summary>
    public void UnregisterShellFrame()
    {
        _shellFrame = null;
        AppLogger.Debug("Shell ContentFrame unregistered");
    }

    /// <summary>
    /// Navigate by page type name (Core interface method) - root-level navigation
    /// </summary>
    public void NavigateTo(string pageTypeName, object? parameter = null)
    {
        if (string.IsNullOrWhiteSpace(pageTypeName))
            throw new ArgumentException("Page type name cannot be null or empty", nameof(pageTypeName));

        var pageType = ResolvePageType(pageTypeName);
        NavigateTo(pageType, parameter);
    }

    /// <summary>
    /// Navigate within the shell's ContentFrame - preserves the dashboard shell
    /// </summary>
    public void NavigateInShell(string pageTypeName, object? parameter = null)
    {
        if (string.IsNullOrWhiteSpace(pageTypeName))
            throw new ArgumentException("Page type name cannot be null or empty", nameof(pageTypeName));

        if (_shellFrame == null)
            throw new InvalidOperationException("Shell frame not registered. Ensure the dashboard shell has called RegisterShellFrame.");

        var pageType = ResolvePageType(pageTypeName);
        NavigateInShell(pageType, parameter);
    }

    /// <summary>
    /// Navigate by Type (convenience method for strongly-typed navigation) - root-level
    /// </summary>
    public void NavigateTo(Type pageType, object? parameter = null)
    {
        if (_rootFrame is null)
            throw new InvalidOperationException("NavigationService must be initialized before use.");

        if (pageType == null)
            throw new ArgumentNullException(nameof(pageType));

        if (_rootFrame.CurrentSourcePageType == pageType && parameter == null)
        {
            AppLogger.Debug($"Skipping root navigation - already on {pageType.Name}");
            return;
        }

        try
        {
            AppLogger.Info($"[Root] Navigating to {pageType.Name}...");
            _rootFrame.Navigate(pageType, parameter);
            AppLogger.Success($"[Root] Navigation to {pageType.Name} completed");
        }
        catch (Exception ex)
        {
            AppLogger.Error($"[Root] Navigation to {pageType.Name} failed", ex);
            throw;
        }
    }

    /// <summary>
    /// Navigate by Type within shell - preserves dashboard shell
    /// </summary>
    private void NavigateInShell(Type pageType, object? parameter = null)
    {
        if (_shellFrame == null)
            throw new InvalidOperationException("Shell frame not registered.");

        if (pageType == null)
            throw new ArgumentNullException(nameof(pageType));

        if (_shellFrame.CurrentSourcePageType == pageType && parameter == null)
        {
            AppLogger.Debug($"Skipping in-shell navigation - already on {pageType.Name}");
            return;
        }

        try
        {
            AppLogger.Info($"[Shell] Navigating to {pageType.Name}...");
            _shellFrame.Navigate(pageType, parameter);
            AppLogger.Success($"[Shell] Navigation to {pageType.Name} completed");
        }
        catch (Exception ex)
        {
            AppLogger.Error($"[Shell] Navigation to {pageType.Name} failed", ex);
            throw;
        }
    }

    /// <summary>
    /// Resolve page type from string name
    /// </summary>
    private Type ResolvePageType(string pageTypeName)
    {
        var pageType = Type.GetType(pageTypeName);
        if (pageType == null)
        {
            // Try to find in current assembly
            var assembly = typeof(NavigationService).Assembly;
            pageType = assembly.GetTypes().FirstOrDefault(t => t.FullName == pageTypeName);
        }

        if (pageType == null)
            throw new InvalidOperationException($"Could not resolve page type: {pageTypeName}");

        return pageType;
    }

    /// <summary>
    /// Navigate back to previous page
    /// </summary>
    public void GoBack()
    {
        if (_rootFrame?.CanGoBack == true)
        {
            AppLogger.Debug("Navigating back");
            _rootFrame.GoBack();
        }
        else
        {
            AppLogger.Warning("Cannot navigate back - no pages in back stack");
        }
    }

    /// <summary>
    /// Check if back navigation is possible
    /// </summary>
    public bool CanGoBack => _rootFrame?.CanGoBack ?? false;

    /// <summary>
    /// Clear navigation stack (remove all back stack entries)
    /// </summary>
    public void ClearNavigationStack()
    {
        if (_rootFrame != null)
        {
            _rootFrame.BackStack.Clear();
            AppLogger.Debug("Navigation stack cleared");
        }
    }

    /// <summary>
    /// Get current page type name
    /// </summary>
    public string? CurrentPageTypeName => _rootFrame?.CurrentSourcePageType?.FullName;
}
