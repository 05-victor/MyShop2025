using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Client.Services;
using MyShop.Client.ViewModels.Base;
using MyShop.Core.Interfaces.Facades;
using MyShop.Core.Interfaces.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace MyShop.Client.ViewModels.Admin;

/// <summary>
/// ViewModel for Admin Products management with server-side paging
/// Extends PagedViewModelBase to inherit paging logic
/// </summary>
public partial class AdminProductsViewModel : PagedViewModelBase<ProductRow>
{
    private readonly IProductFacade _productFacade;
    private readonly IDialogService _dialogService;
    private readonly ProductImportService _importService;

    // Category filter
    [ObservableProperty]
    private ObservableCollection<string> _categories = new() { "All Categories" };

    [ObservableProperty]
    private string? _selectedCategory = "All Categories";

    // Filter properties
    [ObservableProperty]
    private decimal? _minPrice;

    [ObservableProperty]
    private decimal? _maxPrice;

    [ObservableProperty]
    private string _sortBy = "name";

    [ObservableProperty]
    private bool _sortDescending = false;

    public AdminProductsViewModel(
        IProductFacade productFacade,
        IToastService toastService,
        INavigationService navigationService,
        IDialogService dialogService,
        ProductImportService importService)
        : base(toastService, navigationService)
    {
        _productFacade = productFacade;
        _dialogService = dialogService;
        _importService = importService;
    }

    public async Task InitializeAsync()
    {
        await LoadCategoriesAsync();
        await LoadDataAsync();
    }

    /// <summary>
    /// Load categories from API for the filter dropdown
    /// </summary>
    private async Task LoadCategoriesAsync()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("[AdminProductsViewModel] LoadCategoriesAsync: Starting category load from API");

            var result = await _productFacade.LoadCategoriesAsync();

            if (!result.IsSuccess || result.Data == null)
            {
                System.Diagnostics.Debug.WriteLine($"[AdminProductsViewModel] LoadCategoriesAsync: Failed to load - {result.ErrorMessage}");
                return;
            }

            // Clear existing items but keep "All Categories" at index 0
            while (Categories.Count > 1)
            {
                Categories.RemoveAt(Categories.Count - 1);
            }

            // Add API categories
            foreach (var category in result.Data)
            {
                Categories.Add(category.Name);
            }

