// Views/RegisterView.xaml.cs
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using MyShop.Client.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace MyShop.Client.Views {
    public sealed partial class RegisterView : Page {
        public RegisterViewModel ViewModel { get; }

        public RegisterView() {
            this.InitializeComponent();
            ViewModel = App.Current.Services.GetRequiredService<RegisterViewModel>();
            this.DataContext = ViewModel;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e) {
            base.OnNavigatedTo(e);
            
            // Reset form whenever navigating to register page
            ViewModel.ClearForm();
            
            // Clear password boxes
            if (PasswordInput != null) {
                PasswordInput.Password = string.Empty;
            }
            if (ConfirmPasswordInput != null) {
                ConfirmPasswordInput.Password = string.Empty;
            }
            
            System.Diagnostics.Debug.WriteLine("[RegisterView] Navigated to - Form cleared");
        }

        private void PasswordInput_PasswordChanged(object sender, RoutedEventArgs e) {
            if (sender is PasswordBox pb) {
                ViewModel.Password = pb.Password;
            }
        }

        private void ConfirmPasswordInput_PasswordChanged(object sender, RoutedEventArgs e) {
            if (sender is PasswordBox pb) {
                ViewModel.ConfirmPassword = pb.Password;
            }
        }

        /// <summary>
        /// Xử lý phím Enter để submit form
        /// </summary>
        private void Input_KeyDown(object sender, KeyRoutedEventArgs e) {
            if (e.Key == Windows.System.VirtualKey.Enter) {
                // Trigger register command khi nhấn Enter
                if (ViewModel.AttemptRegisterCommand.CanExecute(null)) {
                    ViewModel.AttemptRegisterCommand.Execute(null);
                }
                e.Handled = true;
            }
        }
    }
}
