using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace MyShop.Client.Views.Components.Layout;

/// <summary>
/// A simple horizontal divider line with optional label.
/// </summary>
public sealed partial class Divider : UserControl
{
    public Divider()
    {
        this.InitializeComponent();
    }

    #region Label Property

    public static readonly DependencyProperty LabelProperty =
        DependencyProperty.Register(
            nameof(Label),
            typeof(string),
            typeof(Divider),
            new PropertyMetadata(string.Empty));

    /// <summary>
    /// Gets or sets the optional label text displayed in the center.
    /// </summary>
    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    #endregion

    #region Color Property

    public static readonly DependencyProperty ColorProperty =
        DependencyProperty.Register(
            nameof(Color),
            typeof(Brush),
            typeof(Divider),
            new PropertyMetadata(new SolidColorBrush(Windows.UI.Color.FromArgb(255, 229, 231, 235)))); // #E5E7EB

    /// <summary>
    /// Gets or sets the divider line color.
    /// </summary>
    public Brush Color
    {
        get => (Brush)GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }

    #endregion
}
