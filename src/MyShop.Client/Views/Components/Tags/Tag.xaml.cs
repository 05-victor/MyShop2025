using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace MyShop.Client.Views.Components.Tags;

/// <summary>
/// Tag component with color variants and removable option.
/// Usage:
/// <tags:Tag Text="New" Color="Primary" IsRemovable="False"/>
/// <tags:Tag Text="Category" Color="Secondary" IsRemovable="True" Removed="OnTagRemoved"/>
/// </summary>
public sealed partial class Tag : UserControl
{
    public Tag()
    {
        InitializeComponent();
        UpdateTagStyle();
    }

    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(
            nameof(Text),
            typeof(string),
            typeof(Tag),
            new PropertyMetadata(string.Empty));

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public static readonly DependencyProperty ColorProperty =
        DependencyProperty.Register(
            nameof(Color),
            typeof(TagColor),
            typeof(Tag),
            new PropertyMetadata(TagColor.Default, OnColorChanged));

    public TagColor Color
    {
        get => (TagColor)GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }

    public static readonly DependencyProperty IsRemovableProperty =
        DependencyProperty.Register(
            nameof(IsRemovable),
            typeof(bool),
            typeof(Tag),
            new PropertyMetadata(false, OnIsRemovableChanged));

    public bool IsRemovable
    {
        get => (bool)GetValue(IsRemovableProperty);
        set => SetValue(IsRemovableProperty, value);
    }

    public event EventHandler? Removed;

    private static void OnColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Tag tag)
        {
            tag.UpdateTagStyle();
        }
    }

    private static void OnIsRemovableChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is Tag tag)
        {
            tag.RemoveButton.Visibility = tag.IsRemovable ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    private void UpdateTagStyle()
    {
        var (background, foreground) = Color switch
        {
            TagColor.Primary => ("#E0E7FF", "#4F46E5"),    // Indigo
            TagColor.Secondary => ("#F3F4F6", "#6B7280"),  // Gray
            TagColor.Success => ("#DCFCE7", "#15803D"),    // Green
            TagColor.Warning => ("#FEF3C7", "#B45309"),    // Yellow
            TagColor.Error => ("#FEE2E2", "#B91C1C"),      // Red
            TagColor.Info => ("#DBEAFE", "#1E40AF"),       // Blue
            _ => ("#F3F4F6", "#6B7280")                     // Default Gray
        };

        TagContainer.Background = new SolidColorBrush(ColorHelper(background));
        TagText.Foreground = new SolidColorBrush(ColorHelper(foreground));
        
        if (RemoveButton.Content is FontIcon icon)
        {
            icon.Foreground = new SolidColorBrush(ColorHelper(foreground));
        }
    }

    private void OnRemoveClick(object sender, RoutedEventArgs e)
    {
        Removed?.Invoke(this, EventArgs.Empty);
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

public enum TagColor
{
    Default,
    Primary,
    Secondary,
    Success,
    Warning,
    Error,
    Info
}
