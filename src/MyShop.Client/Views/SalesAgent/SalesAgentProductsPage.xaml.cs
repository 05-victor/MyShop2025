using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using MyShop.Client.ViewModels.SalesAgent;
using MyShop.Client.Services;
using MyShop.Client.Views.Components.Controls;
using MyShop.Client.Common.Helpers;
using MyShop.Client.Common.Converters;
using MyShop.Core.Interfaces.Repositories;
using MyShop.Plugins.API.Forecasts;
using MyShop.Shared.DTOs.Requests;
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
        private readonly IForecastApi _forecastApi;

        public SalesAgentProductsPage()
        {
            this.InitializeComponent();
            ViewModel = App.Current.Services.GetRequiredService<SalesAgentProductsViewModel>();
            this.DataContext = ViewModel;

            // Get auth repository for retrieving current user ID from token
            _authRepository = App.Current.Services.GetRequiredService<IAuthRepository>();
            _forecastApi = App.Current.Services.GetRequiredService<IForecastApi>();

            // Subscribe to edit/delete/predict demand events
            ViewModel.EditProductRequested += ViewModel_EditProductRequested;
            ViewModel.DeleteProductRequested += ViewModel_DeleteProductRequested;
            ViewModel.PredictDemandRequested += ViewModel_PredictDemandRequested;

            Unloaded += SalesAgentProductsPage_Unloaded;
        }

        private void SalesAgentProductsPage_Unloaded(object sender, RoutedEventArgs e)
        {
            ViewModel.EditProductRequested -= ViewModel_EditProductRequested;
            ViewModel.DeleteProductRequested -= ViewModel_DeleteProductRequested;
            ViewModel.PredictDemandRequested -= ViewModel_PredictDemandRequested;
        }

        private async void ViewModel_EditProductRequested(object? sender, ProductViewModel product)
        {
            await ShowEditProductDialogAsync(product);
        }

        private async void ViewModel_DeleteProductRequested(object? sender, ProductViewModel product)
        {
            await ShowDeleteConfirmationAsync(product);
        }

        private async void ViewModel_PredictDemandRequested(object? sender, ProductViewModel product)
        {
            await ShowPredictDemandDialogAsync(product);
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
                double.TryParse(NewCommissionRateTextBox.Text, out var commissionRatePercent);
                // Convert commission rate from percentage (e.g., 5) to decimal (e.g., 0.05)
                var commissionRate = commissionRatePercent / 100.0;
                var description = NewDescriptionTextBox.Text.Trim();
                // Device Type is represented by the Category name from the selected category
                var deviceType = categoryItem?.Name ?? string.Empty;
                var displayStatus = (NewStatusComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Available";
                var status = ConvertDisplayStatusToApiStatus(displayStatus);
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
                var displayStatus = ConvertApiStatusToDisplayStatus(product.Status ?? "AVAILABLE");
                EditStatusComboBox.SelectedItem = displayStatus;
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

                var displayStatus = EditStatusComboBox.SelectedItem as string ?? "Available";
                var status = ConvertDisplayStatusToApiStatus(displayStatus);
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

                    // Reload products and reset to page 1
                    await ViewModel.LoadDataAsync();
                }
            }
            catch (Exception ex)
            {
                LoggingService.Instance.Error("[SalesAgentProductsPage] ShowDeleteConfirmationAsync failed", ex);
            }
        }

        #endregion

        #region Predict Demand Dialog

        private async Task ShowPredictDemandDialogAsync(ProductViewModel product)
        {
            try
            {
                var dialog = new ContentDialog
                {
                    Title = "Predict Demand",
                    DefaultButton = ContentDialogButton.Primary,
                    PrimaryButtonText = "Run",
                    SecondaryButtonText = "Cancel",
                    XamlRoot = this.XamlRoot,
                    Background = Application.Current.Resources["ContentDialogBackground"] as Microsoft.UI.Xaml.Media.Brush,
                    Foreground = Application.Current.Resources["TextFillColorPrimaryBrush"] as Microsoft.UI.Xaml.Media.Brush
                };

                // Create a ScrollViewer with StackPanel for better form layout
                var scrollViewer = new ScrollViewer
                {
                    MinWidth = 450,
                    MaxHeight = 600
                };

                var contentPanel = new StackPanel
                {
                    Spacing = 16,
                    Padding = new Thickness(0, 0, 12, 0)
                };

                // Product Name Section
                var nameLabel = new TextBlock
                {
                    Text = "Product Name",
                    FontSize = 12,
                    Foreground = Application.Current.Resources["TextFillColorSecondaryBrush"] as Microsoft.UI.Xaml.Media.Brush,
                    Margin = new Thickness(0, 0, 0, 4)
                };
                contentPanel.Children.Add(nameLabel);

                var nameValue = new TextBlock
                {
                    Text = product.Name,
                    FontSize = 16,
                    FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                    Foreground = Application.Current.Resources["TextFillColorPrimaryBrush"] as Microsoft.UI.Xaml.Media.Brush,
                    Margin = new Thickness(0, 0, 0, 12),
                    TextWrapping = TextWrapping.Wrap
                };
                contentPanel.Children.Add(nameValue);

                // SKU Section
                var skuLabel = new TextBlock
                {
                    Text = "SKU",
                    FontSize = 12,
                    Foreground = Application.Current.Resources["TextFillColorSecondaryBrush"] as Microsoft.UI.Xaml.Media.Brush,
                    Margin = new Thickness(0, 0, 0, 4)
                };
                contentPanel.Children.Add(skuLabel);

                var skuValue = new TextBlock
                {
                    Text = product.Sku,
                    FontSize = 14,
                    Foreground = Application.Current.Resources["TextFillColorPrimaryBrush"] as Microsoft.UI.Xaml.Media.Brush,
                    Margin = new Thickness(0, 0, 0, 12)
                };
                contentPanel.Children.Add(skuValue);

                // Category Section
                var categoryLabel = new TextBlock
                {
                    Text = "Category",
                    FontSize = 12,
                    Foreground = Application.Current.Resources["TextFillColorSecondaryBrush"] as Microsoft.UI.Xaml.Media.Brush,
                    Margin = new Thickness(0, 0, 0, 4)
                };
                contentPanel.Children.Add(categoryLabel);

                var categoryValue = new TextBlock
                {
                    Text = product.Category,
                    FontSize = 14,
                    Foreground = Application.Current.Resources["TextFillColorPrimaryBrush"] as Microsoft.UI.Xaml.Media.Brush,
                    Margin = new Thickness(0, 0, 0, 12)
                };
                contentPanel.Children.Add(categoryValue);

                // Stock Section
                var stockLabel = new TextBlock
                {
                    Text = "Current Stock",
                    FontSize = 12,
                    Foreground = Application.Current.Resources["TextFillColorSecondaryBrush"] as Microsoft.UI.Xaml.Media.Brush,
                    Margin = new Thickness(0, 0, 0, 4)
                };
                contentPanel.Children.Add(stockLabel);

                var stockValue = new TextBlock
                {
                    Text = product.Stock.ToString(),
                    FontSize = 14,
                    FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                    Foreground = Application.Current.Resources["TextFillColorPrimaryBrush"] as Microsoft.UI.Xaml.Media.Brush,
                    Margin = new Thickness(0, 0, 0, 12)
                };
                contentPanel.Children.Add(stockValue);

                // Price Section
                var priceLabel = new TextBlock
                {
                    Text = "Sale Price",
                    FontSize = 12,
                    Foreground = Application.Current.Resources["TextFillColorSecondaryBrush"] as Microsoft.UI.Xaml.Media.Brush,
                    Margin = new Thickness(0, 0, 0, 4)
                };
                contentPanel.Children.Add(priceLabel);

                // Use CurrencyConverter for consistent formatting
                var currencyConverter = new CurrencyConverter();
                var formattedPrice = currencyConverter.Convert(product.Price, typeof(string), null, null)?.ToString() ?? "0₫";

                var priceValue = new TextBlock
                {
                    Text = formattedPrice,
                    FontSize = 14,
                    FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                    Foreground = Application.Current.Resources["SuccessGreenBrush"] as Microsoft.UI.Xaml.Media.Brush,
                    Margin = new Thickness(0, 0, 0, 20)
                };
                contentPanel.Children.Add(priceValue);

                // Divider
                var divider = new Rectangle
                {
                    Height = 1,
                    Fill = Application.Current.Resources["CardStrokeBrush"] as Microsoft.UI.Xaml.Media.Brush,
                    Margin = new Thickness(0, 0, 0, 16)
                };
                contentPanel.Children.Add(divider);

                // Placeholder message
                var messageTextBlock = new TextBlock
                {
                    Text = "Forecast will be generated based on sales history.",
                    FontSize = 13,
                    Foreground = Application.Current.Resources["TextFillColorTertiaryBrush"] as Microsoft.UI.Xaml.Media.Brush,
                    IsTextSelectionEnabled = false,
                    TextWrapping = TextWrapping.Wrap,
                    LineHeight = 18
                };
                contentPanel.Children.Add(messageTextBlock);

                scrollViewer.Content = contentPanel;
                dialog.Content = scrollViewer;

                var result = await dialog.ShowAsync();

                if (result == ContentDialogResult.Primary)
                {
                    System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsPage] Predict demand started for product: {product.Name} (ID: {product.Id})");
                    await PredictProductDemandAsync(product);
                }
            }
            catch (Exception ex)
            {
                LoggingService.Instance.Error("[SalesAgentProductsPage] ShowPredictDemandDialogAsync failed", ex);
            }
        }

        /// <summary>
        /// Convert SKU string to integer ID using hash function
        /// Same SKU will always produce the same ID
        /// </summary>
        private int GetSkuIdFromString(string sku)
        {
            if (string.IsNullOrWhiteSpace(sku))
                return 0;

            return Math.Abs(sku.GetHashCode());
        }

        /// <summary>
        /// Call Predict My Demand API and show result
        /// </summary>
        private async Task PredictProductDemandAsync(ProductViewModel product)
        {
            try
            {
                // Show loading
                var loadingDialog = new ContentDialog
                {
                    Title = "Analyzing...",
                    Content = new ProgressRing { IsActive = true, Width = 50, Height = 50 },
                    XamlRoot = this.XamlRoot
                };

                _ = loadingDialog.ShowAsync();

                // Convert SKU string to ID using hash
                int skuId = GetSkuIdFromString(product.Sku);

                // Convert price from VND to USD using app constant
                double basePriceInUSD = (double)product.Price / AppConstants.VND_TO_USD_RATE;

                // Prepare request
                var request = new DemandForecastRequest
                {
                    Week = DateTime.Now.ToString("dd/MM/yy"),
                    SkuId = skuId,
                    BasePrice = basePriceInUSD,  // USD price
                    TotalPrice = null,  // Optional
                    IsFeaturedSku = 0,  // Default: not featured
                    IsDisplaySku = 0    // Default: no special display
                };

                System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsPage] Calling API - SKU: {product.Sku}, SkuId: {skuId}, Week: {request.Week}, Price: {product.Price}₫ = {basePriceInUSD:F2}$");

                // Call API
                var apiResponse = await _forecastApi.PredictMyDemandAsync(request);

                // Close loading
                loadingDialog.Hide();

                if (apiResponse.IsSuccessStatusCode && apiResponse.Content?.Success == true)
                {
                    var forecastResult = apiResponse.Content.Result;

                    // Show result dialog
                    var resultDialog = new ContentDialog
                    {
                        Title = "Demand Forecast Result",
                        CloseButtonText = "OK",
                        DefaultButton = ContentDialogButton.Close,
                        XamlRoot = this.XamlRoot
                    };

                    var resultPanel = new StackPanel { Spacing = 16, Padding = new Thickness(0, 0, 12, 0) };

                    // Product info
                    resultPanel.Children.Add(new TextBlock
                    {
                        Text = $"Product: {product.Name}",
                        FontSize = 16,
                        FontWeight = Microsoft.UI.Text.FontWeights.SemiBold
                    });

                    resultPanel.Children.Add(new TextBlock
                    {
                        Text = $"SKU: {product.Sku} (ID: {skuId})",
                        FontSize = 13,
                        Foreground = Application.Current.Resources["TextFillColorSecondaryBrush"] as Microsoft.UI.Xaml.Media.Brush
                    });

                    // Divider
                    resultPanel.Children.Add(new Rectangle
                    {
                        Height = 1,
                        Fill = Application.Current.Resources["CardStrokeBrush"] as Microsoft.UI.Xaml.Media.Brush,
                        Margin = new Thickness(0, 8, 0, 8)
                    });

                    // Prediction result
                    resultPanel.Children.Add(new TextBlock
                    {
                        Text = "Predicted Units Sold:",
                        FontSize = 13,
                        Foreground = Application.Current.Resources["TextFillColorSecondaryBrush"] as Microsoft.UI.Xaml.Media.Brush
                    });

                    resultPanel.Children.Add(new TextBlock
                    {
                        Text = $"{forecastResult.PredictedUnitsSold:F2} units",
                        FontSize = 28,
                        FontWeight = Microsoft.UI.Text.FontWeights.Bold,
                        Foreground = Application.Current.Resources["AccentBlueBrush"] as Microsoft.UI.Xaml.Media.Brush
                    });

                    resultDialog.Content = resultPanel;
                    await resultDialog.ShowAsync();

                    LoggingService.Instance.Information($"Demand prediction successful for {product.Name}: {forecastResult.PredictedUnitsSold:F2} units");
                }
                else
                {
                    // Show error
                    var errorMessage = apiResponse.Content?.Message ?? "Failed to predict demand";
                    await ShowErrorDialogAsync("Prediction Failed", errorMessage);
                    LoggingService.Instance.Error($"Demand prediction failed: {errorMessage}");
                }
            }
            catch (Exception ex)
            {
                LoggingService.Instance.Error("[SalesAgentProductsPage] PredictProductDemandAsync failed", ex);
                await ShowErrorDialogAsync("Error", $"An error occurred: {ex.Message}");
            }
        }

        private async Task ShowErrorDialogAsync(string title, string message)
        {
            var errorDialog = new ContentDialog
            {
                Title = title,
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };
            await errorDialog.ShowAsync();
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

        /// <summary>
        /// Converts display status (Available, Discontinued, Out of Stock) to API format (AVAILABLE, DISCONTINUED, OUT_OF_STOCK)
        /// </summary>
        private string ConvertDisplayStatusToApiStatus(string displayStatus)
        {
            return displayStatus switch
            {
                "Available" => "AVAILABLE",
                "Discontinued" => "DISCONTINUED",
                "Out of Stock" => "OUT_OF_STOCK",
                _ => "AVAILABLE"
            };
        }

        /// <summary>
        /// Converts API status (AVAILABLE, DISCONTINUED, OUT_OF_STOCK) to display format (Available, Discontinued, Out of Stock)
        /// </summary>
        private string ConvertApiStatusToDisplayStatus(string apiStatus)
        {
            return apiStatus switch
            {
                "AVAILABLE" => "Available",
                "DISCONTINUED" => "Discontinued",
                "OUT_OF_STOCK" => "Out of Stock",
                _ => "Available"
            };
        }
    }
}
