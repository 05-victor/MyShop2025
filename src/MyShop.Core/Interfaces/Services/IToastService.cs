namespace MyShop.Core.Interfaces.Services;
using MyShop.Core.Common;

/// <summary>
/// Service for displaying toast notifications
/// </summary>
public interface IToastService
{
    /// <summary>
    /// Show a success toast notification
    /// </summary>
    Task<Result<Unit>> ShowSuccess(string message);

    /// <summary>
    /// Show an error toast notification
    /// </summary>
    Task<Result<Unit>> ShowError(string message);

    /// <summary>
    /// Show an information toast notification
    /// </summary>
    Task<Result<Unit>> ShowInfo(string message);

    /// <summary>
    /// Show a warning toast notification
    /// </summary>
    Task<Result<Unit>> ShowWarning(string message);

    /// <summary>
    /// Show connection error dialog with action options
    /// </summary>
    Task<Result<ConnectionErrorAction>> ShowConnectionErrorAsync(string message);
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
