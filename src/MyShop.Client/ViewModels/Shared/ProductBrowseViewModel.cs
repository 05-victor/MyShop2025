using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using MyShop.Client.Facades;
using MyShop.Core.Interfaces.Facades;
using MyShop.Core.Interfaces.Services;
using MyShop.Shared.Models;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace MyShop.Client.ViewModels.Shared;

/// <summary>
/// ViewModel for Product Browse page with SERVER-SIDE paging
/// Loads products page by page from the API, not all at once
/// </summary>
public partial class ProductBrowseViewModel : ObservableObject
{
    private readonly IProductFacade _productFacade;
    private readonly ICartFacade _cartFacade;
    private readonly INavigationService? _navigationService;

    [ObservableProperty]
    private ObservableCollection<ProductCardViewModel> _products = new();

    [ObservableProperty]
    private ObservableCollection<string> _categories = new() { "All" };

    [ObservableProperty]
    private string _selectedCategory = "All";

    [ObservableProperty]
    private ObservableCollection<string> _brands = new() { "All Brands" };

    [ObservableProperty]
    private string _selectedBrand = "All Brands";

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    [ObservableProperty]
    private string _selectedSort = "Newest";

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasPreviousPage))]
    [NotifyPropertyChangedFor(nameof(HasNextPage))]
    [NotifyPropertyChangedFor(nameof(PageInfoText))]
    private int _currentPage = 1;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowPagination))]
    [NotifyPropertyChangedFor(nameof(HasNextPage))]
    [NotifyPropertyChangedFor(nameof(PageInfoText))]
    private int _totalPages = 1;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ItemsInfoText))]
    private int _totalProducts;

    [ObservableProperty]
    private bool _isLoadingMore;

    [ObservableProperty]
    private bool _hasMoreItems = true;

    // Computed properties for pagination
    public bool HasPreviousPage => CurrentPage > 1;
    public bool HasNextPage => CurrentPage < TotalPages;
    public Visibility ShowPagination => TotalPages > 1 ? Visibility.Visible : Visibility.Collapsed;
    public string PageInfoText => TotalPages > 0 ? $"Page {CurrentPage} of {TotalPages}" : "No data";

    public string ItemsInfoText
    {
        get
        {
            if (TotalProducts == 0) return "No products found";
            var startIndex = (CurrentPage - 1) * PageSize + 1;
            var endIndex = Math.Min(CurrentPage * PageSize, TotalProducts);
            return $"Showing {startIndex}-{endIndex} of {TotalProducts} products";
        }
    }

    private const int PageSize = 12; // Show 12 products per page for grid layout

    public ProductBrowseViewModel(
        IProductFacade productFacade,
        ICartFacade cartFacade,
        INavigationService? navigationService = null)
    {
        _productFacade = productFacade;
        _cartFacade = cartFacade;
        _navigationService = navigationService;
    }

    public async Task InitializeAsync()
    {
        await LoadCategoriesAsync();
        await LoadBrandsAsync();
        await LoadPageAsync();
    }

    private async Task LoadCategoriesAsync()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("[ProductBrowseViewModel] LoadCategoriesAsync: Starting category load from API");

            // Call facade to load categories from API
            var result = await _productFacade.LoadCategoriesAsync();

            if (!result.IsSuccess || result.Data == null)
            {
                System.Diagnostics.Debug.WriteLine($"[ProductBrowseViewModel] LoadCategoriesAsync: Failed to load - {result.ErrorMessage}");
                // Fallback: Keep "All" only if API fails
                return;
            }

            // Clear existing items but keep "All" at index 0
            // This preserves the SelectedItem binding
            while (Categories.Count > 1)
            {
                Categories.RemoveAt(Categories.Count - 1);
            }

            // Add API categories
            foreach (var category in result.Data)
            {
                Categories.Add(category.Name);
            }

            System.Diagnostics.Debug.WriteLine($"[ProductBrowseViewModel] ✅ LoadCategoriesAsync: Loaded {result.Data.Count} categories from API");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ProductBrowseViewModel] ❌ LoadCategoriesAsync: ERROR - {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"[ProductBrowseViewModel] Stack trace: {ex.StackTrace}");
            // Keep "All" if exception occurs
        }
    }

    /// <summary>
    /// Load brands (manufacturers) from server
    /// </summary>
    private async Task LoadBrandsAsync()
    {
        System.Diagnostics.Debug.WriteLine($"[ProductBrowseViewModel] LoadBrandsAsync: Starting brand load from API");
        try
        {
            // Call facade to load brands from API
            var result = await _productFacade.LoadBrandsAsync();

            if (!result.IsSuccess || result.Data == null)
            {
                System.Diagnostics.Debug.WriteLine($"[ProductBrowseViewModel] LoadBrandsAsync: Failed to load - {result.ErrorMessage}");
                // Fallback: Keep "All Brands" only if API fails
                return;
            }

            // Clear existing items but keep "All Brands" at index 0
            // This preserves the SelectedItem binding
            while (Brands.Count > 1)
            {
                Brands.RemoveAt(Brands.Count - 1);
            }

            // Add API brands
            foreach (var brand in result.Data)
            {
                Brands.Add(brand);
            }

            System.Diagnostics.Debug.WriteLine($"[ProductBrowseViewModel] ✅ LoadBrandsAsync: Loaded {result.Data.Count} brands from API");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ProductBrowseViewModel] ❌ LoadBrandsAsync: ERROR - {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"[ProductBrowseViewModel] Stack trace: {ex.StackTrace}");
            // Keep "All Brands" if exception occurs
        }
    }

    /// <summary>
    /// Load products from server with current filters and page
    /// </summary>
    private async Task LoadPageAsync()
    {
        IsLoading = true;
        try
        {
            // Map sort option to API parameter
            var (sortBy, sortDesc) = MapSortOption(SelectedSort);

            // Map category - "All" means no filter
            var category = SelectedCategory == "All" ? null : SelectedCategory;

            // Map brand/manufacturer - "All Brands" means no filter
            var manufacturer = SelectedBrand == "All Brands" ? null : SelectedBrand;

            System.Diagnostics.Debug.WriteLine($"[ProductBrowseViewModel] LoadPageAsync: Page={CurrentPage}, Category={category}, Brand={manufacturer}, Sort={sortBy}, Search={SearchQuery}");

            var result = await _productFacade.LoadProductsAsync(
                searchQuery: string.IsNullOrWhiteSpace(SearchQuery) ? null : SearchQuery,
                categoryName: category,
                manufacturerName: manufacturer,
                minPrice: null,
                maxPrice: null,
                sortBy: sortBy,
                sortDescending: sortDesc,
                page: CurrentPage,
                pageSize: PageSize);

            if (result.IsSuccess && result.Data != null)
            {
                var pagedData = result.Data;

                Products.Clear();
                foreach (var product in pagedData.Items)
                {
                    Products.Add(new ProductCardViewModel
                    {
                        Id = product.Id,
                        Name = product.Name,
                        Price = product.SellingPrice,
                        ImageUrl = product.ImageUrl ?? "/Assets/placeholder-product.png",
                        Rating = product.Rating,
                        RatingCount = product.RatingCount,
                        Stock = product.Quantity,
                        Category = product.CategoryName ?? product.Category ?? "Uncategorized",
                        Manufacturer = product.Manufacturer ?? string.Empty
                    });
                }

                TotalProducts = pagedData.TotalCount;
                TotalPages = pagedData.TotalPages;
                HasMoreItems = CurrentPage < TotalPages;

                System.Diagnostics.Debug.WriteLine($"[ProductBrowseViewModel] Loaded page {CurrentPage}/{TotalPages} ({Products.Count} items, {TotalProducts} total)");
            }
            else
            {
                Products.Clear();
                TotalProducts = 0;
                TotalPages = 1;
                HasMoreItems = false;
                System.Diagnostics.Debug.WriteLine($"[ProductBrowseViewModel] Failed to load products: {result.ErrorMessage}");
            }

            // Notify UI of changes
            OnPropertyChanged(nameof(ItemsInfoText));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ProductBrowseViewModel] Error loading products: {ex.Message}");
            Products.Clear();
            TotalProducts = 0;
            TotalPages = 1;
            HasMoreItems = false;
        }
        finally
        {
            IsLoading = false;
        }
    }

    private (string sortBy, bool descending) MapSortOption(string option) => option switch
    {
        "Name: A to Z" => ("name", false),
        "Name: Z to A" => ("name", true),
        "Newest" => ("created", true),
        "Price: Low to High" => ("price", false),
        "Price: High to Low" => ("price", true),
        "Rating" => ("rating", true),
        _ => ("created", true)
    };

    [RelayCommand]
    private async Task SearchAsync()
    {
        CurrentPage = 1;
        await LoadPageAsync();
    }

    [RelayCommand]
    private async Task FilterByCategoryAsync(string category)
    {
        SelectedCategory = category;
        CurrentPage = 1;
        await LoadPageAsync();
    }

    [RelayCommand]
    private async Task FilterByBrandAsync(string brand)
    {
        SelectedBrand = brand;
        CurrentPage = 1;
        await LoadPageAsync();
    }

    [RelayCommand]
    private async Task SortAsync(string sortOption)
    {
        SelectedSort = sortOption;
        CurrentPage = 1;
        await LoadPageAsync();
    }

    [RelayCommand]
    private async Task ApplyFiltersAsync()
    {
        CurrentPage = 1;
        await LoadPageAsync();
    }

    [RelayCommand]
    private async Task ResetFiltersAsync()
    {
        SelectedCategory = "All";
        SearchQuery = string.Empty;
        SelectedSort = "Newest";
        CurrentPage = 1;
        await LoadPageAsync();
    }

    [RelayCommand]
    public async Task GoToPageAsync(int page)
    {
        System.Diagnostics.Debug.WriteLine($"[ProductBrowseViewModel] GoToPageAsync called: page={page}, CurrentPage={CurrentPage}, TotalPages={TotalPages}");

        if (page >= 1 && page <= TotalPages)
        {
            CurrentPage = page;
            await LoadPageAsync();
        }
    }

    [RelayCommand]
    private async Task NextPageAsync()
    {
        if (HasNextPage)
        {
            CurrentPage++;
            await LoadPageAsync();
        }
    }

    [RelayCommand]
    private async Task PreviousPageAsync()
    {
        if (HasPreviousPage)
        {
            CurrentPage--;
            await LoadPageAsync();
        }
    }

    [RelayCommand]
    private async Task AddToCartAsync(ProductCardViewModel product)
    {
        var result = await _cartFacade.AddToCartAsync(product.Id, 1);
        if (result.IsSuccess)
        {
            System.Diagnostics.Debug.WriteLine($"[ProductBrowseViewModel] Added to cart: {product.Name}");
        }
    }

    [RelayCommand]
    private async Task ViewProductDetailsAsync(ProductCardViewModel product)
    {
        System.Diagnostics.Debug.WriteLine($"[ProductBrowseViewModel] View details for product: {product.Name} (ID: {product.Id})");
        await Task.CompletedTask;
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadCategoriesAsync();
        await LoadBrandsAsync();
        await LoadPageAsync();
    }

    [RelayCommand]
    private async Task LoadMoreProductsAsync()
    {
        if (IsLoadingMore || !HasMoreItems)
            return;

        IsLoadingMore = true;
        try
        {
            CurrentPage++;
            await LoadPageAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ProductBrowseViewModel] Error loading more: {ex.Message}");
        }
        finally
        {
            IsLoadingMore = false;
        }
    }
}

public partial class ProductCardViewModel : ObservableObject
{
    [ObservableProperty]
    private Guid _id;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private decimal _price;

    [ObservableProperty]
    private string _imageUrl = string.Empty;

    [ObservableProperty]
    private double _rating;

    [ObservableProperty]
    private int _ratingCount;

    [ObservableProperty]
    private int _stock;

    [ObservableProperty]
    private string _category = string.Empty;

    [ObservableProperty]
    private string _manufacturer = string.Empty;

    public string FormattedPrice => $"₫{Price:N0}";
    public string StockStatus => Stock > 0 ? $"{Stock} in stock" : "Out of stock";
    public bool IsInStock => Stock > 0;
}
