using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using MyShop.Client.ViewModels.Base;
using MyShop.Client.Facades;
using MyShop.Core.Interfaces.Facades;
using SkiaSharp;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace MyShop.Client.ViewModels.SalesAgent;

public partial class SalesAgentReportsViewModel : BaseViewModel
{
    private readonly IReportFacade _reportFacade;

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
    private string _selectedPeriod = "This Month";

    [ObservableProperty]
    private string _selectedDateRange = "This Month";

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
        new FilterOption { Display = "This Week", Value = "This Week" },
        new FilterOption { Display = "This Month", Value = "This Month" },
        new FilterOption { Display = "Last 3 Months", Value = "Last 3 Months" },
        new FilterOption { Display = "This Year", Value = "This Year" }
    };

    [ObservableProperty]
    private ObservableCollection<string> _categories = new()
    {
        "All Categories",
        "Electronics",
        "Clothing",
        "Home & Garden",
        "Sports",
        "Books"
    };

    public SalesAgentReportsViewModel(IReportFacade reportFacade)
    {
        _reportFacade = reportFacade;
        InitializeCharts();
    }

    private void InitializeCharts()
    {
        // Initialize empty charts to prevent binding errors
        RevenueSeries = Array.Empty<ISeries>();
        OrdersByCategorySeries = Array.Empty<ISeries>();
        RatingDistributionSeries = Array.Empty<ISeries>();
    }

    [RelayCommand]
    public async Task InitializeAsync()
    {
        await LoadReportsAsync();
    }

    [RelayCommand]
    public async Task RefreshAsync()
    {
        await LoadReportsAsync();
    }

    private async Task LoadReportsAsync()
    {
        IsLoading = true;

        try
        {
            var result = await _reportFacade.GetSalesReportAsync(SelectedPeriod);
            if (!result.IsSuccess || result.Data == null)
            {
                // Load mock data for demo
                LoadMockData();
                return;
            }

            var data = result.Data;
            TotalRevenue = data.TotalRevenue;
            TotalCommission = data.TotalCommission;
            TotalOrders = data.TotalOrders;
            AverageOrderValue = data.AverageOrderValue;

            // Load chart data
            LoadChartData();

            // Load product summary
            LoadProductSummary();
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[SalesAgentReportsViewModel] Error loading reports: {ex.Message}");
            // Load mock data on error
            LoadMockData();
        }
        finally
        {
            IsLoading = false;
        }
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
        await LoadReportsAsync();
    }

    [RelayCommand]
    private async Task ApplyFiltersAsync()
    {
        await LoadReportsAsync();
    }

    [RelayCommand]
    private async Task ResetFiltersAsync()
    {
        SelectedPeriod = "This Month";
        SelectedDateRange = "This Month";
        SelectedCategory = "All Categories";
        await LoadReportsAsync();
    }

    [RelayCommand]
    private void ExportReport()
    {
        // TODO: Implement CSV/PDF export when FileSavePicker is integrated
        System.Diagnostics.Debug.WriteLine("[SalesAgentReportsViewModel] Export report requested");
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
