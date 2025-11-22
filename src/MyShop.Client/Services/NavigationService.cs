using Microsoft.UI.Xaml.Controls;
using System;
using System.Linq;
using MyShop.Client.Helpers;

namespace MyShop.Client.Services;

/// <summary>
/// NavigationService implementation for WinUI 3
/// Wraps Frame navigation with logging and error handling
/// Moved from Helpers to Services for better organization
/// </summary>
public class NavigationService : MyShop.Core.Interfaces.Services.INavigationService
{
    private Frame? _rootFrame;

    /// <summary>
    /// Initialize navigation service with WinUI Frame
    /// Must be called before any navigation operations
    /// </summary>
    public void Initialize(Frame frame)
    {
        _rootFrame = frame ?? throw new ArgumentNullException(nameof(frame));
        AppLogger.Debug("NavigationService initialized with Frame");
    }

    /// <summary>
    /// Navigate by page type name (Core interface method)
    /// </summary>
    public void NavigateTo(string pageTypeName, object? parameter = null)
    {
        if (string.IsNullOrWhiteSpace(pageTypeName))
            throw new ArgumentException("Page type name cannot be null or empty", nameof(pageTypeName));

        // Resolve Type from string name
        var pageType = Type.GetType(pageTypeName);
        if (pageType == null)
        {
            // Try to find in current assembly
            var assembly = typeof(NavigationService).Assembly;
            pageType = assembly.GetTypes().FirstOrDefault(t => t.FullName == pageTypeName);
        }

        if (pageType == null)
            throw new InvalidOperationException($"Could not resolve page type: {pageTypeName}");

        NavigateTo(pageType, parameter);
    }

    /// <summary>
    /// Navigate by Type (convenience method for strongly-typed navigation)
    /// </summary>
    public void NavigateTo(Type pageType, object? parameter = null)
    {
        if (_rootFrame is null)
            throw new InvalidOperationException("NavigationService must be initialized before use.");

        if (pageType == null)
            throw new ArgumentNullException(nameof(pageType));

        if (_rootFrame.CurrentSourcePageType == pageType && parameter == null)
        {
            AppLogger.Debug($"Skipping navigation - already on {pageType.Name}");
            return;
        }

        try
        {
            AppLogger.Info($"Navigating to {pageType.Name}...");
            _rootFrame.Navigate(pageType, parameter);
            AppLogger.Success($"Navigation to {pageType.Name} completed");
        }
        catch (Exception ex)
        {
            AppLogger.Error($"Navigation to {pageType.Name} failed", ex);
            throw;
        }
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
