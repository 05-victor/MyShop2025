using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI;
using System.Text.RegularExpressions;

namespace MyShop.Client.Views.Components.Forms;

/// <summary>
/// Password strength meter component showing visual feedback on password complexity.
/// </summary>
public sealed partial class PasswordStrengthMeter : UserControl
{
    private static readonly SolidColorBrush WeakBrush = new(Windows.UI.Color.FromArgb(255, 220, 38, 38));      // Red
    private static readonly SolidColorBrush FairBrush = new(Windows.UI.Color.FromArgb(255, 245, 158, 11));    // Amber
    private static readonly SolidColorBrush GoodBrush = new(Windows.UI.Color.FromArgb(255, 59, 130, 246));    // Blue
    private static readonly SolidColorBrush StrongBrush = new(Windows.UI.Color.FromArgb(255, 16, 185, 129));  // Green
    private static readonly SolidColorBrush InactiveBrush = new(Windows.UI.Color.FromArgb(255, 209, 213, 219)); // Gray

    public PasswordStrengthMeter()
    {
        this.InitializeComponent();
    }

    /// <summary>
    /// The password to evaluate.
    /// </summary>
    public static readonly DependencyProperty PasswordProperty =
        DependencyProperty.Register(
            nameof(Password),
            typeof(string),
            typeof(PasswordStrengthMeter),
            new PropertyMetadata(string.Empty, OnPasswordChanged));

    public string Password
    {
        get => (string)GetValue(PasswordProperty);
        set => SetValue(PasswordProperty, value);
    }

    private static void OnPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is PasswordStrengthMeter meter)
        {
            meter.UpdateStrength(e.NewValue as string ?? string.Empty);
        }
    }

    private void UpdateStrength(string password)
    {
        var strength = CalculateStrength(password);
        
        // Reset all bars
        Bar1.Background = InactiveBrush;
        Bar2.Background = InactiveBrush;
        Bar3.Background = InactiveBrush;
        Bar4.Background = InactiveBrush;

        if (string.IsNullOrEmpty(password))
        {
            StrengthText.Text = string.Empty;
            return;
        }

        switch (strength)
        {
            case PasswordStrength.Weak:
                Bar1.Background = WeakBrush;
                StrengthText.Text = "Weak password";
                StrengthText.Foreground = WeakBrush;
                break;
            case PasswordStrength.Fair:
                Bar1.Background = FairBrush;
                Bar2.Background = FairBrush;
                StrengthText.Text = "Fair password";
                StrengthText.Foreground = FairBrush;
                break;
            case PasswordStrength.Good:
                Bar1.Background = GoodBrush;
                Bar2.Background = GoodBrush;
                Bar3.Background = GoodBrush;
                StrengthText.Text = "Good password";
                StrengthText.Foreground = GoodBrush;
                break;
            case PasswordStrength.Strong:
                Bar1.Background = StrongBrush;
                Bar2.Background = StrongBrush;
                Bar3.Background = StrongBrush;
                Bar4.Background = StrongBrush;
                StrengthText.Text = "Strong password";
                StrengthText.Foreground = StrongBrush;
                break;
        }
    }

    private static PasswordStrength CalculateStrength(string password)
    {
        if (string.IsNullOrEmpty(password))
            return PasswordStrength.Weak;

        int score = 0;

        // Length checks
        if (password.Length >= 6) score++;
        if (password.Length >= 8) score++;
        if (password.Length >= 12) score++;

        // Complexity checks
        if (Regex.IsMatch(password, @"[a-z]")) score++;  // Lowercase
        if (Regex.IsMatch(password, @"[A-Z]")) score++;  // Uppercase
        if (Regex.IsMatch(password, @"[0-9]")) score++;  // Numbers
        if (Regex.IsMatch(password, @"[^a-zA-Z0-9]")) score++;  // Special chars

        return score switch
        {
            <= 2 => PasswordStrength.Weak,
            <= 4 => PasswordStrength.Fair,
            <= 6 => PasswordStrength.Good,
            _ => PasswordStrength.Strong
        };
    }
}

internal enum PasswordStrength
{
    Weak,
    Fair,
    Good,
    Strong
}
