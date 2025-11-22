using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Shared.Models;
using MyShop.Client.ViewModels.Base;
using MyShop.Client.Views.Shared;
using MyShop.Core.Interfaces.Services;
using MyShop.Core.Interfaces.Infrastructure;
using MyShop.Core.Interfaces.Repositories;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Linq;

namespace MyShop.Client.ViewModels.Admin;

public partial class AdminDashboardViewModel : BaseViewModel
{
        private readonly IToastService _toastHelper;
        private readonly IDashboardRepository _dashboardRepository;

        [ObservableProperty]
        private bool _isLoading = false;

        // Dashboard Statistics
        [ObservableProperty]
        private int _totalProducts = 0;

        [ObservableProperty]
        private int _todayOrders = 0;

        [ObservableProperty]
        private double _ordersTrend = 0;

        [ObservableProperty]
        private decimal _todayRevenue = 0;

        [ObservableProperty]
        private decimal _weekRevenue = 0;

        [ObservableProperty]
        private decimal _monthRevenue = 0;

        [ObservableProperty]
        private double _revenueTrend = 0;

        [ObservableProperty]
        private string _topRatedProductName = "Loading...";

        [ObservableProperty]
        private double _topRatedProductRating = 0;

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

        // Top SalesAgents
        [ObservableProperty]
        private ObservableCollection<TopSalesAgentItem> _topSalesAgents = new();

        // Flagged Products
        [ObservableProperty]
        private ObservableCollection<FlaggedProductItem> _flaggedProducts = new();

        public AdminDashboardViewModel(
            IToastService toastHelper, 
            IDashboardRepository dashboardRepository)
        {
            _toastHelper = toastHelper;
            _dashboardRepository = dashboardRepository;
        }

        public void Initialize(User user)
        {
            _ = LoadDashboardDataAsync();
        }

        [RelayCommand]
        private async Task RefreshAsync()
        {
            await LoadDashboardDataAsync();
        }

