using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace MyShop.Client.Views.Components.Cards;

/// <summary>
/// A simpler version of KPICard for basic stats display (no trends, no icons).
/// </summary>
public sealed partial class SimpleStatCard : UserControl
{
    public SimpleStatCard()
    {
        this.InitializeComponent();
    }

    #region Label Property

    public static readonly DependencyProperty LabelProperty =
        DependencyProperty.Register(
            nameof(Label),
            typeof(string),
            typeof(SimpleStatCard),
            new PropertyMetadata(string.Empty));

    /// <summary>
    /// Gets or sets the stat label.
    /// </summary>
    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    #endregion

    #region Value Property

    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(
            nameof(Value),
            typeof(string),
            typeof(SimpleStatCard),
            new PropertyMetadata(string.Empty));

    /// <summary>
    /// Gets or sets the stat value.
    /// </summary>
    public string Value
    {
        get => (string)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    #endregion

    #region Subtitle Property

    public static readonly DependencyProperty SubtitleProperty =
        DependencyProperty.Register(
            nameof(Subtitle),
            typeof(string),
            typeof(SimpleStatCard),
            new PropertyMetadata(string.Empty));

    /// <summary>
    /// Gets or sets the optional subtitle text.
    /// </summary>
    public string Subtitle
    {
        get => (string)GetValue(SubtitleProperty);
        set => SetValue(SubtitleProperty, value);
    }

    #endregion

    #region LabelColor Property

    public static readonly DependencyProperty LabelColorProperty =
        DependencyProperty.Register(
            nameof(LabelColor),
            typeof(Brush),
            typeof(SimpleStatCard),
            new PropertyMetadata(new SolidColorBrush(Color.FromArgb(255, 107, 114, 128)))); // #6B7280

    /// <summary>
    /// Gets or sets the label text color.
    /// </summary>
    public Brush LabelColor
    {
        get => (Brush)GetValue(LabelColorProperty);
        set => SetValue(LabelColorProperty, value);
    }

    #endregion

    #region ValueColor Property

    public static readonly DependencyProperty ValueColorProperty =
        DependencyProperty.Register(
            nameof(ValueColor),
            typeof(Brush),
            typeof(SimpleStatCard),
            new PropertyMetadata(new SolidColorBrush(Color.FromArgb(255, 17, 24, 39)))); // #111827

    /// <summary>
    /// Gets or sets the value text color.
    /// </summary>
    public Brush ValueColor
    {
        get => (Brush)GetValue(ValueColorProperty);
        set => SetValue(ValueColorProperty, value);
    }

    #endregion
}
