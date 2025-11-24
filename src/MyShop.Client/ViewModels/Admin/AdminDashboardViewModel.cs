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

        // New Platform-Owner KPIs
        [ObservableProperty]
        private decimal _totalGmvThisMonth = 0;

        [ObservableProperty]
        private decimal _adminCommission = 0;

        [ObservableProperty]
        private int _activeSalesAgents = 0;

        [ObservableProperty]
        private int _itemsToReview = 0;

        // Dashboard Statistics (kept for backwards compatibility)
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

        // Revenue & Commission Trend Chart Data
        [ObservableProperty]
        private ObservableCollection<TrendDataPoint> _revenueTrendData = new();

        [ObservableProperty]
        private ObservableCollection<TrendDataPoint> _commissionTrendData = new();

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

                // Map Platform-Owner KPIs (temporary mapping using existing data)
                TotalGmvThisMonth = summary.MonthRevenue; // Total GMV across platform
                AdminCommission = Math.Round(summary.MonthRevenue * 0.05m, 2); // 5% commission
                ActiveSalesAgents = 127; // Mock value - replace with summary.ActiveSalesAgentsCount when available
                ItemsToReview = summary.LowStockProducts.Count + 8; // Flagged + pending items
                
                // Map Summary Statistics (kept for backwards compatibility)
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

                // Mock Top SalesAgents data (until real repository methods are available)
                TopSalesAgents = new ObservableCollection<TopSalesAgentItem>
                {
                    new() { 
                        Name = "Michael Chen", 
                        Email = "michael.chen@example.com", 
                        Avatar = "ms-appx:///Assets/Avatars/avatar1.png", 
                        GMV = 127450m, 
                        Commission = 6372.50m, 
                        Rating = 4.9, 
                        Status = "Active" 
                    },
                    new() { 
                        Name = "Sarah Johnson", 
                        Email = "sarah.johnson@example.com", 
                        Avatar = "ms-appx:///Assets/Avatars/avatar2.png", 
                        GMV = 98320m, 
                        Commission = 4916.00m, 
                        Rating = 4.8, 
                        Status = "Active" 
                    },
                    new() { 
                        Name = "David Park", 
                        Email = "david.park@example.com", 
                        Avatar = "ms-appx:///Assets/Avatars/avatar3.png", 
                        GMV = 87650m, 
                        Commission = 4382.50m, 
                        Rating = 4.7, 
                        Status = "Active" 
                    },
                    new() { 
                        Name = "Emma Wilson", 
                        Email = "emma.wilson@example.com", 
                        Avatar = "ms-appx:///Assets/Avatars/avatar4.png", 
                        GMV = 76890m, 
                        Commission = 3844.50m, 
                        Rating = 4.9, 
                        Status = "Active" 
                    },
                    new() { 
                        Name = "James Lee", 
                        Email = "james.lee@example.com", 
                        Avatar = "ms-appx:///Assets/Avatars/avatar5.png", 
                        GMV = 65430m, 
                        Commission = 3271.50m, 
                        Rating = 4.6, 
                        Status = "Active" 
                    }
                };

                // Mock Flagged Products data
                FlaggedProducts = new ObservableCollection<FlaggedProductItem>
                {
                    new() { 
                        Name = "iPhone 14 Pro Max", 
                        Agent = "Michael Chen", 
                        Category = "Smartphones", 
                        State = "Pending Review" 
                    },
                    new() { 
                        Name = "Samsung Galaxy S23 Ultra", 
                        Agent = "Sarah Johnson", 
                        Category = "Smartphones", 
                        State = "Flagged" 
                    },
                    new() { 
                        Name = "MacBook Pro 16\"", 
                        Agent = "David Park", 
                        Category = "Laptops", 
                        State = "Pending Review" 
                    },
                    new() { 
                        Name = "Sony WH-1000XM5", 
                        Agent = "Emma Wilson", 
                        Category = "Audio", 
                        State = "Under Review" 
                    },
                    new() { 
                        Name = "iPad Pro 12.9\"", 
                        Agent = "James Lee", 
                        Category = "Tablets", 
                        State = "Pending Review" 
                    }
                };

                // Mock Revenue & Commission Trend Data (Jun-Nov)
                RevenueTrendData = new ObservableCollection<TrendDataPoint>
                {
                    new() { Label = "Jun", Value = 145000m },
                    new() { Label = "Jul", Value = 168000m },
                    new() { Label = "Aug", Value = 182000m },
                    new() { Label = "Sep", Value = 195000m },
                    new() { Label = "Oct", Value = 178000m },
                    new() { Label = "Nov", Value = 198500m }
                };

                CommissionTrendData = new ObservableCollection<TrendDataPoint>
                {
                    new() { Label = "Jun", Value = 7250m },
                    new() { Label = "Jul", Value = 8400m },
                    new() { Label = "Aug", Value = 9100m },
                    new() { Label = "Sep", Value = 9750m },
                    new() { Label = "Oct", Value = 8900m },
                    new() { Label = "Nov", Value = 9925m }
                };

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

public class TrendDataPoint
{
    public string Label { get; set; } = string.Empty;
    public decimal Value { get; set; }
}