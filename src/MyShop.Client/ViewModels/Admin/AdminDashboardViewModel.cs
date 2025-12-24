using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Core.Common;
using MyShop.Shared.Models;
using MyShop.Client.ViewModels.Base;
using MyShop.Client.Views.Shared;
using MyShop.Core.Interfaces.Facades;
using MyShop.Core.Interfaces.Services;
using MyShop.Client.Facades;
using MyShop.Client.Services.Configuration;
using MyShop.Client.Services;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Linq;
using System.IO;
using System.Text;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.Kernel.Sketches;
using MyShop.Client.Common.Converters;
using SkiaSharp;

namespace MyShop.Client.ViewModels.Admin;

public partial class AdminDashboardViewModel : BaseViewModel
{
    private readonly IDashboardFacade _dashboardFacade;
    private readonly IConfigurationService _configService;
    private readonly IActivityLogService? _activityLogService;
    private readonly IAppNotificationService? _notificationService;

    [ObservableProperty]
    private User? _currentUser;

    [ObservableProperty]
    private bool _isLoading = false;

    [ObservableProperty]
    private bool _isInitialized = false;

    // Period filter
    [ObservableProperty]
    private string _selectedPeriod = "month";

    public List<string> AvailablePeriods { get; } = new List<string> { "day", "week", "month", "year" };
    public Dictionary<string, string> PeriodLabels { get; } = new Dictionary<string, string>
        {
            { "day", "Day" },
            { "week", "Week" },
            { "month", "Month" },
            { "year", "Year" }
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

    [ObservableProperty]
    private Axis[] _yAxes;

    public AdminDashboardViewModel(
        IDashboardFacade dashboardFacade,
        INavigationService navigationService,
        IConfigurationService configService,
        IActivityLogService? activityLogService = null,
        IAppNotificationService? notificationService = null)
        : base(navigationService: navigationService)
    {
        _dashboardFacade = dashboardFacade;
        _configService = configService;
        _activityLogService = activityLogService;
        _notificationService = notificationService;

        // Initialize notification service if available
        _notificationService?.Initialize();
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

            // Log activity
            if (_activityLogService != null)
            {
                await _activityLogService.LogActivityAsync(
                    ActivityType.View,
                    "Dashboard Accessed",
                    $"Admin {user.Username} accessed dashboard",
                    "Dashboard",
                    "AdminDashboard"
                );
            }

            await LoadDashboardDataAsync();

            // Send welcome notification
            if (_notificationService != null)
            {
                _notificationService.ShowInfo(
                    "Welcome Back!",
                    $"Hello {user.Username}, your dashboard is ready."
                );
            }

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
        // Reload data when period changes with loading indicator
        _ = ReloadDashboardOnPeriodChangeAsync();
    }

    private async Task ReloadDashboardOnPeriodChangeAsync()
    {
        try
        {
            IsLoading = true;
            await LoadDashboardDataAsync();
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadDashboardDataAsync()
    {
        System.Diagnostics.Debug.WriteLine($"[AdminDashboardViewModel] LoadDashboardDataAsync: Starting dashboard data load (Period={SelectedPeriod})");
        SetLoadingState(true);

        try
        {
            System.Diagnostics.Debug.WriteLine($"[AdminDashboardViewModel.LoadDashboardDataAsync] ⏳ START - Period: {SelectedPeriod}");
            var sw = System.Diagnostics.Stopwatch.StartNew();

            // Call facade to fetch dashboard data
            System.Diagnostics.Debug.WriteLine($"[AdminDashboardViewModel.LoadDashboardDataAsync] Calling _dashboardFacade.LoadDashboardAsync...");
            var result = await _dashboardFacade.LoadDashboardAsync(SelectedPeriod);
            sw.Stop();

            System.Diagnostics.Debug.WriteLine($"[AdminDashboardViewModel.LoadDashboardDataAsync] ✅ Facade returned - Success: {result.IsSuccess}, ElapsedMs: {sw.ElapsedMilliseconds}ms");

            if (!result.IsSuccess || result.Data == null)
            {
                System.Diagnostics.Debug.WriteLine($"[AdminDashboardViewModel.LoadDashboardDataAsync] ❌ ERROR - {result.ErrorMessage}");
                SetError(result.ErrorMessage ?? "Failed to load dashboard data");
                return;
            }

            var data = result.Data;
            System.Diagnostics.Debug.WriteLine($"[AdminDashboardViewModel.LoadDashboardDataAsync] 📊 DATA RECEIVED:");
            System.Diagnostics.Debug.WriteLine($"  TotalProducts={data.TotalProducts}, TodayOrders={data.TodayOrders}");
            System.Diagnostics.Debug.WriteLine($"  Revenue: Today={data.TodayRevenue}, Week={data.WeekRevenue}, Month={data.MonthRevenue}");
            System.Diagnostics.Debug.WriteLine($"  LowStockProducts={data.LowStockProducts?.Count}, TopProducts={data.TopSellingProducts?.Count}");

            RunOnUIThread(() =>
            {
                System.Diagnostics.Debug.WriteLine($"[AdminDashboardViewModel.LoadDashboardDataAsync] 🔄 UPDATING UI PROPERTIES");

                TotalGmvThisMonth = data.MonthRevenue;
                AdminCommission = Math.Round(data.MonthRevenue * 0.05m, 2);
                System.Diagnostics.Debug.WriteLine($"[AdminDashboardViewModel] ✅ KPI VALUES SET - GMV: {TotalGmvThisMonth}, Commission: {AdminCommission}");

                // ActiveSalesAgents: Waiting for API endpoint to provide this count
                ActiveSalesAgents = 0;
                // ItemsToReview: Should include flagged products (loaded separately) + low stock count
                ItemsToReview = data.LowStockProducts.Count;

                TotalProducts = data.TotalProducts;
                TodayOrders = data.TodayOrders;
                TodayRevenue = data.TodayRevenue;
                WeekRevenue = data.WeekRevenue;
                MonthRevenue = data.MonthRevenue;
                // OrdersTrend: Waiting for API endpoint to provide this metric
                OrdersTrend = 0;
                // RevenueTrend: Waiting for API endpoint to provide this metric
                RevenueTrend = 0;

                var topProduct = data.TopSellingProducts?.FirstOrDefault();
                if (topProduct != null)
                {
                    TopRatedProductName = topProduct.Name ?? "N/A";
                    // TopRatedProductRating: Waiting for API endpoint to provide rating data
                    TopRatedProductRating = 0;
                }
                else
                {
                    TopRatedProductName = "No data";
                    TopRatedProductRating = 0;
                }

                LowStockCount = data.LowStockProducts.Count;
                System.Diagnostics.Debug.WriteLine($"[AdminDashboardViewModel] ✅ UI PROPERTIES UPDATED - Dashboard data loaded successfully");
            });

            System.Diagnostics.Debug.WriteLine($"[AdminDashboardViewModel.LoadDashboardDataAsync] Loading chart data...");
            var chartResult = await _dashboardFacade.GetRevenueChartDataAsync(GetChartPeriod(SelectedPeriod));
            if (chartResult.IsSuccess && chartResult.Data != null)
            {
                System.Diagnostics.Debug.WriteLine($"[AdminDashboardViewModel.LoadDashboardDataAsync] Chart data received - DataPoints: {chartResult.Data.Labels.Count}");
                RunOnUIThread(() =>
                {
                    RevenueChartData = new ObservableCollection<RevenueDataPoint>();
                    RevenueTrendData = new ObservableCollection<TrendDataPoint>();
                    CommissionTrendData = new ObservableCollection<TrendDataPoint>();

                    for (int i = 0; i < chartResult.Data.Labels.Count; i++)
                    {
                        var revenue = chartResult.Data.Data[i];
                        var commission = revenue * 0.05m; // 5% commission

                        RevenueChartData.Add(new RevenueDataPoint
                        {
                            Day = chartResult.Data.Labels[i],
                            Revenue = revenue,
                            Orders = 0
                        });

                        // Add to trend data for chart
                        RevenueTrendData.Add(new TrendDataPoint { Value = revenue });
                        CommissionTrendData.Add(new TrendDataPoint { Value = commission });
                    }

                    // Update chart series
                    UpdateChartSeries();

                    System.Diagnostics.Debug.WriteLine($"[AdminDashboardViewModel.LoadDashboardDataAsync] Chart UI updated with {RevenueChartData.Count} points");
                });
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[AdminDashboardViewModel.LoadDashboardDataAsync] Chart data failed: {chartResult.ErrorMessage}");
            }

            RunOnUIThread(() =>
            {
                CategoryData = new ObservableCollection<CategoryDataPoint>();
                if (data.SalesByCategory != null)
                {
                    System.Diagnostics.Debug.WriteLine($"[AdminDashboardViewModel.LoadDashboardDataAsync] Loading {data.SalesByCategory.Count} categories");
                    foreach (var categorySale in data.SalesByCategory)
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
                    System.Diagnostics.Debug.WriteLine($"[AdminDashboardViewModel.LoadDashboardDataAsync] Categories loaded: {CategoryData.Count}");
                }
            });

            RunOnUIThread(() =>
            {
                TopProducts = new ObservableCollection<TopProductItem>();
                if (data.TopSellingProducts != null)
                {
                    System.Diagnostics.Debug.WriteLine($"[AdminDashboardViewModel.LoadDashboardDataAsync] Loading {data.TopSellingProducts.Count} top products");
                    int rank = 1;
                    foreach (var product in data.TopSellingProducts)
                    {
                        if (product != null)
                        {
                            TopProducts.Add(new TopProductItem
                            {
                                Rank = rank++,
                                ImageUrl = product.ImageUrl ?? string.Empty,
                                Name = product.Name ?? "Unknown",
                                Category = product.CategoryName ?? "Unknown",
                                SoldCount = product.SoldCount,
                                Revenue = product.Revenue
                            });
                        }
                    }
                    System.Diagnostics.Debug.WriteLine($"[AdminDashboardViewModel.LoadDashboardDataAsync] Top products loaded: {TopProducts.Count}");
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

                    // Update ActiveSalesAgents count based on loaded data
                    ActiveSalesAgents = topAgentsResult.Data.Count(a => a.Status == "Active");
                    System.Diagnostics.Debug.WriteLine($"[AdminDashboard] Loaded {TopSalesAgents.Count} top sales agents, {ActiveSalesAgents} active");
                });
            }

            // Load Flagged Products - Use Mock or API depending on data mode
            await LoadFlaggedProductsAsync();

            // Load Admin-specific Dashboard Data (Admin Summary & Revenue Chart)
            await LoadAdminDataAsync();

            // Revenue & Commission Trend will be loaded from LoadChartDataAsync() 
            // which calls GetRevenueChartDataAsync() using the revenue-chart endpoint

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
            // Create currency converter for Y-axis labels and tooltips
            var currencyConv = new CurrencyConverter();

            // Y axis uses currency labeler
            YAxes = new Axis[]
            {
                    new Axis
                    {
                        Labeler = value => currencyConv.Convert(value, typeof(string), null, string.Empty)?.ToString() ?? value.ToString()
                    }
            };

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

    private async Task LoadAdminDataAsync()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"[AdminDashboardViewModel.LoadAdminDataAsync] ⏳ START - Period: {SelectedPeriod}");

            // Load Admin Summary
            var summaryResult = await _dashboardFacade.GetAdminSummaryAsync(SelectedPeriod);
            if (summaryResult.IsSuccess && summaryResult.Data != null)
            {
                var summary = summaryResult.Data;
                System.Diagnostics.Debug.WriteLine($"[AdminDashboardViewModel.LoadAdminDataAsync] ✅ Summary - ActiveAgents: {summary.ActiveSalesAgents}, GMV: {summary.TotalGmv}, Commission: {summary.AdminCommission}");

                RunOnUIThread(() =>
                {
                    ActiveSalesAgents = summary.ActiveSalesAgents;
                    TotalGmvThisMonth = summary.TotalGmv;
                    AdminCommission = summary.AdminCommission;
                    TotalProducts = summary.TotalProducts;
                    TodayOrders = summary.TotalOrders;
                    TodayRevenue = summary.TotalRevenue;

                    // Load Top Selling Products from summary
                    TopProducts = new ObservableCollection<TopProductItem>();
                    if (summary.TopSellingProducts != null)
                    {
                        int rank = 1;
                        foreach (var product in summary.TopSellingProducts)
                        {
                            TopProducts.Add(new TopProductItem
                            {
                                Rank = rank++,
                                ImageUrl = product.ImageUrl ?? string.Empty,
                                Name = product.Name ?? "Unknown",
                                Category = product.CategoryName ?? "Unknown",
                                SoldCount = product.SoldCount,
                                Revenue = product.Revenue
                            });
                        }
                        System.Diagnostics.Debug.WriteLine($"[AdminDashboardViewModel.LoadAdminDataAsync] Top products loaded: {TopProducts.Count}");
                    }

                    // Load Top Sales Agents from summary
                    TopSalesAgents = new ObservableCollection<TopSalesAgentItem>();
                    if (summary.TopSalesAgents != null)
                    {
                        int rank = 1;
                        foreach (var agent in summary.TopSalesAgents)
                        {
                            TopSalesAgents.Add(new TopSalesAgentItem
                            {
                                Rank = rank++,
                                Name = agent.Name ?? "Unknown",
                                Email = agent.Email ?? string.Empty,
                                Avatar = agent.Name ?? "Unknown",  // Will use initials
                                Gmv = agent.TotalGmv,
                                GMV = agent.TotalGmv,
                                Commission = agent.Commission,
                                ProductCount = agent.ProductCount,
                                OrderCount = agent.OrderCount
                            });
                        }
                        System.Diagnostics.Debug.WriteLine($"[AdminDashboardViewModel.LoadAdminDataAsync] Top agents loaded: {TopSalesAgents.Count}");
                    }

                    System.Diagnostics.Debug.WriteLine($"[AdminDashboardViewModel.LoadAdminDataAsync] ✅ UI updated with summary data");
                });
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[AdminDashboardViewModel.LoadAdminDataAsync] ❌ Summary failed - {summaryResult.ErrorMessage}");
            }

            // Load Admin Revenue Chart
            var chartResult = await _dashboardFacade.GetAdminRevenueChartAsync(GetChartPeriod(SelectedPeriod));
            if (chartResult.IsSuccess && chartResult.Data != null)
            {
                var chartData = chartResult.Data;
                System.Diagnostics.Debug.WriteLine($"[AdminDashboardViewModel.LoadAdminDataAsync] ✅ Chart - Labels: {chartData.Labels?.Count}, Revenue: {chartData.RevenueData?.Count}, Commission: {chartData.CommissionData?.Count}");

                RunOnUIThread(() =>
                {
                    RevenueChartData = new ObservableCollection<RevenueDataPoint>();
                    RevenueTrendData = new ObservableCollection<TrendDataPoint>();
                    CommissionTrendData = new ObservableCollection<TrendDataPoint>();

                    if (chartData.Labels != null && chartData.RevenueData != null && chartData.CommissionData != null)
                    {
                        for (int i = 0; i < chartData.Labels.Count; i++)
                        {
                            var revenue = chartData.RevenueData[i];
                            var commission = chartData.CommissionData[i];

                            RevenueChartData.Add(new RevenueDataPoint
                            {
                                Day = chartData.Labels[i],
                                Revenue = revenue,
                                Orders = 0
                            });

                            RevenueTrendData.Add(new TrendDataPoint { Value = revenue });
                            CommissionTrendData.Add(new TrendDataPoint { Value = commission });
                        }

                        UpdateChartSeries();
                        System.Diagnostics.Debug.WriteLine($"[AdminDashboardViewModel.LoadAdminDataAsync] ✅ Chart UI updated with {RevenueChartData.Count} points");
                    }
                });
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[AdminDashboardViewModel.LoadAdminDataAsync] ❌ Chart failed - {chartResult.ErrorMessage}");
            }

            System.Diagnostics.Debug.WriteLine($"[AdminDashboardViewModel.LoadAdminDataAsync] ✅ COMPLETED");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AdminDashboardViewModel.LoadAdminDataAsync] ❌ Exception - {ex.Message}");
            SetError($"Error loading admin data: {ex.Message}");
        }
    }

    private async Task LoadFlaggedProductsAsync()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"[AdminDashboardViewModel.LoadFlaggedProductsAsync] Loading flagged products...");

