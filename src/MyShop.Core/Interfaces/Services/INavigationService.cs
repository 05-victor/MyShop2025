namespace MyShop.Core.Interfaces.Services;

/// <summary>
/// Service quản lý navigation giữa các pages
/// Interface ở Core (không phụ thuộc vào UI framework)
/// Implementation ở Client (sử dụng WinUI Frame)
/// </summary>
public interface INavigationService
{
    /// <summary>
    /// Navigate đến page type cụ thể (root-level navigation - replaces entire shell)
    /// </summary>
    /// <param name="pageTypeName">Full name của page type (e.g., "MyShop.Client.Views.Shared.LoginPage")</param>
    /// <param name="parameter">Optional parameter to pass to the page</param>
    void NavigateTo(string pageTypeName, object? parameter = null);

    /// <summary>
    /// Navigate within the current dashboard shell's ContentFrame (preserves shell)
    /// Use this for navigation between pages that should remain within the dashboard shell
    /// </summary>
    /// <param name="pageTypeName">Full name của page type</param>
    /// <param name="parameter">Optional parameter to pass to the page</param>
    void NavigateInShell(string pageTypeName, object? parameter = null);

    /// <summary>
    /// Register the shell's ContentFrame for in-shell navigation
    /// Should be called by dashboard shells (AdminDashboardShell, SalesAgentDashboardShell, etc.)
    /// </summary>
    /// <param name="shellFrame">The ContentFrame from the dashboard shell</param>
    void RegisterShellFrame(object shellFrame);

    /// <summary>
    /// Unregister the shell's ContentFrame (called when shell is unloaded)
    /// </summary>
    void UnregisterShellFrame();

    /// <summary>
    /// Navigate back to previous page
    /// </summary>
    void GoBack();

    /// <summary>
    /// Check if navigation back is possible
    /// </summary>
    bool CanGoBack { get; }

    /// <summary>
    /// Clear navigation stack
    /// </summary>
    void ClearNavigationStack();

    /// <summary>
    /// Get current page type name
    /// </summary>
    string? CurrentPageTypeName { get; }
}
