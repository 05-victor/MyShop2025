using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Client.ViewModels.Base;
using MyShop.Client.Facades;
using MyShop.Client.Services;
using MyShop.Core.Interfaces.Facades;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;

namespace MyShop.Client.ViewModels.SalesAgent;

public partial class SalesAgentEarningsViewModel : BaseViewModel
{
    private readonly ICommissionFacade _commissionFacade;
    private readonly IEarningsFacade _earningsFacade;

    // Summary KPI Properties
    [ObservableProperty]
    private decimal _totalEarnings;

    [ObservableProperty]
    private decimal _totalPlatformFees;

    [ObservableProperty]
    private decimal _netEarnings;

    [ObservableProperty]
    private decimal _paidEarnings;

    [ObservableProperty]
    private decimal _pendingEarnings;

    [ObservableProperty]
    private int _totalOrders;

    [ObservableProperty]
    private decimal _averageEarningsPerOrder;

    [ObservableProperty]
    private decimal _thisMonthEarnings;

    [ObservableProperty]
    private decimal _lastMonthEarnings;

    // Legacy properties (kept for compatibility)
    [ObservableProperty]
    private decimal _pendingCommission;

    [ObservableProperty]
    private decimal _paidCommission;

    [ObservableProperty]
    private int _totalSales;

    [ObservableProperty]
    private ObservableCollection<CommissionViewModel> _commissions = new();

    [ObservableProperty]
    private string _selectedPeriod = "All Time";

    [ObservableProperty]
    private string _selectedStatus = "All";

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    [ObservableProperty]
    private int _currentPage = 1;

    [ObservableProperty]
    private int _pageSize = 20;

    [ObservableProperty]
    private int _totalItems;

    public SalesAgentEarningsViewModel(ICommissionFacade commissionFacade, IEarningsFacade earningsFacade)
    {
        _commissionFacade = commissionFacade;
        _earningsFacade = earningsFacade;
    }

    [RelayCommand]
    private async Task InitializeAsync()
    {
        await LoadCommissionsAsync();
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        CurrentPage = 1;
        await LoadCommissionsAsync();
    }

    [RelayCommand]
    private async Task ApplyFiltersAsync()
    {
        CurrentPage = 1;
        await LoadCommissionsAsync();
    }

    [RelayCommand]
    private async Task ResetFiltersAsync()
    {
        SelectedStatus = "All";
        SearchQuery = string.Empty;
        SelectedPeriod = "All Time";
        CurrentPage = 1;
        await LoadCommissionsAsync();
    }

    private async Task LoadCommissionsAsync()
    {
        IsLoading = true;

        try
        {
            // Load earnings summary first
            Debug.WriteLine("[SalesAgentEarningsViewModel] Loading earnings summary");
            var summaryResult = await _earningsFacade.GetSummaryAsync();

            if (summaryResult.IsSuccess && summaryResult.Data != null)
            {
                var summary = summaryResult.Data;
                TotalEarnings = summary.TotalEarnings;
                TotalPlatformFees = summary.TotalPlatformFees;
                NetEarnings = summary.NetEarnings;
                PaidEarnings = summary.PaidEarnings;
                PendingEarnings = summary.PendingEarnings;
                TotalOrders = summary.TotalOrders;
                AverageEarningsPerOrder = summary.AverageEarningsPerOrder;
                ThisMonthEarnings = summary.ThisMonthEarnings;
                LastMonthEarnings = summary.LastMonthEarnings;

                // Also update legacy properties
                TotalSales = summary.TotalOrders;
                PaidCommission = summary.PaidEarnings;
                PendingCommission = summary.PendingEarnings;

                Debug.WriteLine($"[SalesAgentEarningsViewModel] Summary loaded: Total={TotalEarnings}, Orders={TotalOrders}");
            }
            else
            {
                Debug.WriteLine($"[SalesAgentEarningsViewModel] Failed to load summary: {summaryResult.ErrorMessage}");
            }

            // Load commissions table
            var result = await _commissionFacade.LoadCommissionsAsync();
            if (!result.IsSuccess || result.Data == null)
            {
                return;
            }

            var pagedList = result.Data;
            TotalItems = pagedList.TotalCount;

            // Filter by search query and status
            var items = pagedList.Items.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                var query = SearchQuery.ToLower();
                items = items.Where(c =>
                    c.OrderNumber.ToLower().Contains(query));
            }

            if (SelectedStatus != "All")
            {
                items = items.Where(c => c.Status == SelectedStatus);
            }

            Commissions.Clear();
            foreach (var commission in items.Skip((CurrentPage - 1) * PageSize).Take(PageSize))
            {
                var customerName = $"Customer #{commission.OrderNumber.Split('-').Last()}";

                Commissions.Add(new CommissionViewModel
                {
                    OrderId = commission.OrderNumber,
                    ProductName = $"Order {commission.OrderNumber}",
                    CustomerName = customerName,
                    SaleAmount = commission.OrderAmount,
                    CommissionAmount = commission.CommissionAmount,
                    CommissionRate = (int)(commission.CommissionRate * 100),
                    NetIncome = commission.OrderAmount - commission.CommissionAmount,
                    Status = commission.Status,
                    StatusColor = GetStatusColor(commission.Status),
                    StatusBgColor = GetStatusBgColor(commission.Status),
                    OrderDate = commission.CreatedDate
                });
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[EarningsViewModel] Error loading commissions: {ex.Message}");
            Commissions.Clear();
        }
        finally
        {
            IsLoading = false;
        }
    }

