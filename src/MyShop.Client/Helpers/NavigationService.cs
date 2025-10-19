using Microsoft.UI.Xaml.Controls;
using System;

namespace MyShop.Client.Helpers {
    public class NavigationService : INavigationService {
        private Frame? _rootFrame;

        public void Initialize(Frame frame) {
            _rootFrame = frame ?? throw new ArgumentNullException(nameof(frame));
        }

        public bool NavigateTo(Type pageType, object? parameter = null) {
            if (_rootFrame is null) {
                throw new InvalidOperationException("NavigationService must be initialized before use.");
            }

            if (_rootFrame.CurrentSourcePageType == pageType && parameter == null) {
                return false; // Don't navigate to the same page without parameters
            }

            return _rootFrame.Navigate(pageType, parameter);
        }
    }
}