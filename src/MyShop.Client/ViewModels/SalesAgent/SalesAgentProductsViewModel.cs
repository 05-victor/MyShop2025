using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Client.Facades;
using MyShop.Core.Interfaces.Facades;
using MyShop.Core.Interfaces.Services;
using MyShop.Core.Common;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace MyShop.Client.ViewModels.SalesAgent;

public partial class SalesAgentProductsViewModel : ObservableObject
{
    private readonly IProductFacade _productFacade;
    private readonly IToastService? _toastService;
    private List<MyShop.Shared.Models.Product> _allProducts = new();

    [ObservableProperty]
    private ObservableCollection<ProductViewModel> _products;

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    [ObservableProperty]
    private string _selectedCategory = "All Categories";

    [ObservableProperty]
    private string _selectedStockStatus = string.Empty;

    [ObservableProperty]
    private string _sortBy = "name";

    [ObservableProperty]
    private bool _sortDescending = false;

    [ObservableProperty]
    private int _currentPage = 1;

    [ObservableProperty]
    private int _pageSize = PaginationConstants.DefaultPageSize;

    [ObservableProperty]
    private int _totalPages = 1;

    [ObservableProperty]
    private int _totalItems = 0;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private MyShop.Shared.Models.Product? _editingProduct;

    public SalesAgentProductsViewModel(IProductFacade productFacade, IToastService? toastService = null)
    {
        _productFacade = productFacade;
        _toastService = toastService;
        Products = new ObservableCollection<ProductViewModel>();
    }

    public async Task InitializeAsync()
    {
        await LoadProductsAsync();
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        CurrentPage = 1;
        await LoadProductsAsync();
    }

    [RelayCommand]
    private async Task ApplyFiltersAsync()
    {
        CurrentPage = 1;
        ApplyFiltersAndSort();
        await Task.CompletedTask;
    }

    [RelayCommand]
    private async Task ResetFiltersAsync()
    {
        SearchQuery = string.Empty;
        SelectedCategory = "All Categories";
        SelectedStockStatus = string.Empty;
        SortBy = "name";
        SortDescending = false;
        CurrentPage = 1;
        ApplyFiltersAndSort();
        await Task.CompletedTask;
    }