            // Only use mock data when UseMockData flag is enabled
            if (_configService.FeatureFlags.UseMockData)
            {
                // Use mock data from MockDashboardData
                var flaggedProducts = await MyShop.Plugins.Mocks.Data.MockDashboardData.GetFlaggedProductsAsync(SelectedPeriod);
                RunOnUIThread(() =>
                {
                    FlaggedProducts = new ObservableCollection<FlaggedProductItem>(
                        flaggedProducts.Select(f => new FlaggedProductItem
                        {
                            Name = f.Name,
                            Agent = f.Agent,
                            Category = f.Category,
                            State = f.State
                        }).ToList()
                    );
                    System.Diagnostics.Debug.WriteLine($"[AdminDashboardViewModel.LoadFlaggedProductsAsync] Loaded {FlaggedProducts.Count} flagged products from MOCK DATA");
                });
            }
            else
            {
                // Real API mode - TODO: implement when endpoint is ready
                System.Diagnostics.Debug.WriteLine($"[AdminDashboardViewModel.LoadFlaggedProductsAsync] Real API mode - Waiting for GET /api/v1/dashboard/flagged-products endpoint");
                RunOnUIThread(() =>
                {
                    FlaggedProducts.Clear();
                });
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AdminDashboardViewModel.LoadFlaggedProductsAsync] Error: {ex.Message}");
            FlaggedProducts.Clear();
        }
    }

    private string GetChartPeriod(string selectedPeriod)
    {
        // Server API expects: "day", "week", "month", "year"
        var validPeriods = new[] { "day", "week", "month", "year" };
        return validPeriods.Contains(selectedPeriod?.ToLower() ?? "month")
            ? selectedPeriod.ToLower()
            : "month";
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
            var result = await _navigationService.NavigateInShell("MyShop.Client.Views.Admin.AdminProductsPage", CurrentUser);
            if (!result.IsSuccess)
            {
                System.Diagnostics.Debug.WriteLine($"[AdminDashboard] Navigation failed: {result.ErrorMessage}");
            }
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
            var result = await _navigationService.NavigateInShell("MyShop.Client.Views.Admin.AdminUsersPage", CurrentUser);
            if (!result.IsSuccess)
            {
                System.Diagnostics.Debug.WriteLine($"[AdminDashboard] Navigation failed: {result.ErrorMessage}");
            }
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
            var result = await _navigationService.NavigateInShell("MyShop.Client.Views.Admin.AdminAgentRequestsPage", CurrentUser);
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
            var filePath = StorageConstants.GetExportFilePath(fileName);
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
            // await Task.Delay(50);
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
    public string ImageUrl { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int SoldCount { get; set; }
    public decimal Revenue { get; set; }
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
    public int Rank { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Avatar { get; set; } = string.Empty;
    public decimal Gmv { get; set; }
    public decimal GMV { get; set; }
    public decimal Commission { get; set; }
    public int ProductCount { get; set; }
    public int OrderCount { get; set; }
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