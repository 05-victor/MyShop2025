// Views/RegisterView.xaml.cs
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
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