using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MyShop.Core.Common;
using MyShop.Core.Interfaces.Services;

namespace MyShop.Client.Services;

/// <summary>
/// Dialog service for WinUI 3 using ContentDialog
/// </summary>
public class DialogService : IDialogService
{
    private XamlRoot? _xamlRoot;

    public void Initialize(XamlRoot xamlRoot)
    {
        _xamlRoot = xamlRoot;
    }

    public async Task<Result<bool>> ShowConfirmationAsync(string title, string message)
    {
        try
        {
            var xamlRoot = ResolveXamlRoot();
            if (xamlRoot is null)
            {
                System.Diagnostics.Debug.WriteLine("DialogService could not resolve XamlRoot.");
                return Result<bool>.Failure("XamlRoot not available");
            }

            var dialog = new ContentDialog
            {
                Title = title,
                Content = message,
                PrimaryButtonText = "Yes",
                CloseButtonText = "No",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = xamlRoot
            };

            var result = await dialog.ShowAsync();
            return Result<bool>.Success(result == ContentDialogResult.Primary);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Failed to show confirmation dialog: {ex.Message}");
        }
    }

    public async Task<Result<string?>> ShowInputAsync(string title, string message, string placeholder = "")
    {
        try
        {
            var xamlRoot = ResolveXamlRoot();
            if (xamlRoot is null)
            {
                System.Diagnostics.Debug.WriteLine("DialogService could not resolve XamlRoot.");
                return Result<string?>.Failure("XamlRoot not available");
            }

            var textBox = new TextBox
            {
                PlaceholderText = placeholder,
                Margin = new Thickness(0, 8, 0, 0)
            };

            var stackPanel = new StackPanel();
            stackPanel.Children.Add(new TextBlock { Text = message });
            stackPanel.Children.Add(textBox);

            var dialog = new ContentDialog
            {
                Title = title,
                Content = stackPanel,
                PrimaryButtonText = "OK",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = xamlRoot
            };

            var result = await dialog.ShowAsync();
            return Result<string?>.Success(result == ContentDialogResult.Primary ? textBox.Text : null);
        }
        catch (Exception ex)
        {
            return Result<string?>.Failure($"Failed to show input dialog: {ex.Message}");
        }
    }

    public async Task<Result<Unit>> ShowDialogAsync(string dialogName, object? parameter = null)
    {
        try
        {
            // Placeholder for custom dialog implementation
            // You would typically use a dialog registry or factory pattern here
            System.Diagnostics.Debug.WriteLine($"DialogService.ShowDialogAsync called with dialog: {dialogName}");
            
            await Task.CompletedTask;
            return Result<Unit>.Success(Unit.Value);
        }
        catch (Exception ex)
        {
            return Result<Unit>.Failure($"Failed to show dialog: {ex.Message}");
        }
    }

    private XamlRoot? ResolveXamlRoot()
    {
        if (_xamlRoot != null)
            return _xamlRoot;

        var main = App.MainWindow;
        if (main?.Content?.XamlRoot != null)
            return main.Content.XamlRoot;

        return null;
    }
}
