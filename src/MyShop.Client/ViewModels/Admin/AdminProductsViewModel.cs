using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Client.ViewModels.Base;
using MyShop.Core.Interfaces.Facades;
using MyShop.Core.Interfaces.Services;
using System;
using System.Threading.Tasks;

namespace MyShop.Client.ViewModels.Admin;

/// <summary>
/// ViewModel for Admin Products management with server-side paging
/// Extends PagedViewModelBase to inherit paging logic
/// </summary>
public partial class AdminProductsViewModel : PagedViewModelBase<ProductRow>
{
    private readonly IProductFacade _productFacade;
    private readonly IDialogService _dialogService;

    // Filter properties
    [ObservableProperty]
    private string? _selectedCategory;

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
        IDialogService dialogService)
        : base(toastService, navigationService)
    {
        _productFacade = productFacade;
        _dialogService = dialogService;
    }

    public async Task InitializeAsync()
    {
        await LoadDataAsync();
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
            var result = await _productFacade.LoadProductsAsync(
                searchQuery: SearchQuery,
                categoryName: SelectedCategory,
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

        var confirmResult = await _dialogService.ShowConfirmationAsync(
            "Delete Product",
            $"Are you sure you want to delete '{row.Name}'? This action cannot be undone.");

        if (!confirmResult.IsSuccess || !confirmResult.Data)
            return;

        SetLoadingState(true);
        try
        {
            var deleteResult = await _productFacade.DeleteProductAsync(row.Id);

            if (deleteResult.IsSuccess)
            {
                await _toastHelper?.ShowSuccess($"Product '{row.Name}' deleted successfully");
                Items.Remove(row);
                UpdatePagingInfo(TotalItems - 1);
            }
            else
            {
                await _toastHelper?.ShowError(deleteResult.ErrorMessage ?? "Failed to delete product");
            }
        }
        catch (Exception ex)
        {
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
        SetLoadingState(true);
        try
        {
            // Pass category as both CategoryName and DeviceType to ensure persistence
            var result = await _productFacade.UpdateProductAsync(
                productId, name, sku, description, imageUrl,
                importPrice, sellingPrice, stock,
                category, string.Empty, category, 0, "Active");

            if (result.IsSuccess)
            {
                // Toast is already shown by ProductFacade, no need to show again
                // Reload the list to show updated data
                await LoadPageAsync();
            }
            else
            {
                await _toastHelper?.ShowError(result.ErrorMessage ?? "Failed to update product");
            }
        }
        catch (Exception ex)
        {
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
        // TODO: Open AddProductDialog when created
        await _toastHelper?.ShowInfo("Add product feature coming soon");
        System.Diagnostics.Debug.WriteLine("[AdminProductsViewModel] Add product requested");
    }

    /// <summary>
    /// Alias for LoadDataCommand to maintain backward compatibility with View bindings
    /// </summary>
    public IAsyncRelayCommand LoadProductsCommand => LoadDataCommand;
}

/// <summary>
/// Model dùng cho 1 row trong bảng Products.
/// Mapping từ Product entity ở Shared.
/// </summary>
public class ProductRow
{
    public Guid Id { get; set; }
    public string? Image { get; set; }  // map từ Product.ImageUrl
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