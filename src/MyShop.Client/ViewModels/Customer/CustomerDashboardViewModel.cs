using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Shared.Models;
using MyShop.Client.ViewModels.Base;
using MyShop.Client.Views.Shared;
using MyShop.Core.Interfaces.Facades;
using MyShop.Core.Interfaces.Repositories;
using MyShop.Core.Interfaces.Services;
using MyShop.Client.Facades;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace MyShop.Client.ViewModels.Customer;

public partial class CustomerDashboardViewModel : BaseViewModel
{
    private new readonly INavigationService _navigationService;
    private readonly IProductFacade _productFacade;
    private readonly IProfileFacade _profileFacade;
    private readonly IOrderFacade _orderFacade;
    private readonly ICartFacade _cartFacade;
    private readonly ISystemActivationRepository _activationRepository;

    [ObservableProperty]
    private User? _currentUser;

    [ObservableProperty]
    private string _title = "Customer Dashboard";

    [ObservableProperty]
    private bool _showBecomeAdminBanner = false;

    [ObservableProperty]
    private ObservableCollection<FeaturedProductItem> _featuredProducts = new();

    [ObservableProperty]
    private ObservableCollection<RecommendedProductItem> _recommendedProducts = new();

    [ObservableProperty]
    private string _welcomeMessage = "Welcome back!";

    [ObservableProperty]
    private bool _isVerified = true;

    [ObservableProperty]
    private bool _profileCompleted = true;

    [ObservableProperty]
    private int _totalOrders = 0;

    [ObservableProperty]
    private double _totalSpent = 0;

    [ObservableProperty]
    private int _pendingOrders = 0;

    [ObservableProperty]
    private int _memberSinceDays = 0;

    [ObservableProperty]
    private bool _isLoadingStats = false;

    // Cart properties
    [ObservableProperty]
    private int _cartItemCount = 0;

    [ObservableProperty]
    private decimal _cartTotal = 0;

    [ObservableProperty]
    private bool _isLoadingCart = false;

    public CustomerDashboardViewModel(
        INavigationService navigationService,
        IProductFacade productFacade,
        IProfileFacade profileFacade,
        IOrderFacade orderFacade,
        ICartFacade cartFacade,
        ISystemActivationRepository activationRepository)
    {
        _navigationService = navigationService;
        _productFacade = productFacade;
        _profileFacade = profileFacade;
        _orderFacade = orderFacade;
        _cartFacade = cartFacade;
        _activationRepository = activationRepository;
    }

    public async void Initialize(User user)
    {
        try
        {
            CurrentUser = user;
            IsVerified = user.IsEmailVerified;

            // Calculate member since days
            MemberSinceDays = (DateTime.Now - user.CreatedAt).Days;

            await CheckAdminBannerAsync();
            UpdateWelcomeMessage();
            await LoadDataAsync();
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[CustomerDashboardViewModel] Initialize failed: {ex.Message}");
        }
    }
    private async Task LoadDataAsync()
    {
        await Task.WhenAll(
            LoadCustomerStatisticsAsync(),
            LoadCartPreviewAsync(),
            LoadFeaturedProductsAsync(),
            LoadRecommendedProductsAsync()
        );
    }

    private async Task CheckAdminBannerAsync()
    {
        if (CurrentUser == null) return;

        try
        {
            // Check if current user is Customer role
            var isCustomer = CurrentUser.GetPrimaryRole() == MyShop.Shared.Models.Enums.UserRole.Customer;

            if (!isCustomer)
            {
                ShowBecomeAdminBanner = false;
                return;
            }

            // Check if any admin exists in the system
            var hasAdminResult = await _activationRepository.HasAnyAdminAsync();
            var hasAdmin = hasAdminResult.IsSuccess && hasAdminResult.Data;

            // Show banner only if: Customer + No admin exists
            ShowBecomeAdminBanner = isCustomer && !hasAdmin;

            System.Diagnostics.Debug.WriteLine($"[CustomerDashboard] isCustomer={isCustomer}, hasAdmin={hasAdmin}, ShowBanner={ShowBecomeAdminBanner}");
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[CustomerDashboard] CheckAdminBannerAsync error: {ex.Message}");
            ShowBecomeAdminBanner = false;
        }
    }

