using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using MyShop.Client.Services;
using System.Collections.ObjectModel;

namespace MyShop.Client.Views.Components.Pagination;

/// <summary>
/// Pagination control with page numbers, jump to page, and page size selector.
/// v2.0 - RELEASE-GRADE ENTERPRISE PAGINATION
/// 
/// FEATURES:
/// • Page navigation: First, Prev, Page Numbers, Next, Last (disabled at edges)
/// • Jump to page: TextBox input (numeric validation, clamp 1 to TotalPages) + Go button, Enter to submit
/// • PageSize selector: ComboBox (10/15/20/50), default 10, reset to page 1 on change
/// • UI-only: Emits PageChangedEventArgs(int Page, int PageSize), no API calls
/// 
/// USAGE:
/// <pagination:PaginationControl CurrentPage="{x:Bind ViewModel.CurrentPage, Mode=TwoWay}"
///                               TotalItems="{x:Bind ViewModel.TotalItems, Mode=OneWay}"
///                               PageSize="{x:Bind ViewModel.PageSize, Mode=TwoWay}"
///                               PageChanged="OnPageChanged"/>
/// </summary>
public sealed partial class PaginationControl : UserControl
{
    private readonly SettingsService _settingsService;
    private bool _isInitialized;

    public PaginationControl()
    {
        InitializeComponent();
        
        // Get SettingsService from DI
        _settingsService = App.Current.Services.GetService(typeof(SettingsService)) as SettingsService 
            ?? new SettingsService();
        
        // Load saved page size
        var savedPageSize = _settingsService.GetDefaultPageSize();
        PageSize = savedPageSize;
        
        _isInitialized = true;
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
            FirstButton == null || LastButton == null ||
            InfoText == null || PageSizeComboBox == null || JumpToPageInput == null)
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
        FirstButton.IsEnabled = CurrentPage > 1;
        PrevButton.IsEnabled = CurrentPage > 1;
        NextButton.IsEnabled = CurrentPage < totalPages;
        LastButton.IsEnabled = CurrentPage < totalPages;

        // Update info text
        int startItem = (CurrentPage - 1) * PageSize + 1;
        int endItem = Math.Min(CurrentPage * PageSize, TotalItems);
        InfoText.Text = $"Showing {startItem}-{endItem} of {TotalItems}";

        // Update Jump to Page placeholder
        JumpToPageInput.PlaceholderText = CurrentPage.ToString();
    }

    private void OnFirstClick(object sender, RoutedEventArgs e)
    {
        if (CurrentPage > 1)
        {
            CurrentPage = 1;
            RaisePageChanged();
        }
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

    private void OnLastClick(object sender, RoutedEventArgs e)
    {
        int totalPages = (int)Math.Ceiling((double)TotalItems / PageSize);
        if (CurrentPage < totalPages)
        {
            CurrentPage = totalPages;
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
        if (!_isInitialized) return;
        
        if (PageSizeComboBox.SelectedItem is ComboBoxItem item &&
            int.TryParse(item.Content?.ToString(), out int newPageSize))
        {
            // Save to settings
            _settingsService.SetDefaultPageSize(newPageSize);
            
            PageSize = newPageSize;
            CurrentPage = 1; // Reset to first page
            RaisePageChanged();
        }
    }

    /// <summary>
    /// Jump to page when Enter key is pressed in TextBox
    /// </summary>
    private void OnJumpToPageKeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == Windows.System.VirtualKey.Enter)
        {
            JumpToPage();
            e.Handled = true;
        }
    }

    /// <summary>
    /// Jump to page when Go button is clicked
    /// </summary>
    private void OnJumpToPageClick(object sender, RoutedEventArgs e)
    {
        JumpToPage();
    }

    /// <summary>
    /// Validates and jumps to the specified page number.
    /// Numeric validation: Must be integer.
    /// Clamps to range [1, TotalPages].
    /// </summary>
    private void JumpToPage()
    {
        int totalPages = (int)Math.Ceiling((double)TotalItems / PageSize);
        
        if (int.TryParse(JumpToPageInput.Text, out int targetPage))
        {
            // Clamp to valid range [1, totalPages]
            int clampedPage = Math.Max(1, Math.Min(targetPage, totalPages));
            
            if (clampedPage != CurrentPage)
            {
                CurrentPage = clampedPage;
                RaisePageChanged();
            }
            
            // Clear input after jump
            JumpToPageInput.Text = string.Empty;
        }
        else if (!string.IsNullOrWhiteSpace(JumpToPageInput.Text))
        {
            // Invalid input: Flash red border (optional UX enhancement)
            // For now, just clear invalid input
            JumpToPageInput.Text = string.Empty;
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