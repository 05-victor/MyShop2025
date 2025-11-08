namespace MyShop.Client.Helpers;

/// <summary>
/// Helper service cho toast notifications
/// Interface ở Core, Implementation ở Client (vì phụ thuộc WinUI)
/// </summary>
public interface IToastHelper
{
    /// <summary>
    /// Hiển thị success toast
    /// </summary>
    void ShowSuccess(string message);

    /// <summary>
    /// Hiển thị error toast
    /// </summary>
    void ShowError(string message);

    /// <summary>
    /// Hiển thị info toast
    /// </summary>
    void ShowInfo(string message);

    /// <summary>
    /// Hiển thị warning toast
    /// </summary>
    void ShowWarning(string message);

    /// <summary>
    /// Hiển thị connection error dialog với actions
    /// </summary>
    Task<ConnectionErrorAction> ShowConnectionErrorAsync(string message);
}

/// <summary>
/// Actions user có thể chọn khi gặp connection error
/// </summary>
public enum ConnectionErrorAction
{
    Retry,
    ConfigureServer,
    Cancel
}
