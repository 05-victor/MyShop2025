using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.Kernel.Sketches;
using MyShop.Client.Common.Converters;
using Microsoft.UI;
using MyShop.Client.ViewModels.Base;
using MyShop.Client.Facades;
using MyShop.Core.Interfaces.Facades;
using MyShop.Core.Interfaces.Services;
using MyShop.Shared.Models.Enums;
using MyShop.Shared.DTOs.Responses;
using SkiaSharp;
using System.Collections.ObjectModel;

namespace MyShop.Client.ViewModels.Admin;

/// <summary>
/// ViewModel for Admin Reports with sales analytics and charts
/// Uses ReportFacade for data aggregation
/// </summary>
public partial class AdminReportsViewModel : BaseViewModel
{
    private readonly IReportFacade _reportFacade;
    private readonly IProductFacade _productFacade;

    // --- Filters (no auto-reload, user must click Apply Filters) ---
    [ObservableProperty]
    private DateRangeOption? _selectedDateRange;

    [ObservableProperty]
    private string? _selectedCategory;

    [ObservableProperty]
    private DateTimeOffset? _startDate;

    [ObservableProperty]
    private DateTimeOffset? _endDate;

    public ObservableCollection<DateRangeOption> DateRanges { get; }
    public ObservableCollection<string> Categories { get; }

    // --- Data Collections ---
    [ObservableProperty]
    private ObservableCollection<ProductPerformance> _filteredProducts;

    [ObservableProperty]
    private ObservableCollection<Salesperson> _salespersonData;

    [ObservableProperty]
    private AdminReportsResponse? _reportData;

    // --- Chart Series ---
    [ObservableProperty]
    private ISeries[] _revenueSeries;

    [ObservableProperty]
    private ISeries[] _ordersByCategorySeries;

    [ObservableProperty]
    private ISeries[] _ratingDistributionSeries;

    [ObservableProperty]
    private Axis[] _xAxes;

    [ObservableProperty]
    private Axis[] _yAxes;

    public AdminReportsViewModel(
        IReportFacade reportFacade,
        IProductFacade productFacade,
        IToastService toastService,
        INavigationService navigationService)
        : base(toastService, navigationService)
    {
        _reportFacade = reportFacade;
        _productFacade = productFacade;

        DateRanges = new ObservableCollection<DateRangeOption>
        {
            new() { Display = "This Week", Value = "week" },
            new() { Display = "This Month", Value = "month" },
            new() { Display = "This Year", Value = "year" },
        };

        // Set default selection to "This Week"
        _selectedDateRange = DateRanges[0];

        // Initialize with last 7 days (matching "This Week")
        _endDate = DateTimeOffset.Now;
        _startDate = DateTimeOffset.Now.AddDays(-7);

        // Initialize collections to prevent DataGrid crash
        Categories = new ObservableCollection<string> { "All" };
        _selectedCategory = "All"; // Set backing field directly to avoid triggering OnChanged
        _filteredProducts = new ObservableCollection<ProductPerformance>();
        _salespersonData = new ObservableCollection<Salesperson>();

        // Initialize with empty arrays, charts will be created lazily in InitializeAsync.
        // This prevents SkiaSharp crash during constructor when DPI/rendering context not ready.
        _revenueSeries = Array.Empty<ISeries>();
        _ordersByCategorySeries = Array.Empty<ISeries>();
        _ratingDistributionSeries = Array.Empty<ISeries>();
    }

    [RelayCommand]
    private async Task InitializeAsync()
    {
        SetLoadingState(true);

        try
        {
            // Setup axes first (currency labeler for revenue)
            var currencyConv = new CurrencyConverter();
            YAxes = new Axis[]
            {
                new Axis
                {
                    Labeler = value => currencyConv.Convert(value, typeof(string), null, string.Empty)?.ToString() ?? value.ToString()
                }
            };

            // Load data - this calls CreateChartsFromApiData() which sets chart series
            // and also populates categories from OrdersByCategory
            await LoadReportDataAsync();

            System.Diagnostics.Debug.WriteLine(
                $"[AdminReportsViewModel] InitializeAsync completed. Charts: Revenue={RevenueSeries?.Length ?? 0}, Orders={OrdersByCategorySeries?.Length ?? 0}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AdminReportsViewModel] InitializeAsync FAILED: {ex.Message}");
            await _toastHelper?.ShowError($"Failed to load reports: {ex.Message}");

            // Ensure non-null series on error
            RevenueSeries ??= Array.Empty<ISeries>();
            OrdersByCategorySeries ??= Array.Empty<ISeries>();
            RatingDistributionSeries ??= Array.Empty<ISeries>();
        }
        finally
        {
            SetLoadingState(false);
        }
    }