    [RelayCommand]
    private async Task ExportAsync()
    {
        IsLoading = true;
        try
        {
            await _productFacade.ExportProductsToCsvAsync(
                SearchQuery,
                SelectedCategory == "All Categories" ? null : SelectedCategory,
                null,
                null);
        }
        catch (System.Exception ex)
        {
            if (_toastService != null)
                await _toastService.ShowError($"Export failed: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsViewModel] Export error: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task LoadPageAsync()
    {
        await LoadProductsAsync();
    }

    private async Task LoadProductsAsync()
    {
        try
        {
            IsLoading = true;
            System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsViewModel.LoadProductsAsync] START - Page: {CurrentPage}, PageSize: {PageSize}, Search: '{SearchQuery}', Category: '{SelectedCategory}'");

            // Call facade with pagination parameters and filters
            var result = await _productFacade.LoadProductsAsync(
                searchQuery: string.IsNullOrWhiteSpace(SearchQuery) ? null : SearchQuery,
                categoryName: (SelectedCategory == "All Categories" || string.IsNullOrEmpty(SelectedCategory)) ? null : SelectedCategory,
                manufacturerName: null,
                minPrice: null,
                maxPrice: null,
                sortBy: SortBy,
                sortDescending: SortDescending,
                page: CurrentPage,
                pageSize: PageSize);

            if (!result.IsSuccess || result.Data == null)
            {
                System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsViewModel.LoadProductsAsync] ❌ Failed to load products - {result.ErrorMessage}");
                Products = new ObservableCollection<ProductViewModel>();
                _allProducts = new List<MyShop.Shared.Models.Product>();
                TotalItems = 0;
                TotalPages = 1;
                return;
            }

            // Store the current page items (already paged by server)
            var pagedResult = result.Data;
            _allProducts = pagedResult.Items.ToList();
            TotalItems = pagedResult.TotalCount;
            TotalPages = pagedResult.TotalPages;

            System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsViewModel.LoadProductsAsync] API Response:");
            System.Diagnostics.Debug.WriteLine($"  - Items Count: {_allProducts.Count}");
            System.Diagnostics.Debug.WriteLine($"  - TotalItems: {TotalItems}");
            System.Diagnostics.Debug.WriteLine($"  - TotalPages: {TotalPages}");
            System.Diagnostics.Debug.WriteLine($"  - CurrentPage: {CurrentPage}");
            System.Diagnostics.Debug.WriteLine($"  - PageSize: {PageSize}");
            System.Diagnostics.Debug.WriteLine($"  - HasNext: {pagedResult.HasNext}");
            System.Diagnostics.Debug.WriteLine($"  - HasPrevious: {pagedResult.HasPrevious}");
            System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsViewModel.LoadProductsAsync] ✅ SUCCESS - Loaded {_allProducts.Count} products, TotalItems: {TotalItems}, TotalPages: {TotalPages}");

            // Display the products from this page
            DisplayProducts(_allProducts);
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsViewModel.LoadProductsAsync] ❌ Exception: {ex.GetType().Name}");
            System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsViewModel.LoadProductsAsync] Message: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsViewModel.LoadProductsAsync] Stack Trace: {ex.StackTrace}");
            Products = new ObservableCollection<ProductViewModel>();
            _allProducts = new List<MyShop.Shared.Models.Product>();
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void DisplayProducts(List<MyShop.Shared.Models.Product> productsToDisplay)
    {
        var viewModels = productsToDisplay
            .Select(p => new ProductViewModel
            {
                Id = p.Id,
                Name = p.Name,
                Category = p.CategoryName ?? p.Category ?? "Uncategorized",
                Price = p.SellingPrice,
                CommissionRate = (int)(p.CommissionRate * 100),
                Stock = p.Quantity,
                ImageUrl = p.ImageUrl ?? "/Assets/placeholder-product.png"
            })
            .ToList();

        Products.Clear();
        foreach (var product in viewModels)
        {
            Products.Add(product);
        }

        System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsViewModel] Displayed page {CurrentPage}/{TotalPages} ({Products.Count} items, {TotalItems} total)");
    }
    private void ApplyFiltersAndSort()
    {
        // Reset to first page when filters change
        CurrentPage = 1;
    }

    [RelayCommand]
    private async Task SearchAsync(string query)
    {
        SearchQuery = query;
        CurrentPage = 1;
        await LoadProductsAsync();
        System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsViewModel.SearchAsync] Searched for: '{query}'");
    }

    [RelayCommand]
    private async Task FilterCategoryAsync(string category)
    {
        SelectedCategory = category;
        CurrentPage = 1;
        await LoadProductsAsync();
        System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsViewModel.FilterCategoryAsync] Filtered by category: '{category}'");
    }

    #region Pagination Navigation

    [RelayCommand]
    private async Task GoToFirstPageAsync()
    {
        if (CurrentPage != 1)
        {
            CurrentPage = 1;
            await LoadProductsAsync();
            System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsViewModel.GoToFirstPageAsync] Navigated to page 1");
        }
    }

    [RelayCommand]
    private async Task GoToPreviousPageAsync()
    {
        if (CurrentPage > 1)
        {
            CurrentPage--;
            await LoadProductsAsync();
            System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsViewModel.GoToPreviousPageAsync] Navigated to page {CurrentPage}");
        }
    }

    [RelayCommand]
    private async Task GoToNextPageAsync()
    {
        if (CurrentPage < TotalPages)
        {
            CurrentPage++;
            await LoadProductsAsync();
            System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsViewModel.GoToNextPageAsync] Navigated to page {CurrentPage}");
        }
    }

    [RelayCommand]
    private async Task GoToLastPageAsync()
    {
        if (CurrentPage != TotalPages && TotalPages > 0)
        {
            CurrentPage = TotalPages;
            await LoadProductsAsync();
            System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsViewModel.GoToLastPageAsync] Navigated to page {TotalPages}");
        }
    }

    [RelayCommand]
    private async Task GoToPageAsync(int pageNumber)
    {
        if (pageNumber >= 1 && pageNumber <= TotalPages && pageNumber != CurrentPage)
        {
            CurrentPage = pageNumber;
            await LoadProductsAsync();
            System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsViewModel.GoToPageAsync] Navigated to page {CurrentPage}");
        }
    }

    [RelayCommand]
    private async Task ChangePageSizeAsync(int newPageSize)
    {
        if (newPageSize > 0 && newPageSize != PageSize)
        {
            PageSize = newPageSize;
            CurrentPage = 1; // Reset to first page when changing page size
            await LoadProductsAsync();
            System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsViewModel.ChangePageSizeAsync] Page size changed to {PageSize}");
        }
    }

    #endregion

    #region Product Actions (Edit/Delete)

    public event EventHandler<ProductViewModel>? EditProductRequested;
    public event EventHandler<ProductViewModel>? DeleteProductRequested;

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

    public async Task ConfirmDeleteProductAsync(Guid productId)
    {
        IsLoading = true;
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
                await LoadProductsAsync();

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
                await LoadProductsAsync();

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
                await LoadProductsAsync();
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

    #endregion
}

public partial class ProductViewModel : ObservableObject
{
    [ObservableProperty]
    private Guid _id;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _category = string.Empty;

    [ObservableProperty]
    private decimal _price;

    [ObservableProperty]
    private int _commissionRate;

    [ObservableProperty]
    private int _stock;

    [ObservableProperty]
    private string _imageUrl = string.Empty;

    public decimal CommissionAmount => Price * CommissionRate / 100;
}
