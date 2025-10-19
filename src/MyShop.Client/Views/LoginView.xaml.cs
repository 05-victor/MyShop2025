// Views/LoginView.xaml.cs
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using MyShop.Client.ViewModels;
using System;
using Windows.System;
using Microsoft.Extensions.DependencyInjection;

namespace MyShop.Client.Views {
    public sealed partial class LoginView : Page {
        public LoginViewModel ViewModel { get; }

        public LoginView() {
            this.InitializeComponent();
            ViewModel = App.Current.Services.GetRequiredService<LoginViewModel>();
            this.DataContext = ViewModel;
        }

        private void PasswordInput_PasswordChanged(object sender, RoutedEventArgs e) {
            if (sender is PasswordBox passwordBox) {
                ViewModel.Password = passwordBox.Password;
            }
        }

        private void Input_KeyDown(object sender, KeyRoutedEventArgs e) {
            if (e.Key == VirtualKey.Enter) {
                if (ViewModel.AttemptLoginCommand.CanExecute(null)) {
                    ViewModel.AttemptLoginCommand.Execute(null);
                }
                e.Handled = true;
            }
        }
    }

    public class BoolToVisibilityConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, string language) =>
            (bool)value ? Visibility.Visible : Visibility.Collapsed;
        public object ConvertBack(object value, Type targetType, object parameter, string language) =>
            (Visibility)value == Visibility.Visible;
    }

    public class StringToVisibilityConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, string language) =>
            string.IsNullOrEmpty(value as string) ? Visibility.Collapsed : Visibility.Visible;
        public object ConvertBack(object value, Type targetType, object parameter, string language) => throw new NotImplementedException();
    }

    public class BoolNegationConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, string language) => !(bool)value;
        public object ConvertBack(object value, Type targetType, object parameter, string language) => !(bool)value;
    }
}