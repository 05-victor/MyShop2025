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
    private int _loyaltyPoints = 0;

    [ObservableProperty]
    private int _memberSinceDays = 0;

    public CustomerDashboardViewModel(
        INavigationService navigationService,
        IProductFacade productFacade,
        IProfileFacade profileFacade,
        ISystemActivationRepository activationRepository)
    {
        _navigationService = navigationService;
        _productFacade = productFacade;
        _profileFacade = profileFacade;
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
            
            // TODO: Load actual statistics from orders/transactions
            // For now, using placeholder values
            TotalOrders = 0;
            TotalSpent = 0;
            LoyaltyPoints = 0;
            
            await CheckAdminBannerAsync();
            UpdateWelcomeMessage();
            await LoadDataAsync();
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[CustomerDashboardViewModel] Initialize failed: {ex.Message}");
        }
    }        private async Task LoadDataAsync()
        {
            await Task.WhenAll(
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
                        Image = product.ImageUrl ?? "https://images.unsplash.com/photo-1505740420928-5e560c06d30e?w=400"
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
                        Image = product.ImageUrl ?? "https://images.unsplash.com/photo-1505740420928-5e560c06d30e?w=400"
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