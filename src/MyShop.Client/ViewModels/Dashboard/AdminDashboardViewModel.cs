using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Client.Helpers;
using MyShop.Client.Models;
using MyShop.Client.ViewModels.Base;
using MyShop.Client.Views.Auth;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace MyShop.Client.ViewModels.Dashboard
{
    public partial class AdminDashboardViewModel : BaseViewModel
    {
        private readonly INavigationService _navigationService;
        private readonly IToastHelper _toastHelper;

        [ObservableProperty]
        private User? _currentUser;

        [ObservableProperty]
        private bool _isLoading = false;

        [ObservableProperty]
        private string _welcomeMessage = "Welcome back!";

        // Dashboard Statistics
        [ObservableProperty]
        private int _totalProducts = 0;

        [ObservableProperty]
        private int _todayOrders = 0;

        [ObservableProperty]
        private decimal _todayRevenue = 0;

        [ObservableProperty]
        private int _lowStockCount = 0;

        // Chart Data
        [ObservableProperty]
        private ObservableCollection<RevenueDataPoint> _revenueChartData = new();

        [ObservableProperty]
        private ObservableCollection<CategoryDataPoint> _categoryData = new();

        // Top Products
        [ObservableProperty]
        private ObservableCollection<TopProductItem> _topProducts = new();

        // Low Stock Items
        [ObservableProperty]
        private ObservableCollection<LowStockItem> _lowStockItems = new();

        // Recent Orders
        [ObservableProperty]
        private ObservableCollection<RecentOrderItem> _recentOrders = new();

        public AdminDashboardViewModel(
            INavigationService navigationService,
            IToastHelper toastHelper)
        {
            _navigationService = navigationService;
            _toastHelper = toastHelper;
        }

        public void Initialize(User user)
        {
            CurrentUser = user;
            WelcomeMessage = $"Welcome back, {user.Username}!";
            LoadDashboardDataAsync();
        }

        private async void LoadDashboardDataAsync()
        {
            SetLoadingState(true);

            try
            {
                // Simulate API call delay
                await Task.Delay(500);

                // Load Statistics (Mock Data)
                TotalProducts = 150;
                TodayOrders = 24;
                TodayRevenue = 12450.50m;
                LowStockCount = 8;

                // Load Revenue Chart Data (Last 7 days)
                RevenueChartData = new ObservableCollection<RevenueDataPoint>
                {
                    new RevenueDataPoint { Day = "Mon", Revenue = 4200, Orders = 45 },
                    new RevenueDataPoint { Day = "Tue", Revenue = 5100, Orders = 52 },
                    new RevenueDataPoint { Day = "Wed", Revenue = 3800, Orders = 38 },
                    new RevenueDataPoint { Day = "Thu", Revenue = 6200, Orders = 61 },
                    new RevenueDataPoint { Day = "Fri", Revenue = 7300, Orders = 73 },
                    new RevenueDataPoint { Day = "Sat", Revenue = 8900, Orders = 89 },
                    new RevenueDataPoint { Day = "Sun", Revenue = 5600, Orders = 56 }
                };

                // Load Category Data
                CategoryData = new ObservableCollection<CategoryDataPoint>
                {
                    new CategoryDataPoint { Name = "Electronics", Percentage = 45 },
                    new CategoryDataPoint { Name = "Clothing", Percentage = 28 },
                    new CategoryDataPoint { Name = "Home & Garden", Percentage = 18 },
                    new CategoryDataPoint { Name = "Sports", Percentage = 12 }
                };

                // Load Top Products
                TopProducts = new ObservableCollection<TopProductItem>
                {
                    new TopProductItem { Rank = 1, Name = "Laptop Dell XPS 13", Category = "Electronics", Price = 1200, Stock = 45 },
                    new TopProductItem { Rank = 2, Name = "iPhone 15 Pro", Category = "Electronics", Price = 1399, Stock = 120 },
                    new TopProductItem { Rank = 3, Name = "Samsung Galaxy S24", Category = "Electronics", Price = 1099, Stock = 85 },
                    new TopProductItem { Rank = 4, Name = "MacBook Pro 16", Category = "Electronics", Price = 2999, Stock = 30 },
                    new TopProductItem { Rank = 5, Name = "AirPods Pro", Category = "Audio", Price = 249, Stock = 200 }
                };

                // Load Low Stock Items
                LowStockItems = new ObservableCollection<LowStockItem>
                {
                    new LowStockItem { Name = "Sony WH-1000XM5", Category = "Audio", Stock = 5 },
                    new LowStockItem { Name = "Canon EOS R5", Category = "Photography", Stock = 3 },
                    new LowStockItem { Name = "Nintendo Switch", Category = "Gaming", Stock = 8 },
                    new LowStockItem { Name = "iPad Air", Category = "Electronics", Stock = 7 },
                    new LowStockItem { Name = "LG OLED TV 55", Category = "Electronics", Stock = 4 }
                };

                // Load Recent Orders
                RecentOrders = new ObservableCollection<RecentOrderItem>
                {
                    new RecentOrderItem { OrderId = "ORD001", CustomerName = "John Smith", OrderDate = "2025-10-22", Amount = 2599, Status = "Completed" },
                    new RecentOrderItem { OrderId = "ORD002", CustomerName = "Sarah Johnson", OrderDate = "2025-10-22", Amount = 1399, Status = "Delivering" },
                    new RecentOrderItem { OrderId = "ORD003", CustomerName = "Michael Brown", OrderDate = "2025-10-22", Amount = 749, Status = "Created" }
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading dashboard data: {ex.Message}");
                SetError("Failed to load dashboard data", ex);
            }
            finally
            {
                SetLoadingState(false);
            }
        }

        [RelayCommand]
        private async Task LogoutAsync()
        {
            CredentialHelper.RemoveToken();
            _toastHelper.ShowInfo("You have been logged out");
            _navigationService.NavigateTo(typeof(LoginPage));
            await Task.CompletedTask;
        }

        [RelayCommand]
        private void NavigateToProducts()
        {
            _toastHelper.ShowInfo("Products management coming soon!");
        }

        [RelayCommand]
        private void NavigateToOrders()
        {
            _toastHelper.ShowInfo("Orders management coming soon!");
        }

        [RelayCommand]
        private void NavigateToReports()
        {
            _toastHelper.ShowInfo("Reports coming soon!");
        }

        [RelayCommand]
        private void NavigateToSettings()
        {
            _toastHelper.ShowInfo("Settings coming soon!");
        }
    }

    // Helper Classes for Data Binding
    public class RevenueDataPoint
    {
        public string Day { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public int Orders { get; set; }
    }

    public class CategoryDataPoint
    {
        public string Name { get; set; } = string.Empty;
        public int Percentage { get; set; }
    }

    public class TopProductItem
    {
        public int Rank { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Stock { get; set; }
    }

    public class LowStockItem
    {
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int Stock { get; set; }
    }

    public class RecentOrderItem
    {
        public string OrderId { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string OrderDate { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
