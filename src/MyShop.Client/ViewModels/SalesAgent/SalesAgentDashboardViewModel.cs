using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Shared.Models;
using MyShop.Client.ViewModels.Base;
using MyShop.Client.Views.Shared;
using MyShop.Core.Interfaces.Facades;
using MyShop.Core.Interfaces.Services;
using MyShop.Client.Facades;
using MyShop.Client.Services;
using MyShop.Core.Common;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace MyShop.Client.ViewModels.SalesAgent;

public partial class SalesAgentDashboardViewModel : BaseViewModel
{
    private new readonly INavigationService _navigationService;
    private readonly IDashboardFacade _dashboardFacade;
    private readonly IProfileFacade _profileFacade;
    private readonly ExportService _exportService = new();

    [ObservableProperty]
    private User? _currentUser;

    [ObservableProperty]
    private string _title = "Sales Agent Dashboard";

    [ObservableProperty]
    private bool _isVerified = true;

    [ObservableProperty]
    private bool _profileCompleted = true;

    // KPI Cards
    [ObservableProperty]
    private int _totalProducts = 0;

    [ObservableProperty]
    private int _totalSales = 0;

    [ObservableProperty]
    private decimal _totalCommission = 0m;

    [ObservableProperty]
    private decimal _totalRevenue = 0m;

    // Date Range Filter (same pattern as Admin)
    [ObservableProperty]
    private int _selectedDateRangeIndex = 0;

    public string[] DateRangeOptions { get; } = new[] { "Day", "Week", "Month", "Year" };

    // Map UI selection to Backend API periods
    // Backend expects: "day", "week", "month", "year"
    private string GetSelectedPeriod() => SelectedDateRangeIndex switch
    {
        0 => "day",       // day
        1 => "week",      // week
        2 => "month",     // month
        3 => "year",      // year
        _ => "month"
    };

    // Map UI selection to Chart period for revenue visualization
    private string GetChartPeriod() => SelectedDateRangeIndex switch
    {
        0 => "day",       // day
        1 => "week",      // week
        2 => "month",     // month
        3 => "year",      // year
        _ => "month"
    };

    // LiveCharts Series (same as Admin)
    [ObservableProperty]
    private ObservableCollection<TrendDataPoint> _revenueTrendData = new();

    [ObservableProperty]
    private ObservableCollection<TrendDataPoint> _commissionTrendData = new();

    [ObservableProperty]
    private ISeries[] _revenueSeries = Array.Empty<ISeries>();

    // Top Selling Products
    [ObservableProperty]
    private ObservableCollection<TopAffiliateLink> _topLinks = new();

    // Low Stock Products
    [ObservableProperty]
    private ObservableCollection<LowStockProduct> _lowStockProducts = new();

    // Recent Orders
    [ObservableProperty]
    private ObservableCollection<RecentSalesOrder> _recentOrders = new();

    public SalesAgentDashboardViewModel(
        INavigationService navigationService,
        IDashboardFacade dashboardFacade,
        IProfileFacade profileFacade)
    {
        _navigationService = navigationService;
        _dashboardFacade = dashboardFacade;
        _profileFacade = profileFacade;
    }

    public void Initialize(User user)
    {
        try
        {
            LoggingService.Instance.LogViewModelEvent(
                nameof(SalesAgentDashboardViewModel),
                "Initialize",
                $"User: {user.Username}, Roles: {string.Join(", ", user.Roles)}"
            );

            CurrentUser = user;
            IsVerified = user.IsEmailVerified;

            if (!user.IsEmailVerified)
            {
                LoggingService.Instance.Warning($"User {user.Username} email not verified");
            }

            _ = LoadDashboardDataAsync();
        }
        catch (Exception ex)
        {
            LoggingService.Instance.Error(
                $"Failed to initialize {nameof(SalesAgentDashboardViewModel)}",
                ex
            );
            GlobalExceptionHandler.LogException(ex, "SalesAgentDashboardViewModel.Initialize");
            throw;
        }
    }