        private async Task LoadDashboardDataAsync()
        {
            SetLoadingState(true);

            try
            {
                // Load Dashboard Summary from Repository
                var summaryResult = await _dashboardRepository.GetSummaryAsync();
                if (!summaryResult.IsSuccess)
                {
                    SetError(summaryResult.ErrorMessage ?? "Failed to load dashboard data");
                    _toastHelper.ShowError(summaryResult.ErrorMessage ?? "Failed to load dashboard data");
                    return;
                }

                var summary = summaryResult.Data;
                if (summary == null)
                {
                    SetError("Dashboard data is empty");
                    return;
                }

                // Map Summary Statistics
                TotalProducts = summary.TotalProducts;
                TodayOrders = summary.TodayOrders;
                TodayRevenue = summary.TodayRevenue;
                WeekRevenue = summary.WeekRevenue;
                MonthRevenue = summary.MonthRevenue;
                
                // Calculate trends (mock calculation - could be from backend)
                OrdersTrend = 12.5; // +12.5% vs yesterday
                RevenueTrend = 8.2; // +8.2% vs yesterday

                // Top Rated Product
                var topProduct = summary.TopSellingProducts?.FirstOrDefault();
                if (topProduct != null)
                {
                    TopRatedProductName = topProduct.Name ?? "N/A";
                    TopRatedProductRating = 4.5; // Default rating, extend TopSellingProduct model if needed
                }
                else
                {
                    TopRatedProductName = "No data";
                    TopRatedProductRating = 0;
                }

                // Low Stock Count
                LowStockCount = summary.LowStockProducts.Count;

                // Load Revenue Chart Data (Daily)
                var chartResult = await _dashboardRepository.GetRevenueChartAsync("daily");
                if (chartResult.IsSuccess && chartResult.Data != null)
                {
                    RunOnUIThread(() =>
                    {
                        RevenueChartData = new ObservableCollection<RevenueDataPoint>();
                        for (int i = 0; i < chartResult.Data.Labels.Count; i++)
                        {
                            RevenueChartData.Add(new RevenueDataPoint
                            {
                                Day = chartResult.Data.Labels[i],
                                Revenue = chartResult.Data.Data[i],
                                Orders = 0 // Could add orders count to chart data
                            });
                        }
                    });
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[AdminDashboard] Failed to load chart data: {chartResult.ErrorMessage}");
                }

                // Map Category Sales Data
                RunOnUIThread(() =>
                {
                    CategoryData = new ObservableCollection<CategoryDataPoint>();
                    if (summary.SalesByCategory != null)
                    {
                        foreach (var categorySale in summary.SalesByCategory)
                        {
                            if (categorySale != null)
                            {
                                CategoryData.Add(new CategoryDataPoint
                                {
                                    Name = categorySale.CategoryName ?? "Unknown",
                                    Percentage = (int)categorySale.Percentage
                                });
                            }
                        }
                    }
                });

                // Map Top Products
                RunOnUIThread(() =>
                {
                    TopProducts = new ObservableCollection<TopProductItem>();
                    if (summary.TopSellingProducts != null)
                    {
                        int rank = 1;
                        foreach (var product in summary.TopSellingProducts)
                        {
                            if (product != null)
                            {
                                TopProducts.Add(new TopProductItem
                                {
                                    Rank = rank++,
                                    Name = product.Name ?? "Unknown",
                                    Category = product.CategoryName ?? "Unknown",
                                    Price = product.Revenue / Math.Max(product.SoldCount, 1), // Avg price
                                    Stock = 0 // Not available in summary
                                });
                            }
                        }
                    }
                });

                // Map Low Stock Items
                RunOnUIThread(() =>
                {
                    LowStockItems = new ObservableCollection<LowStockItem>();
                    if (summary.LowStockProducts != null)
                    {
                        foreach (var lowStock in summary.LowStockProducts)
                        {
                            if (lowStock != null)
                            {
                                LowStockItems.Add(new LowStockItem
                                {
                                    Name = lowStock.Name ?? "Unknown",
                                    Category = lowStock.CategoryName ?? "Unknown",
                                    Stock = lowStock.Quantity
                                });
                            }
                        }
                    }
                });

                // Map Recent Orders
                RunOnUIThread(() =>
                {
                    RecentOrders = new ObservableCollection<RecentOrderItem>();
                    if (summary.RecentOrders != null)
                    {
                        foreach (var order in summary.RecentOrders)
                        {
                            if (order != null)
                            {
                                RecentOrders.Add(new RecentOrderItem
                                {
                                    OrderId = order.Id.ToString().Substring(0, 8).ToUpper(),
                                    CustomerName = order.CustomerName ?? "Unknown",
                                    OrderDate = order.OrderDate.ToString("yyyy-MM-dd"),
                                    Amount = order.TotalAmount,
                                    Status = order.Status ?? "Unknown"
                                });
                            }
                        }
                    }
                });

                // Top sales agents and flagged products not available in current dashboard summary
                // These would require separate repository methods in future:
                // - IUserRepository.GetTopSalesAgentsAsync(limit: 5)
                // - IProductRepository.GetFlaggedProductsAsync()
                TopSalesAgents = new ObservableCollection<TopSalesAgentItem>();
                FlaggedProducts = new ObservableCollection<FlaggedProductItem>();

                System.Diagnostics.Debug.WriteLine($"[AdminDashboard] Loaded dashboard data successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading dashboard data: {ex.Message}");
                SetError("Failed to load dashboard data", ex);
                _toastHelper.ShowError("Failed to load dashboard data. Please try again.");
            }
            finally
            {
                SetLoadingState(false);
            }
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

    public class TopSalesAgentItem
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Avatar { get; set; } = string.Empty;
        public decimal GMV { get; set; }
        public decimal Commission { get; set; }
        public double Rating { get; set; }
        public string Status { get; set; } = string.Empty;
    }

public class FlaggedProductItem
{
    public string Name { get; set; } = string.Empty;
    public string Agent { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
}