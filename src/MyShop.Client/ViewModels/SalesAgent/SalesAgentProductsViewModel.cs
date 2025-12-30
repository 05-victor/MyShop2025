using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Client.Facades;
using MyShop.Client.Services;
using MyShop.Client.ViewModels.Base;
using MyShop.Core.Interfaces.Facades;
using MyShop.Core.Interfaces.Services;
using MyShop.Core.Common;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace MyShop.Client.ViewModels.SalesAgent;

public partial class SalesAgentProductsViewModel : PagedViewModelBase<ProductViewModel>
{
    private readonly IProductFacade _productFacade;
    private readonly ProductImportService _importService;
    private List<MyShop.Shared.Models.Product> _allProducts = new();

    [ObservableProperty]
    private ObservableCollection<MyShop.Shared.Models.Category> _categories = new();

    [ObservableProperty]
    private MyShop.Shared.Models.Category? _selectedCategory;

    [ObservableProperty]
    private ObservableCollection<string> _brands = new() { "All Brands" };

    [ObservableProperty]
    private string _selectedBrand = "All Brands";

    [ObservableProperty]
    private string _selectedStockStatus = string.Empty;

    [ObservableProperty]
    private string _sortBy = "name";

    [ObservableProperty]
    private bool _sortDescending = false;

    [ObservableProperty]
    private MyShop.Shared.Models.Product? _editingProduct;

    // Alias for backward compatibility with XAML bindings
    public ObservableCollection<ProductViewModel> Products => Items;

    public SalesAgentProductsViewModel(
        IProductFacade productFacade,
        ProductImportService importService,
        IToastService? toastService = null)
        : base(toastService, null)
    {
        _productFacade = productFacade;
        _importService = importService;
    }

    public async Task InitializeAsync()
    {
        await LoadCategoriesAsync();
        await LoadBrandsAsync();
        await LoadDataAsync(); // Use base class method
    }

    /// <summary>
    /// Load categories from API for the filter dropdown
    /// </summary>
    private async Task LoadCategoriesAsync()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("[SalesAgentProductsViewModel] LoadCategoriesAsync: Starting category load from API");

            var result = await _productFacade.LoadCategoriesAsync();

