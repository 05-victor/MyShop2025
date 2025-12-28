using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using MyShop.Client.ViewModels.Base;
using MyShop.Client.Facades;
using MyShop.Core.Interfaces.Facades;
using MyShop.Shared.DTOs.Responses;
using MyShop.Shared.Models;
using SkiaSharp;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace MyShop.Client.ViewModels.SalesAgent;

public partial class SalesAgentReportsViewModel : BaseViewModel
{
    private readonly IReportFacade _reportFacade;
    private readonly IProductFacade _productFacade;

    // Backing list for mapping category name -> category Id when calling API
    private List<Category> _categoryItems = new();

    [ObservableProperty]
    private decimal _totalRevenue;

    [ObservableProperty]
    private decimal _totalCommission;

    [ObservableProperty]
    private int _totalOrders;

    [ObservableProperty]
    private decimal _averageOrderValue;

    [ObservableProperty]
    private ObservableCollection<SalesReportViewModel> _salesData = new();

    [ObservableProperty]
    private string _selectedPeriod = "week";

    [ObservableProperty]
    private string _selectedDateRange = "week";

    [ObservableProperty]
    private string _selectedCategory = "All Categories";

    // Chart series for Revenue Trend
    [ObservableProperty]
    private ISeries[] _revenueSeries = Array.Empty<ISeries>();

    // Chart series for Orders by Category
    [ObservableProperty]
    private ISeries[] _ordersByCategorySeries = Array.Empty<ISeries>();

    // Pie chart series for Rating Distribution (Admin only - hidden)
    [ObservableProperty]
    private ISeries[] _ratingDistributionSeries = Array.Empty<ISeries>();

    // Salesperson data (Admin only - hidden)
    [ObservableProperty]
    private ObservableCollection<SalespersonViewModel> _salespersonData = new();

    // Product summary data for DataGrid
    [ObservableProperty]
    private ObservableCollection<ProductSummaryViewModel> _filteredProducts = new();

    // Filter options
    [ObservableProperty]
    private ObservableCollection<FilterOption> _dateRanges = new()
    {
        new FilterOption { Display = "This Week", Value = "week" },
        new FilterOption { Display = "This Month", Value = "month" },
        new FilterOption { Display = "Last 3 Months", Value = "3months" },
        new FilterOption { Display = "This Year", Value = "year" }
    };

    [ObservableProperty]
    private ObservableCollection<string> _categories = new()
    {
        "All Categories"
    };

    public SalesAgentReportsViewModel(IReportFacade reportFacade, IProductFacade productFacade)
    {
        _reportFacade = reportFacade;
        _productFacade = productFacade;
        InitializeCharts();
    }

    private void InitializeCharts()
    {
        // Initialize empty charts to prevent binding errors
        RevenueSeries = Array.Empty<ISeries>();
        OrdersByCategorySeries = Array.Empty<ISeries>();
        RatingDistributionSeries = Array.Empty<ISeries>();
    }

    /// <summary>
    /// Handle SelectedDateRange changes - only update state; API is called when Apply Filters is clicked.
    /// </summary>
    partial void OnSelectedDateRangeChanged(string value)
    {
        System.Diagnostics.Debug.WriteLine($"[SalesAgentReportsViewModel] OnSelectedDateRangeChanged: {value} (no auto-apply)");
        // Keep SelectedPeriod in sync with the selected date range
        SelectedPeriod = value;
    }

    /// <summary>
    /// Handle SelectedCategory changes - only update state; API is called when Apply Filters is clicked.
    /// </summary>
    partial void OnSelectedCategoryChanged(string value)
    {
        System.Diagnostics.Debug.WriteLine($"[SalesAgentReportsViewModel] OnSelectedCategoryChanged: {value} (no auto-apply)");
    }

    [RelayCommand]
    public async Task InitializeAsync()
    {
        await LoadCategoriesFromApiAsync();
        await LoadReportsFromApiAsync();
    }

    [RelayCommand]
    public async Task RefreshAsync()
    {
        await LoadReportsFromApiAsync();
    }

    /// <summary>
    /// Load categories from API for the filter dropdown
    /// </summary>
    private async Task LoadCategoriesFromApiAsync()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("[SalesAgentReportsViewModel] LoadCategoriesFromApiAsync: Starting category load from API");

            var result = await _productFacade.LoadCategoriesAsync();

            if (!result.IsSuccess || result.Data == null)
            {
                System.Diagnostics.Debug.WriteLine($"[SalesAgentReportsViewModel] LoadCategoriesFromApiAsync: Failed to load - {result.ErrorMessage}");
                return;
            }

            // Refresh local cache for category mapping (name -> Id)
            _categoryItems = result.Data.ToList();

