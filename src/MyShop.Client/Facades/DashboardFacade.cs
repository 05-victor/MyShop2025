using MyShop.Core.Common;
using MyShop.Core.Interfaces.Facades;
using MyShop.Core.Interfaces.Repositories;
using MyShop.Core.Interfaces.Services;
using MyShop.Shared.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace MyShop.Client.Facades;

/// <summary>
/// Full implementation of IDashboardFacade
/// Aggregates: IDashboardRepository, INavigationService, IToastService
/// </summary>
public class DashboardFacade : IDashboardFacade
{
    private readonly IDashboardRepository _dashboardRepository;
    private readonly INavigationService _navigationService;
    private readonly IToastService _toastService;

    public DashboardFacade(
        IDashboardRepository dashboardRepository,
        INavigationService navigationService,
        IToastService toastService)
    {
        _dashboardRepository = dashboardRepository ?? throw new ArgumentNullException(nameof(dashboardRepository));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _toastService = toastService ?? throw new ArgumentNullException(nameof(toastService));
    }

    public async Task<Result<DashboardSummary>> LoadDashboardAsync(string period = "current")
    {
        try
        {
            // Validate period
            var validPeriods = new[] { "current", "last", "last3" };
            if (!validPeriods.Contains(period.ToLower()))
            {
                period = "current"; // Default to current if invalid
            }

            System.Diagnostics.Debug.WriteLine($"[DashboardFacade.LoadDashboardAsync] START - Period: {period}");
            System.Diagnostics.Debug.WriteLine($"[DashboardFacade.LoadDashboardAsync] Calling repository.GetSummaryAsync({period})");

            var sw = System.Diagnostics.Stopwatch.StartNew();
            var result = await _dashboardRepository.GetSummaryAsync(period);
            sw.Stop();

            System.Diagnostics.Debug.WriteLine($"[DashboardFacade.LoadDashboardAsync] Repository returned - Success: {result.IsSuccess}, ElapsedMs: {sw.ElapsedMilliseconds}");

            if (!result.IsSuccess || result.Data == null)
            {
                System.Diagnostics.Debug.WriteLine($"[DashboardFacade.LoadDashboardAsync] ERROR - {result.ErrorMessage}");
                _ = _toastService.ShowError("Failed to load dashboard data");
                return Result<DashboardSummary>.Failure(result.ErrorMessage ?? "Failed to load dashboard");
            }

            var data = result.Data;
            System.Diagnostics.Debug.WriteLine($"[DashboardFacade.LoadDashboardAsync] COMPLETED - TotalProducts: {data.TotalProducts}, Orders: {data.TodayOrders}, Revenue: {data.MonthRevenue}");
            return result;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[DashboardFacade.LoadDashboardAsync] Exception - Type: {ex.GetType().Name}, Message: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"[DashboardFacade.LoadDashboardAsync] StackTrace: {ex.StackTrace}");
            _ = _toastService.ShowError($"Error loading dashboard: {ex.Message}");
            return Result<DashboardSummary>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<RevenueChartData>> GetRevenueChartDataAsync(string period = "day")
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"[DashboardFacade.GetRevenueChartDataAsync] START - Period: {period}");
            System.Diagnostics.Debug.WriteLine($"[DashboardFacade.GetRevenueChartDataAsync] Calling repository.GetRevenueChartAsync({period})");

            var sw = System.Diagnostics.Stopwatch.StartNew();
            var result = await _dashboardRepository.GetRevenueChartAsync(period);
            sw.Stop();

            System.Diagnostics.Debug.WriteLine($"[DashboardFacade.GetRevenueChartDataAsync] Repository returned - Success: {result.IsSuccess}, ElapsedMs: {sw.ElapsedMilliseconds}");

            if (!result.IsSuccess || result.Data == null)
            {
                System.Diagnostics.Debug.WriteLine($"[DashboardFacade.GetRevenueChartDataAsync] ERROR - {result.ErrorMessage}");
                _ = _toastService.ShowError("Failed to load revenue chart data");
                return Result<RevenueChartData>.Failure("Failed to load chart data");
            }