            if (!result.IsSuccess || result.Data == null)
            {
                System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsViewModel] LoadCategoriesAsync: Failed to load - {result.ErrorMessage}");
                return;
            }

            // Clear and repopulate with API categories
            Categories.Clear();

            // Add API categories
            foreach (var category in result.Data)
            {
                Categories.Add(category);
            }

            // Reset selected category to null (which means all categories)
            SelectedCategory = null;

            System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsViewModel] ✅ LoadCategoriesAsync: Loaded {result.Data.Count} categories from API");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsViewModel] ❌ LoadCategoriesAsync: ERROR - {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsViewModel] Stack trace: {ex.StackTrace}");
        }
    }

    /// <summary>
    /// Load brands (manufacturers) from API
    /// </summary>
    private async Task LoadBrandsAsync()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("[SalesAgentProductsViewModel] LoadBrandsAsync: Starting brand load from API");

            var result = await _productFacade.LoadBrandsAsync();

            if (!result.IsSuccess || result.Data == null)
            {
                System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsViewModel] LoadBrandsAsync: Failed to load - {result.ErrorMessage}");
                return;
            }

            // Clear existing items but keep "All Brands" at index 0
            while (Brands.Count > 1)
            {
                Brands.RemoveAt(Brands.Count - 1);
            }

            // Add API brands
            foreach (var brand in result.Data)
            {
                Brands.Add(brand);
            }

            System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsViewModel] ✅ LoadBrandsAsync: Loaded {result.Data.Count} brands from API");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsViewModel] ❌ LoadBrandsAsync: ERROR - {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsViewModel] Stack trace: {ex.StackTrace}");
        }
    }

    // NOTE: RefreshAsync is provided by PagedViewModelBase

    [RelayCommand]
    private async Task ApplyFiltersAsync()
    {
        System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsViewModel.ApplyFiltersAsync] Applying filters - Search: '{SearchQuery}', Category: '{SelectedCategory}', Stock: '{SelectedStockStatus}', Sort: '{SortBy}' {(SortDescending ? "DESC" : "ASC")}");
        await LoadDataAsync(); // Use base class method to reset to page 1
    }

    [RelayCommand]
    private async Task ResetFiltersAsync()
    {
        System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsViewModel.ResetFiltersAsync] Resetting all filters");
        SearchQuery = string.Empty;
        SelectedCategory = null;
        SelectedBrand = "All Brands";
        SelectedStockStatus = string.Empty;
        SortBy = "name";
        SortDescending = false;
        await LoadDataAsync(); // Use base class method
    }

    [RelayCommand]
    private async Task ExportAsync()
    {
        SetLoadingState(true);
        try
        {
            await _productFacade.ExportProductsToCsvAsync(
                SearchQuery,
                SelectedCategory?.Name,
                null,
                null);
        }
        catch (System.Exception ex)
        {
            await ShowErrorToast($"Export failed: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsViewModel] Export error: {ex.Message}");
        }
        finally
        {
            SetLoadingState(false);
        }
    }

    [RelayCommand]
    private async Task ImportProductsAsync()
    {
        SetLoadingState(true);
        try
        {
            // TODO: Implement import from CSV functionality
            await ShowErrorToast("Product import feature is not yet implemented.");
            System.Diagnostics.Debug.WriteLine("[SalesAgentProductsViewModel] ImportProductsAsync: Not implemented");
        }
        catch (System.Exception ex)
        {
            await ShowErrorToast($"Import failed: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsViewModel] Import error: {ex.Message}");
        }
        finally
        {
            SetLoadingState(false);
        }
    }

    /// <summary>
    /// Override LoadPageAsync - required by PagedViewModelBase
    /// </summary>
    protected override async Task LoadPageAsync()
    {
        try
        {
            SetLoadingState(true);
            System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsViewModel.LoadPageAsync] START - Page: {CurrentPage}, PageSize: {PageSize}, Search: '{SearchQuery}', Category: '{SelectedCategory?.Name}', Brand: '{SelectedBrand}', StockStatus: '{SelectedStockStatus}'");

            // Call facade with pagination parameters and filters
            var result = await _productFacade.LoadProductsAsync(
                searchQuery: string.IsNullOrWhiteSpace(SearchQuery) ? null : SearchQuery,
                categoryName: SelectedCategory?.Name,
                manufacturerName: null,
                brandName: SelectedBrand != "All Brands" ? SelectedBrand : null,
                minPrice: null,
                maxPrice: null,
                stockStatus: string.IsNullOrWhiteSpace(SelectedStockStatus) ? null : SelectedStockStatus,
                sortBy: SortBy,
                sortDescending: SortDescending,
                page: CurrentPage,
                pageSize: PageSize);

            if (!result.IsSuccess || result.Data == null)
            {
                System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsViewModel.LoadPageAsync] ❌ Failed to load products - {result.ErrorMessage}");
                Items.Clear();
                _allProducts = new List<MyShop.Shared.Models.Product>();
                UpdatePagingInfo(0);
                return;
            }

            // Store the current page items (already paged by server)
            var pagedResult = result.Data;
            _allProducts = pagedResult.Items.ToList();

            System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsViewModel.LoadPageAsync] API Response:");
            System.Diagnostics.Debug.WriteLine($"  - Items Count: {_allProducts.Count}");
            System.Diagnostics.Debug.WriteLine($"  - TotalItems: {pagedResult.TotalCount}");
            System.Diagnostics.Debug.WriteLine($"  - TotalPages: {pagedResult.TotalPages}");
            System.Diagnostics.Debug.WriteLine($"  - CurrentPage: {CurrentPage}");
            System.Diagnostics.Debug.WriteLine($"  - PageSize: {PageSize}");
            System.Diagnostics.Debug.WriteLine($"  - HasNext: {pagedResult.HasNext}");
            System.Diagnostics.Debug.WriteLine($"  - HasPrevious: {pagedResult.HasPrevious}");

            // Update paging info (this handles TotalItems, TotalPages, and property notifications)
            UpdatePagingInfo(pagedResult.TotalCount);

            System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsViewModel.LoadPageAsync] ✅ SUCCESS - Loaded {_allProducts.Count} products, TotalItems: {TotalItems}, TotalPages: {TotalPages}");

            // Display the products from this page
            DisplayProducts(_allProducts);
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsViewModel.LoadPageAsync] ❌ Exception: {ex.GetType().Name}");
            System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsViewModel.LoadPageAsync] Message: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsViewModel.LoadPageAsync] Stack Trace: {ex.StackTrace}");
            Items.Clear();
            _allProducts = new List<MyShop.Shared.Models.Product>();
        }
        finally
        {
            SetLoadingState(false);
        }
    }

    private void DisplayProducts(List<MyShop.Shared.Models.Product> productsToDisplay)
    {
        var viewModels = productsToDisplay
            .Select(p => new ProductViewModel
            {
                Id = p.Id,
                Name = p.Name,
                Sku = p.SKU ?? string.Empty,
                Manufacturer = p.Manufacturer ?? string.Empty,
                Category = p.CategoryName ?? p.Category ?? "Uncategorized",
                Price = p.SellingPrice,
                ImportPrice = p.ImportPrice,
                CommissionRate = (int)(p.CommissionRate * 100),
                Stock = p.Quantity,
                Rating = p.Rating,
                Status = p.Status ?? "AVAILABLE",
                ImageUrl = string.IsNullOrWhiteSpace(p.ImageUrl) ? "ms-appx:///Assets/Images/products/product-placeholder.png" : p.ImageUrl,
                Description = p.Description ?? string.Empty
            })
            .ToList();

        Items.Clear();
        foreach (var product in viewModels)
        {
            Items.Add(product);
        }

        System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsViewModel] Displayed page {CurrentPage}/{TotalPages} ({Items.Count} items, {TotalItems} total)");
    }
    private void ApplyFiltersAndSort()
    {
        // Reset to first page when filters change
        CurrentPage = 1;
    }

    [RelayCommand]
    private async Task PerformSearchAsync(string query)
    {
        SearchQuery = query;
        await LoadDataAsync(); // Use base class method to reset to page 1
        System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsViewModel.PerformSearchAsync] Searched for: '{query}'");
    }

    [RelayCommand]
    private async Task FilterCategoryAsync(string categoryName)
    {
        // Find the category by name
        var selectedCat = Categories.FirstOrDefault(c => c.Name == categoryName);
        SelectedCategory = selectedCat;
        await LoadDataAsync(); // Use base class method to reset to page 1
        System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsViewModel.FilterCategoryAsync] Filtered by category: '{categoryName}'");
    }

    #region Pagination Navigation

    [RelayCommand]
    private async Task GoToFirstPageAsync()
    {
        if (CurrentPage != 1)
        {
            await LoadDataAsync(); // Use base class method to reset to page 1
            System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsViewModel.GoToFirstPageAsync] Navigated to page 1");
        }
    }

    [RelayCommand]
    private async Task GoToLastPageAsync()
    {
        if (CurrentPage != TotalPages && TotalPages > 0)
        {
            CurrentPage = TotalPages;
            await LoadPageAsync();
            System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsViewModel.GoToLastPageAsync] Navigated to page {TotalPages}");
        }
    }

    // NOTE: GoToPageAsync is provided by PagedViewModelBase

    [RelayCommand]
    private async Task ChangePageSizeAsync(int newPageSize)
    {
        if (newPageSize > 0 && newPageSize != PageSize)
        {
            PageSize = newPageSize;
            await LoadDataAsync(); // Use base class method to reset to page 1
            System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsViewModel.ChangePageSizeAsync] Page size changed to {PageSize}");
        }
    }

    #endregion

    #region Product Actions (Edit/Delete)

    public event EventHandler<ProductViewModel>? EditProductRequested;
    public event EventHandler<ProductViewModel>? DeleteProductRequested;
    public event EventHandler<ProductViewModel>? PredictDemandRequested;

    [RelayCommand]
    private void EditProduct(ProductViewModel? product)
    {
        if (product == null) return;
        EditProductRequested?.Invoke(this, product);
    }

    [RelayCommand]
    private async Task DeleteProductAsync(ProductViewModel? product)
    {
        if (product == null) return;

        // Raise event for UI to show confirmation dialog
        DeleteProductRequested?.Invoke(this, product);
    }

    [RelayCommand]
    private void PredictDemand(ProductViewModel? product)
    {
        if (product == null) return;
        PredictDemandRequested?.Invoke(this, product);
    }

    public async Task ConfirmDeleteProductAsync(Guid productId)
    {
        SetLoadingState(true);
        try
        {
            var result = await _productFacade.DeleteProductAsync(productId);
            if (result.IsSuccess)
            {
                // Remove from local list and refresh
                _allProducts.RemoveAll(p => p.Id == productId);
                ApplyFiltersAndSort();
                System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsViewModel] Product deleted: {productId}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsViewModel] Delete failed: {result.ErrorMessage}");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsViewModel] Delete error: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Command to save a new product.
    /// </summary>
    [RelayCommand]
    public async Task SaveNewProductAsync(MyShop.Shared.Models.Product product)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsViewModel.SaveNewProductAsync] START - Product: {product?.Name}");

            // Validate required fields
            if (string.IsNullOrWhiteSpace(product?.Name))
            {
                System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsViewModel.SaveNewProductAsync] ❌ VALIDATION FAILED - Product name is required");
                // Error will be shown in dialog
                return;
            }

            if (product.CategoryId == Guid.Empty)
            {
                System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsViewModel.SaveNewProductAsync] ❌ VALIDATION FAILED - Category is required");
                return;
            }

            IsLoading = true;
            System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsViewModel.SaveNewProductAsync] Validation passed, calling ProductFacade.CreateProductAsync()");

            var result = await _productFacade.CreateProductAsync(product);

            stopwatch.Stop();
            System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsViewModel.SaveNewProductAsync] API Response received in {stopwatch.ElapsedMilliseconds}ms");

            if (result.IsSuccess)
            {
                System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsViewModel.SaveNewProductAsync] ✅ SUCCESS - Product created, refreshing product list");

                // Refresh the product list
                await LoadPageAsync();

                // Re-apply filters if any are active
                ApplyFiltersAndSort();

                System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsViewModel.SaveNewProductAsync] ✅ Product list refreshed, new product should be visible");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsViewModel.SaveNewProductAsync] ❌ FAILED - {result.ErrorMessage}");
            }
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsViewModel.SaveNewProductAsync] ❌ EXCEPTION - {ex.GetType().Name}: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsViewModel.SaveNewProductAsync] Stack Trace: {ex.StackTrace}");
        }
        finally
        {
            IsLoading = false;
            System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsViewModel.SaveNewProductAsync] END - IsLoading set to false");
        }
    }

    /// <summary>
    /// Upload product image to server (for new products without ID)
    /// </summary>
    public async Task<Core.Common.Result<string>> UploadProductImageAsync(Windows.Storage.StorageFile imageFile)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsViewModel.UploadProductImageAsync] START - File: {imageFile.Name}");

            var result = await _productFacade.UploadProductImageForNewProductAsync(imageFile.Path);

            if (result.IsSuccess)
            {
                System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsViewModel.UploadProductImageAsync] ✅ Image uploaded successfully: {result.Data}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsViewModel.UploadProductImageAsync] ❌ Upload failed: {result.ErrorMessage}");
            }

            return result;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsViewModel.UploadProductImageAsync] ❌ EXCEPTION: {ex.Message}");
            return Core.Common.Result<string>.Failure("Failed to upload image", ex);
        }
    }

    /// <summary>
    /// Upload product image to server (for existing products with ID)
    /// Uses the new endpoint: POST /api/v1/products/{id}/uploadImage
    /// </summary>
    public async Task<Core.Common.Result<string>> UploadProductImageAsync(Guid productId, string imageFilePath)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsViewModel.UploadProductImageAsync] START - ProductId: {productId}, File: {imageFilePath}");

            var result = await _productFacade.UploadProductImageAsync(productId, imageFilePath);

            if (result.IsSuccess)
            {
                System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsViewModel.UploadProductImageAsync] ✅ Image uploaded successfully: {result.Data}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsViewModel.UploadProductImageAsync] ❌ Upload failed: {result.ErrorMessage}");
            }

            return result;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsViewModel.UploadProductImageAsync] ❌ EXCEPTION: {ex.Message}");
            return Core.Common.Result<string>.Failure("Failed to upload image", ex);
        }
    }

    /// <summary>
    /// Command to update an existing product.
    /// </summary>
    [RelayCommand]
    public async Task SaveEditProductAsync(MyShop.Shared.Models.Product product)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsViewModel.SaveEditProductAsync] START - Product ID: {product?.Id}, Name: {product?.Name}");

            // Validate required fields
            if (string.IsNullOrWhiteSpace(product?.Name))
            {
                System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsViewModel.SaveEditProductAsync] ❌ VALIDATION FAILED - Product name is required");
                return;
            }

            if (product.CategoryId == Guid.Empty)
            {
                System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsViewModel.SaveEditProductAsync] ❌ VALIDATION FAILED - Category is required");
                return;
            }

            IsLoading = true;
            System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsViewModel.SaveEditProductAsync] Validation passed, calling ProductFacade.UpdateProductAsync()");

            var result = await _productFacade.UpdateProductAsync(product.Id, product);

            stopwatch.Stop();
            System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsViewModel.SaveEditProductAsync] API Response received in {stopwatch.ElapsedMilliseconds}ms");

            if (result.IsSuccess)
            {
                System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsViewModel.SaveEditProductAsync] ✅ SUCCESS - Product updated, refreshing product list");

                // Refresh the product list
                await LoadPageAsync();

                // Re-apply filters if any are active
                ApplyFiltersAndSort();

                EditingProduct = null;
                System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsViewModel.SaveEditProductAsync] ✅ Product list refreshed, EditingProduct cleared");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsViewModel.SaveEditProductAsync] ❌ FAILED - {result.ErrorMessage}");
            }
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsViewModel.SaveEditProductAsync] ❌ EXCEPTION - {ex.GetType().Name}: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsViewModel.SaveEditProductAsync] Stack Trace: {ex.StackTrace}");
        }
        finally
        {
            IsLoading = false;
            System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsViewModel.SaveEditProductAsync] END - IsLoading set to false");
        }
    }

    /// <summary>
    /// Command to search for products.
    /// </summary>
    [RelayCommand]
    public async Task SearchProductsAsync(string searchQuery)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsViewModel.SearchProductsAsync] START - Query: '{searchQuery}'");

            if (string.IsNullOrWhiteSpace(searchQuery))
            {
                System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsViewModel.SearchProductsAsync] ❌ VALIDATION FAILED - Query is empty");
                return;
            }

            IsLoading = true;
            CurrentPage = 1;
            System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsViewModel.SearchProductsAsync] Validation passed, calling ProductFacade.SearchProductsAsync()");

            var result = await _productFacade.SearchProductsAsync(searchQuery);

            stopwatch.Stop();
            System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsViewModel.SearchProductsAsync] API Response received in {stopwatch.ElapsedMilliseconds}ms");

            if (result.IsSuccess && result.Data != null)
            {
                System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsViewModel.SearchProductsAsync] ✅ SUCCESS - Found {result.Data.Count} products");

                // Update _allProducts with search results
                _allProducts = result.Data;

                // Apply filters and sort to display results
                ApplyFiltersAndSort();

                System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsViewModel.SearchProductsAsync] ✅ Search results displayed");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsViewModel.SearchProductsAsync] ❌ FAILED - {result.ErrorMessage}");
                _allProducts.Clear();
                ApplyFiltersAndSort();
            }
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsViewModel.SearchProductsAsync] ❌ EXCEPTION - {ex.GetType().Name}: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsViewModel.SearchProductsAsync] Stack Trace: {ex.StackTrace}");
        }
        finally
        {
            IsLoading = false;
            System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsViewModel.SearchProductsAsync] END - IsLoading set to false");
        }
    }

    /// <summary>
    /// Command to upload product image.
    /// </summary>
    [RelayCommand]
    public async Task UploadProductImageAsync(Guid productId)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        try
        {
            System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsViewModel.UploadProductImageAsync] START - Product ID: {productId}");

            IsLoading = true;
            System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsViewModel.UploadProductImageAsync] Opening file picker");

            // Open file picker
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");
            picker.FileTypeFilter.Add(".gif");
            picker.FileTypeFilter.Add(".webp");

            // Set window handle for file picker
            var window = Microsoft.UI.Xaml.Window.Current;
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

            var file = await picker.PickSingleFileAsync();

            if (file == null)
            {
                System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsViewModel.UploadProductImageAsync] User cancelled file selection");
                return;
            }

            System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsViewModel.UploadProductImageAsync] File selected: {file.Name}");

            var result = await _productFacade.UploadProductImageAsync(productId, file.Path);

            stopwatch.Stop();
            System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsViewModel.UploadProductImageAsync] API Response received in {stopwatch.ElapsedMilliseconds}ms");

            if (result.IsSuccess)
            {
                System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsViewModel.UploadProductImageAsync] ✅ SUCCESS - Image uploaded");

                // Refresh product to get updated image URL
                await LoadPageAsync();
                ApplyFiltersAndSort();

                System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsViewModel.UploadProductImageAsync] ✅ Product list refreshed with new image");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsViewModel.UploadProductImageAsync] ❌ FAILED - {result.ErrorMessage}");
            }
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsViewModel.UploadProductImageAsync] ❌ EXCEPTION - {ex.GetType().Name}: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsViewModel.UploadProductImageAsync] Stack Trace: {ex.StackTrace}");
        }
        finally
        {
            IsLoading = false;
            System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsViewModel.UploadProductImageAsync] END - IsLoading set to false");
        }
    }

    /// <summary>
    /// Process imported CSV file (called from Page code-behind after file picker)
    /// </summary>
    public async Task ProcessImportFileAsync(Windows.Storage.StorageFile file, Microsoft.UI.Xaml.XamlRoot xamlRoot)
    {
        System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsViewModel] ProcessImportFileAsync: {file.Name}");

        try
        {
            IsLoading = true;
            await _toastHelper?.ShowInfo($"Parsing file: {file.Name}...");

            // Parse CSV file
            var parseResult = await _importService.ParseCsvAsync(file);

            if (!parseResult.IsSuccess || parseResult.ValidProducts.Count == 0)
            {
                var errorMsg = parseResult.Errors.Count > 0
                    ? string.Join("\n", parseResult.Errors.Take(5))
                    : "No valid products found in file";

                await _toastHelper?.ShowError(
                    $"Import failed:\n{errorMsg}\n\n" +
                    $"Total: {parseResult.TotalRows}, Failed: {parseResult.FailureCount}");

                return;
            }

            // Show confirmation using ContentDialog (WinUI 3)
            var confirmMsg = $"Found {parseResult.ValidProducts.Count} valid products.\n\n" +
                           $"Total: {parseResult.TotalRows}\n" +
                           $"Success: {parseResult.SuccessCount}\n" +
                           $"Failed: {parseResult.FailureCount}\n\n" +
                           "Import these products?";

            var dialog = new Microsoft.UI.Xaml.Controls.ContentDialog
            {
                Title = "Confirm Import",
                Content = confirmMsg,
                PrimaryButtonText = "Yes",
                CloseButtonText = "No",
                DefaultButton = Microsoft.UI.Xaml.Controls.ContentDialogButton.Primary,
                XamlRoot = xamlRoot
            };

            var result = await dialog.ShowAsync();
            if (result != Microsoft.UI.Xaml.Controls.ContentDialogResult.Primary)
            {
                await _toastHelper?.ShowInfo("Import cancelled");
                return;
            }

            // Import products via bulk API
            var bulkResult = await _productFacade.BulkCreateProductsAsync(parseResult.ValidProducts);

            if (!bulkResult.IsSuccess || bulkResult.Data == null)
            {
                await _toastHelper?.ShowError($"Bulk import failed: {bulkResult.ErrorMessage}");
                return;
            }

            var bulkData = bulkResult.Data;

            // Show result
            await _toastHelper?.ShowSuccess(
                $"Import completed!\n✓ Success: {bulkData.SuccessCount}" +
                (bulkData.FailureCount > 0 ? $"\n✗ Failed: {bulkData.FailureCount}" : ""));

            // Reload
            await LoadPageAsync();
            ApplyFiltersAndSort();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsViewModel] ❌ ProcessImportFileAsync: {ex.Message}");
            await _toastHelper?.ShowError($"Import failed: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task DownloadTemplateAsync()
    {
        try
        {
            var file = await _importService.GenerateSampleCsvAsync();

            if (file != null)
            {
                await _toastHelper?.ShowSuccess($"Template saved: {file.Name}");
            }
        }
        catch (Exception ex)
        {
            await _toastHelper?.ShowError($"Failed to generate template: {ex.Message}");
        }
    }

    #endregion
}

