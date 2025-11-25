using Microsoft.UI.Xaml.Controls;
using System;
using System.Linq;
using MyShop.Client.Helpers;
using MyShop.Core.Common;

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
    public async Task<Result<Unit>> RegisterShellFrame(object shellFrame)
    {
        try
        {
            if (shellFrame is not Frame frame)
                return Result<Unit>.Failure("Shell frame must be of type Frame");

            _shellFrame = frame;
            AppLogger.Debug("Shell ContentFrame registered for in-shell navigation");
            return Result<Unit>.Success(Unit.Value);
        }
        catch (Exception ex)
        {
            return Result<Unit>.Failure($"Failed to register shell frame: {ex.Message}");
        }
    }

    /// <summary>
    /// Unregister the shell's ContentFrame
    /// </summary>
    public async Task<Result<Unit>> UnregisterShellFrame()
    {
        try
        {
            _shellFrame = null;
            AppLogger.Debug("Shell ContentFrame unregistered");
            return Result<Unit>.Success(Unit.Value);
        }
        catch (Exception ex)
        {
            return Result<Unit>.Failure($"Failed to unregister shell frame: {ex.Message}");
        }
    }

    /// <summary>
    /// Navigate by page type name (Core interface method) - root-level navigation
    /// </summary>
    public async Task<Result<Unit>> NavigateTo(string pageTypeName, object? parameter = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(pageTypeName))
                return Result<Unit>.Failure("Page type name cannot be null or empty");

            var pageType = ResolvePageType(pageTypeName);
            return await NavigateToInternal(pageType, parameter);
        }
        catch (Exception ex)
        {
            return Result<Unit>.Failure($"Navigation failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Navigate within the shell's ContentFrame - preserves the dashboard shell
    /// </summary>
    public async Task<Result<Unit>> NavigateInShell(string pageTypeName, object? parameter = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(pageTypeName))
                return Result<Unit>.Failure("Page type name cannot be null or empty");

            if (_shellFrame == null)
                return Result<Unit>.Failure("Shell frame not registered. Ensure the dashboard shell has called RegisterShellFrame.");

            var pageType = ResolvePageType(pageTypeName);
            return await NavigateInShellInternal(pageType, parameter);
        }
        catch (Exception ex)
        {
            return Result<Unit>.Failure($"In-shell navigation failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Navigate by Type (convenience method for strongly-typed navigation) - root-level
    /// </summary>
    private async Task<Result<Unit>> NavigateToInternal(Type pageType, object? parameter = null)
    {
        try
        {
            if (_rootFrame is null)
                return Result<Unit>.Failure("NavigationService must be initialized before use.");

            if (pageType == null)
                return Result<Unit>.Failure("Page type cannot be null");

            if (_rootFrame.CurrentSourcePageType == pageType && parameter == null)
            {
                AppLogger.Debug($"Skipping root navigation - already on {pageType.Name}");
                return Result<Unit>.Success(Unit.Value);
            }

            AppLogger.Info($"[Root] Navigating to {pageType.Name}...");
            _rootFrame.Navigate(pageType, parameter);
            AppLogger.Success($"[Root] Navigation to {pageType.Name} completed");
            return Result<Unit>.Success(Unit.Value);
        }
        catch (Exception ex)
        {
            AppLogger.Error($"[Root] Navigation to {pageType.Name} failed", ex);
            return Result<Unit>.Failure($"Navigation to {pageType.Name} failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Navigate by Type within shell - preserves dashboard shell
    /// </summary>
    private async Task<Result<Unit>> NavigateInShellInternal(Type pageType, object? parameter = null)
    {
        try
        {
            if (_shellFrame == null)
                return Result<Unit>.Failure("Shell frame not registered.");

            if (pageType == null)
                return Result<Unit>.Failure("Page type cannot be null");

            if (_shellFrame.CurrentSourcePageType == pageType && parameter == null)
            {
                AppLogger.Debug($"Skipping in-shell navigation - already on {pageType.Name}");
                return Result<Unit>.Success(Unit.Value);
            }

            AppLogger.Info($"[Shell] Navigating to {pageType.Name}...");
            _shellFrame.Navigate(pageType, parameter);
            AppLogger.Success($"[Shell] Navigation to {pageType.Name} completed");
            return Result<Unit>.Success(Unit.Value);
        }
        catch (Exception ex)
        {
            AppLogger.Error($"[Shell] Navigation to {pageType.Name} failed", ex);
            return Result<Unit>.Failure($"In-shell navigation to {pageType.Name} failed: {ex.Message}");
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
    public async Task<Result<Unit>> GoBack()
    {
        try
        {
            if (_rootFrame?.CanGoBack == true)
            {
                AppLogger.Debug("Navigating back");
                _rootFrame.GoBack();
                return Result<Unit>.Success(Unit.Value);
            }
            else
            {
                AppLogger.Warning("Cannot navigate back - no pages in back stack");
                return Result<Unit>.Failure("Cannot navigate back - no pages in back stack");
            }
        }
        catch (Exception ex)
        {
            return Result<Unit>.Failure($"Go back failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Check if back navigation is possible
    /// </summary>
    public bool CanGoBack => _rootFrame?.CanGoBack ?? false;

    /// <summary>
    /// Clear navigation stack (remove all back stack entries)
    /// </summary>
    public async Task<Result<Unit>> ClearNavigationStack()
    {
        try
        {
            if (_rootFrame != null)
            {
                _rootFrame.BackStack.Clear();
                AppLogger.Debug("Navigation stack cleared");
                return Result<Unit>.Success(Unit.Value);
            }
            return Result<Unit>.Failure("Root frame not initialized");
        }
        catch (Exception ex)
        {
            return Result<Unit>.Failure($"Clear navigation stack failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Get current page type name
    /// </summary>
    public string? CurrentPageTypeName => _rootFrame?.CurrentSourcePageType?.FullName;
}
