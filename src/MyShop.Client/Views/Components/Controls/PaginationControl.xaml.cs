using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace MyShop.Client.Views.Components.Controls;

/// <summary>
/// Standardized pagination control for list pages.
/// Provides First/Prev/Next/Last navigation, page numbers, page size selector, and go-to-page input.
/// </summary>
public sealed partial class PaginationControl : UserControl
{
    // Events
    public event EventHandler<int>? PageChanged;
    public event EventHandler<int>? PageSizeChanged;

    // Dependency Properties
    public static readonly DependencyProperty CurrentPageProperty =
        DependencyProperty.Register(
            nameof(CurrentPage),
            typeof(int),
            typeof(PaginationControl),
            new PropertyMetadata(1, OnCurrentPageChanged));

    public static readonly DependencyProperty TotalPagesProperty =
        DependencyProperty.Register(
            nameof(TotalPages),
            typeof(int),
            typeof(PaginationControl),
            new PropertyMetadata(1, OnTotalPagesChanged));

    public static readonly DependencyProperty TotalItemsProperty =
        DependencyProperty.Register(
            nameof(TotalItems),
            typeof(int),
            typeof(PaginationControl),
            new PropertyMetadata(0, OnTotalItemsChanged));

    public static readonly DependencyProperty PageSizeProperty =
        DependencyProperty.Register(
            nameof(PageSize),
            typeof(int),
            typeof(PaginationControl),
            new PropertyMetadata(10, OnPageSizeChanged));

    public static readonly DependencyProperty PageSizeOptionsProperty =
        DependencyProperty.Register(
            nameof(PageSizeOptions),
            typeof(IList<int>),
            typeof(PaginationControl),
            new PropertyMetadata(new[] { 10, 15, 25, 50 }, OnPageSizeOptionsChanged));

    public int CurrentPage
    {
        get => (int)GetValue(CurrentPageProperty);
        set => SetValue(CurrentPageProperty, value);
    }

    public int TotalPages
    {
        get => (int)GetValue(TotalPagesProperty);
        set => SetValue(TotalPagesProperty, value);
    }

    public int TotalItems
    {
        get => (int)GetValue(TotalItemsProperty);
        set => SetValue(TotalItemsProperty, value);
    }

    public int PageSize
    {
        get => (int)GetValue(PageSizeProperty);
        set => SetValue(PageSizeProperty, value);
    }

    /// <summary>
    /// Gets or sets the available page size options for the ComboBox.
    /// Default: [10, 15, 25, 50]
    /// Can be overridden per page (e.g., [12, 24, 36, 60] for card grid layouts)
    /// </summary>
    public IList<int> PageSizeOptions
    {
        get => (IList<int>)GetValue(PageSizeOptionsProperty);
        set => SetValue(PageSizeOptionsProperty, value);
    }

    private bool _isUpdatingInternally = false;
    private bool _isInitialized = false;
    private bool _suppressEvents = true; // Suppress events during binding initialization

    public PaginationControl()
    {
        this.InitializeComponent();
        UpdateUI();
        _isInitialized = true;
        _suppressEvents = false; // Allow events after init complete
    }