public partial class ProductViewModel : ObservableObject
{
    [ObservableProperty]
    private Guid _id;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _sku = string.Empty;

    [ObservableProperty]
    private string _manufacturer = string.Empty;

    [ObservableProperty]
    private string _category = string.Empty;

    [ObservableProperty]
    private decimal _price;

    [ObservableProperty]
    private decimal _importPrice;

    [ObservableProperty]
    private int _commissionRate;

    [ObservableProperty]
    private int _stock;

    [ObservableProperty]
    private double _rating;

    [ObservableProperty]
    private string _status = string.Empty;

    [ObservableProperty]
    private string _imageUrl = string.Empty;

    [ObservableProperty]
    private string _description = string.Empty;

    /// <summary>
    /// Converts ImageUrl string to safe Uri for XAML binding.
    /// Returns fallback placeholder if ImageUrl is null/empty/invalid.
    /// </summary>
    public Uri ImageUri
    {
        get
        {
            if (string.IsNullOrWhiteSpace(ImageUrl))
            {
                return new Uri("ms-appx:///Assets/placeholder-product.png");
            }

            // Try to parse as absolute URI
            if (Uri.TryCreate(ImageUrl, UriKind.Absolute, out var uri))
            {
                return uri;
            }

            // If it's a relative path like "/Assets/...", convert to ms-appx:/// URI
            if (Uri.TryCreate($"ms-appx:///{ImageUrl.TrimStart('/')}", UriKind.Absolute, out var relativeUri))
            {
                return relativeUri;
            }

            // Fallback to placeholder
            System.Diagnostics.Debug.WriteLine($"[ProductViewModel] Invalid ImageUrl for product '{Name}' (ID: {Id}): '{ImageUrl}'");
            return new Uri("ms-appx:///Assets/placeholder-product.png");
        }
    }

    public decimal CommissionAmount => Price * CommissionRate / 100;
}