            // Clear existing items but keep "All Categories" at index 0
            while (Categories.Count > 1)
            {
                Categories.RemoveAt(Categories.Count - 1);
            }

            // Add API categories (display names)
            foreach (var category in _categoryItems)
            {
                Categories.Add(category.Name);
            }

            System.Diagnostics.Debug.WriteLine($"[SalesAgentReportsViewModel] ✅ LoadCategoriesFromApiAsync: Loaded {_categoryItems.Count} categories from API");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[SalesAgentReportsViewModel] ❌ LoadCategoriesFromApiAsync: ERROR - {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"[SalesAgentReportsViewModel] Stack trace: {ex.StackTrace}");
        }
    }

    /// <summary>
    /// Load sales agent reports from API
    /// </summary>
    private async Task LoadReportsFromApiAsync()
    {
        IsLoading = true;

        try
        {
            System.Diagnostics.Debug.WriteLine($"[SalesAgentReportsViewModel] LoadReportsFromApiAsync: period={SelectedPeriod}, selectedCategory={SelectedCategory}");

            // Convert selected category name to categoryId using cached category list
            Guid? categoryId = null;
            if (!string.IsNullOrEmpty(SelectedCategory) && SelectedCategory != "All Categories")
            {
                var matchedCategory = _categoryItems.FirstOrDefault(c =>
                    string.Equals(c.Name, SelectedCategory, StringComparison.OrdinalIgnoreCase));

                if (matchedCategory != null)
                {
                    categoryId = matchedCategory.Id;
                }

                System.Diagnostics.Debug.WriteLine($"[SalesAgentReportsViewModel] Resolved category '{SelectedCategory}' to Id={categoryId}");
            }

            var result = await _reportFacade.GetSalesAgentReportsAsync(SelectedPeriod, categoryId);

            if (!result.IsSuccess || result.Data == null)
            {
                System.Diagnostics.Debug.WriteLine($"[SalesAgentReportsViewModel] LoadReportsFromApiAsync FAILED: {result.ErrorMessage}");
                // Load mock data as fallback
                LoadMockData();
                return;
            }

            var reportData = result.Data;

            // Update statistics
            TotalOrders = reportData.OrdersByCategory?.Sum(o => o.OrderCount) ?? 0;
            TotalRevenue = reportData.OrdersByCategory?.Sum(o => o.Revenue) ?? 0;
            TotalCommission = reportData.OrdersByCategory?.Sum(o => o.Commission) ?? 0;
            AverageOrderValue = TotalOrders > 0 ? (int)(TotalRevenue / TotalOrders) : 0;

            System.Diagnostics.Debug.WriteLine(
                $"[SalesAgentReportsViewModel] Loaded: TotalOrders={TotalOrders}, TotalRevenue={TotalRevenue}, TotalCommission={TotalCommission}");

            // Create charts from API data
            CreateChartsFromApiData(reportData);

            // Update product summary from top products
            UpdateProductSummary(reportData);

            System.Diagnostics.Debug.WriteLine(
                $"[SalesAgentReportsViewModel] Report data loaded successfully: " +
                $"Revenue={RevenueSeries?.Length ?? 0}, Orders={OrdersByCategorySeries?.Length ?? 0}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[SalesAgentReportsViewModel] LoadReportsFromApiAsync Exception: {ex.Message}");
            // Load mock data on error
            LoadMockData();
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Create charts from API report data
    /// </summary>
    private void CreateChartsFromApiData(SalesAgentReportsResponse reportData)
    {
        try
        {
            var currencyConv = new MyShop.Client.Common.Converters.CurrencyConverter();

            // Debug: Log report data structure
            System.Diagnostics.Debug.WriteLine("[SalesAgentReportsViewModel] CreateChartsFromApiData: Report data inspection:");
            System.Diagnostics.Debug.WriteLine($"  - RevenueTrend count: {reportData.RevenueTrend?.Count ?? 0}");
            System.Diagnostics.Debug.WriteLine($"  - OrdersByCategory count: {reportData.OrdersByCategory?.Count ?? 0}");
            System.Diagnostics.Debug.WriteLine($"  - TopProducts count: {reportData.TopProducts?.Count ?? 0}");

            // Revenue Trend Chart - Line chart from daily revenue
            if (reportData.RevenueTrend?.Count > 0)
            {
                var revenueValues = reportData.RevenueTrend
                    .OrderBy(x => x.Date)
                    .Select(x => (double)x.Revenue)
                    .ToList();

                var revenueSeries = new ISeries[]
                {
                    new LineSeries<double>
                    {
                        Values = revenueValues.Cast<double>().ToList(),
                        Name = "Revenue",
                        Fill = new SolidColorPaint(SKColors.CornflowerBlue),
                        Stroke = new SolidColorPaint(SKColors.CornflowerBlue) { StrokeThickness = 2 },
                        GeometrySize = 0,
                        LineSmoothness = 0.5
                    }
                };
                RevenueSeries = revenueSeries;
            }
            else
            {
                RevenueSeries = Array.Empty<ISeries>();
            }

            // Orders by Category - Column chart
            if (reportData.OrdersByCategory?.Count > 0)
            {
                var categoryNames = reportData.OrdersByCategory.Select(c => c.CategoryName).ToArray();
                var orderCounts = reportData.OrdersByCategory.Select(c => c.OrderCount).ToList();

                var columnSeries = new ColumnSeries<int>
                {
                    Values = orderCounts,
                    Name = "Orders",
                    Fill = new SolidColorPaint(SKColors.CornflowerBlue),
                    DataLabelsSize = 11,
                    DataLabelsFormatter = point => orderCounts[(int)point.Index].ToString()
                };

                OrdersByCategorySeries = new ISeries[] { columnSeries };

                // Set X-axis labels
                var xAxes = new Axis[]
                {
                    new Axis
                    {
                        Labels = categoryNames,
                        LabelsRotation = 45
                    }
                };
                // Note: Need to set this on the page/chart control, not ViewModel
            }
            else
            {
                OrdersByCategorySeries = Array.Empty<ISeries>();
            }

            System.Diagnostics.Debug.WriteLine(
                $"[SalesAgentReportsViewModel] Charts created: Revenue={RevenueSeries.Length}, Orders={OrdersByCategorySeries.Length}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[SalesAgentReportsViewModel] CreateChartsFromApiData ERROR: {ex.Message}\n{ex.StackTrace}");
        }
    }

    /// <summary>
    /// Update product summary from top products API data
    /// </summary>
    private void UpdateProductSummary(SalesAgentReportsResponse reportData)
    {
        if (reportData?.TopProducts == null || reportData.TopProducts.Count == 0)
        {
            FilteredProducts.Clear();
            return;
        }

        Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread().TryEnqueue(() =>
        {
            FilteredProducts.Clear();
            foreach (var product in reportData.TopProducts)
            {
                // Find commission for this product's category
                var categoryData = reportData.OrdersByCategory?
                    .FirstOrDefault(c => c.CategoryName == product.CategoryName);
                var commission = categoryData?.Commission ?? 0;

                FilteredProducts.Add(new ProductSummaryViewModel
                {
                    Name = product.ProductName,
                    Category = product.CategoryName,
                    Sold = product.UnitsSold,
                    Revenue = product.Revenue,
                    Rating = product.AverageRating,
                    Commission = commission,
                    Stock = 0 // Not provided by API
                });
            }

            System.Diagnostics.Debug.WriteLine(
                $"[SalesAgentReportsViewModel] ✅ UpdateProductSummary: Loaded {FilteredProducts.Count} products");
        });
    }

    private async Task LoadReportsAsync()
    {
        await LoadReportsFromApiAsync();
    }

    private void LoadMockData()
    {
        TotalRevenue = 12500;
        TotalCommission = 1250;
        TotalOrders = 45;
        AverageOrderValue = 278;

        LoadChartData();
        LoadProductSummary();
    }

    private void LoadChartData()
    {
        // Revenue trend data (last 7 days)
        var revenueValues = new double[] { 1000, 1500, 1200, 1800, 2000, 1700, 2300 };
        RevenueSeries = new ISeries[]
        {
            new LineSeries<double>
            {
                Values = revenueValues,
                Name = "Revenue",
                Fill = null,
                Stroke = new SolidColorPaint(SKColors.CornflowerBlue) { StrokeThickness = 3 },
                GeometryFill = new SolidColorPaint(SKColors.CornflowerBlue),
                GeometryStroke = new SolidColorPaint(SKColors.White) { StrokeThickness = 2 },
                GeometrySize = 10
            }
        };

        // Orders by category
        OrdersByCategorySeries = new ISeries[]
        {
            new ColumnSeries<double>
            {
                Values = new double[] { 15, 22, 8, 12, 5 },
                Name = "Orders",
                Fill = new SolidColorPaint(SKColors.CornflowerBlue)
            }
        };

        // Rating distribution (for admin - hidden but prevent binding errors)
        RatingDistributionSeries = new ISeries[]
        {
            new PieSeries<double> { Values = new double[] { 45 }, Name = "5 Stars" },
            new PieSeries<double> { Values = new double[] { 30 }, Name = "4 Stars" },
            new PieSeries<double> { Values = new double[] { 15 }, Name = "3 Stars" },
            new PieSeries<double> { Values = new double[] { 7 }, Name = "2 Stars" },
            new PieSeries<double> { Values = new double[] { 3 }, Name = "1 Star" }
        };

        // Salesperson data (for admin - hidden)
        SalespersonData.Clear();
        SalespersonData.Add(new SalespersonViewModel { Name = "John Doe", Initials = "JD", Sales = 45, Revenue = 4500 });
        SalespersonData.Add(new SalespersonViewModel { Name = "Jane Smith", Initials = "JS", Sales = 38, Revenue = 3800 });
    }

    private void LoadProductSummary()
    {
        FilteredProducts.Clear();
        FilteredProducts.Add(new ProductSummaryViewModel
        {
            Name = "Wireless Mouse",
            Category = "Electronics",
            Sold = 25,
            Revenue = 625,
            Rating = 4.5m,
            Commission = 62.5m,
            Stock = 45
        });
        FilteredProducts.Add(new ProductSummaryViewModel
        {
            Name = "USB-C Hub",
            Category = "Electronics",
            Sold = 18,
            Revenue = 720,
            Rating = 4.8m,
            Commission = 72m,
            Stock = 12
        });
        FilteredProducts.Add(new ProductSummaryViewModel
        {
            Name = "Laptop Stand",
            Category = "Electronics",
            Sold = 12,
            Revenue = 480,
            Rating = 4.2m,
            Commission = 48m,
            Stock = 0
        });
        FilteredProducts.Add(new ProductSummaryViewModel
        {
            Name = "Mechanical Keyboard",
            Category = "Electronics",
            Sold = 8,
            Revenue = 640,
            Rating = 4.7m,
            Commission = 64m,
            Stock = 5
        });
    }

    private (DateTime startDate, DateTime endDate) GetDateRange(string period)
    {
        var endDate = DateTime.Now;
        var startDate = period switch
        {
            "This Week" => endDate.AddDays(-7),
            "This Month" => endDate.AddMonths(-1),
            "Last 3 Months" => endDate.AddMonths(-3),
            "This Year" => endDate.AddYears(-1),
            _ => endDate.AddMonths(-1)
        };
        return (startDate, endDate);
    }

    [RelayCommand]
    private async Task FilterByPeriodAsync(string period)
    {
        SelectedPeriod = period;
        SelectedDateRange = period;
        await LoadReportsFromApiAsync();
    }

    [RelayCommand]
    private async Task ApplyFiltersAsync()
    {
        await LoadReportsFromApiAsync();
    }

    [RelayCommand]
    private async Task ResetFiltersAsync()
    {
        SelectedPeriod = "week";
        SelectedDateRange = "week";
        SelectedCategory = "All Categories";
        await LoadReportsFromApiAsync();
    }

    [RelayCommand]
    private async Task ExportReportAsync()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("[SalesAgentReportsViewModel] ExportReportAsync: Starting export");

            // Use the selected period for export
            var period = SelectedPeriod ?? "week";

            // Call the ReportFacade to export the sales report
            var result = await _reportFacade.ExportSalesReportAsync(period);

            if (result.IsSuccess && !string.IsNullOrEmpty(result.Data))
            {
                System.Diagnostics.Debug.WriteLine("[SalesAgentReportsViewModel] ExportReportAsync: Export completed successfully");
                // Toast is shown by the facade
            }
            else if (!result.IsSuccess)
            {
                System.Diagnostics.Debug.WriteLine($"[SalesAgentReportsViewModel] ExportReportAsync: Export failed - {result.ErrorMessage}");
            }
            // Empty result means user cancelled the file picker
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[SalesAgentReportsViewModel] ExportReportAsync error: {ex.Message}");
        }
    }
}

/// <summary>
/// Filter option for ComboBox
/// </summary>
public partial class FilterOption : ObservableObject
{
    [ObservableProperty]
    private string _display = string.Empty;

    [ObservableProperty]
    private string _value = string.Empty;
}

/// <summary>
/// Salesperson contribution data (Admin only)
/// </summary>
public partial class SalespersonViewModel : ObservableObject
{
    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _initials = string.Empty;

    [ObservableProperty]
    private int _sales;

    [ObservableProperty]
    private decimal _revenue;
}

/// <summary>
/// Product summary for DataGrid
/// </summary>
public partial class ProductSummaryViewModel : ObservableObject
{
    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _category = string.Empty;

    [ObservableProperty]
    private int _sold;

    [ObservableProperty]
    private decimal _revenue;

    [ObservableProperty]
    private decimal _rating;

    [ObservableProperty]
    private decimal _commission;

    [ObservableProperty]
    private int _stock;
}

public partial class SalesReportViewModel : ObservableObject
{
    [ObservableProperty]
    private string _date = string.Empty;

    [ObservableProperty]
    private int _orders;

    [ObservableProperty]
    private decimal _revenue;

    [ObservableProperty]
    private decimal _commission;
}