    private static void OnCurrentPageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is PaginationControl control && !control._isUpdatingInternally)
        {
            control.UpdateUI();
        }
    }

    private static void OnTotalPagesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is PaginationControl control)
        {
            control.UpdateUI();
        }
    }

    private static void OnTotalItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is PaginationControl control)
        {
            control.UpdateItemsInfoText();
        }
    }

    private static void OnPageSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is PaginationControl control && !control._isUpdatingInternally)
        {
            control.UpdatePageSizeComboBox();
            control.UpdateItemsInfoText();
        }
    }

    private static void OnPageSizeOptionsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is PaginationControl control && control._isInitialized)
        {
            control.OnPageSizeOptionsChanged();
        }
    }

    private void OnPageSizeOptionsChanged()
    {
        // Regenerate ComboBox items based on PageSizeOptions
        if (PageSizeOptions != null && PageSizeOptions.Count > 0)
        {
            PageSizeComboBox.Items.Clear();
            foreach (var option in PageSizeOptions)
            {
                var item = new ComboBoxItem
                {
                    Content = option.ToString(),
                    Tag = option
                };
                PageSizeComboBox.Items.Add(item);
            }

            // Verify that current PageSize is in the new options
            // If not, fallback to the first option
            if (!PageSizeOptions.Contains(PageSize))
            {
                _isUpdatingInternally = true;
                PageSize = PageSizeOptions[0];
                _isUpdatingInternally = false;
            }
            UpdatePageSizeComboBox();
        }
    }

    private void UpdateUI()
    {
        UpdateNavigationButtons();
        UpdatePageNumbers();
        UpdateItemsInfoText();
    }

    private void UpdateNavigationButtons()
    {
        bool canGoBack = CurrentPage > 1;
        bool canGoForward = CurrentPage < TotalPages;

        FirstButton.IsEnabled = canGoBack;
        PreviousButton.IsEnabled = canGoBack;
        NextButton.IsEnabled = canGoForward;
        LastButton.IsEnabled = canGoForward;

        // FontIcon will inherit Foreground from ContentPresenter in the template
        // No need to manually set Foreground - the Disabled VisualState handles it
    }

    private void UpdatePageNumbers()
    {
        var pageNumbers = GetVisiblePageNumbers();
        PageNumbersRepeater.ItemsSource = pageNumbers;

        // Force re-apply styles for current page highlight
        // The ElementPrepared event will handle this
    }

    private void PageNumbersRepeater_ElementPrepared(ItemsRepeater sender, ItemsRepeaterElementPreparedEventArgs args)
    {
        if (args.Element is Button button && button.Tag is int pageNumber)
        {
            ApplyPageButtonStyle(button, pageNumber);
        }
    }

    private void ApplyPageButtonStyle(Button button, int pageNumber)
    {
        bool isCurrent = pageNumber == CurrentPage;

        if (isCurrent)
        {
            // Current page: Primary/Active style
            if (Application.Current.Resources.TryGetValue("PrimaryButtonSmallStyle", out var primaryStyle))
            {
                button.Style = primaryStyle as Style;
            }
        }
        else
        {
            // Other pages: Secondary/Ghost style
            if (Application.Current.Resources.TryGetValue("SecondaryButtonSmallStyle", out var secondaryStyle))
            {
                button.Style = secondaryStyle as Style;
            }
        }
    }

    private void RefreshPageNumberStyles()
    {
        // Iterate through visible page buttons and re-apply styles
        for (int i = 0; i < PageNumbersRepeater.ItemsSourceView?.Count; i++)
        {
            if (PageNumbersRepeater.TryGetElement(i) is Button button &&
                button.Tag is int pageNumber)
            {
                ApplyPageButtonStyle(button, pageNumber);
            }
        }
    }

    private List<int> GetVisiblePageNumbers()
    {
        const int maxVisible = 7;
        var pages = new List<int>();

        if (TotalPages <= maxVisible)
        {
            // Show all pages
            for (int i = 1; i <= TotalPages; i++)
            {
                pages.Add(i);
            }
        }
        else
        {
            // Show smart range around current page
            int start, end;

            if (CurrentPage <= 4)
            {
                // Near start: 1 2 3 4 5 ... 10
                start = 1;
                end = 5;
            }
            else if (CurrentPage >= TotalPages - 3)
            {
                // Near end: 1 ... 6 7 8 9 10
                start = TotalPages - 4;
                end = TotalPages;
            }
            else
            {
                // Middle: 1 ... 4 5 6 ... 10
                start = CurrentPage - 1;
                end = CurrentPage + 1;
            }

            for (int i = start; i <= end; i++)
            {
                pages.Add(i);
            }
        }

        return pages;
    }

    private void UpdateItemsInfoText()
    {
        if (TotalItems == 0)
        {
            ItemsInfoText.Text = "No items";
            return;
        }

        int startItem = (CurrentPage - 1) * PageSize + 1;
        int endItem = Math.Min(CurrentPage * PageSize, TotalItems);
        ItemsInfoText.Text = $"Showing {startItem}-{endItem} of {TotalItems} items";
    }

    private void UpdatePageSizeComboBox()
    {
        _isUpdatingInternally = true;
        if (PageSizeOptions != null && PageSizeOptions.Count > 0)
        {
            int index = PageSizeOptions.IndexOf(PageSize);
            if (index < 0)
            {
                // PageSize not in options, fallback to first option
                index = 0;
                PageSize = PageSizeOptions[0];
            }
            PageSizeComboBox.SelectedIndex = index;
        }
        _isUpdatingInternally = false;
    }

    // Event Handlers
    private void FirstButton_Click(object sender, RoutedEventArgs e)
    {
        ChangePage(1);
    }

    private void PreviousButton_Click(object sender, RoutedEventArgs e)
    {
        ChangePage(CurrentPage - 1);
    }

    private void NextButton_Click(object sender, RoutedEventArgs e)
    {
        ChangePage(CurrentPage + 1);
    }

    private void LastButton_Click(object sender, RoutedEventArgs e)
    {
        ChangePage(TotalPages);
    }

    private void PageNumberButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is int pageNumber)
        {
            ChangePage(pageNumber);
        }
    }

    private void PageSizeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_isUpdatingInternally || _suppressEvents) return;

        if (PageSizeComboBox.SelectedItem is ComboBoxItem item &&
            int.TryParse(item.Tag?.ToString(), out int newSize))
        {
            _isUpdatingInternally = true;
            PageSize = newSize;
            _isUpdatingInternally = false;
            PageSizeChanged?.Invoke(this, newSize);
        }
    }

    private void GoToPageInput_KeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
    {
        if (e.Key == Windows.System.VirtualKey.Enter)
        {
            ProcessGoToPage();
            e.Handled = true;
        }
    }

    private void GoToPageInput_LostFocus(object sender, RoutedEventArgs e)
    {
        ProcessGoToPage();
    }

    private void ProcessGoToPage()
    {
        if (string.IsNullOrWhiteSpace(GoToPageInput.Text)) return;

        if (int.TryParse(GoToPageInput.Text, out int targetPage))
        {
            // Validate and clamp
            if (targetPage < 1)
            {
                targetPage = 1;
            }
            else if (targetPage > TotalPages)
            {
                targetPage = TotalPages;
            }

            GoToPageInput.Text = string.Empty;
            ChangePage(targetPage);
        }
        else
        {
            GoToPageInput.Text = string.Empty;
        }
    }

    private void ChangePage(int newPage)
    {
        if (newPage < 1 || newPage > TotalPages || newPage == CurrentPage)
        {
            return;
        }

        _isUpdatingInternally = true;
        CurrentPage = newPage;
        _isUpdatingInternally = false;

        UpdateUI();
        RefreshPageNumberStyles();

        // Only raise events after control initialization is complete
        if (!_suppressEvents)
        {
            PageChanged?.Invoke(this, newPage);
        }
    }
}
