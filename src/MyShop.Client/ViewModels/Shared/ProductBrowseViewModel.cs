using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using MyShop.Client.Facades;
using MyShop.Client.ViewModels.Base;
using MyShop.Core.Interfaces.Facades;
using MyShop.Core.Interfaces.Services;
using MyShop.Core.Interfaces.Repositories;
using MyShop.Shared.Models;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace MyShop.Client.ViewModels.Shared;

/// <summary>
/// ViewModel for Product Browse page with SERVER-SIDE paging
/// Loads products page by page from the API, not all at once
/// </summary>
public partial class ProductBrowseViewModel : PagedViewModelBase<ProductCardViewModel>
{
    private readonly IProductFacade _productFacade;
    private readonly ICartFacade _cartFacade;
    private readonly IAuthRepository _authRepository;
    private bool _isInitializing = true; // Guard to prevent LoadPageAsync during initial setup
    private bool _hasInitialized = false; // Track if InitializeAsync has completed

    // Alias for backward compatibility with XAML
    public ObservableCollection<ProductCardViewModel> Products => Items;

    /// <summary>Flag to indicate if initialization is still in progress</summary>
    public bool IsInitializing => _isInitializing;

    // Event for requesting product details dialog (handled by view)
    public event EventHandler<ProductCardViewModel>? ProductDetailsRequested;

    [ObservableProperty]
    private ObservableCollection<string> _categories = new() { "All" };

    [ObservableProperty]
    private string _selectedCategory = "All";

    [ObservableProperty]
    private ObservableCollection<string> _brands = new() { "All Brands" };

    [ObservableProperty]
    private string _selectedBrand = "All Brands";

    [ObservableProperty]
    private string _selectedSort = "Newest";

    [ObservableProperty]
    private bool _isLoadingMore;

    [ObservableProperty]
    private bool _hasMoreItems = true;

    // Additional pagination property for UI
    public Visibility ShowPagination => TotalPages > 1 ? Visibility.Visible : Visibility.Collapsed;

    /// <summary>
    /// Page size options for card grid layout (12, 24, 36, 60 products per page)
    /// </summary>
    public IReadOnlyList<int> CardPageSizeOptions { get; } = new[] { 12, 24, 36, 60 };

    public ProductBrowseViewModel(
        IProductFacade productFacade,
        ICartFacade cartFacade,
        IAuthRepository authRepository,
        INavigationService? navigationService = null)
        : base(null, navigationService)
    {
        _productFacade = productFacade;
        _cartFacade = cartFacade;
        _authRepository = authRepository;
        PageSize = 12; // Show 12 products per page for grid layout
    }

