using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Input;
using Windows.System;
using MyShop.Client.ViewModels.Shared;
using MyShop.Client.Views.Components.Pagination;
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
            SetupKeyboardShortcuts();
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
                await ViewModel.InitializeAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ProductBrowsePage] OnNavigatedTo failed: {ex.Message}");
            }
        }

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

        #region Pagination Event Handler

        private async void OnPageChanged(object sender, PageChangedEventArgs e)
        {
            if (ViewModel == null) return;

            await ViewModel.GoToPageAsync(e.CurrentPage);
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
                    ViewProductStockBadge.Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 254, 226, 226));
                    ViewProductStockStatus.Text = "Out of Stock";
                    ViewProductStockStatus.Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 220, 38, 38));
                    DialogAddToCartButton.IsEnabled = false;
                }
                else if (product.Stock <= 10)
                {
                    ViewProductStockBadge.Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 254, 243, 199));
                    ViewProductStockStatus.Text = "Low Stock";
                    ViewProductStockStatus.Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 217, 119, 6));
                    DialogAddToCartButton.IsEnabled = true;
                }
                else
                {
                    ViewProductStockBadge.Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 209, 250, 229));
                    ViewProductStockStatus.Text = "In Stock";
                    ViewProductStockStatus.Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 5, 150, 105));
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
