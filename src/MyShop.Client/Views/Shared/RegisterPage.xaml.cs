using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using MyShop.Client.ViewModels.Shared;

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


    }
}
