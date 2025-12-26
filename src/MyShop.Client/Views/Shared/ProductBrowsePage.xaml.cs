using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Input;
using Windows.System;
using Windows.UI.Xaml; // For DependencyObject
using MyShop.Client.ViewModels.Shared;
using MyShop.Client.Views.Components.Controls;
using System.Linq;

namespace MyShop.Client.Views.Shared
{
    public sealed partial class ProductBrowsePage : Page
    {
        public ProductBrowseViewModel ViewModel { get; }
        private ProductCardViewModel? _selectedProduct;

        public ProductBrowsePage()
        {
            this.InitializeComponent();
            ViewModel = App.Current.Services.GetRequiredService<ProductBrowseViewModel>();
            this.DataContext = ViewModel;
            
            // Wire up product details event
            ViewModel.ProductDetailsRequested += OnProductDetailsRequested;
            
            SetupKeyboardShortcuts();
        }

        private async void OnProductDetailsRequested(object? sender, ProductCardViewModel product)
        {
            await ShowViewProductDialogAsync(product);
        }

        private void SetupKeyboardShortcuts()
        {
            // Ctrl+F: Focus search box
            var searchShortcut = new KeyboardAccelerator { Key = VirtualKey.F, Modifiers = VirtualKeyModifiers.Control };
            searchShortcut.Invoked += (s, e) => { e.Handled = true; };
            KeyboardAccelerators.Add(searchShortcut);

            // F5 or Ctrl+R: Refresh
            var refreshShortcut1 = new KeyboardAccelerator { Key = VirtualKey.F5 };
            refreshShortcut1.Invoked += async (s, e) => { await ViewModel.RefreshCommand.ExecuteAsync(null); e.Handled = true; };
            KeyboardAccelerators.Add(refreshShortcut1);

            var refreshShortcut2 = new KeyboardAccelerator { Key = VirtualKey.R, Modifiers = VirtualKeyModifiers.Control };
            refreshShortcut2.Invoked += async (s, e) => { await ViewModel.RefreshCommand.ExecuteAsync(null); e.Handled = true; };
            KeyboardAccelerators.Add(refreshShortcut2);

            // Ctrl+Down: Load more products
            var loadMoreShortcut = new KeyboardAccelerator { Key = VirtualKey.Down, Modifiers = VirtualKeyModifiers.Control };
            loadMoreShortcut.Invoked += async (s, e) => { if (ViewModel.HasMoreItems) await ViewModel.LoadMoreProductsCommand.ExecuteAsync(null); e.Handled = true; };
            KeyboardAccelerators.Add(loadMoreShortcut);
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            try
            {
                // Always refresh data when navigating to this page
                // This ensures products are up-to-date even if ViewModel is cached
                if (e.NavigationMode == NavigationMode.Back || e.NavigationMode == NavigationMode.Forward)
                {
                    // User navigated back - refresh to show latest data
                    await ViewModel.RefreshAsync();
                }
                else
                {
                    // First time navigation - initialize with categories/brands
                    await ViewModel.InitializeAsync();
                }
                
                AdjustGridColumns(); // Initial responsive setup
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ProductBrowsePage] OnNavigatedTo failed: {ex.Message}");
            }
        }

        #region Responsive Grid Logic

