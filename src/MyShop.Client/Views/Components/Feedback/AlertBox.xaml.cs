using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace MyShop.Client.Views.Components.Feedback;

/// <summary>
/// A reusable alert/message box with variants (Info, Warning, Error, Success).
/// </summary>
public sealed partial class AlertBox : UserControl
{
    public AlertBox()
    {
        this.InitializeComponent();
        this.Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        ApplyVariantStyle();
    }

    #region Title Property

    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(
            nameof(Title),
            typeof(string),
            typeof(AlertBox),
            new PropertyMetadata(string.Empty));

    /// <summary>
    /// Gets or sets the alert title text.
    /// </summary>
    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    #endregion

    #region Message Property

    public static readonly DependencyProperty MessageProperty =
        DependencyProperty.Register(
            nameof(Message),
            typeof(string),
            typeof(AlertBox),
            new PropertyMetadata(string.Empty));

    /// <summary>
    /// Gets or sets the alert message text.
    /// </summary>
    public string Message
    {
        get => (string)GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    #endregion

    #region Variant Property

    public static readonly DependencyProperty VariantProperty =
        DependencyProperty.Register(
            nameof(Variant),
            typeof(AlertVariant),
            typeof(AlertBox),
            new PropertyMetadata(AlertVariant.Info, OnVariantChanged));

    /// <summary>
    /// Gets or sets the alert variant (Info, Warning, Error, Success).
    /// </summary>
    public AlertVariant Variant
    {
        get => (AlertVariant)GetValue(VariantProperty);
        set => SetValue(VariantProperty, value);
    }

    private static void OnVariantChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is AlertBox alert)
        {
            alert.ApplyVariantStyle();
        }
    }

    #endregion

    #region Icon Property

    public static readonly DependencyProperty IconProperty =
        DependencyProperty.Register(
            nameof(Icon),
            typeof(string),
            typeof(AlertBox),
            new PropertyMetadata(null)); // null means use default for variant

    /// <summary>
    /// Gets or sets a custom icon glyph. If null, uses the default icon for the variant.
    /// </summary>
    public string Icon
    {
        get => (string)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    #endregion

    private void ApplyVariantStyle()
    {
        // Define colors for each variant
        Color backgroundColor;
        Color borderColor;
        Color iconColor;
        Color textColor;
        string iconGlyph;

        switch (Variant)
        {
            case AlertVariant.Info:
                backgroundColor = Color.FromArgb(255, 238, 242, 255); // #EEF2FF
                borderColor = Color.FromArgb(255, 199, 210, 254);     // #C7D2FE
                iconColor = Color.FromArgb(255, 99, 102, 241);        // #6366F1
                textColor = Color.FromArgb(255, 79, 70, 229);         // #4F46E5
                iconGlyph = "\uE946"; // Info icon
                break;

            case AlertVariant.Warning:
                backgroundColor = Color.FromArgb(255, 254, 243, 199); // #FEF3C7
                borderColor = Color.FromArgb(255, 253, 230, 138);     // #FDE68A
                iconColor = Color.FromArgb(255, 245, 158, 11);        // #F59E0B
                textColor = Color.FromArgb(255, 146, 64, 14);         // #92400E
                iconGlyph = "\uE7BA"; // Warning icon
                break;

            case AlertVariant.Error:
                backgroundColor = Color.FromArgb(255, 254, 226, 226); // #FEE2E2
                borderColor = Color.FromArgb(255, 252, 165, 165);     // #FCA5A5
                iconColor = Color.FromArgb(255, 220, 38, 38);         // #DC2626
                textColor = Color.FromArgb(255, 185, 28, 28);         // #B91C1C
                iconGlyph = "\uE783"; // Error icon
                break;

            case AlertVariant.Success:
            default:
                backgroundColor = Color.FromArgb(255, 209, 250, 229); // #D1FAE5
                borderColor = Color.FromArgb(255, 167, 243, 208);     // #A7F3D0
                iconColor = Color.FromArgb(255, 16, 185, 129);        // #10B981
                textColor = Color.FromArgb(255, 5, 150, 105);         // #059669
                iconGlyph = "\uE73E"; // Checkmark icon
                break;
        }

        // Apply styles
        AlertContainer.Background = new SolidColorBrush(backgroundColor);
        AlertContainer.BorderBrush = new SolidColorBrush(borderColor);
        AlertIcon.Foreground = new SolidColorBrush(iconColor);
        AlertTitle.Foreground = new SolidColorBrush(textColor);
        AlertMessage.Foreground = new SolidColorBrush(textColor);

        // Use custom icon if provided, otherwise use default
        AlertIcon.Glyph = string.IsNullOrEmpty(Icon) ? iconGlyph : Icon;
    }
}

/// <summary>
/// Defines the visual variants for AlertBox.
/// </summary>
public enum AlertVariant
{
    /// <summary>
    /// Blue informational alert.
    /// </summary>
    Info,

    /// <summary>
    /// Yellow/amber warning alert.
    /// </summary>
    Warning,

    /// <summary>
    /// Red error alert.
    /// </summary>
    Error,

    /// <summary>
    /// Green success alert.
    /// </summary>
    Success
}
