using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections;

namespace MyShop.Client.Views.Components.Tables;

/// <summary>
/// Simplified data table component.
/// Note: For complex tables with custom templates, use ListView directly with custom ItemTemplate.
/// This component is a lightweight wrapper for basic table scenarios.
/// Usage:
/// <tables:DataTable ItemsSource="{x:Bind ViewModel.Items, Mode=OneWay}"
///                   ItemClick="OnItemClick"/>
/// </summary>
public sealed partial class DataTable : UserControl
{
    public DataTable()
    {
        InitializeComponent();
    }

    public static readonly DependencyProperty ItemsSourceProperty =
        DependencyProperty.Register(
            nameof(ItemsSource),
            typeof(IEnumerable),
            typeof(DataTable),
            new PropertyMetadata(null, OnItemsSourceChanged));

    public IEnumerable? ItemsSource
    {
        get => (IEnumerable?)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public event ItemClickEventHandler? ItemClick;

    private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DataTable table)
        {
            table.TableListView.ItemsSource = table.ItemsSource;
        }
    }

    private void OnItemClick(object sender, ItemClickEventArgs e)
    {
        ItemClick?.Invoke(this, e);
    }
}
