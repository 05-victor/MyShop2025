using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MyShop.Client.ViewModels.Auth;

namespace MyShop.Client.Views.Auth
{
    public sealed partial class RegisterPage : Page
    {
        public RegisterViewModel ViewModel { get; }

        public RegisterPage()
        {
            this.InitializeComponent();
            ViewModel = App.Current.Services.GetRequiredService<RegisterViewModel>();
            this.DataContext = ViewModel;
        }

        private void PasswordInput_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (sender is PasswordBox passwordBox)
            {
                ViewModel.Password = passwordBox.Password;
            }
        }

        private void ConfirmPasswordInput_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (sender is PasswordBox passwordBox)
            {
                ViewModel.ConfirmPassword = passwordBox.Password;
            }
        }
    }
}
