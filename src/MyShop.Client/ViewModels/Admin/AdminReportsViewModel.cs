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
    private bool _isLoadingData; // Prevent recursive filter calls

    // --- Filters ---
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

        // Initialize with last 30 days
        _endDate = DateTimeOffset.Now;
        _startDate = DateTimeOffset.Now.AddDays(-30);

        // Initialize collections to prevent DataGrid crash
        Categories = new ObservableCollection<string> { "All", "Electronics", "Clothing", "Home & Garden", "Food" };
        _selectedCategory = "All"; // Set backing field directly to avoid triggering OnChanged
        _filteredProducts = new ObservableCollection<ProductPerformance>();
        _salespersonData = new ObservableCollection<Salesperson>();

        // COPILOT-FIX: Initialize with empty arrays, charts will be created lazily in InitializeAsync
        // This prevents SkiaSharp crash during constructor when DPI/rendering context not ready
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
            await Task.Delay(100);
            
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

    // --- Filter Logic ---
    partial void OnSelectedDateRangeChanged(DateRangeOption? value)
    {
        if (!_isLoadingData && !IsLoading)
        {
            _ = LoadReportDataAsync();
        }
    }
    
    partial void OnSelectedCategoryChanged(string? value)
    {
        if (!_isLoadingData && !IsLoading)
        {
            _ = LoadReportDataAsync();
        }
    }
    
    partial void OnStartDateChanged(DateTimeOffset? value)
    {
        if (!_isLoadingData && !IsLoading)
        {
            _ = LoadReportDataAsync();
        }
    }
    
    partial void OnEndDateChanged(DateTimeOffset? value)
    {
        if (!_isLoadingData && !IsLoading)
        {
            _ = LoadReportDataAsync();
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
                
                if (result.IsSuccess)
                {
                    await _toastHelper?.ShowSuccess($"Report exported to: {result.Data}");
                }
                else
                {
                    await _toastHelper?.ShowError(result.ErrorMessage ?? "Export failed");
                }
            }
            else
            {
                // Use preset period if no custom dates
                var period = SelectedDateRange?.Value ?? "month";
                var result = await _reportFacade.ExportSalesReportAsync(period);
                
                if (result.IsSuccess)
                {
                    await _toastHelper?.ShowSuccess($"Report exported to: {result.Data}");
                }
                else
                {
                    await _toastHelper?.ShowError(result.ErrorMessage ?? "Export failed");
                }
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
        _isLoadingData = true;
        try
        {
            // Load sales report data
            var period = SelectedDateRange?.Value ?? "month";
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

            if (performanceResult.IsSuccess && performanceResult.Data != null)
            {
                System.Diagnostics.Debug.WriteLine($"[AdminReportsViewModel] Loaded {performanceResult.Data.Count} products");
                
                // Thread-safe collection modification
                var products = performanceResult.Data.Take(20).Select(product => new ProductPerformance
                {
                    Name = product.ProductName,
                    Category = product.CategoryName,
                    Sold = product.TotalSold,
                    Revenue = product.TotalRevenue,
                    Rating = 4.5,
                    Stock = 100,
                    Commission = product.TotalCommission
                }).ToList();

                Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread().TryEnqueue(() =>
                {
                    FilteredProducts.Clear();
                    foreach (var p in products)
                    {
                        FilteredProducts.Add(p);
                    }
                });
            }

            // Load agent performance for Salesperson Data
            var agentResult = await _reportFacade.GetAgentPerformanceAsync(
                StartDate?.DateTime,
                EndDate?.DateTime);

            if (agentResult.IsSuccess && agentResult.Data != null)
            {
                System.Diagnostics.Debug.WriteLine($"[AdminReportsViewModel] Loaded {agentResult.Data.Count} agents");
                
                // Thread-safe collection modification
                var agents = agentResult.Data.Select(agent => new Salesperson
                {
                    Name = agent.AgentName,
                    Sales = agent.TotalOrders,
                    Revenue = agent.TotalRevenue,
                    Initials = GetInitials(agent.AgentName)
                }).ToList();

                Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread().TryEnqueue(() =>
                {
                    SalespersonData.Clear();
                    foreach (var a in agents)
                    {
                        SalespersonData.Add(a);
                    }
                });
            }

            System.Diagnostics.Debug.WriteLine("[AdminReportsViewModel] All data loaded successfully");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AdminReportsViewModel] Error loading report data: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"[AdminReportsViewModel] StackTrace: {ex.StackTrace}");
        }
        finally
        {
            _isLoadingData = false;
        }
    }

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