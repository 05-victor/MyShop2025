namespace MyShop.Core.Interfaces.Services;

using MyShop.Core.Common;

/// <summary>
/// Service for managing navigation between pages.
/// Interface defined in Core layer (UI framework independent).
/// Implementation in Client layer (uses WinUI Frame).
/// Supports both root-level navigation and in-shell navigation patterns.
/// </summary>
public interface INavigationService
{
    /// <summary>
    /// Navigate to a specific page type (root-level navigation - replaces entire shell).
    /// Use this for major navigation changes like login/logout or switching shells.
    /// </summary>
    /// <param name="pageTypeName">Full name of the page type (e.g., "MyShop.Client.Views.Shared.LoginPage")</param>
    /// <param name="parameter">Optional parameter to pass to the page</param>
    Task<Result<Unit>> NavigateTo(string pageTypeName, object? parameter = null);

    /// <summary>
    /// Navigate within the current dashboard shell's ContentFrame (preserves shell).
    /// Use this for navigation between pages that should remain within the dashboard shell.
    /// </summary>
    /// <param name="pageTypeName">Full name of the page type</param>
    /// <param name="parameter">Optional parameter to pass to the page</param>
    Task<Result<Unit>> NavigateInShell(string pageTypeName, object? parameter = null);

    /// <summary>
    /// Register the shell's ContentFrame for in-shell navigation
    /// Should be called by dashboard shells (AdminDashboardShell, SalesAgentDashboardShell, etc.)
    /// </summary>
    /// <param name="shellFrame">The ContentFrame from the dashboard shell</param>
    Task<Result<Unit>> RegisterShellFrame(object shellFrame);

    /// <summary>
    /// Unregister the shell's ContentFrame (called when shell is unloaded)
    /// </summary>
    Task<Result<Unit>> UnregisterShellFrame();

    /// <summary>
    /// Navigate back to previous page
    /// </summary>
    Task<Result<Unit>> GoBack();

    /// <summary>
    /// Check if navigation back is possible
    /// </summary>
    bool CanGoBack { get; }

    /// <summary>
    /// Clear navigation stack
    /// </summary>
    Task<Result<Unit>> ClearNavigationStack();

    /// <summary>
    /// Get current page type name
    /// </summary>
    string? CurrentPageTypeName { get; }

    /// <summary>
    /// Show user details dialog (for Admin Users page)
    /// </summary>
    Task ShowUserDetailsDialogAsync(MyShop.Shared.Models.User userDetails);
}
