using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Client.ViewModels.Base;
using MyShop.Client.Facades;
using MyShop.Core.Interfaces.Facades;
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

    public SalesAgentReportsViewModel(IReportFacade reportFacade)
    {
        _reportFacade = reportFacade;
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
            var result = await _reportFacade.GetSalesReportAsync(SelectedPeriod);
            if (!result.IsSuccess || result.Data == null)
            {
                return;
            }

            var data = result.Data;
            TotalRevenue = data.TotalRevenue;
            TotalCommission = data.TotalCommission;
            TotalOrders = data.TotalOrders;
            AverageOrderValue = data.AverageOrderValue;

            SalesData.Clear();
            // Mock trend data
            for (int i = 0; i < 7; i++)
            {
                SalesData.Add(new SalesReportViewModel
                {
                    Date = DateTime.Now.AddDays(-6 + i).ToString("MMM dd"),
                    Orders = 10 + i * 2,
                    Revenue = 1000 + i * 200,
                    Commission = 100 + i * 20
                });
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
