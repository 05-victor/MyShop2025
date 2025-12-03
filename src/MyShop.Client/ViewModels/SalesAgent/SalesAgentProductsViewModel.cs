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
        ApplyFiltersAndSort();
        await Task.CompletedTask;
    }

    private async Task LoadProductsAsync()
    {
        try
        {
            IsLoading = true;
            var result = await _productFacade.LoadProductsAsync();
            if (!result.IsSuccess || result.Data == null)
            {
                Products = new ObservableCollection<ProductViewModel>();
                _allProducts = new List<MyShop.Shared.Models.Product>();
                TotalItems = 0;
                TotalPages = 1;
                return;
            }
            
            _allProducts = result.Data.Items.ToList();
            TotalItems = _allProducts.Count;
            ApplyFiltersAndSort();
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsViewModel] Error loading products: {ex.Message}");
            Products = new ObservableCollection<ProductViewModel>();
            _allProducts = new List<MyShop.Shared.Models.Product>();
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void ApplyFiltersAndSort()
    {
        var filtered = _allProducts.AsEnumerable();

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(SearchQuery))
        {
            filtered = filtered.Where(p => 
                p.Name.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase) ||
                (p.Category?.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase) ?? false));
        }

        // Apply category filter
        if (!string.IsNullOrEmpty(SelectedCategory) && SelectedCategory != "All Categories")
        {
            filtered = filtered.Where(p => p.Category == SelectedCategory || p.CategoryName == SelectedCategory);
        }

        // Apply stock status filter
        if (!string.IsNullOrEmpty(SelectedStockStatus))
        {
            filtered = SelectedStockStatus switch
            {
                "InStock" => filtered.Where(p => p.Quantity > 10),
                "LowStock" => filtered.Where(p => p.Quantity > 0 && p.Quantity <= 10),
                "OutOfStock" => filtered.Where(p => p.Quantity == 0),
                _ => filtered
            };
        }

        // Apply sorting
        filtered = (SortBy, SortDescending) switch
        {
            ("name", false) => filtered.OrderBy(p => p.Name),
            ("name", true) => filtered.OrderByDescending(p => p.Name),
            ("price", false) => filtered.OrderBy(p => p.SellingPrice),
            ("price", true) => filtered.OrderByDescending(p => p.SellingPrice),
            ("stock", false) => filtered.OrderBy(p => p.Quantity),
            ("stock", true) => filtered.OrderByDescending(p => p.Quantity),
            _ => filtered.OrderBy(p => p.Name)
        };

        var filteredList = filtered.ToList();
        TotalItems = filteredList.Count;
        TotalPages = (int)Math.Ceiling((double)TotalItems / PageSize);

        // Apply pagination
        var pagedProducts = filteredList
            .Skip((CurrentPage - 1) * PageSize)
            .Take(PageSize)
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
        foreach (var product in pagedProducts)
        {
            Products.Add(product);
        }

        System.Diagnostics.Debug.WriteLine($"[SalesAgentProductsViewModel] Loaded page {CurrentPage}/{TotalPages} ({Products.Count} items, {TotalItems} total)");
    }

    [RelayCommand]
    private void Search(string query)
    {
        SearchQuery = query;
        CurrentPage = 1;
        ApplyFiltersAndSort();
    }

    [RelayCommand]
    private void FilterCategory(string category)
    {
        SelectedCategory = category;
        CurrentPage = 1;
        ApplyFiltersAndSort();
    }

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
