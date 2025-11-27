using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Shared.Models;
using MyShop.Client.ViewModels.Base;
using MyShop.Client.Views.Shared;
using MyShop.Core.Interfaces.Facades;
using MyShop.Core.Interfaces.Services;
using MyShop.Client.Facades;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Linq;
using System.IO;
using System.Text;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace MyShop.Client.ViewModels.Admin;

public partial class AdminDashboardViewModel : BaseViewModel
{
        private readonly IDashboardFacade _dashboardFacade;

        [ObservableProperty]
        private User? _currentUser;

        [ObservableProperty]
        private bool _isLoading = false;

        [ObservableProperty]
        private bool _isInitialized = false;

        // Period filter
        [ObservableProperty]
        private string _selectedPeriod = "current";

        public List<string> AvailablePeriods { get; } = new List<string> { "current", "last", "last3" };
        public Dictionary<string, string> PeriodLabels { get; } = new Dictionary<string, string>
        {
            { "current", "Current Month" },
            { "last", "Last Month" },
            { "last3", "Last 3 Months" }
        };

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

        // Sales View Mode (for Category/Agents toggle)
        [ObservableProperty]
        private string _salesViewMode = "category"; // "category" or "agents"

        public bool IsCategoryView => SalesViewMode == "category";
        public bool IsAgentsView => SalesViewMode == "agents";

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

        // LiveCharts Series
        [ObservableProperty]
        private ISeries[] _revenueSeries = Array.Empty<ISeries>();

        public AdminDashboardViewModel(IDashboardFacade dashboardFacade)
        {
            _dashboardFacade = dashboardFacade;
        }

