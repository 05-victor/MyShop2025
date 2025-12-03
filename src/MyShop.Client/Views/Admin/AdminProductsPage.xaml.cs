using System.Collections.ObjectModel;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using MyShop.Client.ViewModels.Admin;
using MyShop.Core.Common;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace MyShop.Client.Views.Admin;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class AdminProductsPage : Page
{
    public AdminProductsViewModel ViewModel { get; }

    public AdminProductsPage()
    {
        InitializeComponent();

        ViewModel = App.Current.Services.GetRequiredService<AdminProductsViewModel>();
        this.DataContext = ViewModel;

        // Subscribe to edit product event
        ViewModel.EditProductRequested += ViewModel_EditProductRequested;
        // Subscribe to view product event
        ViewModel.ViewProductRequested += ViewModel_ViewProductRequested;

        Loaded += AdminProductPage_Loaded;
        Unloaded += AdminProductPage_Unloaded;
    }

    private void AdminProductPage_Unloaded(object sender, RoutedEventArgs e)
    {
        ViewModel.EditProductRequested -= ViewModel_EditProductRequested;
        ViewModel.ViewProductRequested -= ViewModel_ViewProductRequested;
    }

    private async void ViewModel_EditProductRequested(object? sender, ProductRow product)
    {
        await ShowEditProductDialogAsync(product);
    }

    private async void ViewModel_ViewProductRequested(object? sender, ProductRow product)
    {
        await ShowViewProductDialogAsync(product);
    }

    #region Sample data

    private async void AdminProductPage_Loaded(object sender, RoutedEventArgs e)
    {
        // Load products from repository
        if (ViewModel.LoadProductsCommand.CanExecute(null))
        {
            await ViewModel.LoadProductsCommand.ExecuteAsync(null);
        }
    }

    #endregion

    #region Filter Handlers

    private void CategoryComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // Guard: ViewModel may be null during page initialization
        if (ViewModel == null) return;
        
        if (CategoryComboBox.SelectedItem is ComboBoxItem item)
        {
            var category = item.Tag?.ToString();
            ViewModel.SelectedCategory = string.IsNullOrEmpty(category) ? null : category;
        }
    }

    private void SortByComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // Guard: ViewModel may be null during page initialization
        if (ViewModel == null) return;
        
        if (SortByComboBox.SelectedItem is ComboBoxItem item)
        {
            var tag = item.Tag?.ToString() ?? "name-asc";
            var parts = tag.Split('-');
            if (parts.Length == 2)
            {
                ViewModel.SortBy = parts[0];
                ViewModel.SortDescending = parts[1] == "desc";
            }
        }
    }

    private void PriceTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        // Parse min price
        if (decimal.TryParse(MinPriceTextBox.Text, out var minPrice))
        {
            ViewModel.MinPrice = minPrice;
        }
        else
        {
            ViewModel.MinPrice = null;
        }

        // Parse max price
        if (decimal.TryParse(MaxPriceTextBox.Text, out var maxPrice))
        {
            ViewModel.MaxPrice = maxPrice;
        }
        else
        {
            ViewModel.MaxPrice = null;
        }
    }

    private async void ApplyFiltersButton_Click(object sender, RoutedEventArgs e)
    {
        // Apply filters and reload from page 1
        await ViewModel.LoadDataAsync();
    }

    #endregion

    #region Search AutoSuggestBox Handlers

    private List<string> _searchSuggestions = new();

    private void SearchAutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        // Only get suggestions when the user types
        if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
        {
            var query = sender.Text?.ToLower() ?? string.Empty;
            
            if (string.IsNullOrWhiteSpace(query))
            {
                sender.ItemsSource = null;
                return;
            }

            // Filter current items as suggestions (from loaded products)
            var suggestions = ViewModel.Items
                .Where(p => p.Name.ToLower().Contains(query) || 
                           (p.Sku?.ToLower().Contains(query) ?? false) ||
                           (p.Category?.ToLower().Contains(query) ?? false))
                .Select(p => p.Name)
                .Distinct()
                .Take(8)
                .ToList();

            sender.ItemsSource = suggestions;
        }
    }

    private void SearchAutoSuggestBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
    {
        // Set the text to the chosen suggestion
        sender.Text = args.SelectedItem?.ToString() ?? string.Empty;
    }

    private async void SearchButton_Click(object sender, RoutedEventArgs e)
    {
        // Apply search and reload
        await ViewModel.LoadDataAsync();
    }

    private async void SearchAutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        // When user submits query (Enter key or selects suggestion), apply filters
        if (args.ChosenSuggestion != null)
        {
            ViewModel.SearchQuery = args.ChosenSuggestion.ToString();
        }
        else
        {
            ViewModel.SearchQuery = args.QueryText;
        }
        
        // Apply the search
        await ViewModel.LoadDataAsync();
    }

    #endregion

    #region Button handlers (Header)

    private async void AddProductButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // Reset field trong dialog
            NewNameTextBox.Text = string.Empty;
            NewStockTextBox.Text = string.Empty;
            NewPriceTextBox.Text = string.Empty;
            NewImportPriceTextBox.Text = string.Empty;
            NewDescriptionTextBox.Text = string.Empty;
            NewCategoryComboBox.SelectedIndex = -1;

            AddProductDialog.XamlRoot = this.XamlRoot;
            await AddProductDialog.ShowAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AdminProductsPage] AddProductButton_Click failed: {ex.Message}");
        }
    }

    private async void ExportButton_Click(object sender, RoutedEventArgs e)
    {
        // Execute export command and open Explorer
        if (ViewModel.ExportCommand.CanExecute(null))
        {
            await ViewModel.ExportCommand.ExecuteAsync(null);
            // Note: The ExportCommand in ViewModel should call StorageConstants.OpenExplorerAndSelectFile
        }
    }

    private void ImportButton_Click(object sender, RoutedEventArgs e)
    {
        // TODO: Implement import from CSV/Excel when FileOpenPicker is integrated
        System.Diagnostics.Debug.WriteLine("[AdminProductsPage] Import products requested");
    }

    private void PrevPageButton_Click(object sender, RoutedEventArgs e)
    {
        // TODO: Implement pagination when ViewModel has PreviousPage method
        System.Diagnostics.Debug.WriteLine("[AdminProductsPage] Previous page requested");
    }

    private void NextPageButton_Click(object sender, RoutedEventArgs e)
    {
        // TODO: Implement pagination when ViewModel has NextPage method
        System.Diagnostics.Debug.WriteLine("[AdminProductsPage] Next page requested");
    }

    #endregion

    #region Add Product Dialog handlers

    private void AddProductDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        var name = NewNameTextBox.Text.Trim();
        if (string.IsNullOrEmpty(name))
        {
            // If you want to prevent dialog from closing when name is missing:
            // args.Cancel = true;
            return;
        }

        var categoryItem = NewCategoryComboBox.SelectedItem as ComboBoxItem;
        var category = categoryItem?.Content?.ToString() ?? "Uncategorized";

        int.TryParse(NewStockTextBox.Text, out var stock);
        decimal.TryParse(NewPriceTextBox.Text, out var price);
        decimal.TryParse(NewImportPriceTextBox.Text, out var importPrice);

        // TODO: Implement AddProduct through ViewModel
        // For now, just close the dialog
        System.Diagnostics.Debug.WriteLine($"[AdminProductsPage] Add product requested: {name}");
        // Would call: await ViewModel.AddProductCommand.ExecuteAsync(productData);
    }

    private void AddProductDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        // No action needed, dialog closes automatically
    }

    #endregion

    #region Edit Product Dialog handlers

    private ProductRow? _editingProduct;

    public async Task ShowEditProductDialogAsync(ProductRow product)
    {
        try
        {
            _editingProduct = product;

            // Populate dialog fields with product data
            EditProductIdTextBox.Text = product.Id.ToString();
            EditNameTextBox.Text = product.Name;
            EditSkuTextBox.Text = product.Sku ?? string.Empty;
            EditStockTextBox.Text = product.Stock.ToString();
            EditPriceTextBox.Text = product.Price.ToString("F2");
            EditImportPriceTextBox.Text = product.ImportPrice.ToString("F2");
            EditDescriptionTextBox.Text = string.Empty; // We don't have description in ProductRow

            // Select the correct category
            for (int i = 0; i < EditCategoryComboBox.Items.Count; i++)
            {
                if (EditCategoryComboBox.Items[i] is ComboBoxItem item && 
                    item.Tag?.ToString() == product.Category)
                {
                    EditCategoryComboBox.SelectedIndex = i;
                    break;
                }
            }

            EditProductDialog.XamlRoot = this.XamlRoot;
            await EditProductDialog.ShowAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AdminProductsPage] ShowEditProductDialogAsync failed: {ex.Message}");
        }
    }

    private async void EditProductDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        var name = EditNameTextBox.Text.Trim();
        if (string.IsNullOrEmpty(name))
        {
            args.Cancel = true;
            return;
        }

        var categoryItem = EditCategoryComboBox.SelectedItem as ComboBoxItem;
        var category = categoryItem?.Tag?.ToString() ?? "Uncategorized";

        int.TryParse(EditStockTextBox.Text, out var stock);
        decimal.TryParse(EditPriceTextBox.Text, out var price);
        decimal.TryParse(EditImportPriceTextBox.Text, out var importPrice);

        if (_editingProduct != null)
        {
            // Update via ViewModel
            await ViewModel.UpdateProductAsync(
                _editingProduct.Id,
                name,
                EditSkuTextBox.Text.Trim(),
                EditDescriptionTextBox.Text.Trim(),
                _editingProduct.Image ?? string.Empty,
                importPrice,
                price,
                stock,
                category);
        }

        _editingProduct = null;
    }

    private void EditProductDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        _editingProduct = null;
    }

    #endregion

    private async void RefreshContainer_RefreshRequested(RefreshContainer sender, RefreshRequestedEventArgs args)
    {
        using var deferral = args.GetDeferral();
        await ViewModel.RefreshCommand.ExecuteAsync(null);
    }

    #region View Product Dialog handlers

    public async Task ShowViewProductDialogAsync(ProductRow product)
    {
        try
        {
            // Populate dialog fields with product data
            ViewProductId.Text = product.Id.ToString();
            ViewProductName.Text = product.Name;
            ViewProductSku.Text = string.IsNullOrEmpty(product.Sku) ? "N/A" : product.Sku;
            ViewProductCategory.Text = string.IsNullOrEmpty(product.Category) ? "Uncategorized" : product.Category;
            ViewProductPrice.Text = product.Price.ToString("C0");
            ViewProductImportPrice.Text = product.ImportPrice.ToString("C0");
            ViewProductStock.Text = product.Stock.ToString();
            ViewProductRating.Text = product.Rating.ToString("F1");

            // Calculate and display profit margin
            if (product.ImportPrice > 0)
            {
                var margin = ((product.Price - product.ImportPrice) / product.ImportPrice) * 100;
                ViewProductMargin.Text = $"+{margin:F1}%";
            }
            else
            {
                ViewProductMargin.Text = "N/A";
            }

            // Set stock status badge
            if (product.Stock <= 0)
            {
                ViewProductStockBadge.Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 254, 226, 226)); // Red background
                ViewProductStockStatus.Text = "Out of Stock";
                ViewProductStockStatus.Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 220, 38, 38)); // Red text
            }
            else if (product.Stock <= 10)
            {
                ViewProductStockBadge.Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 254, 243, 199)); // Yellow background
                ViewProductStockStatus.Text = "Low Stock";
                ViewProductStockStatus.Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 217, 119, 6)); // Yellow text
            }
            else
            {
                ViewProductStockBadge.Background = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 209, 250, 229)); // Green background
                ViewProductStockStatus.Text = "In Stock";
                ViewProductStockStatus.Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 5, 150, 105)); // Green text
            }

            // Load product image
            if (!string.IsNullOrEmpty(product.Image))
            {
                try
                {
                    ViewProductImage.Source = new BitmapImage(new Uri(product.Image));
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
            System.Diagnostics.Debug.WriteLine($"[AdminProductsPage] ShowViewProductDialogAsync failed: {ex.Message}");
        }
    }

    #endregion
}
