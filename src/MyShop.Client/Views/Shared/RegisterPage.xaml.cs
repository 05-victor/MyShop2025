using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using MyShop.Client.ViewModels.Shared;
using Windows.System;

namespace MyShop.Client.Views.Shared
{
    /// <summary>
    /// Registration page for creating new user accounts with real-time validation,
    /// accessibility support, and responsive design.
    /// </summary>
    public sealed partial class RegisterPage : Page
    {
        public RegisterViewModel ViewModel { get; }

        public RegisterPage()
        {
            this.InitializeComponent();
            ViewModel = App.Current.Services.GetRequiredService<RegisterViewModel>();
            this.DataContext = ViewModel;

            Loaded += OnPageLoaded;
            SetupKeyboardShortcuts();
        }

        private void SetupKeyboardShortcuts()
        {
            // Ctrl+Enter: Submit registration
            var registerShortcut = new KeyboardAccelerator { Key = VirtualKey.Enter, Modifiers = VirtualKeyModifiers.Control };
            registerShortcut.Invoked += async (s, e) => { if (ViewModel.CanRegister) await ViewModel.AttemptRegisterCommand.ExecuteAsync(null); e.Handled = true; };
            KeyboardAccelerators.Add(registerShortcut);

            // Ctrl+G: Google sign up
            var googleShortcut = new KeyboardAccelerator { Key = VirtualKey.G, Modifiers = VirtualKeyModifiers.Control };
            googleShortcut.Invoked += async (s, e) => { await ViewModel.GoogleLoginCommand.ExecuteAsync(null); e.Handled = true; };
            KeyboardAccelerators.Add(googleShortcut);
        }

        /// <summary>
        /// Handles page load initialization, including auto-focus for better UX.
        /// </summary>
        private void OnPageLoaded(object sender, RoutedEventArgs e)
        {
            // Auto-focus the first input field for better user experience
            UsernameTextBox.Focus(FocusState.Programmatic);
        }

        /// <summary>
        /// Clears sensitive data when navigating away from the page for security.
        /// </summary>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // Clear passwords for security when navigating to this page
            ViewModel.Password = string.Empty;
            ViewModel.ConfirmPassword = string.Empty;
        }

        /// <summary>
        /// Handles LostFocus event for Username field to show validation errors.
        /// </summary>
        private void UsernameTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            ViewModel.UsernameTouched = true;
        }

        /// <summary>
        /// Handles LostFocus event for Email field to show validation errors.
        /// </summary>
        private void EmailTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            ViewModel.EmailTouched = true;
        }

        /// <summary>
        /// Handles LostFocus event for Phone Number field to show validation errors.
        /// </summary>
        private void PhoneTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            ViewModel.PhoneTouched = true;
        }

        /// <summary>
        /// Handles LostFocus event for Password field to show validation errors.
        /// </summary>
        private void PasswordInput_LostFocus(object sender, RoutedEventArgs e)
        {
            ViewModel.PasswordTouched = true;
        }

        /// <summary>
        /// Handles LostFocus event for Confirm Password field to show validation errors.
        /// </summary>
        private void ConfirmPasswordInput_LostFocus(object sender, RoutedEventArgs e)
        {
            ViewModel.ConfirmPasswordTouched = true;
        }

    }
}
