using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;
using MyShop.Client;

namespace MyShop.Client.Helpers {
    public class ToastHelper : IToastHelper {
        private XamlRoot? _xamlRoot;
        private static readonly object _dialogLock = new object();
        private static bool _isDialogShowing = false;

        public void Initialize(XamlRoot xamlRoot) {
            _xamlRoot = xamlRoot;
        }

        public async Task ShowSuccessAsync(string message) {
            await ShowDialogAsync("Success", message);
        }

        public async Task ShowErrorAsync(string message) {
            await ShowDialogAsync("Error", message);
        }

        public async Task ShowInfoAsync(string message) {
            await ShowDialogAsync("Information", message);
        }

        public void ShowSuccess(string message) {
            _ = ShowSuccessAsync(message);
        }

        public void ShowError(string message) {
            _ = ShowErrorAsync(message);
        }

        public void ShowInfo(string message) {
            _ = ShowInfoAsync(message);
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

        private async Task ShowDialogAsync(string title, string content) {
            var xamlRoot = ResolveXamlRoot();
            if (xamlRoot is null) {
                System.Diagnostics.Debug.WriteLine("ToastHelper could not resolve XamlRoot. Skipping dialog.");
                return;
            }

            lock (_dialogLock) {
                if (_isDialogShowing) {
                    return; // Prevent multiple dialogs
                }
                _isDialogShowing = true;
            }

            try {
                var dialog = new ContentDialog {
                    Title = title,
                    Content = content,
                    CloseButtonText = "OK",
                    XamlRoot = xamlRoot
                };
                await dialog.ShowAsync();
            }
            finally {
                lock (_dialogLock) {
                    _isDialogShowing = false;
                }
            }
        }

        public async Task<ConnectionErrorAction> ShowConnectionErrorAsync(string message) {
            var xamlRoot = ResolveXamlRoot();
            if (xamlRoot is null) {
                System.Diagnostics.Debug.WriteLine("ToastHelper could not resolve XamlRoot. Returning Cancel.");
                return ConnectionErrorAction.Cancel;
            }

            lock (_dialogLock) {
                if (_isDialogShowing) {
                    return ConnectionErrorAction.Cancel; // Prevent multiple dialogs
                }
                _isDialogShowing = true;
            }

            try {
                var dialog = new ContentDialog {
                    Title = "⚠️ Cannot Connect to Server",
                    Content = message + "\n\nWhat would you like to do?",
                    PrimaryButtonText = "🔄 Retry",
                    SecondaryButtonText = "⚙️ Configure Server",
                    CloseButtonText = "Cancel",
                    DefaultButton = ContentDialogButton.Primary,
                    XamlRoot = xamlRoot
                };

                var result = await dialog.ShowAsync();

                return result switch {
                    ContentDialogResult.Primary => ConnectionErrorAction.Retry,
                    ContentDialogResult.Secondary => ConnectionErrorAction.ConfigureServer,
                    _ => ConnectionErrorAction.Cancel
                };
            }
            finally {
                lock (_dialogLock) {
                    _isDialogShowing = false;
                }
            }
        }
    }
}