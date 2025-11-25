using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Core.Interfaces.Repositories;
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
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ICartRepository _cartRepository;
    private readonly IAuthRepository _authRepository;
    private readonly IToastService _toastHelper;
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

    private const int PageSize = 12;

    public ProductBrowseViewModel(
        IProductRepository productRepository, 
        ICategoryRepository categoryRepository,
        ICartRepository cartRepository,
        IAuthRepository authRepository,
        IToastService toastHelper,
        INavigationService? navigationService = null)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _cartRepository = cartRepository;
        _authRepository = authRepository;
        _toastHelper = toastHelper;
        _navigationService = navigationService;
    }

    public async Task InitializeAsync()
    {
        await LoadCategoriesAsync();
        await LoadProductsAsync();
    }

    private async Task LoadCategoriesAsync()
    {
        try
        {
            var result = await _categoryRepository.GetAllAsync();
            if (result.IsSuccess && result.Data != null)
            {
                Categories = new ObservableCollection<string>(
                    new[] { "All Categories" }.Concat(result.Data.Select(c => c.Name))
                );
            }
            else
            {
                Categories = new ObservableCollection<string> { "All Categories" };
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ProductBrowseViewModel] Error loading categories: {ex.Message}");
            Categories = new ObservableCollection<string> { "All Categories" };
        }
    }

    private async Task LoadProductsAsync()
    {
        IsLoading = true;
        try
        {
            var result = await _productRepository.GetAllAsync();
            if (result.IsSuccess && result.Data != null)
            {
                _allProducts = result.Data.ToList();
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

    private void ApplyFiltersAndSort()
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
            "Newest" => filtered.OrderByDescending(p => p.CreatedAt),
            "Price: Low to High" => filtered.OrderBy(p => p.SellingPrice),
            "Price: High to Low" => filtered.OrderByDescending(p => p.SellingPrice),
            "Rating" => filtered.OrderByDescending(p => p.Rating),
            _ => filtered
        };

        var productList = filtered.ToList();
        TotalProducts = productList.Count;
        TotalPages = (int)Math.Ceiling((double)TotalProducts / PageSize);

        // Paginate
        var paged = productList
            .Skip((CurrentPage - 1) * PageSize)
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

        Products = new ObservableCollection<ProductCardViewModel>(paged);
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
        try
        {
            // Get current user ID
            var userIdResult = await _authRepository.GetCurrentUserIdAsync();
            
            if (!userIdResult.IsSuccess || userIdResult.Data == Guid.Empty)
            {
                await _toastHelper.ShowError("Please login to add items to cart");
                return;
            }

            var userId = userIdResult.Data;

            // Add to cart
            var result = await _cartRepository.AddToCartAsync(userId, product.Id, 1);

            if (result.IsSuccess && result.Data)
            {
                await _toastHelper.ShowSuccess($"Added {product.Name} to cart");
                System.Diagnostics.Debug.WriteLine($"[ProductBrowseViewModel] Added to cart: {product.Name}");
            }
            else
            {
                await _toastHelper.ShowError("Failed to add to cart. Product may be out of stock.");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ProductBrowseViewModel] Error adding to cart: {ex.Message}");
            await _toastHelper.ShowError("An error occurred while adding to cart");
        }
    }

    [RelayCommand]
    private async Task ViewProductDetailsAsync(ProductCardViewModel product)
    {
        // Navigate to product details page
        // When ProductDetailsPage is created, uncomment this:
        // _navigationService?.NavigateTo("MyShop.Client.Views.Shared.ProductDetailsPage", product.Id);
        
        System.Diagnostics.Debug.WriteLine($"[ProductBrowseViewModel] View details for product: {product.Name} (ID: {product.Id})");
        
        // For now, show a toast notification
        await _toastHelper.ShowInfo($"Product Details: {product.Name}\nPrice: ${product.Price:F2}\nStock: {product.Stock} units");
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