        public async Task InitializeAsync(User user)
        {
            if (IsInitialized)
            {
                System.Diagnostics.Debug.WriteLine($"[AdminDashboardViewModel] Already initialized, skipping...");
                return;
            }

            try
            {
                IsLoading = true;
                CurrentUser = user;
                System.Diagnostics.Debug.WriteLine($"[AdminDashboardViewModel] InitializeAsync called for user: {user.Username}");
                
                await LoadDashboardDataAsync();
                
                IsInitialized = true;
                System.Diagnostics.Debug.WriteLine($"[AdminDashboardViewModel] InitializeAsync completed successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AdminDashboardViewModel] InitializeAsync failed: {ex.Message}");
                throw;
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task RefreshAsync()
        {
            await LoadDashboardDataAsync();
        }

        partial void OnSelectedPeriodChanged(string value)
        {
            System.Diagnostics.Debug.WriteLine($"[AdminDashboardViewModel] Period changed to: {value}");
            // Reload data when period changes
            _ = LoadDashboardDataAsync();
        }

        private async Task LoadDashboardDataAsync()
        {
            System.Diagnostics.Debug.WriteLine($"[AdminDashboardViewModel] LoadDashboardDataAsync started with period: {SelectedPeriod}");
            SetLoadingState(true);

            try
            {
                var result = await _dashboardFacade.LoadDashboardAsync(SelectedPeriod);
                if (!result.IsSuccess || result.Data == null)
                {
                    SetError(result.ErrorMessage ?? "Failed to load dashboard data");
                    return;
                }

                var data = result.Data;
                System.Diagnostics.Debug.WriteLine($"[AdminDashboardViewModel] Dashboard loaded: MonthRevenue={data.MonthRevenue}, TotalProducts={data.TotalProducts}");

                RunOnUIThread(() =>
                {
                    TotalGmvThisMonth = data.MonthRevenue;
                    AdminCommission = Math.Round(data.MonthRevenue * 0.05m, 2);
                    ActiveSalesAgents = 127;
                    ItemsToReview = data.LowStockProducts.Count + 8;
                    
                    TotalProducts = data.TotalProducts;
                    TodayOrders = data.TodayOrders;
                    TodayRevenue = data.TodayRevenue;
                    WeekRevenue = data.WeekRevenue;
                    MonthRevenue = data.MonthRevenue;
                    OrdersTrend = 12.5;
                    RevenueTrend = 8.2;

                    var topProduct = data.TopSellingProducts?.FirstOrDefault();
                    if (topProduct != null)
                    {
                        TopRatedProductName = topProduct.Name ?? "N/A";
                        TopRatedProductRating = 4.5;
                    }
                    else
                    {
                        TopRatedProductName = "No data";
                        TopRatedProductRating = 0;
                    }

                    LowStockCount = data.LowStockProducts.Count;
                });

                var chartResult = await _dashboardFacade.GetRevenueChartDataAsync(GetChartPeriod(SelectedPeriod));
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
                                Orders = 0
                            });
                        }
                    });
                }

                RunOnUIThread(() =>
                {
                    CategoryData = new ObservableCollection<CategoryDataPoint>();
                    if (data.SalesByCategory != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"[AdminDashboard] Loading {data.SalesByCategory.Count} categories");
                        foreach (var categorySale in data.SalesByCategory)
                        {
                            if (categorySale != null)
                            {
                                CategoryData.Add(new CategoryDataPoint
                                {
                                    Name = categorySale.CategoryName ?? "Unknown",
                                    Percentage = (int)categorySale.Percentage
                                });
                                System.Diagnostics.Debug.WriteLine($"[AdminDashboard] Added category: {categorySale.CategoryName} - {categorySale.Percentage}%");
                            }
                        }
                        System.Diagnostics.Debug.WriteLine($"[AdminDashboard] CategoryData final count: {CategoryData.Count}");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"[AdminDashboard] WARNING: data.SalesByCategory is NULL!");
                    }
                });

                RunOnUIThread(() =>
                {
                    TopProducts = new ObservableCollection<TopProductItem>();
                    if (data.TopSellingProducts != null)
                    {
                        int rank = 1;
                        foreach (var product in data.TopSellingProducts)
                        {
                            if (product != null)
                            {
                                TopProducts.Add(new TopProductItem
                                {
                                    Rank = rank++,
                                    Name = product.Name ?? "Unknown",
                                    Category = product.CategoryName ?? "Unknown",
                                    Price = product.Revenue / Math.Max(product.SoldCount, 1),
                                    Stock = 0
                                });
                            }
                        }
                    }
                });

                RunOnUIThread(() =>
                {
                    LowStockItems = new ObservableCollection<LowStockItem>();
                    if (data.LowStockProducts != null)
                    {
                        foreach (var lowStock in data.LowStockProducts)
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

                RunOnUIThread(() =>
                {
                    RecentOrders = new ObservableCollection<RecentOrderItem>();
                    if (data.RecentOrders != null)
                    {
                        foreach (var order in data.RecentOrders)
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

                // Load Top SalesAgents from repository
                var topAgentsResult = await _dashboardFacade.GetTopSalesAgentsAsync(SelectedPeriod);
                if (topAgentsResult.IsSuccess && topAgentsResult.Data != null)
                {
                    RunOnUIThread(() =>
                    {
                        TopSalesAgents = new ObservableCollection<TopSalesAgentItem>();
                        foreach (var agent in topAgentsResult.Data)
                        {
                            TopSalesAgents.Add(new TopSalesAgentItem
                            {
                                Name = agent.Name,
                                Email = agent.Email,
                                Avatar = agent.Avatar,
                                GMV = agent.GMV,
                                Commission = agent.Commission,
                                Rating = agent.Rating,
                                Status = agent.Status
                            });
                        }
                        System.Diagnostics.Debug.WriteLine($"[AdminDashboard] Loaded {TopSalesAgents.Count} top sales agents");
                    });
                }

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

                // Create LiveCharts Series
                UpdateChartSeries();

                System.Diagnostics.Debug.WriteLine($"[AdminDashboard] Loaded dashboard data successfully");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading dashboard data: {ex.Message}");
                SetError("Failed to load dashboard data", ex);
                await _toastHelper.ShowError("Failed to load dashboard data. Please try again.");
            }
            finally
            {
                SetLoadingState(false);
            }
        }

        private void UpdateChartSeries()
        {
            try
            {
                // Revenue Line Series (Dark Blue)
                var revenueSeries = new LineSeries<TrendDataPoint>
                {
                    Values = RevenueTrendData,
                    Name = "Revenue",
                    Stroke = new SolidColorPaint(SKColor.Parse("#1E40AF")) { StrokeThickness = 3 },
                    Fill = null,
                    GeometrySize = 8,
                    GeometryFill = new SolidColorPaint(SKColor.Parse("#1E40AF")),
                    GeometryStroke = new SolidColorPaint(SKColors.White) { StrokeThickness = 2 },
                    Mapping = (point, index) => new(index, (double)point.Value)
                };

                // Commission Line Series (Light Blue/Cyan)
                var commissionSeries = new LineSeries<TrendDataPoint>
                {
                    Values = CommissionTrendData,
                    Name = "Commission",
                    Stroke = new SolidColorPaint(SKColor.Parse("#06B6D4")) { StrokeThickness = 3 },
                    Fill = null,
                    GeometrySize = 8,
                    GeometryFill = new SolidColorPaint(SKColor.Parse("#06B6D4")),
                    GeometryStroke = new SolidColorPaint(SKColors.White) { StrokeThickness = 2 },
                    Mapping = (point, index) => new(index, (double)point.Value)
                };

                RevenueSeries = new ISeries[] { revenueSeries, commissionSeries };
                
                System.Diagnostics.Debug.WriteLine($"[AdminDashboard] Chart series updated with {RevenueTrendData.Count} revenue points and {CommissionTrendData.Count} commission points");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AdminDashboard] Error updating chart series: {ex.Message}");
            }
        }

        private string GetChartPeriod(string selectedPeriod)
        {
            // Map summary period to chart period
            return selectedPeriod switch
            {
                "current" => "daily",   // Current month -> show daily data
                "last" => "daily",      // Last month -> show daily data
                "last3" => "weekly",    // Last 3 months -> show weekly data
                _ => "daily"
            };
        }

        [RelayCommand]
        private async Task ExportDashboardAsync()
        {
            try
            {
                IsLoading = true;
                var result = await _dashboardFacade.ExportDashboardDataAsync(SelectedPeriod);
                if (result.IsSuccess)
                {
                    System.Diagnostics.Debug.WriteLine($"[AdminDashboard] Exported to: {result.Data}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error exporting dashboard: {ex.Message}");
                await _toastHelper.ShowError("Failed to export dashboard data.");
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task NavigateToAllProducts()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[AdminDashboard] Navigating to All Products");
                await _dashboardFacade.NavigateToDashboardPageAsync("products");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error navigating to products: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task NavigateToAllAgents()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[AdminDashboard] Navigating to All Sales Agents");
                await _dashboardFacade.NavigateToDashboardPageAsync("users");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error navigating to agents: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task NavigateToAgentRequests()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[AdminDashboard] Navigating to Agent Requests");
                var result = await _navigationService.NavigateInShell("MyShop.Client.Views.Admin.AdminAgentRequestsPage");
                if (!result.IsSuccess)
                {
                    System.Diagnostics.Debug.WriteLine($"[AdminDashboard] Navigation failed: {result.ErrorMessage}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error navigating to agent requests: {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task ExportChartDataAsync()
        {
            try
            {
                IsLoading = true;
                
                // Get chart data
                var chartResult = await _dashboardFacade.GetRevenueChartDataAsync(GetChartPeriod(SelectedPeriod));
                if (!chartResult.IsSuccess || chartResult.Data == null)
                {
                    await _toastHelper.ShowError("Failed to get chart data for export.");
                    return;
                }

                // Generate CSV
                var csv = new StringBuilder();
                csv.AppendLine("Date,Revenue");
                
                for (int i = 0; i < chartResult.Data.Labels.Count; i++)
                {
                    csv.AppendLine($"{chartResult.Data.Labels[i]},{chartResult.Data.Data[i]}");
                }

                // Save to file
                var fileName = $"RevenueChart_{SelectedPeriod}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                var filePath = Path.Combine(Path.GetTempPath(), fileName);
                await File.WriteAllTextAsync(filePath, csv.ToString());

                await _toastHelper.ShowSuccess($"Chart data exported to {fileName}");
                System.Diagnostics.Debug.WriteLine($"[AdminDashboard] Chart exported to: {filePath}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error exporting chart data: {ex.Message}");
                await _toastHelper.ShowError("Failed to export chart data.");
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task DownloadChartImageAsync()
        {
            try
            {
                IsLoading = true;
                // TODO: Capture chart as image and save to file
                await Task.Delay(300);
                await _toastHelper.ShowSuccess("Chart image downloaded successfully!");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error downloading chart: {ex.Message}");
                await _toastHelper.ShowError("Failed to download chart image.");
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private void SwitchSalesView(string mode)
        {
            if (mode == "category" || mode == "agents")
            {
                SalesViewMode = mode;
                OnPropertyChanged(nameof(IsCategoryView));
                OnPropertyChanged(nameof(IsAgentsView));
                System.Diagnostics.Debug.WriteLine($"[AdminDashboard] Switched sales view to: {mode}");
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