    private string GetStatusColor(string status) => status switch
    {
        "Paid" => "#10B981",
        "Pending" => "#F59E0B",
        "Approved" => "#3B82F6",
        _ => "#6B7280"
    };

    private string GetStatusBgColor(string status) => status switch
    {
        "Paid" => "#D1FAE5",
        "Pending" => "#FEF3C7",
        "Approved" => "#DBEAFE",
        _ => "#F3F4F6"
    };

    [RelayCommand]
    private async Task FilterByPeriodAsync(string period)
    {
        SelectedPeriod = period;
        await LoadCommissionsAsync();
    }

    [RelayCommand]
    private async Task RequestPayoutAsync()
    {
        if (PendingCommission <= 0) return;

        SetLoadingState(true);
        try
        {
            // Note: RequestPayoutAsync not available in ICommissionFacade
            // This functionality needs to be implemented
            await Task.CompletedTask; // Placeholder
            // var result = await _commissionFacade.RequestPayoutAsync(PendingCommission);
            // if (result.IsSuccess)
            {
                PendingCommission = 0;
                await LoadCommissionsAsync();
            }
        }
        finally
        {
            SetLoadingState(false);
        }
    }

    [RelayCommand]
    private async Task ExportCommissionsAsync()
    {
        SetLoadingState(true);
        try
        {
            var result = await _commissionFacade.ExportCommissionsAsync();
            if (result.IsSuccess)
            {
                System.Diagnostics.Debug.WriteLine($"[EarningsViewModel] Commissions exported to: {result.Data}");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[EarningsViewModel] Export failed: {result.ErrorMessage}");
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[EarningsViewModel] Export error: {ex.Message}");
        }
        finally
        {
            SetLoadingState(false);
        }
    }
}

public partial class CommissionViewModel : ObservableObject
{
    [ObservableProperty]
    private string _orderId = string.Empty;

    [ObservableProperty]
    private string _productName = string.Empty;

    [ObservableProperty]
    private string _customerName = string.Empty;

    [ObservableProperty]
    private decimal _saleAmount;

    [ObservableProperty]
    private decimal _commissionAmount;

    [ObservableProperty]
    private decimal _netIncome;

    [ObservableProperty]
    private int _commissionRate;

    [ObservableProperty]
    private string _status = string.Empty;

    [ObservableProperty]
    private string _statusColor = string.Empty;

    [ObservableProperty]
    private string _statusBgColor = string.Empty;

    [ObservableProperty]
    private System.DateTime _orderDate;

    public string FormattedDate => OrderDate.ToString("MMM dd, yyyy");
}
