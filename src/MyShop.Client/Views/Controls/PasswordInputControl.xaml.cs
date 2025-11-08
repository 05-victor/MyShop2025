using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace MyShop.Client.Views.Controls
{
    public sealed partial class PasswordInputControl : UserControl
    {
        private bool _isPasswordVisible = false;

        public PasswordInputControl()
        {
            this.InitializeComponent();
        }

        // Dependency Properties for binding support
        public string Password
        {
            get => (string)GetValue(PasswordProperty);
            set => SetValue(PasswordProperty, value);
        }

        public static readonly DependencyProperty PasswordProperty =
            DependencyProperty.Register(
                nameof(Password),
                typeof(string),
                typeof(PasswordInputControl),
                new PropertyMetadata(string.Empty, OnPasswordPropertyChanged));

        private static void OnPasswordPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (PasswordInputControl)d;
            if (control.PasswordBox.Password != e.NewValue as string)
            {
                control.PasswordBox.Password = e.NewValue as string ?? string.Empty;
            }
        }

        public string PlaceholderText
        {
            get => (string)GetValue(PlaceholderTextProperty);
            set => SetValue(PlaceholderTextProperty, value);
        }

        public static readonly DependencyProperty PlaceholderTextProperty =
            DependencyProperty.Register(
                nameof(PlaceholderText),
                typeof(string),
                typeof(PasswordInputControl),
                new PropertyMetadata(string.Empty, OnPlaceholderTextChanged));

        private static void OnPlaceholderTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (PasswordInputControl)d;
            control.PasswordBox.PlaceholderText = e.NewValue as string ?? string.Empty;
        }

        // Event for password changes
        public event RoutedEventHandler? PasswordChanged;

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            Password = PasswordBox.Password;
            PasswordChanged?.Invoke(this, e);
        }

        private void ToggleVisibilityButton_Click(object sender, RoutedEventArgs e)
        {
            _isPasswordVisible = !_isPasswordVisible;

            if (_isPasswordVisible)
            {
                PasswordBox.PasswordRevealMode = PasswordRevealMode.Visible;
                VisibilityIcon.Glyph = "\uF78D"; // EyeOff icon
            }
            else
            {
                PasswordBox.PasswordRevealMode = PasswordRevealMode.Hidden;
                VisibilityIcon.Glyph = "\uED1A"; // Eye icon
            }
        }
    }
}