    private async Task LoadDashboardDataAsync()
    {
        try
        {
            var period = GetSelectedPeriod();
            System.Diagnostics.Debug.WriteLine($"[SalesAgentDashboardViewModel.LoadDashboardDataAsync] START - Period: {period}");
            SetLoadingState(true);

            var sw = System.Diagnostics.Stopwatch.StartNew();
            System.Diagnostics.Debug.WriteLine($"[SalesAgentDashboardViewModel.LoadDashboardDataAsync] Calling facade.LoadDashboardAsync({period})");

            var result = await _dashboardFacade.LoadDashboardAsync(period);
            sw.Stop();

            System.Diagnostics.Debug.WriteLine($"[SalesAgentDashboardViewModel.LoadDashboardDataAsync] Facade returned - Success: {result.IsSuccess}, ElapsedMs: {sw.ElapsedMilliseconds}");

            if (!result.IsSuccess || result.Data == null)
            {
                System.Diagnostics.Debug.WriteLine($"[SalesAgentDashboardViewModel.LoadDashboardDataAsync] ERROR - {result.ErrorMessage}");
                SetLoadingState(false);
                return;
            }

            var data = result.Data;

            // Support both API formats: Real API uses TotalOrders/TotalRevenue, Mock uses TodayOrders/MonthRevenue
            var orders = data.TotalOrders > 0 ? data.TotalOrders : data.TodayOrders;
            var revenue = data.TotalRevenue > 0 ? data.TotalRevenue : data.MonthRevenue;

            System.Diagnostics.Debug.WriteLine($"[SalesAgentDashboardViewModel.LoadDashboardDataAsync] Data: Products={data.TotalProducts}, Sales={orders}, Revenue={revenue}, TopProducts={data.TopSellingProducts?.Count}, RecentOrders={data.RecentOrders?.Count}");

            RunOnUIThread(() =>
            {
                TotalProducts = data.TotalProducts;
                TotalSales = orders;
                TotalCommission = Math.Round(revenue * 0.05m, 2);
                TotalRevenue = revenue;

                TopLinks.Clear();
                if (data.TopSellingProducts != null)
                {
                    System.Diagnostics.Debug.WriteLine($"[SalesAgentDashboardViewModel.LoadDashboardDataAsync] Loading {data.TopSellingProducts.Count} top products");
                    foreach (var product in data.TopSellingProducts.Take(3))
                    {
                        TopLinks.Add(new TopAffiliateLink
                        {
                            Id = product.Id.ToString(),
                            Product = product.Name ?? "Unknown",
                            CategoryName = product.CategoryName ?? "Uncategorized",
                            SoldCount = product.SoldCount,
                            Revenue = product.Revenue,
                            ImageUrl = product.ImageUrl ?? string.Empty,
                            Status = "Active"
                        });
                    }
                    System.Diagnostics.Debug.WriteLine($"[SalesAgentDashboardViewModel.LoadDashboardDataAsync] Top links loaded: {TopLinks.Count}");
                }

                LowStockProducts.Clear();
                if (data.LowStockProducts != null && data.LowStockProducts.Count > 0)
                {
                    System.Diagnostics.Debug.WriteLine($"[SalesAgentDashboardViewModel.LoadDashboardDataAsync] Loading {data.LowStockProducts.Count} low stock products");
                    foreach (var product in data.LowStockProducts)
                    {
                        LowStockProducts.Add(new LowStockProduct
                        {
                            Id = product.Id,
                            Name = product.Name ?? "Unknown",
                            CategoryName = product.CategoryName ?? "Uncategorized",
                            Quantity = product.Quantity,
                            ImageUrl = product.ImageUrl ?? "",
                            Status = product.Status ?? "0"
                        });
                    }
                    System.Diagnostics.Debug.WriteLine($"[SalesAgentDashboardViewModel.LoadDashboardDataAsync] Low stock products loaded: {LowStockProducts.Count}");
                }

                RecentOrders.Clear();
                if (data.RecentOrders != null)
                {
                    System.Diagnostics.Debug.WriteLine($"[SalesAgentDashboardViewModel.LoadDashboardDataAsync] Loading {data.RecentOrders.Count} recent orders");
                    foreach (var order in data.RecentOrders.Take(5))
                    {
                        RecentOrders.Add(new RecentSalesOrder
                        {
                            OrderId = $"ORD-{order.Id.ToString()[..8]}",
                            Customer = order.CustomerName ?? "Unknown",
                            Product = "Product",
                            OrderDate = order.OrderDate.ToString("yyyy-MM-dd"),
                            Amount = order.TotalAmount,
                            Commission = Math.Round(order.TotalAmount * 0.05m, 2),
                            Status = order.Status ?? "Pending"
                        });
                    }
                    System.Diagnostics.Debug.WriteLine($"[SalesAgentDashboardViewModel.LoadDashboardDataAsync] Recent orders loaded: {RecentOrders.Count}");
                }
            });

            System.Diagnostics.Debug.WriteLine($"[SalesAgentDashboardViewModel.LoadDashboardDataAsync] Loading chart data...");
            await LoadChartDataAsync();

            SetLoadingState(false);
            System.Diagnostics.Debug.WriteLine($"[SalesAgentDashboardViewModel.LoadDashboardDataAsync] COMPLETED");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[SalesAgentDashboardViewModel.LoadDashboardDataAsync] Exception - {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"[SalesAgentDashboardViewModel.LoadDashboardDataAsync] StackTrace: {ex.StackTrace}");

            SetLoadingState(false);
            TopLinks.Clear();
            RecentOrders.Clear();
            SetError("Failed to load dashboard data. Please try again.", ex);
        }
    }

