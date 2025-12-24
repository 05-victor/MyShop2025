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
        // Map variant to ThemeResource keys
        string bgKey, borderKey, iconKey, textKey, iconGlyph;

        switch (Variant)
        {
            case AlertVariant.Info:
                bgKey = "InfoBackgroundBrush";
                borderKey = "InfoBrush";
                iconKey = "InfoBrush";
                textKey = "InfoTextBrush";
                iconGlyph = "\uE946"; // Info icon
                break;

            case AlertVariant.Warning:
                bgKey = "WarningBackgroundBrush";
                borderKey = "WarningBrush";
                iconKey = "WarningBrush";
                textKey = "WarningTextBrush";
                iconGlyph = "\uE7BA"; // Warning icon
                break;

            case AlertVariant.Error:
                bgKey = "ErrorBackgroundBrush";
                borderKey = "ErrorBrush";
                iconKey = "ErrorBrush";
                textKey = "ErrorTextBrush";
                iconGlyph = "\uE783"; // Error icon
                break;

            case AlertVariant.Success:
            default:
                bgKey = "SuccessBackgroundBrush";
                borderKey = "SuccessBrush";
                iconKey = "SuccessBrush";
                textKey = "SuccessTextBrush";
                iconGlyph = "\uE73E"; // Checkmark icon
                break;
        }

        // Apply ThemeResource brushes
        if (Application.Current.Resources.TryGetValue(bgKey, out var bgBrush) && bgBrush is Brush bg)
            AlertContainer.Background = bg;
        
        if (Application.Current.Resources.TryGetValue(borderKey, out var borderBrush) && borderBrush is Brush border)
            AlertContainer.BorderBrush = border;
        
        if (Application.Current.Resources.TryGetValue(iconKey, out var iconBrush) && iconBrush is Brush icon)
            AlertIcon.Foreground = icon;
        
        if (Application.Current.Resources.TryGetValue(textKey, out var textBrush) && textBrush is Brush text)
        {
            AlertTitle.Foreground = text;
            AlertMessage.Foreground = text;
        }

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
