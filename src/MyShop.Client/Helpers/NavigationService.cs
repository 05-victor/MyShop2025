using Microsoft.UI.Xaml.Controls;
using System;

namespace MyShop.Client.Helpers {
    public class NavigationService : INavigationService {
        private Frame? _rootFrame;

        public void Initialize(Frame frame) {
            _rootFrame = frame ?? throw new ArgumentNullException(nameof(frame));
        }

        public void NavigateTo(Type pageType, object? parameter = null) {
            if (_rootFrame is null) {
                throw new InvalidOperationException("NavigationService must be initialized before use.");
            }

            if (_rootFrame.CurrentSourcePageType == pageType && parameter == null) {
                AppLogger.Debug($"Skipping navigation - already on {pageType.Name}");
                return; // Don't navigate to the same page without parameters
            }

            try
            {
                AppLogger.Info($"Navigating to {pageType.Name}...");
                _rootFrame.Navigate(pageType, parameter);
                AppLogger.Success($"Navigation to {pageType.Name} completed");
            }
            catch (Exception ex)
            {
                AppLogger.Error($"Navigation to {pageType.Name} failed", ex);
                throw;
            }
        }

        public void GoBack() {
            if (_rootFrame?.CanGoBack == true) {
                _rootFrame.GoBack();
            }
        }

        public bool CanGoBack => _rootFrame?.CanGoBack ?? false;
    }
}