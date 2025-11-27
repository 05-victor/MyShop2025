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

    /// <summary>
    /// Reload when filters change
    /// </summary>
    partial void OnSelectedCategoryChanged(string? value)
    {
        CurrentPage = 1;
        _ = LoadPageAsync();
    }

    partial void OnMinPriceChanged(decimal? value)
    {
        CurrentPage = 1;
        _ = LoadPageAsync();
    }

    partial void OnMaxPriceChanged(decimal? value)
    {
        CurrentPage = 1;
        _ = LoadPageAsync();
    }

    partial void OnSortByChanged(string value)
    {
        _ = LoadPageAsync();
    }

    partial void OnSortDescendingChanged(bool value)
    {
        _ = LoadPageAsync();
    }

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
            var result = await _productFacade.ExportProductsToCsvAsync(
                SearchQuery, SelectedCategory, MinPrice, MaxPrice);

            if (result.IsSuccess)
            {
                await _toastHelper?.ShowSuccess($"Products exported to: {result.Data}");
            }
            else
            {
                await _toastHelper?.ShowError(result.ErrorMessage ?? "Export failed");
            }
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
        // TODO: Open EditProductDialog when created
        await _toastHelper?.ShowInfo("Edit product feature coming soon");
        System.Diagnostics.Debug.WriteLine($"[AdminProductsViewModel] Edit product: {row.Name}");
    }

    [RelayCommand]
    private async Task ViewProductAsync(ProductRow? row)
    {
        if (row is null) return;
        // TODO: Open ProductDetailsDialog when created
        await _toastHelper?.ShowInfo("View product details feature coming soon");
        System.Diagnostics.Debug.WriteLine($"[AdminProductsViewModel] View product: {row.Name}");
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