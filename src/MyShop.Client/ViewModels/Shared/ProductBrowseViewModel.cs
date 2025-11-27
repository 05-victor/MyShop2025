using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Client.Facades;
using MyShop.Core.Interfaces.Facades;
using MyShop.Core.Interfaces.Services;
using MyShop.Shared.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace MyShop.Client.ViewModels.Shared;

public partial class ProductBrowseViewModel : ObservableObject
{
    private readonly IProductFacade _productFacade;
    private readonly ICartFacade _cartFacade;
    private readonly INavigationService? _navigationService;
    private List<Product> _allProducts = new();

    [ObservableProperty]
    private ObservableCollection<ProductCardViewModel> _products = new();

    [ObservableProperty]
    private ObservableCollection<string> _categories = new();

    [ObservableProperty]
    private string _selectedCategory = "All Categories";

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    [ObservableProperty]
    private decimal _minPrice = 0;

    [ObservableProperty]
    private decimal _maxPrice = 10000;

    [ObservableProperty]
    private string _selectedSort = "Newest";

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private int _currentPage = 1;

    [ObservableProperty]
    private int _totalPages = 1;

    [ObservableProperty]
    private int _totalProducts;

    [ObservableProperty]
    private bool _isLoadingMore;

    [ObservableProperty]
    private bool _hasMoreItems = true;

    private const int PageSize = Core.Common.PaginationConstants.ProductsPageSize;
    private int _loadedCount = 0;

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
        await LoadProductsAsync();
    }

    private async Task LoadCategoriesAsync()
    {
        Categories = new ObservableCollection<string> { "All Categories", "Electronics", "Clothing", "Books", "Home" };
        await Task.CompletedTask;
    }

    private async Task LoadProductsAsync()
    {
        IsLoading = true;
        try
        {
            var result = await _productFacade.LoadProductsAsync();
            if (result.IsSuccess && result.Data != null)
            {
                _allProducts = result.Data.Items.ToList();
            }
            else
            {
                _allProducts = new List<Product>();
            }
            
            ApplyFiltersAndSort();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ProductBrowseViewModel] Error loading products: {ex.Message}");
            Products.Clear();
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void ApplyFiltersAndSort(bool append = false)
    {
        var filtered = _allProducts.AsEnumerable();

        // Filter by category
        if (SelectedCategory != "All Categories")
        {
            filtered = filtered.Where(p => p.Category == SelectedCategory || p.CategoryName == SelectedCategory);
        }

        // Filter by search query
        if (!string.IsNullOrWhiteSpace(SearchQuery))
        {
            filtered = filtered.Where(p =>
                p.Name.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase) ||
                (p.Manufacturer?.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase) ?? false)
            );
        }

        // Filter by price range
        filtered = filtered.Where(p => p.SellingPrice >= MinPrice && p.SellingPrice <= MaxPrice);

        // Sort
        filtered = SelectedSort switch
        {
            "Name: A to Z" => filtered.OrderBy(p => p.Name),
            "Name: Z to A" => filtered.OrderByDescending(p => p.Name),
            "Newest" => filtered.OrderByDescending(p => p.CreatedAt),
            "Price: Low to High" => filtered.OrderBy(p => p.SellingPrice),
            "Price: High to Low" => filtered.OrderByDescending(p => p.SellingPrice),
            "Rating" => filtered.OrderByDescending(p => p.Rating),
            _ => filtered
        };

        var productList = filtered.ToList();
        TotalProducts = productList.Count;
        TotalPages = (int)Math.Ceiling((double)TotalProducts / PageSize);

        if (!append)
        {
            _loadedCount = 0;
            Products.Clear();
        }

        // Paginate
        var paged = productList
            .Skip(_loadedCount)
            .Take(PageSize)
            .Select(p => new ProductCardViewModel
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.SellingPrice,
                ImageUrl = p.ImageUrl ?? "/Assets/placeholder-product.png",
                Rating = p.Rating,
                RatingCount = p.RatingCount,
                Stock = p.Quantity,
                Category = p.CategoryName ?? p.Category ?? "Uncategorized",
                Manufacturer = p.Manufacturer ?? string.Empty
            })
            .ToList();

        foreach (var product in paged)
        {
            Products.Add(product);
        }

        _loadedCount += paged.Count;
        HasMoreItems = _loadedCount < TotalProducts;
    }

    [RelayCommand]
    private async Task SearchAsync(string query)
    {
        SearchQuery = query;
        CurrentPage = 1;
        ApplyFiltersAndSort();
        await Task.CompletedTask;
    }

    [RelayCommand]
    private async Task FilterByCategoryAsync(string category)
    {
        SelectedCategory = category;
        CurrentPage = 1;
        ApplyFiltersAndSort();
        await Task.CompletedTask;
    }

    [RelayCommand]
    private async Task SortAsync(string sortOption)
    {
        SelectedSort = sortOption;
        ApplyFiltersAndSort();
        await Task.CompletedTask;
    }

    [RelayCommand]
    private async Task ApplyPriceFilterAsync()
    {
        CurrentPage = 1;
        ApplyFiltersAndSort();
        await Task.CompletedTask;
    }

    [RelayCommand]
    private async Task GoToPageAsync(int page)
    {
        if (page >= 1 && page <= TotalPages)
        {
            CurrentPage = page;
            ApplyFiltersAndSort();
        }
        await Task.CompletedTask;
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
        await LoadProductsAsync();
    }

    [RelayCommand]
    private async Task LoadMoreProductsAsync()
    {
        if (IsLoadingMore || !HasMoreItems)
            return;

        IsLoadingMore = true;
        try
        {
            await Task.Delay(500); // Simulate network delay
            ApplyFiltersAndSort(append: true);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ProductBrowseViewModel] Error loading more products: {ex.Message}");
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

    public string FormattedPrice => $"${Price:N2}";
    public string StockStatus => Stock > 0 ? $"{Stock} in stock" : "Out of stock";
    public bool IsInStock => Stock > 0;
}
