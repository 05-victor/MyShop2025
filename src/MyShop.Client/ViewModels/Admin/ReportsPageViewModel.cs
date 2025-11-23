using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using Microsoft.UI;
using MyShop.Client.ViewModels.Base;
using MyShop.Core.Interfaces.Repositories;
using MyShop.Shared.Models.Enums;
using SkiaSharp;
using System.Collections.ObjectModel;

namespace MyShop.Client.ViewModels.Admin;

public partial class ReportsPageViewModel : BaseViewModel
{
    private readonly IReportRepository? _reportRepository;
    private readonly IProductRepository? _productRepository;

    // --- Filters ---
    [ObservableProperty]
    private DateRangeOption? _selectedDateRange;

    [ObservableProperty]
    private string? _selectedCategory;

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

    // TODO: Replace with real user role check from IAuthService
    public bool IsAdmin => false; // await _authService.HasRoleAsync("Admin")
    public bool IsSalesAgent => false; // await _authService.HasRoleAsync("SalesAgent")

    public ReportsPageViewModel(
        IReportRepository? reportRepository = null,
        IProductRepository? productRepository = null)
    {
        _reportRepository = reportRepository;
        _productRepository = productRepository;

        DateRanges = new ObservableCollection<DateRangeOption>
        {
            new() { Display = "This Week", Value = "week" },
            new() { Display = "This Month", Value = "month" },
            new() { Display = "This Year", Value = "year" },
        };

        Categories = [];
        _filteredProducts = [];
        _salespersonData = [];

        // Initialize with mock data
        _revenueSeries = CreateMockRevenueSeries();
        _ordersByCategorySeries = CreateMockCategorySeries();
        _ratingDistributionSeries = CreateMockRatingDistributionSeries();
    }

    [RelayCommand]
    private async Task InitializeAsync()
    {
        SetLoadingState(true);
        await LoadReportDataAsync();
        SetLoadingState(false);
    }

    // --- Filter Logic ---
    partial void OnSelectedDateRangeChanged(DateRangeOption? value) => ApplyFilters();
    partial void OnSelectedCategoryChanged(string? value) => ApplyFilters();

    private void ApplyFilters()
    {
        _ = LoadReportDataAsync();
    }

    [RelayCommand]
    private static async Task ExportReportAsync()
    {
        await Task.CompletedTask;
    }

    private async Task LoadReportDataAsync()
    {
        // Load categories from product repository
        if (_productRepository != null)
        {
            try
            {
                var productsResult = await _productRepository.GetAllAsync();
                var categories = productsResult
                    .Select(p => p.CategoryName)
                    .Where(c => !string.IsNullOrEmpty(c))
                    .Distinct()
                    .OrderBy(c => c);

                Categories.Clear();
                Categories.Add("All");
                foreach (var cat in categories)
                {
                    Categories.Add(cat!);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ReportsPageViewModel] Error loading categories: {ex.Message}");
                Categories.Clear();
                Categories.Add("All");
            }
        }
        
        SelectedCategory ??= "All";

        // Load chart data from report repository
        if (_reportRepository != null)
        {
            try
            {
                // Load revenue data
                var salesAgentId = Guid.Empty; // Get from auth service
                var report = await _reportRepository.GetSalesReportAsync(salesAgentId);
                
                // For now, use mock data. Backend report endpoints will provide structured chart data
                RevenueSeries = CreateMockRevenueSeries();
                OrdersByCategorySeries = CreateMockCategorySeries();
                RatingDistributionSeries = CreateMockRatingDistributionSeries();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ReportsPageViewModel] Error loading reports: {ex.Message}");
                RevenueSeries = CreateMockRevenueSeries();
                OrdersByCategorySeries = CreateMockCategorySeries();
                RatingDistributionSeries = CreateMockRatingDistributionSeries();
            }
        }
        else
        {
            // Use mock data
            RevenueSeries = CreateMockRevenueSeries();
            OrdersByCategorySeries = CreateMockCategorySeries();
            RatingDistributionSeries = CreateMockRatingDistributionSeries();
        }

        // Load salesperson performance data (if Admin)
        if (IsAdmin && _reportRepository != null)
        {
            // TODO: Implement when IReportRepository is available
            // var salespersons = await _reportRepository.GetTopSalespersonsAsync(SelectedDateRange?.Value, limit: 10);
            // SalespersonData = new ObservableCollection<Salesperson>(salespersons);
        }

        // Load product performance data
        if (_reportRepository != null)
        {
            // TODO: Implement when IReportRepository is available
            // var products = await _reportRepository.GetProductPerformanceAsync(SelectedDateRange?.Value, SelectedCategory);
            // FilteredProducts = new ObservableCollection<ProductPerformance>(products);
        }

        await Task.CompletedTask;
    }

    #region Mock Data Generators

    /// <summary>
    /// Create mock revenue series (Line chart)
    /// </summary>
    private ISeries[] CreateMockRevenueSeries()
    {
        return new ISeries[]
        {
            new LineSeries<double>
            {
                Name = "Revenue",
                Values = new double[] { 12000, 15000, 18000, 22000, 25000, 28000, 32000 },
                Fill = null,
                Stroke = new SolidColorPaint(SKColors.Blue) { StrokeThickness = 3 },
                GeometrySize = 8,
                GeometryStroke = new SolidColorPaint(SKColors.Blue) { StrokeThickness = 3 },
                GeometryFill = new SolidColorPaint(SKColors.White)
            }
        };
    }

    /// <summary>
    /// Create mock orders by category series (Column chart)
    /// </summary>
    private ISeries[] CreateMockCategorySeries()
    {
        return new ISeries[]
        {
            new ColumnSeries<double>
            {
                Name = "Electronics",
                Values = new double[] { 120 },
                Fill = new SolidColorPaint(SKColor.Parse("#2563eb")),
                MaxBarWidth = 50
            },
            new ColumnSeries<double>
            {
                Name = "Clothing",
                Values = new double[] { 85 },
                Fill = new SolidColorPaint(SKColor.Parse("#10b981")),
                MaxBarWidth = 50
            },
            new ColumnSeries<double>
            {
                Name = "Home & Garden",
                Values = new double[] { 65 },
                Fill = new SolidColorPaint(SKColor.Parse("#f59e0b")),
                MaxBarWidth = 50
            },
            new ColumnSeries<double>
            {
                Name = "Food",
                Values = new double[] { 45 },
                Fill = new SolidColorPaint(SKColor.Parse("#ef4444")),
                MaxBarWidth = 50
            }
        };
    }

    /// <summary>
    /// Create mock rating distribution series (Pie chart)
    /// </summary>
    private ISeries[] CreateMockRatingDistributionSeries()
    {
        return new ISeries[]
        {
            new PieSeries<double>
            {
                Name = "5 Stars",
                Values = new double[] { 45 },
                Fill = new SolidColorPaint(SKColor.Parse("#10b981"))
            },
            new PieSeries<double>
            {
                Name = "4 Stars",
                Values = new double[] { 30 },
                Fill = new SolidColorPaint(SKColor.Parse("#3b82f6"))
            },
            new PieSeries<double>
            {
                Name = "3 Stars",
                Values = new double[] { 15 },
                Fill = new SolidColorPaint(SKColor.Parse("#f59e0b"))
            },
            new PieSeries<double>
            {
                Name = "2 Stars",
                Values = new double[] { 7 },
                Fill = new SolidColorPaint(SKColor.Parse("#f97316"))
            },
            new PieSeries<double>
            {
                Name = "1 Star",
                Values = new double[] { 3 },
                Fill = new SolidColorPaint(SKColor.Parse("#ef4444"))
            }
        };
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