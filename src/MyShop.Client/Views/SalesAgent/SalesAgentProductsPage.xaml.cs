using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MyShop.Client.ViewModels.SalesAgent;
using MyShop.Client.Services;
using MyShop.Client.Views.Components.Controls;
using MyShop.Core.Interfaces.Repositories;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MyShop.Client.Views.SalesAgent
{
    public sealed partial class SalesAgentProductsPage : Page
    {
        public SalesAgentProductsViewModel ViewModel { get; }
        private Timer? _searchDebounceTimer;
        private readonly IAuthRepository _authRepository;

        public SalesAgentProductsPage()
        {
            this.InitializeComponent();
            ViewModel = App.Current.Services.GetRequiredService<SalesAgentProductsViewModel>();
            this.DataContext = ViewModel;

            // Get auth repository for retrieving current user ID from token
            _authRepository = App.Current.Services.GetRequiredService<IAuthRepository>();

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
                    ViewModel.SearchQuery = string.Empty;
                    return;
                }

                // Generate suggestions from current products (don't filter list yet)
                var suggestions = ViewModel.Products
                    .Where(p => p.Name.ToLower().Contains(query) ||
                               p.Category.ToLower().Contains(query))
                    .Select(p => p.Name)
                    .Distinct()
                    .Take(8)
                    .ToList();

                sender.ItemsSource = suggestions;

                // Debounce: just update SearchQuery for binding, don't apply filters
                // Filters will only be applied when user submits search or selects suggestion
                _searchDebounceTimer?.Dispose();
                _searchDebounceTimer = new Timer(_ =>
                {
                    this.DispatcherQueue.TryEnqueue(() =>
                    {
                        try
                        {
                            // Only update the property, don't trigger filter application
                            ViewModel.SearchQuery = query;
                            System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsPage.SearchCard_TextChanged] SearchQuery updated to: '{query}' (no filter applied yet)");
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsPage.SearchCard_TextChanged] Error in debounce timer: {ex.Message}");
                        }
                    });
                }, null, 500, Timeout.Infinite);
            }
        }

        private async void SearchCard_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            _searchDebounceTimer?.Dispose();
            sender.Text = args.SelectedItem?.ToString() ?? string.Empty;

            // Apply filters when suggestion is selected
            ViewModel.SearchQuery = sender.Text;
            System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsPage.SearchCard_SuggestionChosen] Suggestion selected: '{ViewModel.SearchQuery}', applying filters");
            if (ViewModel.ApplyFiltersCommand?.CanExecute(null) == true)
            {
                await ViewModel.ApplyFiltersCommand.ExecuteAsync(null);
            }
        }

        private async void SearchCard_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            _searchDebounceTimer?.Dispose();

            if (args.ChosenSuggestion != null)
            {
                ViewModel.SearchQuery = args.ChosenSuggestion.ToString() ?? string.Empty;
            }
            else
            {
                ViewModel.SearchQuery = args.QueryText;
            }

            System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsPage.SearchCard_QuerySubmitted] Search query: '{ViewModel.SearchQuery}'");
            if (ViewModel.ApplyFiltersCommand?.CanExecute(null) == true)
            {
                await ViewModel.ApplyFiltersCommand.ExecuteAsync(null);
            }
        }

        private async void SearchCard_SearchRequested(object sender, RoutedEventArgs e)
        {
            _searchDebounceTimer?.Dispose();

            var query = ProductSearchCard.SearchText?.ToLower() ?? string.Empty;
            ViewModel.SearchQuery = query;

            System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsPage.SearchCard_SearchRequested] Search button clicked, query: '{ViewModel.SearchQuery}'");
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

            if (CategoryComboBox.SelectedItem is MyShop.Shared.Models.Category category)
            {
                ViewModel.SelectedCategory = category;
                System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsPage.CategoryComboBox_SelectionChanged] Selected category: '{ViewModel.SelectedCategory?.Name}'");
            }
        }

        private async void BrandComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ViewModel == null) return;

            if (BrandComboBox.SelectedItem is string brand)
            {
                ViewModel.SelectedBrand = brand;
                System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsPage.BrandComboBox_SelectionChanged] Selected brand: '{ViewModel.SelectedBrand}'");
            }
        }

        private async void StockStatusComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ViewModel == null) return;

            if (StockStatusComboBox.SelectedItem is ComboBoxItem item)
            {
                ViewModel.SelectedStockStatus = item.Tag?.ToString() ?? string.Empty;
                System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsPage.StockStatusComboBox_SelectionChanged] Selected stock status: '{ViewModel.SelectedStockStatus}'");
            }
        }

        private async void SortByComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
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
                    System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsPage.SortByComboBox_SelectionChanged] Sort: {ViewModel.SortBy} {(ViewModel.SortDescending ? "DESC" : "ASC")}");
                }
            }
        }

        #endregion

        #region Pagination

        private async void OnPageChanged(object sender, int currentPage)
        {
            await ViewModel.GoToPageAsync(currentPage);
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

        private async void ExportPdfButton_Click(object sender, RoutedEventArgs e)
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
                NewSkuTextBox.Text = string.Empty;
                NewManufacturerComboBox.SelectedItem = null;
                NewStockTextBox.Text = string.Empty;
                NewPriceTextBox.Text = string.Empty;
                NewImportPriceTextBox.Text = string.Empty;
                NewCommissionRateTextBox.Text = string.Empty;
                NewDescriptionTextBox.Text = string.Empty;
                NewImageUrlTextBox.Text = string.Empty;
                NewCategoryComboBox.SelectedItem = null;
                NewStatusComboBox.SelectedIndex = 0;

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
                    return;
                }

                var sku = NewSkuTextBox.Text.Trim();
                if (string.IsNullOrEmpty(sku))
                {
                    System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsPage.AddProductDialog_PrimaryButtonClick] ❌ VALIDATION FAILED - SKU is empty");
                    args.Cancel = true;
                    return;
                }

                var manufacturer = (NewManufacturerComboBox.SelectedItem as string) ?? string.Empty;
                manufacturer = manufacturer.Trim();
                if (string.IsNullOrEmpty(manufacturer))
                {
                    System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsPage.AddProductDialog_PrimaryButtonClick] ❌ VALIDATION FAILED - Manufacturer is empty");
                    args.Cancel = true;
                    return;
                }

                var categoryItem = NewCategoryComboBox.SelectedItem as MyShop.Shared.Models.Category;
                var categoryId = categoryItem?.Id ?? Guid.Empty;

                if (categoryId == Guid.Empty)
                {
                    System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsPage.AddProductDialog_PrimaryButtonClick] ❌ VALIDATION FAILED - Category is not selected");
                    args.Cancel = true;
                    return;
                }

                int.TryParse(NewStockTextBox.Text, out var stock);
                decimal.TryParse(NewPriceTextBox.Text, out var price);
                decimal.TryParse(NewImportPriceTextBox.Text, out var importPrice);
                double.TryParse(NewCommissionRateTextBox.Text, out var commissionRate);
                var description = NewDescriptionTextBox.Text.Trim();
                // Device Type is represented by the Category name from the selected category
                var deviceType = categoryItem?.Name ?? string.Empty;
                var status = (NewStatusComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "AVAILABLE";
                var imageUrl = NewImageUrlTextBox.Text.Trim();

                // Get current user ID from auth repository
                var userIdResult = await _authRepository.GetCurrentUserIdAsync();
                var saleAgentId = userIdResult.IsSuccess ? userIdResult.Data : Guid.Empty;

                // Create product object
                var product = new MyShop.Shared.Models.Product
                {
                    Id = Guid.NewGuid(),
                    Name = name,
                    SKU = sku,
                    Manufacturer = manufacturer,
                    DeviceType = deviceType,
                    CategoryId = categoryId,
                    Quantity = stock,
                    SellingPrice = price,
                    ImportPrice = importPrice,
                    CommissionRate = commissionRate,
                    Description = description,
                    Status = status,
                    ImageUrl = imageUrl,
                    SaleAgentId = saleAgentId
                };

                System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsPage.AddProductDialog_PrimaryButtonClick] Dialog data extracted - Product: {product.Name}, SKU: {product.SKU}, Manufacturer: {product.Manufacturer}, Category: {product.CategoryId}, SaleAgentId: {product.SaleAgentId}");

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

        private async void NewImagePickButton_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsPage.NewImagePickButton_Click] Image picker button clicked");

            try
            {
                var picker = new Windows.Storage.Pickers.FileOpenPicker();
                picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
                picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
                picker.FileTypeFilter.Add(".jpg");
                picker.FileTypeFilter.Add(".jpeg");
                picker.FileTypeFilter.Add(".png");
                picker.FileTypeFilter.Add(".gif");
                picker.FileTypeFilter.Add(".bmp");

                // Get window handle from App.MainWindow
                var window = App.MainWindow;
                var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
                WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

                var file = await picker.PickSingleFileAsync();
                if (file != null)
                {
                    System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsPage.NewImagePickButton_Click] File selected: {file.Name}");

                    // Show loading indicator
                    ViewModel.IsLoading = true;

                    try
                    {
                        // Upload image and get URL
                        var result = await ViewModel.UploadProductImageAsync(file);
                        if (result.IsSuccess)
                        {
                            System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsPage.NewImagePickButton_Click] ✅ Image uploaded successfully: {result.Data}");
                            NewImageUrlTextBox.Text = result.Data;
                            System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsPage.NewImagePickButton_Click] Image URL set to textbox");
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsPage.NewImagePickButton_Click] ❌ Image upload failed: {result.ErrorMessage}");
                            // Log error - cannot show dialog while AddProductDialog is open
                            LoggingService.Instance.Error("[SalesAgentProductsPage.NewImagePickButton_Click] Upload failed", new Exception(result.ErrorMessage));
                        }
                    }
                    finally
                    {
                        ViewModel.IsLoading = false;
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsPage.NewImagePickButton_Click] File selection cancelled");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsPage.NewImagePickButton_Click] ❌ EXCEPTION - {ex.GetType().Name}: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsPage.NewImagePickButton_Click] Stack Trace: {ex.StackTrace}");

                // Log error - cannot show dialog while AddProductDialog is open
                LoggingService.Instance.Error("[SalesAgentProductsPage.NewImagePickButton_Click] Exception during image selection", ex);
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
                EditSkuTextBox.Text = product.Sku ?? string.Empty;
                EditManufacturerComboBox.SelectedItem = product.Manufacturer ?? "Select manufacturer";
                EditStockTextBox.Text = product.Stock.ToString();
                EditCommissionRateTextBox.Text = product.CommissionRate.ToString("F2");
                EditPriceTextBox.Text = product.Price.ToString("F0");
                EditImportPriceTextBox.Text = product.ImportPrice.ToString("F0") ?? "0";
                EditStatusComboBox.SelectedItem = product.Status ?? "AVAILABLE";
                EditImageUrlTextBox.Text = product.ImageUrl ?? string.Empty;
                EditDescriptionTextBox.Text = product.Description ?? string.Empty;

                // Select the correct category by matching Name
                var categoryToSelect = ViewModel.Categories.FirstOrDefault(c => c.Name == product.Category);
                if (categoryToSelect != null)
                {
                    EditCategoryComboBox.SelectedItem = categoryToSelect;
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
                    return;
                }

                var sku = EditSkuTextBox.Text.Trim();
                if (string.IsNullOrEmpty(sku))
                {
                    System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsPage.EditProductDialog_PrimaryButtonClick] ❌ VALIDATION FAILED - SKU is empty");
                    args.Cancel = true;
                    return;
                }

                var manufacturer = EditManufacturerComboBox.SelectedItem as string;
                if (string.IsNullOrEmpty(manufacturer))
                {
                    System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsPage.EditProductDialog_PrimaryButtonClick] ❌ VALIDATION FAILED - Manufacturer is not selected");
                    args.Cancel = true;
                    return;
                }

                var categoryItem = EditCategoryComboBox.SelectedItem as MyShop.Shared.Models.Category;
                var categoryId = categoryItem?.Id ?? Guid.Empty;

                if (categoryId == Guid.Empty)
                {
                    System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsPage.EditProductDialog_PrimaryButtonClick] ❌ VALIDATION FAILED - Category is not selected");
                    args.Cancel = true;
                    return;
                }

                int.TryParse(EditStockTextBox.Text, out var stock);
                decimal.TryParse(EditPriceTextBox.Text, out var price);
                decimal.TryParse(EditImportPriceTextBox.Text, out var importPrice);
                double.TryParse(EditCommissionRateTextBox.Text, out var commissionRatePercent);

                // Convert commission rate from percentage (e.g., 4) to decimal (e.g., 0.04)
                var commissionRate = commissionRatePercent / 100.0;

                var status = EditStatusComboBox.SelectedItem as string ?? "AVAILABLE";
                var imageUrl = EditImageUrlTextBox.Text.Trim();
                var description = EditDescriptionTextBox.Text.Trim();

                // Create product object with updated values
                var product = new MyShop.Shared.Models.Product
                {
                    Id = _editingProduct.Id,
                    Name = name,
                    SKU = sku,
                    Manufacturer = manufacturer,
                    CategoryId = categoryId,
                    Quantity = stock,
                    CommissionRate = commissionRate,
                    SellingPrice = price,
                    ImportPrice = importPrice,
                    Status = status,
                    ImageUrl = imageUrl,
                    Description = description
                };

                System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsPage.EditProductDialog_PrimaryButtonClick] Dialog data extracted - Product ID: {product.Id}, Name: {product.Name}, SKU: {product.SKU}, Manufacturer: {product.Manufacturer}");

                System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsPage.EditProductDialog_PrimaryButtonClick] Validation passed, executing SaveEditProductCommand");

                // Execute the command from ViewModel
                if (ViewModel.SaveEditProductCommand.CanExecute(product))
                {
                    System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsPage.EditProductDialog_PrimaryButtonClick] SaveEditProductCommand is executable, executing");
                    await ViewModel.SaveEditProductCommand.ExecuteAsync(product);
                    System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsPage.EditProductDialog_PrimaryButtonClick] ✅ SaveEditProductCommand executed successfully");
                }

                _editingProduct = null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsPage.EditProductDialog_PrimaryButtonClick] ❌ EXCEPTION - {ex.GetType().Name}: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsPage.EditProductDialog_PrimaryButtonClick] Stack Trace: {ex.StackTrace}");

                args.Cancel = true;
                LoggingService.Instance.Error("[SalesAgentProductsPage] EditProductDialog_PrimaryButtonClick failed", ex);

                _editingProduct = null;
            }
        }

        private void EditProductDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            _editingProduct = null;
        }

        private async void EditImagePickButton_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsPage.EditImagePickButton_Click] Image picker button clicked");

            // Validate that we have an editing product with ID
            if (_editingProduct == null || _editingProduct.Id == Guid.Empty)
            {
                System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsPage.EditImagePickButton_Click] ❌ ERROR - _editingProduct is null or has empty ID");
                LoggingService.Instance.Error("[SalesAgentProductsPage.EditImagePickButton_Click] No product ID available for image upload", new Exception("_editingProduct is null or ID is empty"));
                return;
            }

            try
            {
                var picker = new Windows.Storage.Pickers.FileOpenPicker();
                picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
                picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
                picker.FileTypeFilter.Add(".jpg");
                picker.FileTypeFilter.Add(".jpeg");
                picker.FileTypeFilter.Add(".png");
                picker.FileTypeFilter.Add(".gif");
                picker.FileTypeFilter.Add(".bmp");

                // Get window handle from App.MainWindow
                var window = App.MainWindow;
                var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
                WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

                var file = await picker.PickSingleFileAsync();
                if (file != null)
                {
                    System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsPage.EditImagePickButton_Click] File selected: {file.Name}");

                    // Show loading indicator
                    ViewModel.IsLoading = true;

                    try
                    {
                        // Upload image with product ID using new API endpoint: /api/v1/products/{id}/uploadImage
                        System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsPage.EditImagePickButton_Click] Uploading image for product ID: {_editingProduct.Id}");
                        var result = await ViewModel.UploadProductImageAsync(_editingProduct.Id, file.Path);

                        if (result.IsSuccess)
                        {
                            System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsPage.EditImagePickButton_Click] ✅ Image uploaded successfully: {result.Data}");
                            EditImageUrlTextBox.Text = result.Data;
                            System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsPage.EditImagePickButton_Click] Image URL set to textbox");
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsPage.EditImagePickButton_Click] ❌ Image upload failed: {result.ErrorMessage}");
                            // Log error - cannot show dialog while EditProductDialog is open
                            LoggingService.Instance.Error("[SalesAgentProductsPage.EditImagePickButton_Click] Upload failed", new Exception(result.ErrorMessage));
                        }
                    }
                    finally
                    {
                        ViewModel.IsLoading = false;
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsPage.EditImagePickButton_Click] File selection cancelled");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsPage.EditImagePickButton_Click] ❌ EXCEPTION - {ex.GetType().Name}: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsPage.EditImagePickButton_Click] Stack Trace: {ex.StackTrace}");

                // Log error - cannot show dialog while EditProductDialog is open
                LoggingService.Instance.Error("[SalesAgentProductsPage.EditImagePickButton_Click] Exception during image selection", ex);
            }
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

        private async void ImportCsvMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var openPicker = new Windows.Storage.Pickers.FileOpenPicker();
                
                // Initialize with window handle using XamlRoot
                var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
                WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hwnd);

                openPicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
                openPicker.FileTypeFilter.Add(".csv");
                openPicker.FileTypeFilter.Add(".txt");

                var file = await openPicker.PickSingleFileAsync();
                if (file != null)
                {
                    // Call ViewModel method to process the file, passing XamlRoot for dialogs
                    await ViewModel.ProcessImportFileAsync(file, this.XamlRoot);
                }
            }
            catch (Exception ex)
            {
                LoggingService.Instance.Error("[SalesAgentProductsPage] Import CSV failed", ex);
            }
        }
    }
}
