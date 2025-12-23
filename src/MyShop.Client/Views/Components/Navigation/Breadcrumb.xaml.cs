using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using MyShop.Client.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;

namespace MyShop.Client.Views.Components.Navigation;

/// <summary>
/// Breadcrumb navigation component for showing current navigation path.
/// </summary>
public sealed partial class Breadcrumb : UserControl
{
    private readonly NavigationService? _navigationService;

    public Breadcrumb()
    {
        this.InitializeComponent();
        _navigationService = App.Current.Services?.GetService<NavigationService>();
    }

    /// <summary>
    /// Collection of breadcrumb items to display.
    /// </summary>
    public static readonly DependencyProperty ItemsProperty =
        DependencyProperty.Register(
            nameof(Items),
            typeof(ObservableCollection<BreadcrumbItem>),
            typeof(Breadcrumb),
            new PropertyMetadata(new ObservableCollection<BreadcrumbItem>()));

    public ObservableCollection<BreadcrumbItem> Items
    {
        get => (ObservableCollection<BreadcrumbItem>)GetValue(ItemsProperty);
        set => SetValue(ItemsProperty, value);
    }

    private void BreadcrumbItem_Click(object sender, RoutedEventArgs e)
    {
        if (sender is HyperlinkButton button && button.Tag is BreadcrumbItem item)
        {
            if (item.IsClickable && !string.IsNullOrEmpty(item.PageName))
            {
                _navigationService?.NavigateInShell(item.PageName, item.Parameter);
            }
        }
    }
}

/// <summary>
/// Represents a single breadcrumb navigation item.
/// </summary>
public class BreadcrumbItem
{
    /// <summary>
    /// Display title of the breadcrumb.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Page name for navigation.
    /// </summary>
    public string? PageName { get; set; }

    /// <summary>
    /// Optional navigation parameter.
    /// </summary>
    public object? Parameter { get; set; }

    /// <summary>
    /// Whether this item is clickable (usually false for current page).
    /// </summary>
    public bool IsClickable { get; set; } = true;

    /// <summary>
    /// Whether this is the first item (hides separator).
    /// </summary>
    public bool IsFirst { get; set; }

    /// <summary>
    /// Icon glyph (optional).
    /// </summary>
    public string? IconGlyph { get; set; }
}
