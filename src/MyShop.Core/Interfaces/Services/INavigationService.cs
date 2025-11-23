namespace MyShop.Core.Interfaces.Services;

/// <summary>
/// Service quản lý navigation giữa các pages
/// Interface ở Core (không phụ thuộc vào UI framework)
/// Implementation ở Client (sử dụng WinUI Frame)
/// </summary>
public interface INavigationService
{
    /// <summary>
    /// Navigate đến page type cụ thể
    /// </summary>
    /// <param name="pageTypeName">Full name của page type (e.g., "MyShop.Client.Views.Shared.LoginPage")</param>
    /// <param name="parameter">Optional parameter to pass to the page</param>
    void NavigateTo(string pageTypeName, object? parameter = null);

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
