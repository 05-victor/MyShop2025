using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.ObjectModel;

namespace MyShop.Client.Views.Components.Pagination;

/// <summary>
/// Pagination control with page numbers and page size selector.
/// Usage:
/// <pagination:PaginationControl CurrentPage="{x:Bind ViewModel.CurrentPage, Mode=TwoWay}"
///                               TotalItems="{x:Bind ViewModel.TotalItems, Mode=OneWay}"
///                               PageSize="{x:Bind ViewModel.PageSize, Mode=TwoWay}"
///                               PageChanged="OnPageChanged"/>
/// </summary>
public sealed partial class PaginationControl : UserControl
{
    public PaginationControl()
    {
        InitializeComponent();
        UpdatePagination();
    }

    public static readonly DependencyProperty CurrentPageProperty =
        DependencyProperty.Register(
            nameof(CurrentPage),
            typeof(int),
            typeof(PaginationControl),
            new PropertyMetadata(1, OnPaginationChanged));

    public int CurrentPage
    {
        get => (int)GetValue(CurrentPageProperty);
        set => SetValue(CurrentPageProperty, value);
    }

    public static readonly DependencyProperty TotalItemsProperty =
        DependencyProperty.Register(
            nameof(TotalItems),
            typeof(int),
            typeof(PaginationControl),
            new PropertyMetadata(0, OnPaginationChanged));

    public int TotalItems
    {
        get => (int)GetValue(TotalItemsProperty);
        set => SetValue(TotalItemsProperty, value);
    }

    public static readonly DependencyProperty PageSizeProperty =
        DependencyProperty.Register(
            nameof(PageSize),
            typeof(int),
            typeof(PaginationControl),
            new PropertyMetadata(10, OnPaginationChanged));

    public int PageSize
    {
        get => (int)GetValue(PageSizeProperty);
        set => SetValue(PageSizeProperty, value);
    }

    public event EventHandler<PageChangedEventArgs>? PageChanged;

    private static void OnPaginationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is PaginationControl control)
        {
            control.UpdatePagination();
        }
    }

    private void UpdatePagination()
    {
        // Guard clause: ensure all UI elements are loaded
        if (PageNumbersContainer == null || PrevButton == null || NextButton == null ||
            InfoText == null || PageSizeComboBox == null)
        {
            return;
        }

        int totalPages = (int)Math.Ceiling((double)TotalItems / PageSize);

        // Update page size combobox
        var pageSizeText = PageSize.ToString();
        for (int i = 0; i < PageSizeComboBox.Items.Count; i++)
        {
            if ((PageSizeComboBox.Items[i] as ComboBoxItem)?.Content?.ToString() == pageSizeText)
            {
                PageSizeComboBox.SelectedIndex = i;
                break;
            }
        }

        // Generate page numbers (show max 7 pages)
        var pageNumbers = new ObservableCollection<PageNumberItem>();
        int startPage = Math.Max(1, CurrentPage - 3);
        int endPage = Math.Min(totalPages, CurrentPage + 3);

        for (int i = startPage; i <= endPage; i++)
        {
            pageNumbers.Add(new PageNumberItem { PageNumber = i, IsActive = i == CurrentPage });
        }

        PageNumbersContainer.ItemsSource = pageNumbers;

        // Update buttons state
        PrevButton.IsEnabled = CurrentPage > 1;
        NextButton.IsEnabled = CurrentPage < totalPages;

        // Update info text
        int startItem = (CurrentPage - 1) * PageSize + 1;
        int endItem = Math.Min(CurrentPage * PageSize, TotalItems);
        InfoText.Text = $"Showing {startItem}-{endItem} of {TotalItems}";
    }

    private void OnPreviousClick(object sender, RoutedEventArgs e)
    {
        if (CurrentPage > 1)
        {
            CurrentPage--;
            RaisePageChanged();
        }
    }

    private void OnNextClick(object sender, RoutedEventArgs e)
    {
        int totalPages = (int)Math.Ceiling((double)TotalItems / PageSize);
        if (CurrentPage < totalPages)
        {
            CurrentPage++;
            RaisePageChanged();
        }
    }

    private void OnPageNumberClick(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is int pageNumber)
        {
            CurrentPage = pageNumber;
            RaisePageChanged();
        }
    }

    private void OnPageSizeChanged(object sender, SelectionChangedEventArgs e)
    {
        if (PageSizeComboBox.SelectedItem is ComboBoxItem item &&
            int.TryParse(item.Content?.ToString(), out int newPageSize))
        {
            PageSize = newPageSize;
            CurrentPage = 1; // Reset to first page
            RaisePageChanged();
        }
    }

    private void RaisePageChanged()
    {
        PageChanged?.Invoke(this, new PageChangedEventArgs(CurrentPage, PageSize));
        UpdatePagination();
    }
}

public class PageNumberItem
{
    public int PageNumber { get; set; }
    public bool IsActive { get; set; }
}

public class PageChangedEventArgs : EventArgs
{
    public int CurrentPage { get; }
    public int PageSize { get; }

    public PageChangedEventArgs(int currentPage, int pageSize)
    {
        CurrentPage = currentPage;
        PageSize = pageSize;
    }
}