            var data = result.Data;
            System.Diagnostics.Debug.WriteLine($"[DashboardFacade.GetRevenueChartDataAsync] COMPLETED - DataPoints: {data.Labels.Count}");
            return result;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[DashboardFacade.GetRevenueChartDataAsync] Exception - Type: {ex.GetType().Name}, Message: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"[DashboardFacade.GetRevenueChartDataAsync] StackTrace: {ex.StackTrace}");
            _ = _toastService.ShowError($"Error loading chart: {ex.Message}");
            return Result<RevenueChartData>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<TopSellingProduct>>> GetTopSellingProductsAsync(int top = 10)
    {
        try
        {
            if (top < 1 || top > 100)
            {
                _ = _toastService.ShowError("Top value must be between 1 and 100");
                return Result<List<TopSellingProduct>>.Failure("Invalid top value");
            }

            var dashboardResult = await _dashboardRepository.GetSummaryAsync();
            if (!dashboardResult.IsSuccess || dashboardResult.Data?.TopSellingProducts == null)
            {
                _ = _toastService.ShowError("Failed to load top selling products");
                return Result<List<TopSellingProduct>>.Failure("Failed to load data");
            }

            var topProducts = dashboardResult.Data.TopSellingProducts.Take(top).ToList();
            System.Diagnostics.Debug.WriteLine($"[DashboardFacade] Loaded top {topProducts.Count} selling products");
            return Result<List<TopSellingProduct>>.Success(topProducts);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[DashboardFacade] Error loading top products: {ex.Message}");
            _ = _toastService.ShowError($"Error loading products: {ex.Message}");
            return Result<List<TopSellingProduct>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<LowStockProduct>>> GetLowStockProductsAsync(int threshold = 10)
    {
        try
        {
            if (threshold < 0)
            {
                _ = _toastService.ShowError("Threshold must be a positive number");
                return Result<List<LowStockProduct>>.Failure("Invalid threshold");
            }

            var dashboardResult = await _dashboardRepository.GetSummaryAsync();
            if (!dashboardResult.IsSuccess || dashboardResult.Data?.LowStockProducts == null)
            {
                _ = _toastService.ShowError("Failed to load low stock products");
                return Result<List<LowStockProduct>>.Failure("Failed to load data");
            }

            var lowStockProducts = dashboardResult.Data.LowStockProducts
                .Where(p => p.Quantity <= threshold)
                .ToList();

            if (lowStockProducts.Any())
            {
                _ = _toastService.ShowWarning($"Warning: {lowStockProducts.Count} products are low on stock!");
            }

            System.Diagnostics.Debug.WriteLine($"[DashboardFacade] Found {lowStockProducts.Count} low stock products");
            return Result<List<LowStockProduct>>.Success(lowStockProducts);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[DashboardFacade] Error loading low stock: {ex.Message}");
            _ = _toastService.ShowError($"Error loading stock data: {ex.Message}");
            return Result<List<LowStockProduct>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<RecentOrder>>> GetRecentOrdersAsync(int count = 10)
    {
        try
        {
            if (count < 1 || count > 100)
            {
                _ = _toastService.ShowError("Count must be between 1 and 100");
                return Result<List<RecentOrder>>.Failure("Invalid count");
            }

            var dashboardResult = await _dashboardRepository.GetSummaryAsync();
            if (!dashboardResult.IsSuccess || dashboardResult.Data?.RecentOrders == null)
            {
                _ = _toastService.ShowError("Failed to load recent orders");
                return Result<List<RecentOrder>>.Failure("Failed to load data");
            }

            var recentOrders = dashboardResult.Data.RecentOrders.Take(count).ToList();
            System.Diagnostics.Debug.WriteLine($"[DashboardFacade] Loaded {recentOrders.Count} recent orders");
            return Result<List<RecentOrder>>.Success(recentOrders);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[DashboardFacade] Error loading recent orders: {ex.Message}");
            _ = _toastService.ShowError($"Error loading orders: {ex.Message}");
            return Result<List<RecentOrder>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<CategorySales>>> GetSalesByCategoryAsync(string period = "current")
    {
        try
        {
            var dashboardResult = await LoadDashboardAsync(period);
            if (!dashboardResult.IsSuccess || dashboardResult.Data?.SalesByCategory == null)
            {
                _ = _toastService.ShowError("Failed to load category sales");
                return Result<List<CategorySales>>.Failure("Failed to load data");
            }

            var categorySales = dashboardResult.Data.SalesByCategory.ToList();
            System.Diagnostics.Debug.WriteLine($"[DashboardFacade] Loaded sales for {categorySales.Count} categories");
            return Result<List<CategorySales>>.Success(categorySales);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[DashboardFacade] Error loading category sales: {ex.Message}");
            _ = _toastService.ShowError($"Error loading sales: {ex.Message}");
            return Result<List<CategorySales>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<List<TopSalesAgent>>> GetTopSalesAgentsAsync(string period = "current", int topCount = 5)
    {
        try
        {
            if (topCount < 1 || topCount > 50)
            {
                _ = _toastService.ShowError("Top count must be between 1 and 50");
                return Result<List<TopSalesAgent>>.Failure("Invalid count");
            }

            var result = await _dashboardRepository.GetTopSalesAgentsAsync(period, topCount);
            if (!result.IsSuccess || result.Data == null)
            {
                _ = _toastService.ShowError("Failed to load top sales agents");
                return Result<List<TopSalesAgent>>.Failure("Failed to load data");
            }

            System.Diagnostics.Debug.WriteLine($"[DashboardFacade] Loaded {result.Data.Count} top sales agents");
            return Result<List<TopSalesAgent>>.Success(result.Data);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[DashboardFacade] Error loading top sales agents: {ex.Message}");
            _ = _toastService.ShowError($"Error loading agents: {ex.Message}");
            return Result<List<TopSalesAgent>>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task<Result<string>> ExportDashboardDataAsync(string period = "current")
    {
        try
        {
            // Show file save picker first
            var savePicker = new FileSavePicker();
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
            WinRT.Interop.InitializeWithWindow.Initialize(savePicker, hwnd);

            savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            savePicker.FileTypeChoices.Add("CSV Files", new List<string> { ".csv" });
            savePicker.SuggestedFileName = $"Dashboard_{period}_{DateTime.Now:yyyyMMdd_HHmmss}";

            var file = await savePicker.PickSaveFileAsync();
            if (file == null)
            {
                // User cancelled the picker
                return Result<string>.Success(string.Empty);
            }

            var dashboardResult = await LoadDashboardAsync(period);
            if (!dashboardResult.IsSuccess || dashboardResult.Data == null)
            {
                _ = _toastService.ShowError("Failed to load dashboard data for export");
                return Result<string>.Failure("Failed to load data");
            }

            var dashboard = dashboardResult.Data;
            var csv = new StringBuilder();

            // Summary section
            csv.AppendLine("DASHBOARD SUMMARY");
            csv.AppendLine($"Period,{period}");
            csv.AppendLine($"Export Date,{DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            csv.AppendLine();

            // Key metrics
            csv.AppendLine("KEY METRICS");
            csv.AppendLine("Metric,Value");
            csv.AppendLine($"Today Revenue,\"{dashboard.TodayRevenue:F2}\"");
            csv.AppendLine($"Today Orders,\"{dashboard.TodayOrders}\"");
            csv.AppendLine($"Week Revenue,\"{dashboard.WeekRevenue:F2}\"");
            csv.AppendLine($"Month Revenue,\"{dashboard.MonthRevenue:F2}\"");
            csv.AppendLine($"Total Products,\"{dashboard.TotalProducts}\"");
            csv.AppendLine();

            // Top selling products
            csv.AppendLine("TOP SELLING PRODUCTS");
            csv.AppendLine("Product Name,Units Sold,Revenue");
            foreach (var product in dashboard.TopSellingProducts ?? Enumerable.Empty<TopSellingProduct>())
            {
                csv.AppendLine($"\"{product.Name}\",\"{product.SoldCount}\",\"{product.Revenue:F2}\"");
            }
            csv.AppendLine();

            // Low stock products
            csv.AppendLine("LOW STOCK PRODUCTS");
            csv.AppendLine("Product Name,Category,Stock Quantity");
            foreach (var product in dashboard.LowStockProducts ?? Enumerable.Empty<LowStockProduct>())
            {
                csv.AppendLine($"\"{product.Name}\",\"{product.CategoryName}\",\"{product.Quantity}\"");
            }
            csv.AppendLine();

            // Category sales
            csv.AppendLine("SALES BY CATEGORY");
            csv.AppendLine("Category,Total Revenue,Percentage");
            foreach (var category in dashboard.SalesByCategory ?? Enumerable.Empty<CategorySales>())
            {
                csv.AppendLine($"\"{category.CategoryName}\",\"{category.TotalRevenue:F2}\",\"{category.Percentage:F2}%\"");
            }

            // Write to user-selected file
            await FileIO.WriteTextAsync(file, csv.ToString());

            _ = _toastService.ShowSuccess($"Dashboard data exported to {file.Name}");
            System.Diagnostics.Debug.WriteLine($"[DashboardFacade] Exported dashboard to {file.Path}");
            return Result<string>.Success(file.Path);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[DashboardFacade] Error exporting dashboard: {ex.Message}");
            _ = _toastService.ShowError($"Error exporting data: {ex.Message}");
            return Result<string>.Failure($"Error: {ex.Message}");
        }
    }

    public async Task NavigateToDashboardPageAsync(string pageName)
    {
        try
        {
            var validPages = new Dictionary<string, string>
            {
                { "products", "AdminProductsPage" },
                { "users", "AdminUsersPage" },
                { "orders", "AdminOrdersPage" },
                { "reports", "AdminReportsPage" },
                { "categories", "AdminCategoriesPage" },
                { "agents", "AdminAgentRequestsPage" }
            };

            var pageKey = pageName.ToLower();
            if (validPages.TryGetValue(pageKey, out var actualPageName))
            {
                _ = _navigationService.NavigateTo(actualPageName);
                System.Diagnostics.Debug.WriteLine($"[DashboardFacade] Navigated to {actualPageName}");
            }
            else
            {
                _ = _toastService.ShowWarning($"Unknown page: {pageName}");
                System.Diagnostics.Debug.WriteLine($"[DashboardFacade] Unknown page: {pageName}");
            }

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[DashboardFacade] Navigation error: {ex.Message}");
            _ = _toastService.ShowError($"Navigation failed: {ex.Message}");
        }
    }
}