    private async Task LoadCustomerStatisticsAsync()
    {
        if (CurrentUser == null) return;

        try
        {
            IsLoadingStats = true;
            System.Diagnostics.Debug.WriteLine("[CustomerDashboard] Loading customer order statistics...");

            // Load orders for current customer (page 1, get all in first 100)
            var result = await _orderFacade.LoadOrdersPagedAsync(
                page: 1,
                pageSize: 100,
                status: null,
                paymentStatus: null,
                customerId: CurrentUser.Id);

            if (result.IsSuccess && result.Data != null)
            {
                var orders = result.Data.Items;

                // Calculate statistics
                TotalOrders = orders.Count;

                // Sum total spent from all orders (using FinalPrice which is the actual amount paid)
                TotalSpent = (double)orders
                    .Where(o => o.Status != "CANCELLED") // Exclude cancelled orders
                    .Sum(o => o.FinalPrice);

                // Count pending orders (API returns status in UPPERCASE)
                PendingOrders = orders.Count(o => o.Status == "PENDING");

                System.Diagnostics.Debug.WriteLine(
                    $"[CustomerDashboard] Loaded: Orders={TotalOrders}, TotalSpent={TotalSpent}đ, PendingOrders={PendingOrders}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[CustomerDashboard] Failed to load orders: {result.ErrorMessage}");
                // Keep default values if API call fails
                TotalOrders = 0;
                TotalSpent = 0;
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(
                $"[CustomerDashboard] Error loading customer statistics: {ex.Message}");
            // Keep default values if exception occurs
            TotalOrders = 0;
            TotalSpent = 0;
        }
        finally
        {
            IsLoadingStats = false;
        }
    }

    private async Task LoadCartPreviewAsync()
    {
        try
        {
            IsLoadingCart = true;
            System.Diagnostics.Debug.WriteLine("[CustomerDashboard] Loading cart preview...");

            // Get cart summary
            var result = await _cartFacade.GetCartSummaryAsync();

            if (result.IsSuccess && result.Data != null)
            {
                var cartSummary = result.Data;
                CartItemCount = cartSummary.TotalItems;
                CartTotal = cartSummary.Total;

                System.Diagnostics.Debug.WriteLine(
                    $"[CustomerDashboard] Cart loaded: Items={CartItemCount}, Total={CartTotal}đ");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine(
                    $"[CustomerDashboard] Failed to load cart: {result.ErrorMessage}");
                // Reset to empty cart
                CartItemCount = 0;
                CartTotal = 0;
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(
                $"[CustomerDashboard] Error loading cart preview: {ex.Message}");
            // Reset to empty cart if exception
            CartItemCount = 0;
            CartTotal = 0;
        }
        finally
        {
            IsLoadingCart = false;
        }
    }

    private void UpdateWelcomeMessage()
    {
        if (CurrentUser == null) return;

        var hour = DateTime.Now.Hour;
        var greeting = hour < 12 ? "Good morning" : hour < 18 ? "Good afternoon" : "Good evening";
        WelcomeMessage = $"{greeting}, {CurrentUser.FullName}!";
    }

    private async Task LoadFeaturedProductsAsync()
    {
        try
        {
            var result = await _productFacade.LoadProductsAsync();
            if (!result.IsSuccess || result.Data == null)
            {
                return;
            }

            var featured = result.Data.Items.Take(4).ToList();

            FeaturedProducts.Clear();
            foreach (var product in featured)
            {
                FeaturedProducts.Add(new FeaturedProductItem
                {
                    Id = product.Id.ToString(),
                    Name = product.Name,
                    Category = product.CategoryName ?? "Unknown",
                    Brand = product.Manufacturer ?? "Unknown",
                    Price = product.SellingPrice,
                    Rating = product.Rating,
                    Reviews = product.RatingCount,
                    Image = string.IsNullOrWhiteSpace(product.ImageUrl) ? "ms-appx:///Assets/Images/products/product-placeholder.png" : product.ImageUrl
                });
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[CustomerDashboard] Error loading featured products: {ex.Message}");
            FeaturedProducts.Clear();
        }
    }

    private async Task LoadRecommendedProductsAsync()
    {
        try
        {
            var result = await _productFacade.LoadProductsAsync();
            if (!result.IsSuccess || result.Data == null)
            {
                return;
            }

            var recommended = result.Data.Items.Skip(4).Take(4).ToList();

            RecommendedProducts.Clear();
            foreach (var product in recommended)
            {
                RecommendedProducts.Add(new RecommendedProductItem
                {
                    Id = product.Id.ToString(),
                    Name = product.Name,
                    Category = product.CategoryName ?? "Unknown",
                    Brand = product.Manufacturer ?? "Unknown",
                    Price = product.SellingPrice,
                    Rating = product.Rating,
                    Reviews = product.RatingCount,
                    Image = string.IsNullOrWhiteSpace(product.ImageUrl) ? "ms-appx:///Assets/Images/products/product-placeholder.png" : product.ImageUrl
                });
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[CustomerDashboard] Error loading recommended products: {ex.Message}");
            RecommendedProducts.Clear();
        }
    }

    [RelayCommand]
    private async Task ViewCartAsync()
    {
        System.Diagnostics.Debug.WriteLine("[CustomerDashboard] Navigating to cart page");
        await _navigationService.NavigateTo(typeof(CartPage).FullName!);
    }

    [RelayCommand]
    private async Task RefreshCartAsync()
    {
        System.Diagnostics.Debug.WriteLine("[CustomerDashboard] Refreshing cart");
        await LoadCartPreviewAsync();
    }

    [RelayCommand]
    private async Task LogoutAsync()
    {
        // Note: LogoutAsync should be in IAuthFacade, not IProfileFacade
        // TODO: Inject IAuthFacade and use _authFacade.LogoutAsync()
        await _navigationService.NavigateTo(typeof(LoginPage).FullName!);
    }
}

// Helper classes for data binding
public class FeaturedProductItem
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public double Rating { get; set; }
    public int Reviews { get; set; }
    public string Image { get; set; } = string.Empty;
}

public class RecommendedProductItem
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public double Rating { get; set; }
    public int Reviews { get; set; }
    public string Image { get; set; } = string.Empty;
}