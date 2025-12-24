using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace MyShop.Client.Views.Components.Layout;

/// <summary>
/// A reusable page header component with title, breadcrumb, and action buttons.
/// v2.0 - RELEASE-GRADE: 64px fixed height, 24px SemiBold title, optional breadcrumb.
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
    /// Gets or sets the page title (24px SemiBold).
    /// </summary>
    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    #endregion

    #region Breadcrumb Property

    public static readonly DependencyProperty BreadcrumbProperty =
        DependencyProperty.Register(
            nameof(Breadcrumb),
            typeof(string),
            typeof(PageHeader),
            new PropertyMetadata(string.Empty));

    /// <summary>
    /// Gets or sets the breadcrumb navigation text (optional, e.g., "Home > Admin").
    /// </summary>
    public string Breadcrumb
    {
        get => (string)GetValue(BreadcrumbProperty);
        set => SetValue(BreadcrumbProperty, value);
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
