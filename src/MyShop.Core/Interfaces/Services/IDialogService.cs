namespace MyShop.Core.Interfaces.Services;
using MyShop.Core.Common;

/// <summary>
/// Service for displaying dialogs
/// </summary>
public interface IDialogService
{
    /// <summary>
    /// Show a confirmation dialog
    /// </summary>
    Task<Result<bool>> ShowConfirmationAsync(string title, string message);

    /// <summary>
    /// Show an input dialog
    /// </summary>
    Task<Result<string?>> ShowInputAsync(string title, string message, string placeholder = "");

    /// <summary>
    /// Show a custom dialog
    /// </summary>
    Task<Result<Unit>> ShowDialogAsync(string dialogName, object? parameter = null);
}