            System.Diagnostics.Debug.WriteLine($"[AdminProductsViewModel] ✅ LoadCategoriesAsync: Loaded {result.Data.Count} categories from API");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AdminProductsViewModel] ❌ LoadCategoriesAsync: ERROR - {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"[AdminProductsViewModel] Stack trace: {ex.StackTrace}");
        }
    }

    // NOTE: Filter changes no longer auto-reload
    // User must click "Apply Filters" button to reduce API calls

    /// <summary>
    /// Override LoadPageAsync to fetch products with server-side paging
    /// </summary>
    protected override async Task LoadPageAsync()
    {
        SetLoadingState(true);
        try
        {
            // Convert "All Categories" to null for API call
            var categoryName = SelectedCategory == "All Categories" ? null : SelectedCategory;

            var result = await _productFacade.LoadProductsAsync(
                searchQuery: SearchQuery,
                categoryName: categoryName,
                minPrice: MinPrice,
                maxPrice: MaxPrice,
                sortBy: SortBy,
                sortDescending: SortDescending,
                page: CurrentPage,
                pageSize: PageSize);

            if (!result.IsSuccess || result.Data == null)
            {
                await _toastHelper?.ShowError(result.ErrorMessage ?? "Failed to load products");
                Items.Clear();
                UpdatePagingInfo(0);
                return;
            }

            var pagedData = result.Data;
            Items.Clear();
            foreach (var product in pagedData.Items)
            {
                Items.Add(ProductRow.FromProduct(product));
            }

            UpdatePagingInfo(pagedData.TotalCount);

            System.Diagnostics.Debug.WriteLine($"[AdminProductsViewModel] Loaded page {CurrentPage}/{TotalPages} ({Items.Count} items, {TotalItems} total)");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AdminProductsViewModel] Error loading products: {ex.Message}");
            await _toastHelper?.ShowError($"Error loading products: {ex.Message}");
            Items.Clear();
            UpdatePagingInfo(0);
        }
        finally
        {
            SetLoadingState(false);
        }
    }

    [RelayCommand]
    private async Task ExportAsync()
    {
        SetLoadingState(true);
        try
        {
            // Facade handles FileSavePicker and toast notifications
            await _productFacade.ExportProductsToCsvAsync(
                SearchQuery, SelectedCategory, MinPrice, MaxPrice);
        }
        catch (Exception ex)
        {
            await _toastHelper?.ShowError($"Export failed: {ex.Message}");
        }
        finally
        {
            SetLoadingState(false);
        }
    }

    [RelayCommand]
    private async Task DeleteProductAsync(ProductRow? row)
    {
        if (row is null) return;

        System.Diagnostics.Debug.WriteLine($"[AdminProductsViewModel] DeleteProductAsync: Product ID={row.Id}, Name={row.Name}");

        var confirmResult = await _dialogService.ShowConfirmationAsync(
            "Delete Product",
            $"Are you sure you want to delete '{row.Name}'? This action cannot be undone.");

        if (!confirmResult.IsSuccess || !confirmResult.Data)
        {
            System.Diagnostics.Debug.WriteLine($"[AdminProductsViewModel] DeleteProductAsync: User cancelled deletion");
            return;
        }

        SetLoadingState(true);
        try
        {
            System.Diagnostics.Debug.WriteLine($"[AdminProductsViewModel] DeleteProductAsync: Calling facade to delete product");
            var deleteResult = await _productFacade.DeleteProductAsync(row.Id);

            if (deleteResult.IsSuccess)
            {
                System.Diagnostics.Debug.WriteLine($"[AdminProductsViewModel] ✅ DeleteProductAsync: Product deleted successfully");
                await _toastHelper?.ShowSuccess($"Product '{row.Name}' deleted successfully");
                Items.Remove(row);
                UpdatePagingInfo(TotalItems - 1);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[AdminProductsViewModel] ❌ DeleteProductAsync: Failed - {deleteResult.ErrorMessage}");
                await _toastHelper?.ShowError(deleteResult.ErrorMessage ?? "Failed to delete product");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AdminProductsViewModel] ❌ DeleteProductAsync: Exception - {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"[AdminProductsViewModel] Stack trace: {ex.StackTrace}");
            await _toastHelper?.ShowError($"Failed to delete product: {ex.Message}");
        }
        finally
        {
            SetLoadingState(false);
        }
    }

    [RelayCommand]
    private async Task EditProductAsync(ProductRow? row)
    {
        if (row is null) return;
        // The View will handle showing the dialog and call UpdateProductAsync
        EditProductRequested?.Invoke(this, row);
    }

    /// <summary>
    /// Event raised when edit dialog should be shown
    /// </summary>
    public event EventHandler<ProductRow>? EditProductRequested;

    /// <summary>
    /// Event raised when view details dialog should be shown
    /// </summary>
    public event EventHandler<ProductRow>? ViewProductRequested;

    /// <summary>
    /// Update a product via the facade
    /// </summary>
    public async Task UpdateProductAsync(
        Guid productId, string name, string sku, string description, string imageUrl,
        decimal importPrice, decimal sellingPrice, int stock, string category)
    {
        System.Diagnostics.Debug.WriteLine($"[AdminProductsViewModel] UpdateProductAsync: ID={productId}, Name={name}, Category={category}");
        System.Diagnostics.Debug.WriteLine($"[AdminProductsViewModel] UpdateProductAsync: Price={sellingPrice}, ImportPrice={importPrice}, Stock={stock}");

        SetLoadingState(true);
        try
        {
            System.Diagnostics.Debug.WriteLine($"[AdminProductsViewModel] UpdateProductAsync: Calling facade to update product");
            // Pass category as both CategoryName and DeviceType to ensure persistence
            var result = await _productFacade.UpdateProductAsync(
                productId, name, sku, description, imageUrl,
                importPrice, sellingPrice, stock,
                category, string.Empty, category, 0, "Active");

            if (result.IsSuccess)
            {
                System.Diagnostics.Debug.WriteLine($"[AdminProductsViewModel] ✅ UpdateProductAsync: Product updated successfully");
                // Toast is already shown by ProductFacade, no need to show again
                // Reload the list to show updated data
                await LoadPageAsync();
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[AdminProductsViewModel] ❌ UpdateProductAsync: Failed - {result.ErrorMessage}");
                await _toastHelper?.ShowError(result.ErrorMessage ?? "Failed to update product");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AdminProductsViewModel] ❌ UpdateProductAsync: Exception - {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"[AdminProductsViewModel] Stack trace: {ex.StackTrace}");
            await _toastHelper?.ShowError($"Failed to update product: {ex.Message}");
        }
        finally
        {
            SetLoadingState(false);
        }
    }

    [RelayCommand]
    private Task ViewProductAsync(ProductRow? row)
    {
        if (row is null) return Task.CompletedTask;

        // Raise event to show View Product Details dialog
        ViewProductRequested?.Invoke(this, row);
        System.Diagnostics.Debug.WriteLine($"[AdminProductsViewModel] View product: {row.Name}");

        return Task.CompletedTask;
    }

    [RelayCommand]
    private async Task AddProductAsync()
    {
        System.Diagnostics.Debug.WriteLine("[AdminProductsViewModel] AddProductAsync: Add product requested");
        // TODO: Open AddProductDialog when created
        await _toastHelper?.ShowInfo("Add product feature coming soon");
    }

    [RelayCommand]
    private async Task ImportProductsAsync()
    {
        System.Diagnostics.Debug.WriteLine("[AdminProductsViewModel] ImportProductsAsync: Starting import");
        
        try
        {
            if (App.MainWindow == null)
            {
                await _toastHelper?.ShowError("Main window not available. Please try again.");
                return;
            }

            Windows.Storage.StorageFile? file = null;

            // Must run FileOpenPicker on UI thread
            var taskCompletionSource = new TaskCompletionSource<Windows.Storage.StorageFile?>();
            
            App.MainWindow.DispatcherQueue.TryEnqueue(async () =>
            {
                try
                {
                    var openPicker = new FileOpenPicker();
                    // Get window handle on UI thread
                    var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
                    WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hwnd);

                    openPicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
                    openPicker.FileTypeFilter.Add(".csv");
                    openPicker.FileTypeFilter.Add(".txt");

                    var result = await openPicker.PickSingleFileAsync();
                    taskCompletionSource.SetResult(result);
                }
                catch (Exception ex)
                {
                    taskCompletionSource.SetException(ex);
                }
            });

            file = await taskCompletionSource.Task;

            if (file == null)
            {
                System.Diagnostics.Debug.WriteLine("[AdminProductsViewModel] ImportProductsAsync: User cancelled file selection");
                return;
            }

            SetLoadingState(true);
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

            // Show confirmation dialog with import summary
            var confirmMsg = $"Found {parseResult.ValidProducts.Count} valid products.\n\n" +
                           $"Total rows: {parseResult.TotalRows}\n" +
                           $"Success: {parseResult.SuccessCount}\n" +
                           $"Failed: {parseResult.FailureCount}\n\n" +
                           (parseResult.Errors.Count > 0 
                               ? $"Errors:\n{string.Join("\n", parseResult.Errors.Take(3))}\n\n" 
                               : "") +
                           "Do you want to import these products?";

            var confirmResult = await _dialogService.ShowConfirmationAsync(
                "Confirm Import",
                confirmMsg);

            if (!confirmResult.IsSuccess || !confirmResult.Data)
            {
                await _toastHelper?.ShowInfo("Import cancelled");
                return;
            }

            // Import products via bulk API (single call)
            var bulkResult = await _productFacade.BulkCreateProductsAsync(parseResult.ValidProducts);

            if (!bulkResult.IsSuccess || bulkResult.Data == null)
            {
                await _toastHelper?.ShowError($"Bulk import failed: {bulkResult.ErrorMessage}");
                return;
            }

            var bulkData = bulkResult.Data;

            // Show result
            await _toastHelper?.ShowSuccess(
                $"Import completed!\n" +
                $"✓ Success: {bulkData.SuccessCount}\n" +
                (bulkData.FailureCount > 0 ? $"✗ Failed: {bulkData.FailureCount}" : ""));

            // Show errors if any
            if (bulkData.Errors.Count > 0)
            {
                System.Diagnostics.Debug.WriteLine($"[Import] Errors:\n{string.Join("\n", bulkData.Errors)}");
            }

            // Reload products list
            await LoadPageAsync();

            System.Diagnostics.Debug.WriteLine($"[AdminProductsViewModel] ✅ Import completed: {bulkData.SuccessCount} success, {bulkData.FailureCount} failed");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AdminProductsViewModel] ❌ ImportProductsAsync: Exception - {ex.Message}");
            await _toastHelper?.ShowError($"Import failed: {ex.Message}");
        }
        finally
        {
            SetLoadingState(false);
        }
    }

    [RelayCommand]
    private async Task DownloadTemplateAsync()
    {
        System.Diagnostics.Debug.WriteLine("[AdminProductsViewModel] DownloadTemplateAsync: Generating template");
        
        try
        {
            var file = await _importService.GenerateSampleCsvAsync();
            
            if (file != null)
            {
                await _toastHelper?.ShowSuccess($"Template saved: {file.Name}");
                System.Diagnostics.Debug.WriteLine($"[AdminProductsViewModel] ✅ Template generated: {file.Path}");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AdminProductsViewModel] ❌ DownloadTemplateAsync: Exception - {ex.Message}");
            await _toastHelper?.ShowError($"Failed to generate template: {ex.Message}");
        }
    }

    /// <summary>
    /// Alias for LoadDataCommand to maintain backward compatibility with View bindings
    /// </summary>
    public IAsyncRelayCommand LoadProductsCommand => LoadDataCommand;
}

/// <summary>
/// Model used for a single row in the Products table.
/// Maps from the Product entity in Shared layer.
/// </summary>
public class ProductRow
{
    public Guid Id { get; set; }
    public string? Image { get; set; }  // mapped from Product.ImageUrl
    public string Name { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;

    public decimal Price { get; set; }        // SellingPrice
    public decimal ImportPrice { get; set; }  // ImportPrice
    public int Stock { get; set; }            // Quantity
    public double Rating { get; set; }

    public static ProductRow FromProduct(MyShop.Shared.Models.Product product)
    {
        return new ProductRow
        {
            Id = product.Id,
            Image = product.ImageUrl,
            Name = product.Name,
            Sku = product.SKU ?? string.Empty,
            Category = product.CategoryName ?? product.Category ?? string.Empty,
            Price = product.SellingPrice,
            ImportPrice = product.ImportPrice,
            Stock = product.Quantity,
            Rating = product.Rating
        };
    }
}