using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using Microsoft.UI;
using MyShop.Client.ViewModels.Base;
using MyShop.Client.Facades;
using MyShop.Core.Interfaces.Facades;
using MyShop.Core.Interfaces.Services;
using MyShop.Shared.Models.Enums;
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

    // --- Chart Series ---
    [ObservableProperty]
    private ISeries[] _revenueSeries;

    [ObservableProperty]
    private ISeries[] _ordersByCategorySeries;

    [ObservableProperty]
    private ISeries[] _ratingDistributionSeries;

    public AdminReportsViewModel(
        IReportFacade reportFacade,
        IToastService toastService,
        INavigationService navigationService)
        : base(toastService, navigationService)
    {
        _reportFacade = reportFacade;

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
        Categories = new ObservableCollection<string> { "All", "Electronics", "Clothing", "Home & Garden", "Food" };
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
            // Load data FIRST, then create charts
            await LoadReportDataAsync();
            
            System.Diagnostics.Debug.WriteLine("[AdminReportsViewModel] Data loaded, creating chart series...");
            
            // Delay chart creation to avoid SkiaSharp rendering crash
            // await Task.Delay(100);
            
            try
            {
                RevenueSeries = CreateMockRevenueSeries() ?? Array.Empty<ISeries>();
                OrdersByCategorySeries = CreateMockCategorySeries() ?? Array.Empty<ISeries>();
                RatingDistributionSeries = CreateMockRatingDistributionSeries() ?? Array.Empty<ISeries>();
                System.Diagnostics.Debug.WriteLine($"[AdminReportsViewModel] Charts created: Revenue={RevenueSeries.Length}, Orders={OrdersByCategorySeries.Length}, Rating={RatingDistributionSeries.Length}");
            }
            catch (Exception chartEx)
            {
                System.Diagnostics.Debug.WriteLine($"[AdminReportsViewModel] Chart creation failed: {chartEx.Message}");
                RevenueSeries = Array.Empty<ISeries>();
                OrdersByCategorySeries = Array.Empty<ISeries>();
                RatingDistributionSeries = Array.Empty<ISeries>();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AdminReportsViewModel] InitializeAsync FAILED: {ex.Message}");
            await _toastHelper?.ShowError($"Failed to load reports: {ex.Message}");
            
            // Ensure non-null series
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
            // Load sales report data
            var period = SelectedDateRange?.Value ?? "week";
            var salesResult = await _reportFacade.GetSalesReportAsync(period);

            if (salesResult.IsSuccess && salesResult.Data != null)
            {
                System.Diagnostics.Debug.WriteLine($"[AdminReportsViewModel] Loaded sales report: {salesResult.Data.TotalRevenue:C}");
            }

            // Load product performance data
            var performanceResult = await _reportFacade.GetProductPerformanceAsync(
                StartDate?.DateTime,
                EndDate?.DateTime,
                top: 50);

            List<ProductPerformance> products;
            if (performanceResult.IsSuccess && performanceResult.Data != null && performanceResult.Data.Count > 0)
            {
                System.Diagnostics.Debug.WriteLine($"[AdminReportsViewModel] Loaded {performanceResult.Data.Count} products from API");
                
                products = performanceResult.Data.Take(20).Select(product => new ProductPerformance
                {
                    Name = product.ProductName,
                    Category = product.CategoryName,
                    Sold = product.TotalSold,
                    Revenue = product.TotalRevenue,
                    Rating = 4.5,
                    Stock = 100,
                    Commission = product.TotalCommission
                }).ToList();
            }
            else
            {
                // Use mock data when API not available
                System.Diagnostics.Debug.WriteLine("[AdminReportsViewModel] Using mock product performance data");
                products = GetMockProductPerformance();
            }

            Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread().TryEnqueue(() =>
            {
                FilteredProducts.Clear();
                foreach (var p in products)
                {
                    FilteredProducts.Add(p);
                }
            });

            // Load agent performance for Salesperson Data
            var agentResult = await _reportFacade.GetAgentPerformanceAsync(
                StartDate?.DateTime,
                EndDate?.DateTime);

            List<Salesperson> agents;
            if (agentResult.IsSuccess && agentResult.Data != null && agentResult.Data.Count > 0)
            {
                System.Diagnostics.Debug.WriteLine($"[AdminReportsViewModel] Loaded {agentResult.Data.Count} agents from API");
                
                agents = agentResult.Data.Select(agent => new Salesperson
                {
                    Name = agent.AgentName,
                    Sales = agent.TotalOrders,
                    Revenue = agent.TotalRevenue,
                    Initials = GetInitials(agent.AgentName)
                }).ToList();
            }
            else
            {
                // Use mock data when API not available
                System.Diagnostics.Debug.WriteLine("[AdminReportsViewModel] Using mock salesperson data");
                agents = GetMockSalespersonData();
            }

            Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread().TryEnqueue(() =>
            {
                SalespersonData.Clear();
                foreach (var a in agents)
                {
                    SalespersonData.Add(a);
                }
            });

            System.Diagnostics.Debug.WriteLine("[AdminReportsViewModel] All data loaded successfully");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AdminReportsViewModel] Error loading report data: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"[AdminReportsViewModel] StackTrace: {ex.StackTrace}");
            
            // Load mock data on error
            Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread().TryEnqueue(() =>
            {
                FilteredProducts.Clear();
                foreach (var p in GetMockProductPerformance())
                {
                    FilteredProducts.Add(p);
                }
                
                SalespersonData.Clear();
                foreach (var a in GetMockSalespersonData())
                {
                    SalespersonData.Add(a);
                }
            });
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
    private ISeries[] CreateMockRevenueSeries()
    {
        try
        {
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
                    GeometryFill = null
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