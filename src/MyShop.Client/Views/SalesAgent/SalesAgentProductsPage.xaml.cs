using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MyShop.Client.ViewModels.SalesAgent;
using MyShop.Client.Services;
using MyShop.Client.Views.Components.Pagination;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MyShop.Client.Views.SalesAgent
{
    public sealed partial class SalesAgentProductsPage : Page
    {
        public SalesAgentProductsViewModel ViewModel { get; }

        public SalesAgentProductsPage()
        {
            this.InitializeComponent();
            ViewModel = App.Current.Services.GetRequiredService<SalesAgentProductsViewModel>();
            this.DataContext = ViewModel;

            // Subscribe to edit/delete events
            ViewModel.EditProductRequested += ViewModel_EditProductRequested;
            ViewModel.DeleteProductRequested += ViewModel_DeleteProductRequested;

            Unloaded += SalesAgentProductsPage_Unloaded;
        }

        private void SalesAgentProductsPage_Unloaded(object sender, RoutedEventArgs e)
        {
            ViewModel.EditProductRequested -= ViewModel_EditProductRequested;
            ViewModel.DeleteProductRequested -= ViewModel_DeleteProductRequested;
        }

        private async void ViewModel_EditProductRequested(object? sender, ProductViewModel product)
        {
            await ShowEditProductDialogAsync(product);
        }

        private async void ViewModel_DeleteProductRequested(object? sender, ProductViewModel product)
        {
            await ShowDeleteConfirmationAsync(product);
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
                LoggingService.Instance.Error("[SalesAgentProductsPage] OnNavigatedTo failed", ex);
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
                               p.Category.ToLower().Contains(query))
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

        private void CategoryComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ViewModel == null) return;

            if (CategoryComboBox.SelectedItem is ComboBoxItem item)
            {
                var category = item.Tag?.ToString();
                ViewModel.SelectedCategory = string.IsNullOrEmpty(category) ? "All Categories" : category;
            }
        }

        private void StockStatusComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ViewModel == null) return;

            if (StockStatusComboBox.SelectedItem is ComboBoxItem item)
            {
                ViewModel.SelectedStockStatus = item.Tag?.ToString() ?? string.Empty;
            }
        }

        private void SortByComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
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

        #endregion

        #region Pagination

        private async void OnPageChanged(object sender, PageChangedEventArgs e)
        {
            ViewModel.CurrentPage = e.CurrentPage;
            await ViewModel.LoadPageAsync();
        }

        #endregion

        #region Action Buttons

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.RefreshCommand?.CanExecute(null) == true)
            {
                await ViewModel.RefreshCommand.ExecuteAsync(null);
            }
        }

        private async void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.ExportCommand?.CanExecute(null) == true)
            {
                await ViewModel.ExportCommand.ExecuteAsync(null);
            }
        }

        private async void AddProductButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Reset dialog fields
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
                LoggingService.Instance.Error("[SalesAgentProductsPage] AddProductButton_Click failed", ex);
            }
        }

        #endregion

        #region Add Product Dialog Handlers

        private async void AddProductDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsPage.AddProductDialog_PrimaryButtonClick] START - Dialog closed with AddProduct action");

            try
            {
                var name = NewNameTextBox.Text.Trim();
                if (string.IsNullOrEmpty(name))
                {
                    System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsPage.AddProductDialog_PrimaryButtonClick] ❌ VALIDATION FAILED - Product name is empty");
                    args.Cancel = true;

                    ContentDialog errorDialog = new ContentDialog
                    {
                        Title = "Validation Error",
                        Content = "Product name is required.",
                        CloseButtonText = "OK",
                        XamlRoot = this.XamlRoot
                    };
                    await errorDialog.ShowAsync();
                    return;
                }

                var categoryItem = NewCategoryComboBox.SelectedItem as ComboBoxItem;
                var categoryId = categoryItem?.Tag?.ToString() ?? string.Empty;

                if (string.IsNullOrEmpty(categoryId) || !Guid.TryParse(categoryId, out var parsedCategoryId) || parsedCategoryId == Guid.Empty)
                {
                    System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsPage.AddProductDialog_PrimaryButtonClick] ❌ VALIDATION FAILED - Category is not selected");
                    args.Cancel = true;

                    ContentDialog errorDialog = new ContentDialog
                    {
                        Title = "Validation Error",
                        Content = "Please select a category.",
                        CloseButtonText = "OK",
                        XamlRoot = this.XamlRoot
                    };
                    await errorDialog.ShowAsync();
                    return;
                }

                int.TryParse(NewStockTextBox.Text, out var stock);
                decimal.TryParse(NewPriceTextBox.Text, out var price);
                decimal.TryParse(NewImportPriceTextBox.Text, out var importPrice);
                var description = NewDescriptionTextBox.Text.Trim();

                // Create product object
                var product = new MyShop.Shared.Models.Product
                {
                    Id = Guid.NewGuid(),
                    Name = name,
                    CategoryId = parsedCategoryId,
                    Quantity = stock,
                    SellingPrice = price,
                    ImportPrice = importPrice,
                    Description = description
                };

                System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsPage.AddProductDialog_PrimaryButtonClick] Dialog data extracted - Product: {product.Name}, Category: {product.CategoryId}, Quantity: {product.Quantity}");

                System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsPage.AddProductDialog_PrimaryButtonClick] Validation passed, executing SaveNewProductCommand");

                // Execute the command from ViewModel
                if (ViewModel.SaveNewProductCommand.CanExecute(product))
                {
                    System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsPage.AddProductDialog_PrimaryButtonClick] SaveNewProductCommand is executable, executing");
                    await ViewModel.SaveNewProductCommand.ExecuteAsync(product);
                    System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsPage.AddProductDialog_PrimaryButtonClick] ✅ SaveNewProductCommand executed successfully");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsPage.AddProductDialog_PrimaryButtonClick] ❌ SaveNewProductCommand is not executable");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsPage.AddProductDialog_PrimaryButtonClick] ❌ EXCEPTION - {ex.GetType().Name}: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsPage.AddProductDialog_PrimaryButtonClick] Stack Trace: {ex.StackTrace}");

                ContentDialog errorDialog = new ContentDialog
                {
                    Title = "Error",
                    Content = $"An error occurred: {ex.Message}",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await errorDialog.ShowAsync();
            }
        }

        private void AddProductDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            // Do nothing, dialog closes automatically
        }

        #endregion

        #region Edit Product Dialog Handlers

        private ProductViewModel? _editingProduct;

        private async Task ShowEditProductDialogAsync(ProductViewModel product)
        {
            try
            {
                _editingProduct = product;

                // Populate dialog with product data
                EditProductIdTextBox.Text = product.Id.ToString();
                EditNameTextBox.Text = product.Name;
                EditStockTextBox.Text = product.Stock.ToString();
                EditPriceTextBox.Text = product.Price.ToString("F0");
                EditImportPriceTextBox.Text = "0"; // We don't have import price in ProductViewModel
                EditDescriptionTextBox.Text = string.Empty;

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
                LoggingService.Instance.Error("[SalesAgentProductsPage] ShowEditProductDialogAsync failed", ex);
            }
        }

        private async void EditProductDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsPage.EditProductDialog_PrimaryButtonClick] START - Dialog closed with EditProduct action");

            try
            {
                if (_editingProduct == null)
                {
                    System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsPage.EditProductDialog_PrimaryButtonClick] ❌ ERROR - _editingProduct is null");
                    args.Cancel = true;
                    return;
                }

                var name = EditNameTextBox.Text.Trim();
                if (string.IsNullOrEmpty(name))
                {
                    System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsPage.EditProductDialog_PrimaryButtonClick] ❌ VALIDATION FAILED - Product name is empty");
                    args.Cancel = true;

                    ContentDialog errorDialog = new ContentDialog
                    {
                        Title = "Validation Error",
                        Content = "Product name is required.",
                        CloseButtonText = "OK",
                        XamlRoot = this.XamlRoot
                    };
                    await errorDialog.ShowAsync();
                    return;
                }

                var categoryItem = EditCategoryComboBox.SelectedItem as ComboBoxItem;
                var categoryId = categoryItem?.Tag?.ToString() ?? string.Empty;

                if (string.IsNullOrEmpty(categoryId) || !Guid.TryParse(categoryId, out var parsedCategoryId) || parsedCategoryId == Guid.Empty)
                {
                    System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsPage.EditProductDialog_PrimaryButtonClick] ❌ VALIDATION FAILED - Category is not selected");
                    args.Cancel = true;

                    ContentDialog errorDialog = new ContentDialog
                    {
                        Title = "Validation Error",
                        Content = "Please select a category.",
                        CloseButtonText = "OK",
                        XamlRoot = this.XamlRoot
                    };
                    await errorDialog.ShowAsync();
                    return;
                }

                int.TryParse(EditStockTextBox.Text, out var stock);
                decimal.TryParse(EditPriceTextBox.Text, out var price);
                decimal.TryParse(EditImportPriceTextBox.Text, out var importPrice);
                var description = EditDescriptionTextBox.Text.Trim();

                // Create product object with updated values
                var product = new MyShop.Shared.Models.Product
                {
                    Id = _editingProduct.Id,
                    Name = name,
                    CategoryId = parsedCategoryId,
                    Quantity = stock,
                    SellingPrice = price,
                    ImportPrice = importPrice,
                    Description = description
                };

                System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsPage.EditProductDialog_PrimaryButtonClick] Dialog data extracted - Product ID: {product.Id}, Name: {product.Name}, Category: {product.CategoryId}");

                System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsPage.EditProductDialog_PrimaryButtonClick] Validation passed, executing SaveEditProductCommand");

                // Execute the command from ViewModel
                if (ViewModel.SaveEditProductCommand.CanExecute(product))
                {
                    System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsPage.EditProductDialog_PrimaryButtonClick] SaveEditProductCommand is executable, executing");
                    await ViewModel.SaveEditProductCommand.ExecuteAsync(product);
                    System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsPage.EditProductDialog_PrimaryButtonClick] ✅ SaveEditProductCommand executed successfully");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsPage.EditProductDialog_PrimaryButtonClick] ❌ SaveEditProductCommand is not executable");
                }

                _editingProduct = null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsPage.EditProductDialog_PrimaryButtonClick] ❌ EXCEPTION - {ex.GetType().Name}: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsPage.EditProductDialog_PrimaryButtonClick] Stack Trace: {ex.StackTrace}");

                ContentDialog errorDialog = new ContentDialog
                {
                    Title = "Error",
                    Content = $"An error occurred: {ex.Message}",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await errorDialog.ShowAsync();

                _editingProduct = null;
            }
        }

        private void EditProductDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            _editingProduct = null;
        }

        #endregion

        #region Delete Confirmation

        private async Task ShowDeleteConfirmationAsync(ProductViewModel product)
        {
            try
            {
                var dialog = new ContentDialog
                {
                    Title = "Delete Product",
                    Content = $"Are you sure you want to delete '{product.Name}'?\n\nThis action cannot be undone.",
                    PrimaryButtonText = "Delete",
                    SecondaryButtonText = "Cancel",
                    DefaultButton = ContentDialogButton.Secondary,
                    XamlRoot = this.XamlRoot
                };

                var result = await dialog.ShowAsync();

                if (result == ContentDialogResult.Primary)
                {
                    await ViewModel.ConfirmDeleteProductAsync(product.Id);
                }
            }
            catch (Exception ex)
            {
                LoggingService.Instance.Error("[SalesAgentProductsPage] ShowDeleteConfirmationAsync failed", ex);
            }
        }

        #endregion

        private async void RefreshContainer_RefreshRequested(RefreshContainer sender, RefreshRequestedEventArgs args)
        {
            using var deferral = args.GetDeferral();
            try
            {
                if (ViewModel.RefreshCommand?.CanExecute(null) == true)
                {
                    await ViewModel.RefreshCommand.ExecuteAsync(null);
                }
            }
            catch (Exception ex)
            {
                LoggingService.Instance.Error("Failed to refresh products", ex);
            }
        }
    }
}
