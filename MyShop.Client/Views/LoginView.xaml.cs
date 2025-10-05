using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MyShop.Client.ViewModels;

namespace MyShop.Client.Views
{
    public sealed partial class LoginView : Page
    {
        public LoginViewModel ViewModel { get; }

        public LoginView()
        {
            this.InitializeComponent();
            ViewModel = App.GetService<LoginViewModel>();
            this.DataContext = ViewModel;
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            // Set password from PasswordBox to ViewModel
            ViewModel.Password = PasswordInput.Password;
            
            // Execute login command
            if (ViewModel.AttemptLoginCommand.CanExecute(null))
            {
                ViewModel.AttemptLoginCommand.Execute(null);
            }
        }
    }
}