using Microsoft.UI.Xaml.Controls;

namespace MyShop.Client.Helpers;

/// <summary>
/// Service quản lý navigation giữa các pages
/// Interface ở Core, Implementation ở Client (vì phụ thuộc WinUI)
/// </summary>
public interface INavigationService
{
    /// <summary>
    /// Initialize navigation service với Frame
    /// </summary>
    void Initialize(Frame frame);

    /// <summary>
    /// Navigate đến page type cụ thể
    /// </summary>
    void NavigateTo(Type pageType, object? parameter = null);

    /// <summary>
    /// Navigate back
    /// </summary>
    void GoBack();

    /// <summary>
    /// Check xem có thể go back không
    /// </summary>
    bool CanGoBack { get; }
}
