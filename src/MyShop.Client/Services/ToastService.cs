using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MyShop.Core.Common;
using MyShop.Core.Interfaces.Services;
using System;
using System.Threading.Tasks;

namespace MyShop.Client.Services;

/// <summary>
/// Toast notification service for WinUI 3 using ContentDialog
/// Renamed from ToastHelper to ToastService for consistency
/// Moved from Helpers to Services
/// </summary>
public class ToastService : IToastService
{
    private XamlRoot? _xamlRoot;
    private static readonly object _dialogLock = new object();
    private static bool _isDialogShowing = false;

    public void Initialize(XamlRoot xamlRoot)
    {
        _xamlRoot = xamlRoot;
    }

    // Async methods returning Result<Unit>
    public async Task<Result<Unit>> ShowSuccess(string message)
    {
        try
        {
            await ShowDialogAsync("Success", message);
            return Result<Unit>.Success(Unit.Value);
        }
        catch (Exception ex)
        {
            return Result<Unit>.Failure($"Failed to show success: {ex.Message}");
        }
    }

    public async Task<Result<Unit>> ShowError(string message)
    {
        try
        {
            await ShowDialogAsync("Error", message);
            return Result<Unit>.Success(Unit.Value);
        }
        catch (Exception ex)
        {
            return Result<Unit>.Failure($"Failed to show error: {ex.Message}");
        }
    }

    public async Task<Result<Unit>> ShowInfo(string message)
    {
        try
        {
            await ShowDialogAsync("Information", message);
            return Result<Unit>.Success(Unit.Value);
        }
        catch (Exception ex)
        {
            return Result<Unit>.Failure($"Failed to show info: {ex.Message}");
        }
    }

    public async Task<Result<Unit>> ShowWarning(string message)
    {
        try
        {
            await ShowDialogAsync("Warning", message);
            return Result<Unit>.Success(Unit.Value);
        }
        catch (Exception ex)
        {
            return Result<Unit>.Failure($"Failed to show warning: {ex.Message}");
        }
    }

    private XamlRoot? ResolveXamlRoot()
    {
        if (_xamlRoot != null)
            return _xamlRoot;

        // Try to reuse App.MainWindow if available
        var main = App.MainWindow;
        if (main?.Content?.XamlRoot != null)
            return main.Content.XamlRoot;

        return null;
    }

    private async Task ShowDialogAsync(string title, string content)
    {
        var xamlRoot = ResolveXamlRoot();
        if (xamlRoot is null)
        {
            System.Diagnostics.Debug.WriteLine("ToastService could not resolve XamlRoot. Skipping dialog.");
            return;
        }

        lock (_dialogLock)
        {
            if (_isDialogShowing)
            {
                return; // Prevent multiple dialogs
            }
            _isDialogShowing = true;
        }

        try
        {
            var dialog = new ContentDialog
            {
                Title = title,
                Content = content,
                CloseButtonText = "OK",
                XamlRoot = xamlRoot
            };
            await dialog.ShowAsync();
        }
        finally
        {
            lock (_dialogLock)
            {
                _isDialogShowing = false;
            }
        }
    }

    public async Task<Result<ConnectionErrorAction>> ShowConnectionErrorAsync(string message)
    {
        try
        {
            var xamlRoot = ResolveXamlRoot();
            if (xamlRoot is null)
            {
                System.Diagnostics.Debug.WriteLine("ToastService could not resolve XamlRoot. Returning Cancel.");
                return Result<ConnectionErrorAction>.Success(ConnectionErrorAction.Cancel);
            }

            lock (_dialogLock)
            {
                if (_isDialogShowing)
                {
                    return Result<ConnectionErrorAction>.Success(ConnectionErrorAction.Cancel); // Prevent multiple dialogs
                }
                _isDialogShowing = true;
            }

            try
            {
                var dialog = new ContentDialog
                {
                    Title = "⚠️ Cannot Connect to Server",
                    Content = message + "\n\nWhat would you like to do?",
                    PrimaryButtonText = "🔄 Retry",
                    SecondaryButtonText = "⚙️ Configure Server",
                    CloseButtonText = "Cancel",
                    DefaultButton = ContentDialogButton.Primary,
                    XamlRoot = xamlRoot
                };

                var result = await dialog.ShowAsync();

                var action = result switch
                {
                    ContentDialogResult.Primary => ConnectionErrorAction.Retry,
                    ContentDialogResult.Secondary => ConnectionErrorAction.ConfigureServer,
                    _ => ConnectionErrorAction.Cancel
                };
                
                return Result<ConnectionErrorAction>.Success(action);
            }
            finally
            {
                lock (_dialogLock)
                {
                    _isDialogShowing = false;
                }
            }
        }
        catch (Exception ex)
        {
            return Result<ConnectionErrorAction>.Failure($"Failed to show connection error: {ex.Message}");
        }
    }
}
