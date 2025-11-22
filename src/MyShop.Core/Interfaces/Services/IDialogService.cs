namespace MyShop.Core.Interfaces.Services;

/// <summary>
/// Service for displaying dialogs
/// </summary>
public interface IDialogService
{
    /// <summary>
    /// Show a confirmation dialog
    /// </summary>
    Task<bool> ShowConfirmationAsync(string title, string message);

    /// <summary>
    /// Show an input dialog
    /// </summary>
    Task<string?> ShowInputAsync(string title, string message, string placeholder = "");

    /// <summary>
    /// Show a custom dialog
    /// </summary>
    Task ShowDialogAsync(string dialogName, object? parameter = null);
}
