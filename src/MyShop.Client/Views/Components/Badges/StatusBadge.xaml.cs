using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace MyShop.Client.Views.Components.Badges;

/// <summary>
/// Status badge component with color variants.
/// Usage:
/// <badges:StatusBadge Text="Success" Variant="Success"/>
/// <badges:StatusBadge Text="Pending" Variant="Warning" ShowIcon="True"/>
/// </summary>
public sealed partial class StatusBadge : UserControl
{
    public StatusBadge()
    {
        InitializeComponent();
        UpdateBadgeStyle();
    }

    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(
            nameof(Text),
            typeof(string),
            typeof(StatusBadge),
            new PropertyMetadata(string.Empty));

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public static readonly DependencyProperty VariantProperty =
        DependencyProperty.Register(
            nameof(Variant),
            typeof(BadgeVariant),
            typeof(StatusBadge),
            new PropertyMetadata(BadgeVariant.Default, OnVariantChanged));

    public BadgeVariant Variant
    {
        get => (BadgeVariant)GetValue(VariantProperty);
        set => SetValue(VariantProperty, value);
    }

    public static readonly DependencyProperty ShowIconProperty =
        DependencyProperty.Register(
            nameof(ShowIcon),
            typeof(bool),
            typeof(StatusBadge),
            new PropertyMetadata(false, OnShowIconChanged));

    public bool ShowIcon
    {
        get => (bool)GetValue(ShowIconProperty);
        set => SetValue(ShowIconProperty, value);
    }

    private static void OnVariantChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is StatusBadge badge)
        {
            badge.UpdateBadgeStyle();
        }
    }

    private static void OnShowIconChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is StatusBadge badge)
        {
            badge.BadgeIcon.Visibility = badge.ShowIcon ? Visibility.Visible : Visibility.Collapsed;
            badge.UpdateBadgeStyle();
        }
    }

    private void UpdateBadgeStyle()
    {
        var (background, foreground, icon) = Variant switch
        {
            BadgeVariant.Success => ("#DCFCE7", "#15803D", "\uE73E"), // Green + Checkmark
            BadgeVariant.Warning => ("#FEF3C7", "#B45309", "\uE7BA"), // Yellow + Warning
            BadgeVariant.Error => ("#FEE2E2", "#B91C1C", "\uE783"),   // Red + Error
            BadgeVariant.Info => ("#DBEAFE", "#1E40AF", "\uE946"),    // Blue + Info
            BadgeVariant.Primary => ("#E0E7FF", "#4F46E5", "\uE735"), // Indigo + Star
            _ => ("#F3F4F6", "#6B7280", "\uE734")                      // Gray + Circle
        };

        BadgeContainer.Background = new SolidColorBrush(ColorHelper(background));
        BadgeText.Foreground = new SolidColorBrush(ColorHelper(foreground));
        BadgeIcon.Foreground = new SolidColorBrush(ColorHelper(foreground));
        BadgeIcon.Glyph = icon;
    }

    private Windows.UI.Color ColorHelper(string hex)
    {
        hex = hex.Replace("#", "");
        byte r = Convert.ToByte(hex.Substring(0, 2), 16);
        byte g = Convert.ToByte(hex.Substring(2, 2), 16);
        byte b = Convert.ToByte(hex.Substring(4, 2), 16);
        return Windows.UI.Color.FromArgb(255, r, g, b);
    }
}

public enum BadgeVariant
{
    Default,
    Success,
    Warning,
    Error,
    Info,
    Primary
}