    private async Task LoadChartDataAsync()
    {
        try
        {
            var chartPeriod = GetChartPeriod();  // Use GetChartPeriod() for chart visualization

            System.Diagnostics.Debug.WriteLine($"[SalesAgentDashboardViewModel.LoadChartDataAsync] START - ChartPeriod: {chartPeriod}");
            System.Diagnostics.Debug.WriteLine($"[SalesAgentDashboardViewModel.LoadChartDataAsync] Calling facade.GetRevenueChartDataAsync({chartPeriod})");

            var sw = System.Diagnostics.Stopwatch.StartNew();
            var chartResult = await _dashboardFacade.GetRevenueChartDataAsync(chartPeriod);
            sw.Stop();

            System.Diagnostics.Debug.WriteLine($"[SalesAgentDashboardViewModel.LoadChartDataAsync] Facade returned - Success: {chartResult.IsSuccess}, ElapsedMs: {sw.ElapsedMilliseconds}");

            RevenueTrendData.Clear();
            CommissionTrendData.Clear();

            if (chartResult.IsSuccess && chartResult.Data != null)
            {
                var labels = chartResult.Data.Labels;
                var values = chartResult.Data.Data;

                System.Diagnostics.Debug.WriteLine($"[SalesAgentDashboardViewModel.LoadChartDataAsync] Chart data: DataPoints={labels.Count}");

                for (int i = 0; i < Math.Min(labels.Count, values.Count); i++)
                {
                    var revenue = values[i];
                    var commission = revenue * 0.05m;

                    RevenueTrendData.Add(new TrendDataPoint
                    {
                        Date = DateTime.Now.AddDays(-labels.Count + i + 1),
                        Label = labels[i],
                        Value = Math.Round(revenue, 0)
                    });

                    CommissionTrendData.Add(new TrendDataPoint
                    {
                        Date = DateTime.Now.AddDays(-labels.Count + i + 1),
                        Label = labels[i],
                        Value = Math.Round(commission, 0)
                    });
                }
                System.Diagnostics.Debug.WriteLine($"[SalesAgentDashboardViewModel.LoadChartDataAsync] Chart UI updated with {RevenueTrendData.Count} points");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[SalesAgentDashboardViewModel.LoadChartDataAsync] Chart data failed: {chartResult.ErrorMessage}");
                // Fallback: generate mock data if API fails
                GenerateMockChartData();
            }

            UpdateChartSeries();
            System.Diagnostics.Debug.WriteLine($"[SalesAgentDashboardViewModel.LoadChartDataAsync] COMPLETED");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[SalesAgentDashboardViewModel.LoadChartDataAsync] Exception - {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"[SalesAgentDashboardViewModel.LoadChartDataAsync] StackTrace: {ex.StackTrace}");
            GenerateMockChartData();
            UpdateChartSeries();
        }
    }

