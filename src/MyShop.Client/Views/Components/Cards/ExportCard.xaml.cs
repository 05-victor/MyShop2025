using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace MyShop.Client.Views.Components.Cards;

/// <summary>
/// A reusable card with title, description, and export button.
/// </summary>
public sealed partial class ExportCard : UserControl
{
    public ExportCard()
    {
        this.InitializeComponent();
    }

    #region Title Property

    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(
            nameof(Title),
            typeof(string),
            typeof(ExportCard),
            new PropertyMetadata("Export"));

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    #endregion

    #region Description Property

    public static readonly DependencyProperty DescriptionProperty =
        DependencyProperty.Register(
            nameof(Description),
            typeof(string),
            typeof(ExportCard),
            new PropertyMetadata(string.Empty));

    public string Description
    {
        get => (string)GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    #endregion

    #region ButtonText Property

    public static readonly DependencyProperty ButtonTextProperty =
        DependencyProperty.Register(
            nameof(ButtonText),
            typeof(string),
            typeof(ExportCard),
            new PropertyMetadata("Export"));

    public string ButtonText
    {
        get => (string)GetValue(ButtonTextProperty);
        set => SetValue(ButtonTextProperty, value);
    }

    #endregion

    #region IconGlyph Property

    public static readonly DependencyProperty IconGlyphProperty =
        DependencyProperty.Register(
            nameof(IconGlyph),
            typeof(string),
            typeof(ExportCard),
            new PropertyMetadata("\uE896")); // Default: Save/Export icon

    public string IconGlyph
    {
        get => (string)GetValue(IconGlyphProperty);
        set => SetValue(IconGlyphProperty, value);
    }

    #endregion

    #region Tooltip Property

    public static readonly DependencyProperty TooltipProperty =
        DependencyProperty.Register(
            nameof(Tooltip),
            typeof(string),
            typeof(ExportCard),
            new PropertyMetadata("Export data"));

    public string Tooltip
    {
        get => (string)GetValue(TooltipProperty);
        set => SetValue(TooltipProperty, value);
    }

    #endregion

    #region Events

    public event RoutedEventHandler ExportRequested;

    private void ExportButton_Click(object sender, RoutedEventArgs e)
    {
        ExportRequested?.Invoke(this, e);
    }

    #endregion
}