    // NOTE: Filter property changes no longer auto-reload
    // User must click "Apply Filters" button to reduce API calls

    /// <summary>
    /// Apply current filter settings and reload data
    /// </summary>
    [RelayCommand]
    private async Task ApplyFiltersAsync()
    {
        SetLoadingState(true);
        try
        {
            await LoadReportDataAsync();
            await _toastHelper?.ShowSuccess("Filters applied successfully");
        }
        catch (Exception ex)
        {
            await _toastHelper?.ShowError($"Failed to apply filters: {ex.Message}");
        }
        finally
        {
            SetLoadingState(false);
        }
    }

    /// <summary>
    /// Load categories from API using LoadCategoriesAsync endpoint
    /// Called when report data is loaded to populate category filter dropdown
    /// </summary>
    private async Task LoadCategoriesFromApiAsync()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("[AdminReportsViewModel] LoadCategoriesFromApiAsync: Starting category load from API");

            var result = await _productFacade.LoadCategoriesAsync();

            if (!result.IsSuccess || result.Data == null)
            {
                System.Diagnostics.Debug.WriteLine($"[AdminReportsViewModel] LoadCategoriesFromApiAsync: Failed to load - {result.ErrorMessage}");
                return;
            }

            // Clear existing items but keep "All" at index 0
            while (Categories.Count > 1)
            {
                Categories.RemoveAt(Categories.Count - 1);
            }

            // Add API categories
            foreach (var category in result.Data)
            {
                Categories.Add(category.Name);
            }

