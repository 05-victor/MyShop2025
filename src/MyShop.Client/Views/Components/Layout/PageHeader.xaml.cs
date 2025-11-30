using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace MyShop.Client.Views.Components.Layout;

/// <summary>
/// A reusable page header component with title, subtitle, and action buttons.
/// </summary>
public sealed partial class PageHeader : UserControl
{
    public PageHeader()
    {
        this.InitializeComponent();
    }

    #region Title Property

    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(
            nameof(Title),
            typeof(string),
            typeof(PageHeader),
            new PropertyMetadata(string.Empty));

    /// <summary>
    /// Gets or sets the page title.
    /// </summary>
    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    #endregion

    #region Subtitle Property

    public static readonly DependencyProperty SubtitleProperty =
        DependencyProperty.Register(
            nameof(Subtitle),
            typeof(string),
            typeof(PageHeader),
            new PropertyMetadata(string.Empty));

    /// <summary>
    /// Gets or sets the page subtitle/description.
    /// </summary>
    public string Subtitle
    {
        get => (string)GetValue(SubtitleProperty);
        set => SetValue(SubtitleProperty, value);
    }

    #endregion

    #region ActionContent Property

    public static readonly DependencyProperty ActionContentProperty =
        DependencyProperty.Register(
            nameof(ActionContent),
            typeof(object),
            typeof(PageHeader),
            new PropertyMetadata(null));

    /// <summary>
    /// Gets or sets the action buttons content (e.g., StackPanel with buttons).
    /// </summary>
    public object ActionContent
    {
        get => GetValue(ActionContentProperty);
        set => SetValue(ActionContentProperty, value);
    }

    #endregion
}
