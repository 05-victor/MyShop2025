using Microsoft.UI.Xaml.Controls;
using System;
using System.Linq;
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
        LoggingService.Instance.Information("NavigationService initialized with root Frame");
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
            LoggingService.Instance.Debug("Shell ContentFrame registered for in-shell navigation");
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
            LoggingService.Instance.Debug("Shell ContentFrame unregistered");
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
            {
                LoggingService.Instance.Error("[Root] NavigationService not initialized");
                return Result<Unit>.Failure("NavigationService must be initialized before use.");
            }

            if (pageType == null)
            {
                LoggingService.Instance.Error("[Root] Page type is null");
                return Result<Unit>.Failure("Page type cannot be null");
            }

            if (_rootFrame.CurrentSourcePageType == pageType && parameter == null)
            {
                LoggingService.Instance.Debug($"[Root] Skipping navigation - already on {pageType.Name}");
                return Result<Unit>.Success(Unit.Value);
            }

            // Use NavigationLogger.SafeNavigate instead of direct Frame.Navigate
            var success = NavigationLogger.SafeNavigate(_rootFrame, pageType, parameter, "Root");

            if (success)
            {
                return Result<Unit>.Success(Unit.Value);
            }
            else
            {
                return Result<Unit>.Failure($"Navigation to {pageType.Name} failed: Frame.Navigate returned false");
            }
        }
        catch (Exception ex)
        {
            LoggingService.Instance.Error($"[Root] Navigation to {pageType.Name} failed", ex);
            NavigationLogger.LogNavigationFailure(
                _rootFrame?.CurrentSourcePageType?.Name ?? "Unknown",
                pageType.Name,
                ex,
                parameter
            );
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
            {
                LoggingService.Instance.Error("[Shell] Shell frame not registered");
                return Result<Unit>.Failure("Shell frame not registered.");
            }

            if (pageType == null)
            {
                LoggingService.Instance.Error("[Shell] Page type is null");
                return Result<Unit>.Failure("Page type cannot be null");
            }

            if (_shellFrame.CurrentSourcePageType == pageType && parameter == null)
            {
                LoggingService.Instance.Debug($"[Shell] Skipping navigation - already on {pageType.Name}");
                return Result<Unit>.Success(Unit.Value);
            }

            // Use NavigationLogger.SafeNavigate instead of direct Frame.Navigate
            var success = NavigationLogger.SafeNavigate(_shellFrame, pageType, parameter, "Shell");

            if (success)
            {
                return Result<Unit>.Success(Unit.Value);
            }
            else
            {
                return Result<Unit>.Failure($"In-shell navigation to {pageType.Name} failed: Frame.Navigate returned false");
            }
        }
        catch (Exception ex)
        {
            LoggingService.Instance.Error($"[Shell] Navigation to {pageType.Name} failed", ex);
            NavigationLogger.LogNavigationFailure(
                _shellFrame?.CurrentSourcePageType?.Name ?? "Unknown",
                pageType.Name,
                ex,
                parameter
            );
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
                LoggingService.Instance.Debug("Navigating back");
                _rootFrame.GoBack();
                return Result<Unit>.Success(Unit.Value);
            }
            else
            {
                LoggingService.Instance.Warning("Cannot navigate back - no pages in back stack");
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
                LoggingService.Instance.Debug("Navigation stack cleared");
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

    /// <summary>
    /// Show user details dialog on Admin Users Page
    /// </summary>
    public async Task ShowUserDetailsDialogAsync(MyShop.Shared.Models.User userDetails)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"[NavigationService] ShowUserDetailsDialogAsync START for user: {userDetails?.Username}");
            LoggingService.Instance.Debug($"[NavigationService] ShowUserDetailsDialogAsync called for user: {userDetails?.Username}");

            // Get current page from root frame
            if (_rootFrame?.Content is not Microsoft.UI.Xaml.Controls.Page currentPage)
            {
                System.Diagnostics.Debug.WriteLine("[NavigationService] ERROR: Current page is not a Page type or frame content is null");
                LoggingService.Instance.Warning("Current page is not a Page type or frame content is null");
                return;
            }

            System.Diagnostics.Debug.WriteLine($"[NavigationService] Root frame content type: {currentPage.GetType().Name}");

            // Try to find AdminUsersPage directly (if it's the direct root content)
            var adminUsersPage = currentPage as MyShop.Client.Views.Admin.AdminUsersPage;
            if (adminUsersPage != null)
            {
                System.Diagnostics.Debug.WriteLine($"[NavigationService] Found AdminUsersPage as root content");
                await adminUsersPage.ShowUserDetailsDialog(userDetails);
                System.Diagnostics.Debug.WriteLine($"[NavigationService] Dialog method called on AdminUsersPage");
                return;
            }

            // If not found, check if it's AdminDashboardShell with ContentFrame
            System.Diagnostics.Debug.WriteLine($"[NavigationService] AdminUsersPage not found as root content, checking for AdminDashboardShell...");
            var shell = currentPage as MyShop.Client.Views.Shell.AdminDashboardShell;
            if (shell != null)
            {
                System.Diagnostics.Debug.WriteLine($"[NavigationService] Found AdminDashboardShell");

                // Try to get ContentFrame from shell
                var contentFrame = shell.FindName("ContentFrame") as Microsoft.UI.Xaml.Controls.Frame;
                if (contentFrame != null)
                {
                    System.Diagnostics.Debug.WriteLine($"[NavigationService] Found ContentFrame in shell");

                    // Get the content of the frame
                    if (contentFrame.Content is MyShop.Client.Views.Admin.AdminUsersPage frameAdminUsersPage)
                    {
                        System.Diagnostics.Debug.WriteLine($"[NavigationService] Found AdminUsersPage in ContentFrame, calling ShowUserDetailsDialog");
                        await frameAdminUsersPage.ShowUserDetailsDialog(userDetails);
                        System.Diagnostics.Debug.WriteLine($"[NavigationService] Dialog method called on AdminUsersPage from ContentFrame");
                        return;
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"[NavigationService] ContentFrame.Content type: {contentFrame.Content?.GetType().Name ?? "null"}");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[NavigationService] ContentFrame not found in shell");
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[NavigationService] Not AdminDashboardShell, shell type: {currentPage.GetType().Name}");
            }

            System.Diagnostics.Debug.WriteLine($"[NavigationService] ERROR: Could not find AdminUsersPage in shell hierarchy");
            LoggingService.Instance.Warning($"Could not find AdminUsersPage in shell hierarchy");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[NavigationService] ShowUserDetailsDialogAsync EXCEPTION: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"[NavigationService] Stack trace: {ex.StackTrace}");
            LoggingService.Instance.Error($"[NavigationService] ShowUserDetailsDialogAsync error: {ex.Message}", ex);
        }
    }
}
