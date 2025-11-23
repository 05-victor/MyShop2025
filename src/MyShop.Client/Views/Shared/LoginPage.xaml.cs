using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using MyShop.Client.ViewModels.Shared;
using MyShop.Client.Config;

namespace MyShop.Client.Views.Shared
{
    /// <summary>
    /// Login page với MVVM pattern và accessibility support
    /// </summary>
    public sealed partial class LoginPage : Page
    {
        public LoginViewModel ViewModel { get; }

        public LoginPage()
        {
            this.InitializeComponent();
            
            // Resolve ViewModel via DI
            ViewModel = App.Current.Services.GetRequiredService<LoginViewModel>();
            this.DataContext = ViewModel;

            // Subscribe to Loaded event for initialization
            Loaded += OnPageLoaded;
        }

        /// <summary>
        /// Khởi tạo UI sau khi page được load
        /// </summary>
        private void OnPageLoaded(object sender, RoutedEventArgs e)
        {
            // Configure UI based on app config
            MockModeBanner.IsOpen = AppConfig.Instance.UseMockData;
            RegisterButton.IsEnabled = !AppConfig.Instance.UseMockData;
            
            if (AppConfig.Instance.UseMockData)
            {
                RegisterDisabledText.Visibility = Visibility.Visible;
            }

            // Focus username field for better UX
            UsernameTextBox.Focus(FocusState.Programmatic);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            
            // Clear password for security when navigating to page
            ViewModel.Password = string.Empty;
        }

        /// <summary>
        /// Handle Enter key to submit form (better UX)
        /// </summary>
        private void Input_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter && !ViewModel.IsLoading)
            {
                // Check if command can execute (form validation)
                if (ViewModel.AttemptLoginCommand.CanExecute(null))
                {
                    ViewModel.AttemptLoginCommand.Execute(null);
                }
                
                e.Handled = true;
            }
        }
    }
}
