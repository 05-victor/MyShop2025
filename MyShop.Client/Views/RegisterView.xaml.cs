using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MyShop.Client.ViewModels;

namespace MyShop.Client.Views
{
    public sealed partial class RegisterView : Page
    {
        public RegisterViewModel ViewModel { get; }

        public RegisterView()
        {
            this.InitializeComponent();
            ViewModel = App.GetService<RegisterViewModel>();
            this.DataContext = ViewModel;
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            // Set passwords from PasswordBoxes to ViewModel
            ViewModel.Password = PasswordInput.Password;
            ViewModel.ConfirmPassword = ConfirmPasswordInput.Password;
            
            // Execute register command
            if (ViewModel.AttemptRegisterCommand.CanExecute(null))
            {
                ViewModel.AttemptRegisterCommand.Execute(null);
            }
        }
    }
}