    private void GenerateMockChartData()
    {
        var baseRevenue = TotalRevenue > 0 ? TotalRevenue / 30m : 10000m;
        var random = new Random(42); // Fixed seed for consistent data

        for (int i = 0; i < 30; i++)
        {
            var date = DateTime.Now.AddDays(-29 + i);
            var dayMultiplier = 0.7 + random.NextDouble() * 0.6;
            var revenue = baseRevenue * (decimal)dayMultiplier;
            var commission = revenue * 0.05m;

            RevenueTrendData.Add(new TrendDataPoint
            {
                Date = date,
                Label = date.ToString("MMM dd"),
                Value = Math.Round(revenue, 0)
            });

            CommissionTrendData.Add(new TrendDataPoint
            {
                Date = date,
                Label = date.ToString("MMM dd"),
                Value = Math.Round(commission, 0)
            });
        }
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadDashboardDataAsync();
    }

    partial void OnSelectedDateRangeIndexChanged(int value)
    {
        _ = LoadDashboardDataAsync();
    }

    // Export Dashboard with FileSavePicker (same pattern as Admin)
    [RelayCommand]
    private async Task ExportAsync()
    {
        try
        {
            SetLoadingState(true);
            LoggingService.Instance.Information("Exporting Sales Agent dashboard data...");

            var suggestedFileName = $"SalesAgentDashboard_{GetSelectedPeriod()}_{DateTime.Now:yyyyMMdd_HHmmss}";

            var result = await _exportService.ExportWithPickerAsync(suggestedFileName, csv =>
            {
                csv.AddTitle("Sales Agent Dashboard Export")
                   .AddMetadata("Period", DateRangeOptions[SelectedDateRangeIndex])
                   .AddMetadata("Generated", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
                   .AddBlankLine();

                // KPIs
                csv.AddHeader("KPI Summary")
                   .AddRow("Total Products", TotalProducts)
                   .AddRow("Total Sales", TotalSales)
                   .AddRow("Total Commission", $"${TotalCommission:N2}")
                   .AddRow("Total Revenue", $"${TotalRevenue:N2}")
                   .AddBlankLine();

                // Top Links
                csv.AddHeader("Top Selling Products")
                   .AddColumnHeaders("Product", "Category", "Sold Count", "Revenue");
                foreach (var link in TopLinks)
                {
                    csv.AddRow(link.Product, link.CategoryName, link.SoldCount.ToString(), $"${link.Revenue:N2}");
                }
                csv.AddBlankLine();

                // Recent Orders
                csv.AddHeader("Recent Orders")
                   .AddColumnHeaders("Order ID", "Customer", "Date", "Amount", "Status");
                foreach (var order in RecentOrders)
                {
                    csv.AddRow(order.OrderId, order.Customer, order.OrderDate, $"${order.Amount:N2}", order.Status);
                }
            });

            if (result.IsSuccess && !string.IsNullOrEmpty(result.Data))
            {
                LoggingService.Instance.Information($"Dashboard exported to: {result.Data}");
            }
            else if (string.IsNullOrEmpty(result.Data))
            {
                // User cancelled - no action needed
                LoggingService.Instance.Debug("Export cancelled by user");
            }
            else
            {
                LoggingService.Instance.Warning($"Export failed: {result.ErrorMessage}");
            }
        }
        catch (Exception ex)
        {
            LoggingService.Instance.Error("Export failed", ex);
        }
        finally
        {
            SetLoadingState(false);
        }
    }

    // Navigation Commands (same pattern as Admin)
    [RelayCommand]
    private async Task NavigateToMyProducts()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("[SalesAgentDashboard] Navigating to My Products");
            var result = await _navigationService.NavigateInShell("MyShop.Client.Views.SalesAgent.SalesAgentProductsPage", CurrentUser);
            if (!result.IsSuccess)
            {
                System.Diagnostics.Debug.WriteLine($"[SalesAgentDashboard] Navigation failed: {result.ErrorMessage}");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error navigating to products: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task NavigateToOrders()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("[SalesAgentDashboard] Navigating to Sales Orders");
            var result = await _navigationService.NavigateInShell("MyShop.Client.Views.SalesAgent.SalesAgentOrdersPage", CurrentUser);
            if (!result.IsSuccess)
            {
                System.Diagnostics.Debug.WriteLine($"[SalesAgentDashboard] Navigation failed: {result.ErrorMessage}");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error navigating to orders: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task NavigateToEarnings()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("[SalesAgentDashboard] Navigating to Earnings");
            var result = await _navigationService.NavigateInShell("MyShop.Client.Views.SalesAgent.SalesAgentEarningsPage", CurrentUser);
            if (!result.IsSuccess)
            {
                System.Diagnostics.Debug.WriteLine($"[SalesAgentDashboard] Navigation failed: {result.ErrorMessage}");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error navigating to earnings: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task NavigateToReports()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("[SalesAgentDashboard] Navigating to Reports");
            var result = await _navigationService.NavigateInShell("MyShop.Client.Views.SalesAgent.SalesAgentReportsPage", CurrentUser);
            if (!result.IsSuccess)
            {
                System.Diagnostics.Debug.WriteLine($"[SalesAgentDashboard] Navigation failed: {result.ErrorMessage}");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error navigating to reports: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task ViewAllOrders()
    {
        await NavigateToOrders();
    }

    private void UpdateChartSeries()
    {
        try
        {
            // Revenue Line Series (Blue - same as Admin)
            var revenueSeries = new LineSeries<TrendDataPoint>
            {
                Values = RevenueTrendData,
                Name = "Revenue",
                Stroke = new SolidColorPaint(SKColor.Parse("#3B82F6")) { StrokeThickness = 3 },
                Fill = null,
                GeometrySize = 6,
                GeometryFill = new SolidColorPaint(SKColor.Parse("#3B82F6")),
                GeometryStroke = new SolidColorPaint(SKColors.White) { StrokeThickness = 2 },
                Mapping = (point, index) => new(index, (double)point.Value)
            };

            // Commission Line Series (Green)
            var commissionSeries = new LineSeries<TrendDataPoint>
            {
                Values = CommissionTrendData,
                Name = "Commission",
                Stroke = new SolidColorPaint(SKColor.Parse("#10B981")) { StrokeThickness = 3 },
                Fill = null,
                GeometrySize = 6,
                GeometryFill = new SolidColorPaint(SKColor.Parse("#10B981")),
                GeometryStroke = new SolidColorPaint(SKColors.White) { StrokeThickness = 2 },
                Mapping = (point, index) => new(index, (double)point.Value)
            };

            RevenueSeries = new ISeries[] { revenueSeries, commissionSeries };

            LoggingService.Instance.Debug($"[SalesAgentDashboard] Chart series updated with {RevenueTrendData.Count} points");
        }
        catch (Exception ex)
        {
            LoggingService.Instance.Error("Error updating chart series", ex);
        }
    }

    [RelayCommand]
    private async Task LogoutAsync()
    {
        await _navigationService.NavigateTo(typeof(LoginPage).FullName!);
    }
}

// Helper classes for data binding
public class TopAffiliateLink
{
    public string Id { get; set; } = string.Empty;
    public string Product { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public int SoldCount { get; set; }
    public decimal Revenue { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}

public class RecentSalesOrder
{
    public string OrderId { get; set; } = string.Empty;
    public string Customer { get; set; } = string.Empty;
    public string Product { get; set; } = string.Empty;
    public string OrderDate { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal Commission { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class TrendDataPoint
{
    public DateTime Date { get; set; }
    public string Label { get; set; } = string.Empty;
    public decimal Value { get; set; }
}

public class LowStockProduct
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}