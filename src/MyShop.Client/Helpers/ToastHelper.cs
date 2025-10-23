using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;

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

        private async Task ShowDialogAsync(string title, string content) {
            if (_xamlRoot is null) {
                System.Diagnostics.Debug.WriteLine("ToastHelper not initialized with XamlRoot.");
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
                    XamlRoot = _xamlRoot
                };
                await dialog.ShowAsync();
            }
            finally {
                lock (_dialogLock) {
                    _isDialogShowing = false;
                }
            }
        }
    }
}