    public async Task InitializeAsync()
    {
        // Prevent double initialization
        if (_hasInitialized)
        {
            System.Diagnostics.Debug.WriteLine("[ProductBrowseViewModel] InitializeAsync: Already initialized, skipping");
            return;
        }

        System.Diagnostics.Debug.WriteLine("[ProductBrowseViewModel] InitializeAsync: START");

        try
        {
            // Check email verification status
            var userResult = await _authRepository.GetCurrentUserAsync();
            var isEmailVerified = userResult.IsSuccess && userResult.Data?.IsEmailVerified == true;
            var currentUserId = userResult.IsSuccess && userResult.Data != null ? userResult.Data.Id : Guid.Empty;

            await LoadCategoriesAsync();
            await LoadBrandsAsync();

            // Now enable event firing and load page
            _isInitializing = false;
            await LoadPageAsync(); // LoadPageAsync will set loading state and handle it

            // Update all product cards with email verification status
            foreach (var product in Products)
            {
                // Agent cannot buy their own products
                var isOwnProduct = product.SaleAgentId == currentUserId;
                product.CanAddToCart = isEmailVerified && product.Stock > 0 && !isOwnProduct;
                product.ShowEmailVerification = !isEmailVerified;
            }

            _hasInitialized = true;
            System.Diagnostics.Debug.WriteLine("[ProductBrowseViewModel] InitializeAsync: COMPLETE");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ProductBrowseViewModel] InitializeAsync: ERROR - {ex.Message}");
            SetLoadingState(false);
        }
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
    /// Override LoadPageAsync - required by PagedViewModelBase
    /// Load products from server with current filters and page
    /// </summary>
    protected override async Task LoadPageAsync()
    {
        try
        {
            SetLoadingState(true);

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

                // Check email verification once per page load
                var userResult = await _authRepository.GetCurrentUserAsync();
                var isEmailVerified = userResult.IsSuccess && userResult.Data?.IsEmailVerified == true;
                var currentUserId = userResult.IsSuccess && userResult.Data != null ? userResult.Data.Id : Guid.Empty;

                Items.Clear();
                foreach (var product in pagedData.Items)
                {
                    // Normalize image URL - handle old/incorrect placeholder paths
                    var imageUrl = product.ImageUrl;
                    if (string.IsNullOrWhiteSpace(imageUrl) ||
                        imageUrl.Contains("product-placeholder.png") ||
                        imageUrl.Contains("placeholder-product.png"))
                    {
                        imageUrl = "ms-appx:///Assets/Images/products/product-placeholder.png";
                    }

                    // Debug agent data
                    var agentName = product.SaleAgentFullName ?? product.SaleAgentUsername;
                    System.Diagnostics.Debug.WriteLine($"[ProductBrowseViewModel] Product '{product.Name}': AgentId={product.SaleAgentId}, FullName='{product.SaleAgentFullName}', Username='{product.SaleAgentUsername}', Final='{agentName}'");

                    var productCard = new ProductCardViewModel
                    {
                        Id = product.Id,
                        Name = product.Name,
                        Price = product.SellingPrice,
                        ImageUrl = imageUrl,
                        Rating = product.Rating,
                        RatingCount = product.RatingCount,
                        Stock = product.Quantity,
                        Category = product.CategoryName ?? product.Category ?? "Uncategorized",
                        Manufacturer = product.Manufacturer ?? string.Empty,
                        AgentName = agentName,
                        SaleAgentFullName = product.SaleAgentFullName ?? product.SaleAgentUsername ?? string.Empty,
                        SaleAgentId = product.SaleAgentId,
                        Description = product.Description ?? string.Empty,

                        // Email verification UX + prevent agents from buying own products
                        CanAddToCart = isEmailVerified && product.Quantity > 0 && product.SaleAgentId != currentUserId,
                        ShowEmailVerification = !isEmailVerified
                    };
                    Items.Add(productCard);
                }

                UpdatePagingInfo(pagedData.TotalCount);
                HasMoreItems = CurrentPage < TotalPages;

                System.Diagnostics.Debug.WriteLine($"[ProductBrowseViewModel] Loaded page {CurrentPage}/{TotalPages} ({Items.Count} items, {TotalItems} total)");
            }
            else
            {
                Items.Clear();
                UpdatePagingInfo(0);
                HasMoreItems = false;
                System.Diagnostics.Debug.WriteLine($"[ProductBrowseViewModel] Failed to load products: {result.ErrorMessage}");
            }

            OnPropertyChanged(nameof(ShowPagination));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ProductBrowseViewModel] Error loading products: {ex.Message}");
            Items.Clear();
            UpdatePagingInfo(0);
            HasMoreItems = false;
        }
        finally
        {
            SetLoadingState(false);
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
    private async Task FilterByCategoryAsync(string category)
    {
        SelectedCategory = category;
        await LoadDataAsync(); // Use base method to reset to page 1
    }

    [RelayCommand]
    private async Task FilterByBrandAsync(string brand)
    {
        SelectedBrand = brand;
        await LoadDataAsync(); // Use base method to reset to page 1
    }

    [RelayCommand]
    private async Task SortAsync(string sortOption)
    {
        SelectedSort = sortOption;
        await LoadDataAsync(); // Use base method to reset to page 1
    }

    [RelayCommand]
    private async Task ApplyFiltersAsync()
    {
        await LoadDataAsync(); // Use base method to reset to page 1
    }

    [RelayCommand]
    private async Task ResetFiltersAsync()
    {
        SelectedCategory = "All";
        SearchQuery = string.Empty;
        SelectedSort = "Newest";
        await LoadDataAsync(); // Use base method to reset to page 1
    }

    // NOTE: NextPageAsync, PreviousPageAsync, SearchAsync, GoToPageAsync are provided by PagedViewModelBase

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

        // Raise event for view to handle dialog display
        ProductDetailsRequested?.Invoke(this, product);

        await Task.CompletedTask;
    }

    [RelayCommand]
    private async Task VerifyEmailAsync()
    {
        System.Diagnostics.Debug.WriteLine("[ProductBrowseViewModel] Verify email command triggered");
        // Navigate to profile/email verification page
        await Task.CompletedTask;
    }

    // NOTE: RefreshAsync is provided by PagedViewModelBase

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

    [ObservableProperty]
    private bool _canAddToCart = true;

    [ObservableProperty]
    private bool _showEmailVerification = false;

    [ObservableProperty]
    private string? _agentName;

    [ObservableProperty]
    private Guid? _saleAgentId;

    [ObservableProperty]
    private string? _saleAgentFullName = string.Empty;

    [ObservableProperty]
    private string? _description = string.Empty;
    public string StockStatus => Stock > 0 ? $"{Stock} in stock" : "Out of stock";
    public bool IsInStock => Stock > 0;
    public string AgentDisplay => !string.IsNullOrEmpty(AgentName) ? $"by {AgentName}" : string.Empty;
}
