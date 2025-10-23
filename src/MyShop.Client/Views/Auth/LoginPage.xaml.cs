using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using MyShop.Client.ViewModels.Auth;

namespace MyShop.Client.Views.Auth
{
    public sealed partial class LoginPage : Page
    {
        public LoginViewModel ViewModel { get; }

        public LoginPage()
        {
            this.InitializeComponent();
            ViewModel = App.Current.Services.GetRequiredService<LoginViewModel>();
            this.DataContext = ViewModel;
        }

        private void PasswordInput_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (sender is PasswordBox passwordBox)
            {
                ViewModel.Password = passwordBox.Password;
            }
        }

        private void Input_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter && !ViewModel.IsLoading)
            {
                ViewModel.AttemptLoginCommand.Execute(null);
            }
        }
    }
}