        private void ProductGridView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            AdjustGridColumns();
        }

        private void AdjustGridColumns()
        {
            if (ProductGridView == null) return;

            // Find ItemsWrapGrid in ItemsPanel
            var itemsWrapGrid = FindVisualChild<ItemsWrapGrid>(ProductGridView);
            if (itemsWrapGrid == null) return;

            // Use ContentHost ActualWidth (accounts for sidebar/nav)
            var contentWidth = ProductGridView.ActualWidth;
            
            // Task B: Responsive rule - 1400×850 → 3 columns based on CONTENT width
            const double minCardWidth = 240;
            const double cardGap = 16;

            // At 1400px+ content width → force 3 columns minimum
            // Below 1400px → calculate dynamically
            int columns;
            if (contentWidth >= 1400)
            {
                // At 1400+ enforce minimum 3 columns, can go up to 4-5 if space allows
                columns = Math.Max(3, (int)Math.Floor((contentWidth + cardGap) / (minCardWidth + cardGap)));
            }
            else
            {
                // Below 1400: dynamic (2 cols at ~900px, 1 col at ~500px)
                columns = Math.Max(1, (int)Math.Floor((contentWidth + cardGap) / (minCardWidth + cardGap)));
            }

            // Set ItemWidth to fill space evenly
            var totalSpacing = (columns - 1) * cardGap;
            var adjustedItemWidth = (contentWidth - totalSpacing) / columns;

            itemsWrapGrid.ItemWidth = Math.Max(220, adjustedItemWidth); // Min 220px per card
            itemsWrapGrid.MaximumRowsOrColumns = columns;

            System.Diagnostics.Debug.WriteLine($"[ProductBrowsePage] Responsive: ContentWidth={contentWidth:F0}px, Columns={columns}, CardWidth={adjustedItemWidth:F0}px");
        }

        private static T? FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T typedChild)
                    return typedChild;

                var result = FindVisualChild<T>(child);
                if (result != null)
                    return result;
            }
            return null;
        }

        #endregion

        #region Pagination Event Handlers

        private async void OnPageChanged(object sender, int newPage)
        {
            // PaginationControl already updated CurrentPage in its ChangePage method
            // Just trigger the load - don't check if CurrentPage != newPage
            await ViewModel.GoToPageAsync(newPage);
            
            // Scroll to top after page change for better UX
            if (ProductGridView.ItemsPanelRoot != null)
            {
                var scrollViewer = FindVisualChild<ScrollViewer>(this);
                scrollViewer?.ChangeView(null, 0, null, false);
            }
        }

        private async void OnPageSizeChanged(object sender, int newPageSize)
        {
            // PaginationControl already updated PageSize before raising event
            // Just trigger the reload - don't check if PageSize != newPageSize
            ViewModel.PageSize = newPageSize;
            ViewModel.CurrentPage = 1; // Reset to first page
            await ViewModel.RefreshAsync();
            
            // Scroll to top after page size change
            var scrollViewer = FindVisualChild<ScrollViewer>(this);
            scrollViewer?.ChangeView(null, 0, null, false);
        }

        #endregion

        #region Search Card Event Handlers

        private void SearchCard_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                var query = sender.Text?.ToLower() ?? string.Empty;

                if (string.IsNullOrWhiteSpace(query))
                {
                    sender.ItemsSource = null;
                    return;
                }

                // Generate suggestions from current products
                var suggestions = ViewModel.Products
                    .Where(p => p.Name.ToLower().Contains(query) ||
                               p.Category.ToLower().Contains(query) ||
                               p.Manufacturer.ToLower().Contains(query))
                    .Select(p => p.Name)
                    .Distinct()
                    .Take(8)
                    .ToList();

                sender.ItemsSource = suggestions;
            }
        }

        private void SearchCard_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            sender.Text = args.SelectedItem?.ToString() ?? string.Empty;
        }

        private async void SearchCard_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (args.ChosenSuggestion != null)
            {
                ViewModel.SearchQuery = args.ChosenSuggestion.ToString() ?? string.Empty;
            }
            else
            {
                ViewModel.SearchQuery = args.QueryText;
            }

            if (ViewModel.ApplyFiltersCommand?.CanExecute(null) == true)
            {
                await ViewModel.ApplyFiltersCommand.ExecuteAsync(null);
            }
        }

        #endregion

        #region Filter Event Handlers

        private async void CategoryComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ViewModel == null) return;

            if (CategoryComboBox.SelectedItem is string category)
            {
                await ViewModel.FilterByCategoryCommand.ExecuteAsync(category);
            }
        }

        private async void BrandComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ViewModel == null) return;

            if (BrandComboBox.SelectedItem is string brand)
            {
                await ViewModel.FilterByBrandCommand.ExecuteAsync(brand);
            }
        }

        private async void SortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Guard: Prevent NullReferenceException during page initialization
            if (ViewModel == null) return;

            if (sender is ComboBox comboBox && comboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                var sortOption = selectedItem.Tag?.ToString() ?? selectedItem.Content?.ToString();
                if (!string.IsNullOrEmpty(sortOption))
                {
                    await ViewModel.SortCommand.ExecuteAsync(sortOption);
                }
            }
        }

        #endregion

        private async void RefreshContainer_RefreshRequested(RefreshContainer sender, RefreshRequestedEventArgs args)
        {
            using var deferral = args.GetDeferral();
            await ViewModel.RefreshCommand.ExecuteAsync(null);
        }

        #region View Product Details Dialog

        private async void ViewProductButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is ProductCardViewModel product)
            {
                await ShowViewProductDialogAsync(product);
            }
        }

        public async Task ShowViewProductDialogAsync(ProductCardViewModel product)
        {
            try
            {
                _selectedProduct = product;

                // Populate dialog fields with product data
                ViewProductName.Text = product.Name;
                ViewProductCategory.Text = string.IsNullOrEmpty(product.Category) ? "Uncategorized" : product.Category;
                ViewProductManufacturer.Text = string.IsNullOrEmpty(product.Manufacturer) ? "Unknown" : product.Manufacturer;
                ViewProductPrice.Text = $"₫{product.Price:N0}";
                ViewProductStock.Text = product.Stock.ToString();
                ViewProductRating.Text = product.Rating.ToString("F1");
                ViewProductRatingCount.Text = $"({product.RatingCount} reviews)";

                // Set stock status badge
                if (product.Stock <= 0)
                {
                    ViewProductStockBadge.Background = (Brush)Application.Current.Resources["StockOutOfStockBackgroundBrush"];
                    ViewProductStockStatus.Text = "Out of Stock";
                    ViewProductStockStatus.Foreground = (Brush)Application.Current.Resources["StockOutOfStockForegroundBrush"];
                    DialogAddToCartButton.IsEnabled = false;
                }
                else if (product.Stock <= 10)
                {
                    ViewProductStockBadge.Background = (Brush)Application.Current.Resources["StockLowStockBackgroundBrush"];
                    ViewProductStockStatus.Text = "Low Stock";
                    ViewProductStockStatus.Foreground = (Brush)Application.Current.Resources["StockLowStockForegroundBrush"];
                    DialogAddToCartButton.IsEnabled = true;
                }
                else
                {
                    ViewProductStockBadge.Background = (Brush)Application.Current.Resources["StockInStockBackgroundBrush"];
                    ViewProductStockStatus.Text = "In Stock";
                    ViewProductStockStatus.Foreground = (Brush)Application.Current.Resources["StockInStockForegroundBrush"];
                    DialogAddToCartButton.IsEnabled = true;
                }

                // Load product image
                if (!string.IsNullOrEmpty(product.ImageUrl) && product.ImageUrl != "/Assets/placeholder-product.png")
                {
                    try
                    {
                        ViewProductImage.Source = new BitmapImage(new Uri(product.ImageUrl));
                        ViewProductImagePlaceholder.Visibility = Visibility.Collapsed;
                    }
                    catch
                    {
                        ViewProductImage.Source = null;
                        ViewProductImagePlaceholder.Visibility = Visibility.Visible;
                    }
                }
                else
                {
                    ViewProductImage.Source = null;
                    ViewProductImagePlaceholder.Visibility = Visibility.Visible;
                }

                ViewProductDialog.XamlRoot = this.XamlRoot;
                await ViewProductDialog.ShowAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ProductBrowsePage] ShowViewProductDialogAsync failed: {ex.Message}");
            }
        }

        private async void DialogAddToCartButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedProduct != null)
            {
                await ViewModel.AddToCartCommand.ExecuteAsync(_selectedProduct);
                ViewProductDialog.Hide();
            }
        }

        #endregion
    }
}
