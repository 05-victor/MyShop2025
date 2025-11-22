namespace MyShop.Core.Interfaces.Services;

/// <summary>
/// Service for displaying toast notifications
/// </summary>
public interface IToastService
{
    /// <summary>
    /// Show a success toast notification
    /// </summary>
    void ShowSuccess(string message);

    /// <summary>
    /// Show an error toast notification
    /// </summary>
    void ShowError(string message);

    /// <summary>
    /// Show an information toast notification
    /// </summary>
    void ShowInfo(string message);

    /// <summary>
    /// Show a warning toast notification
    /// </summary>
    void ShowWarning(string message);

    /// <summary>
    /// Show connection error dialog with action options
    /// </summary>
    Task<ConnectionErrorAction> ShowConnectionErrorAsync(string message);
}

/// <summary>
/// Actions user can choose when encountering connection errors
/// </summary>
public enum ConnectionErrorAction
{
    Retry,
    ConfigureServer,
    Cancel
}
