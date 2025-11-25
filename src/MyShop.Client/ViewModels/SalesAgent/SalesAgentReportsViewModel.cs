using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Client.ViewModels.Base;
using MyShop.Core.Interfaces.Repositories;
using MyShop.Core.Interfaces.Infrastructure;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace MyShop.Client.ViewModels.SalesAgent;

public partial class SalesAgentReportsViewModel : BaseViewModel
{
    private readonly IReportRepository _reportRepository;
    private readonly IAuthRepository _authRepository;

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

    public SalesAgentReportsViewModel(
        IReportRepository reportRepository,
        IAuthRepository authRepository)
    {
        _reportRepository = reportRepository;
        _authRepository = authRepository;
    }

    public async Task InitializeAsync()
    {
        await LoadReportsAsync();
    }

    private async Task LoadReportsAsync()
    {
        IsLoading = true;

        try
        {
            // Get current user ID from auth repository
            var userIdResult = await _authRepository.GetCurrentUserIdAsync();

            if (!userIdResult.IsSuccess || userIdResult.Data == Guid.Empty)
            {
                SetError("User not authenticated", new UnauthorizedAccessException());
                return;
            }

            var userId = userIdResult.Data;

            // Calculate date range based on selected period
            var (startDate, endDate) = GetDateRange(SelectedPeriod);

            // Load sales report
            var reportResult = await _reportRepository.GetSalesReportAsync(userId, startDate, endDate);
            if (reportResult.IsSuccess && reportResult.Data != null)
            {
                TotalRevenue = reportResult.Data.TotalRevenue;
                TotalCommission = reportResult.Data.TotalCommission;
                TotalOrders = reportResult.Data.TotalOrders;
                AverageOrderValue = reportResult.Data.AverageOrderValue;
            }

            // Load sales trend data
            var trendResult = await _reportRepository.GetSalesTrendAsync(userId, "daily");

            SalesData.Clear();
            if (trendResult.IsSuccess && trendResult.Data != null)
            {
                var trend = trendResult.Data;
                for (int i = 0; i < trend.Labels.Count; i++)
                {
                    SalesData.Add(new SalesReportViewModel
                    {
                        Date = trend.Labels[i],
                        Orders = trend.OrdersData[i],
                        Revenue = trend.RevenueData[i],
                        Commission = trend.CommissionData[i]
                    });
                }
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[SalesAgentReportsViewModel] Error loading reports: {ex.Message}");
            SalesData.Clear();
        }
        finally
        {
            IsLoading = false;
        }
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
    private void ExportReport()
    {
        // TODO: Implement CSV/PDF export when FileSavePicker is integrated
        System.Diagnostics.Debug.WriteLine("[SalesAgentReportsViewModel] Export report requested");
    }
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