            System.Diagnostics.Debug.WriteLine($"[AdminReportsViewModel] ✅ LoadCategoriesFromApiAsync: Loaded {result.Data.Count} categories from API");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AdminReportsViewModel] ❌ LoadCategoriesFromApiAsync: ERROR - {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"[AdminReportsViewModel] Stack trace: {ex.StackTrace}");
        }
    }

    /// <summary>
    /// Auto-update StartDate and EndDate when Quick Date Range selection changes
    /// </summary>
    partial void OnSelectedDateRangeChanged(DateRangeOption? value)
    {
        if (value == null) return;

        var today = DateTimeOffset.Now;

        switch (value.Value)
        {
            case "week":
                // This Week: from 7 days ago to today
                StartDate = today.AddDays(-7);
                EndDate = today;
                System.Diagnostics.Debug.WriteLine($"[AdminReportsViewModel] Updated dates for This Week: {StartDate:yyyy-MM-dd} to {EndDate:yyyy-MM-dd}");
                break;

            case "month":
                // This Month: from first day of month to today
                var firstDayOfMonth = new DateTimeOffset(today.Year, today.Month, 1, 0, 0, 0, today.Offset);
                StartDate = firstDayOfMonth;
                EndDate = today;
                System.Diagnostics.Debug.WriteLine($"[AdminReportsViewModel] Updated dates for This Month: {StartDate:yyyy-MM-dd} to {EndDate:yyyy-MM-dd}");
                break;

            case "year":
                // This Year: from first day of year to today
                var firstDayOfYear = new DateTimeOffset(today.Year, 1, 1, 0, 0, 0, today.Offset);
                StartDate = firstDayOfYear;
                EndDate = today;
                System.Diagnostics.Debug.WriteLine($"[AdminReportsViewModel] Updated dates for This Year: {StartDate:yyyy-MM-dd} to {EndDate:yyyy-MM-dd}");
                break;
        }
    }

    [RelayCommand]
    private async Task ExportReportAsync()
    {
        SetLoadingState(true);
        try
        {
            // Use custom date range if both StartDate and EndDate are set
            if (StartDate.HasValue && EndDate.HasValue)
            {
                var result = await _reportFacade.ExportProductPerformanceAsync(
                    StartDate.Value.DateTime,
                    EndDate.Value.DateTime);

                if (result.IsSuccess && !string.IsNullOrEmpty(result.Data))
                {
                    // Toast already shown by facade
                }
                else if (!result.IsSuccess)
                {
                    await _toastHelper?.ShowError(result.ErrorMessage ?? "Export failed");
                }
                // Empty result means user cancelled the picker
            }
            else
            {
                // Use preset period if no custom dates
                var period = SelectedDateRange?.Value ?? "month";
                var result = await _reportFacade.ExportSalesReportAsync(period);

                if (result.IsSuccess && !string.IsNullOrEmpty(result.Data))
                {
                    // Toast already shown by facade
                }
                else if (!result.IsSuccess)
                {
                    await _toastHelper?.ShowError(result.ErrorMessage ?? "Export failed");
                }
                // Empty result means user cancelled the picker
            }
        }
        catch (Exception ex)
        {
            await _toastHelper?.ShowError($"Export error: {ex.Message}");
        }
        finally
        {
            SetLoadingState(false);
        }
    }

    private async Task LoadReportDataAsync()
    {
        try
        {
            // Calculate date range based on selected option or custom dates
            DateTime from, to;

            if (StartDate.HasValue && EndDate.HasValue)
            {
                // Convert DateTimeOffset to UTC DateTime (properly converts the time instant)
                from = StartDate.Value.UtcDateTime;
                to = EndDate.Value.UtcDateTime;
            }
            else
            {
                // Use preset date ranges
                (from, to) = GetDateRangeFromSelection();
            }

            // Validate date range
            if (from > to)
            {
                await _toastHelper?.ShowError("Start date must be before end date");
                return;
            }

            // Parse category ID if user selected a specific category
            Guid? categoryId = null;
            if (!string.IsNullOrEmpty(SelectedCategory) && SelectedCategory != "All" &&
                ReportData?.OrdersByCategory != null)
            {
                // Find the CategoryId by matching CategoryName
                var selectedCat = ReportData.OrdersByCategory.FirstOrDefault(c => c.CategoryName == SelectedCategory);
                if (selectedCat != null)
                {
                    categoryId = selectedCat.CategoryId;
                }
            }

            System.Diagnostics.Debug.WriteLine(
                $"[AdminReportsViewModel] LoadReportDataAsync: from={from:O}, to={to:O}, categoryId={categoryId}, selectedCategory={SelectedCategory}");

            // Call API through facade
            var result = await _reportFacade.GetAdminReportsAsync(from, to, categoryId);

            if (result.IsSuccess && result.Data != null)
            {
                ReportData = result.Data;

                // Load/update categories from API
                await LoadCategoriesFromApiAsync();

                // Update product table from API data
                UpdateProductTable();

                // Update salesperson table from API data
                UpdateSalespersonTable();

                // Create charts from real API data
                CreateChartsFromApiData();

                System.Diagnostics.Debug.WriteLine(
                    $"[AdminReportsViewModel] Report data loaded successfully: {result.Data.ProductSummary?.Data?.Count ?? 0} products");
                System.Diagnostics.Debug.WriteLine(
                    $"[AdminReportsViewModel] Final chart series lengths - Revenue: {RevenueSeries?.Length ?? 0}, Orders: {OrdersByCategorySeries?.Length ?? 0}");
            }
            else
            {
                await _toastHelper?.ShowError(result.ErrorMessage ?? "Failed to load admin reports");
                System.Diagnostics.Debug.WriteLine($"[AdminReportsViewModel] LoadReportDataAsync FAILED: {result.ErrorMessage}");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AdminReportsViewModel] LoadReportDataAsync Exception: {ex.Message}");
            await _toastHelper?.ShowError($"Error loading reports: {ex.Message}");
        }
    }

    private (DateTime from, DateTime to) GetDateRangeFromSelection()
    {
        // Use UtcNow which already has Kind=Utc
        var todayUtc = DateTime.UtcNow.Date;
        var period = SelectedDateRange?.Value ?? "week";

        return period switch
        {
            "week" => (
                DateTime.SpecifyKind(todayUtc.AddDays(-7), DateTimeKind.Utc),
                DateTime.SpecifyKind(todayUtc.AddDays(1), DateTimeKind.Utc)
            ),
            "month" => (
                new DateTime(todayUtc.Year, todayUtc.Month, 1, 0, 0, 0, DateTimeKind.Utc),
                DateTime.SpecifyKind(todayUtc.AddDays(1), DateTimeKind.Utc)
            ),
            "year" => (
                new DateTime(todayUtc.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                DateTime.SpecifyKind(todayUtc.AddDays(1), DateTimeKind.Utc)
            ),
            _ => (
                DateTime.SpecifyKind(todayUtc.AddDays(-7), DateTimeKind.Utc),
                DateTime.SpecifyKind(todayUtc.AddDays(1), DateTimeKind.Utc)
            )
        };
    }

    private void UpdateProductTable()
    {
        if (ReportData?.ProductSummary?.Data == null || ReportData.ProductSummary.Data.Count == 0)
        {
            FilteredProducts.Clear();
            return;
        }

        Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread().TryEnqueue(() =>
        {
            FilteredProducts.Clear();
            foreach (var product in ReportData.ProductSummary.Data)
            {
                FilteredProducts.Add(new ProductPerformance
                {
                    Name = product.ProductName,
                    Category = product.CategoryName,
                    Sold = product.TotalOrders,
                    Revenue = product.TotalRevenue,
                    Rating = (float)product.AverageRating,
                    Stock = product.StockLevel,
                    Commission = 0 // Not in API response
                });
            }
        });
    }

    private void UpdateSalespersonTable()
    {
        if (ReportData?.SalespersonContributions == null || ReportData.SalespersonContributions.Count == 0)
        {
            SalespersonData.Clear();
            return;
        }

        Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread().TryEnqueue(() =>
        {
            SalespersonData.Clear();
            foreach (var salesperson in ReportData.SalespersonContributions)
            {
                SalespersonData.Add(new Salesperson
                {
                    Name = $"{salesperson.FirstName} {salesperson.LastName}",
                    Sales = salesperson.TotalSales,
                    Revenue = salesperson.TotalRevenue,
                    Initials = GetInitials($"{salesperson.FirstName} {salesperson.LastName}")
                });
            }
        });
    }

    private void CreateChartsFromApiData()
    {
        if (ReportData == null)
        {
            System.Diagnostics.Debug.WriteLine("[AdminReportsViewModel] CreateChartsFromApiData: ReportData is null");
            return;
        }

        try
        {
            var currencyConv = new CurrencyConverter();

            // Debug: Check all report data
            System.Diagnostics.Debug.WriteLine("[AdminReportsViewModel] CreateChartsFromApiData: Report data inspection:");
            System.Diagnostics.Debug.WriteLine($"  - RevenueTrend count: {ReportData.RevenueTrend?.Count ?? 0}");
            System.Diagnostics.Debug.WriteLine($"  - OrdersByCategory count: {ReportData.OrdersByCategory?.Count ?? 0}");
            System.Diagnostics.Debug.WriteLine($"  - ProductSummary.Data count: {ReportData.ProductSummary?.Data?.Count ?? 0}");
            System.Diagnostics.Debug.WriteLine($"  - SalespersonContributions count: {ReportData.SalespersonContributions?.Count ?? 0}");

            // Revenue Trend Chart - Line chart from daily revenue
            if (ReportData.RevenueTrend?.Count > 0)
            {
                var revenueValues = ReportData.RevenueTrend
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
                // No revenue data - clear chart
                RevenueSeries = Array.Empty<ISeries>();
            }

            // Orders by Category - Single Column chart with category labels on X-axis
            if (ReportData.OrdersByCategory?.Count > 0)
            {
                // Get category names and order counts
                var categoryNames = ReportData.OrdersByCategory.Select(c => c.CategoryName).ToArray();
                var orderCounts = ReportData.OrdersByCategory.Select(c => c.OrderCount).ToList();

                // Create single ColumnSeries with data labels showing order counts
                var columnSeries = new ColumnSeries<int>
                {
                    Values = orderCounts,
                    Name = "Orders",
                    Fill = new SolidColorPaint(SKColors.RoyalBlue),
                    // Show order counts on top of bars
                    DataLabelsSize = 11,
                    DataLabelsFormatter = point => orderCounts[(int)point.Index].ToString()
                };

                OrdersByCategorySeries = new ISeries[] { columnSeries };

                // Set X-axis labels to category names  
                XAxes = new Axis[]
                {
                    new Axis
                    {
                        Labels = categoryNames,
                        LabelsRotation = 45
                    }
                };

                // Debug: Log the data we're creating
                System.Diagnostics.Debug.WriteLine(
                    $"[AdminReportsViewModel] Orders by Category chart created with {ReportData.OrdersByCategory.Count} categories:");
                foreach (var cat in ReportData.OrdersByCategory)
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"  - {cat.CategoryName}: {cat.OrderCount} orders, Revenue: {cat.Revenue:N0}, {cat.Percentage:F1}%");
                }
            }
            else
            {
                // No category data - clear chart and axes
                OrdersByCategorySeries = Array.Empty<ISeries>();
                XAxes = new Axis[] { new Axis() };
            }

            System.Diagnostics.Debug.WriteLine(
                $"[AdminReportsViewModel] Charts created: Revenue={RevenueSeries.Length}, Orders={OrdersByCategorySeries.Length}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AdminReportsViewModel] CreateChartsFromApiData ERROR: {ex.Message}\n{ex.StackTrace}");
        }
    }

    /// <summary>
    /// Get mock product performance data for display
    /// </summary>
    private List<ProductPerformance> GetMockProductPerformance()
    {
        return new List<ProductPerformance>
        {
            new() { Name = "iPhone 14 Pro Max", Category = "Smartphones", Sold = 156, Revenue = 171444, Rating = 4.8, Stock = 45, Commission = 8572 },
            new() { Name = "MacBook Pro 16\"", Category = "Laptops", Sold = 89, Revenue = 222411, Rating = 4.9, Stock = 23, Commission = 11120 },
            new() { Name = "AirPods Pro 2", Category = "Audio", Sold = 312, Revenue = 77688, Rating = 4.7, Stock = 8, Commission = 3884 },
            new() { Name = "iPad Pro 12.9\"", Category = "Tablets", Sold = 67, Revenue = 80333, Rating = 4.6, Stock = 34, Commission = 4017 },
            new() { Name = "Apple Watch Ultra", Category = "Wearables", Sold = 98, Revenue = 78302, Rating = 4.5, Stock = 56, Commission = 3915 },
            new() { Name = "Samsung Galaxy S23", Category = "Smartphones", Sold = 134, Revenue = 120466, Rating = 4.4, Stock = 67, Commission = 6023 },
            new() { Name = "Sony WH-1000XM5", Category = "Audio", Sold = 189, Revenue = 66087, Rating = 4.8, Stock = 12, Commission = 3304 },
            new() { Name = "Dell XPS 15", Category = "Laptops", Sold = 45, Revenue = 89955, Rating = 4.5, Stock = 19, Commission = 4498 },
        };
    }

    /// <summary>
    /// Get mock salesperson data for display
    /// </summary>
    private List<Salesperson> GetMockSalespersonData()
    {
        return new List<Salesperson>
        {
            new() { Name = "John Doe", Sales = 45, Revenue = 12450, Initials = "JD" },
            new() { Name = "Sarah Smith", Sales = 38, Revenue = 9870, Initials = "SS" },
            new() { Name = "Mike Johnson", Sales = 32, Revenue = 8520, Initials = "MJ" },
            new() { Name = "Emma Wilson", Sales = 28, Revenue = 7340, Initials = "EW" },
        };
    }

    /// <summary>
    /// View details of a product
    /// </summary>
    [RelayCommand]
    private void ViewProductDetails(ProductPerformance? product)
    {
        if (product == null) return;
        ViewProductDetailsRequested?.Invoke(this, product);
    }

    /// <summary>
    /// View details of a salesperson
    /// </summary>
    [RelayCommand]
    private void ViewSalespersonDetails(Salesperson? salesperson)
    {
        if (salesperson == null) return;
        ViewSalespersonDetailsRequested?.Invoke(this, salesperson);
    }

    /// <summary>
    /// Event raised when product details should be shown
    /// </summary>
    public event EventHandler<ProductPerformance>? ViewProductDetailsRequested;

    /// <summary>
    /// Event raised when salesperson details should be shown
    /// </summary>
    public event EventHandler<Salesperson>? ViewSalespersonDetailsRequested;

    private static string GetInitials(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return "??";

        var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 1) return parts[0].Substring(0, Math.Min(2, parts[0].Length)).ToUpper();

        return $"{parts[0][0]}{parts[^1][0]}".ToUpper();
    }

    #region Mock Data Generators

    /// <summary>
    /// Create mock revenue series (Line chart)
    /// </summary>
    private ISeries[] CreateMockRevenueSeries(CurrencyConverter? currencyConv = null)
    {
        try
        {
            currencyConv ??= new CurrencyConverter();

            return new ISeries[]
            {
                new LineSeries<double>
                {
                    Name = "Revenue",
                    Values = new double[] { 12000, 15000, 18000, 22000, 25000, 28000, 32000 },
                    Fill = null,
                    Stroke = new SolidColorPaint(SKColors.Blue) { StrokeThickness = 2 },
                    GeometrySize = 6,
                    GeometryStroke = null,
                    GeometryFill = null,
                    // Tooltip formatting handled by chart-level tooltip or axis labeler; keep series simple
                }
            };
        }
        catch
        {
            return Array.Empty<ISeries>();
        }
    }

    /// <summary>
    /// Create mock orders by category series (Column chart)
    /// </summary>
    private ISeries[] CreateMockCategorySeries()
    {
        try
        {
            return new ISeries[]
            {
                new ColumnSeries<double>
                {
                    Name = "Electronics",
                    Values = new double[] { 120 },
                    Fill = new SolidColorPaint(SKColors.Blue)
                },
                new ColumnSeries<double>
                {
                    Name = "Clothing",
                    Values = new double[] { 85 },
                    Fill = new SolidColorPaint(SKColors.Green)
                },
                new ColumnSeries<double>
                {
                    Name = "Home & Garden",
                    Values = new double[] { 65 },
                    Fill = new SolidColorPaint(SKColors.Orange)
                },
                new ColumnSeries<double>
                {
                    Name = "Food",
                    Values = new double[] { 45 },
                    Fill = new SolidColorPaint(SKColors.Red)
                }
            };
        }
        catch
        {
            return Array.Empty<ISeries>();
        }
    }

    /// <summary>
    /// Create mock rating distribution series (Pie chart)
    /// </summary>
    private ISeries[] CreateMockRatingDistributionSeries()
    {
        try
        {
            return new ISeries[]
            {
                new PieSeries<double>
                {
                    Name = "5 Stars",
                    Values = new double[] { 45 },
                    Fill = new SolidColorPaint(SKColors.Green)
                },
                new PieSeries<double>
                {
                    Name = "4 Stars",
                    Values = new double[] { 30 },
                    Fill = new SolidColorPaint(SKColors.Blue)
                },
                new PieSeries<double>
                {
                    Name = "3 Stars",
                    Values = new double[] { 15 },
                    Fill = new SolidColorPaint(SKColors.Orange)
                },
                new PieSeries<double>
                {
                    Name = "2 Stars",
                    Values = new double[] { 7 },
                    Fill = new SolidColorPaint(SKColors.OrangeRed)
                },
                new PieSeries<double>
                {
                    Name = "1 Star",
                    Values = new double[] { 3 },
                    Fill = new SolidColorPaint(SKColors.Red)
                }
            };
        }
        catch
        {
            return Array.Empty<ISeries>();
        }
    }

    #endregion
}

// --- Models for ViewModel State ---
public class DateRangeOption
{
    public required string Display { get; set; }
    public required string Value { get; set; }
}

public class Salesperson
{
    public required string Name { get; set; }
    public int Sales { get; set; }
    public decimal Revenue { get; set; }
    public required string Initials { get; set; }
}

public class ProductPerformance
{
    public required string Name { get; set; }
    public required string Category { get; set; }
    public int Sold { get; set; }
    public decimal Revenue { get; set; }
    public double Rating { get; set; }
    public int Stock { get; set; }
    public decimal Commission { get; set; }
}