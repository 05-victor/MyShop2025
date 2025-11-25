using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Shared.Models;
using MyShop.Client.ViewModels.Base;
using MyShop.Client.Views.Shared;
using MyShop.Core.Interfaces.Services;
using MyShop.Core.Interfaces.Infrastructure;
using MyShop.Core.Interfaces.Repositories;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace MyShop.Client.ViewModels.Customer;

public partial class CustomerDashboardViewModel : BaseViewModel
{
        private readonly INavigationService _navigationService;
        private readonly IToastService _toastHelper;
        private readonly ICredentialStorage _credentialStorage;
        private readonly IProductRepository _productRepository;
        private readonly IOrderRepository _orderRepository;

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

        public CustomerDashboardViewModel(
            INavigationService navigationService,
            IToastService toastHelper,
            ICredentialStorage credentialStorage,
            IProductRepository productRepository,
            IOrderRepository orderRepository)
        {
            _navigationService = navigationService;
            _toastHelper = toastHelper;
            _credentialStorage = credentialStorage;
            _productRepository = productRepository;
            _orderRepository = orderRepository;
        }

        public async void Initialize(User user)
        {
            CurrentUser = user;
            IsVerified = user.IsEmailVerified;
            CheckAdminBanner();
            UpdateWelcomeMessage();
            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            await Task.WhenAll(
                LoadFeaturedProductsAsync(),
                LoadRecommendedProductsAsync()
            );
        }

        private void CheckAdminBanner()
        {
            if (CurrentUser == null) return;

            // Show banner only if:
            // 1. No admin exists in system (hasAdmin flag not set)
            // 2. Current user is Customer role
            // Note: ICredentialStorage doesn't support key-value storage
            // For now, always show banner for Customers (mock implementation)
            var isCustomer = CurrentUser.GetPrimaryRole() == MyShop.Shared.Models.Enums.UserRole.Customer;

            ShowBecomeAdminBanner = isCustomer; // Show banner for customers only
            
            System.Diagnostics.Debug.WriteLine($"[CustomerDashboard] isCustomer={isCustomer}, ShowBanner={ShowBecomeAdminBanner}");
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
                var result = await _productRepository.GetAllAsync();
                if (!result.IsSuccess || result.Data == null)
                {
                    return;
                }
                
                var featured = result.Data.Take(4).ToList();

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
                var result = await _productRepository.GetAllAsync();
                if (!result.IsSuccess || result.Data == null)
                {
                    return;
                }
                
                var recommended = result.Data.Skip(4).Take(4).ToList();

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
            await _credentialStorage.RemoveToken();
            await _toastHelper.ShowInfo("Logged out");
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