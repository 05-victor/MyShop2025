using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Client.ViewModels.Base;
using MyShop.Client.Facades;
using MyShop.Client.Services;
using MyShop.Core.Interfaces.Facades;
using MyShop.Shared.DTOs.Responses;
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

    // Trend properties (calculated)
    [ObservableProperty]
    private string _thisMonthTrendText = "0%";

    // Filter properties
    [ObservableProperty]
    private DateTime? _startDate = null;

    [ObservableProperty]
    private DateTime? _endDate = null;

    [ObservableProperty]
    private string _selectedPaymentStatus = "All";

    [ObservableProperty]
    private string _selectedOrderStatus = "All";

    // Transaction collection
    [ObservableProperty]
    private ObservableCollection<EarningTransactionViewModel> _earningTransactions = new();
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

                // Calculate trends
                CalculateTrends(summary);

                Debug.WriteLine($"[SalesAgentEarningsViewModel] Summary loaded: Total={TotalEarnings}, Orders={TotalOrders}");
            }
            else
            {
                Debug.WriteLine($"[SalesAgentEarningsViewModel] Failed to load summary: {summaryResult.ErrorMessage}");
            }

            // Load earnings history with filters
            var historyStatus = SelectedOrderStatus != "All" ? SelectedOrderStatus : null;
            var historyPaymentStatus = SelectedPaymentStatus != "All" ? SelectedPaymentStatus.ToUpper() : null;

            var historyResult = await _earningsFacade.GetHistoryAsync(
                CurrentPage,
                PageSize,
                StartDate,
                EndDate,
                historyStatus,
                historyPaymentStatus);

            if (historyResult.IsSuccess && historyResult.Data != null)
            {
                var pagedList = historyResult.Data;
                TotalItems = pagedList.TotalCount;

                EarningTransactions.Clear();
                foreach (var item in pagedList.Items)
                {
                    EarningTransactions.Add(new EarningTransactionViewModel
                    {
                        OrderId = item.OrderId,
                        OrderCode = item.OrderCode,
                        CustomerName = item.CustomerName,
                        OrderDate = item.OrderDate,
                        OrderStatus = item.OrderStatus,
                        PaymentStatus = item.PaymentStatus,
                        OrderAmount = item.OrderAmount,
                        PlatformFee = item.PlatformFee,
                        NetEarnings = item.NetEarnings,
                        StatusColor = EarningTransactionViewModel.GetStatusColor(item.PaymentStatus),
                        StatusBgColor = EarningTransactionViewModel.GetStatusBgColor(item.PaymentStatus)
                    });
                }

                Debug.WriteLine($"[SalesAgentEarningsViewModel] Earnings history loaded: {EarningTransactions.Count} items");
            }
            else
            {
                Debug.WriteLine($"[SalesAgentEarningsViewModel] Failed to load history: {historyResult?.ErrorMessage}");
                EarningTransactions.Clear();
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[EarningsViewModel] Error loading data: {ex.Message}");
            EarningTransactions.Clear();
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

    private void CalculateTrends(EarningsSummaryResponse summary)
    {
        // Calculate This Month trend: compare with Last Month
        if (summary.LastMonthEarnings > 0)
        {
            var trendValue = (summary.ThisMonthEarnings - summary.LastMonthEarnings) / summary.LastMonthEarnings;
            ThisMonthTrendText = $"{trendValue:+0.0%;-0.0%;0.0%}";
        }
        else
        {
            ThisMonthTrendText = "0%";
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

public partial class EarningTransactionViewModel : ObservableObject
{
    [ObservableProperty]
    private Guid _orderId;

    [ObservableProperty]
    private string _orderCode = string.Empty;

    [ObservableProperty]
    private string _customerName = string.Empty;

    [ObservableProperty]
    private DateTime _orderDate;

    [ObservableProperty]
    private string _orderStatus = string.Empty;

    [ObservableProperty]
    private string _paymentStatus = string.Empty;

    [ObservableProperty]
    private decimal _orderAmount;

    [ObservableProperty]
    private decimal _platformFee;

    [ObservableProperty]
    private decimal _netEarnings;

    [ObservableProperty]
    private string _statusColor = string.Empty;

    [ObservableProperty]
    private string _statusBgColor = string.Empty;

    public static string GetStatusColor(string status) => status switch
    {
        "PAID" => "#2E7D32",      // Green
        "UNPAID" => "#F57C00",    // Orange
        _ => "#666666"            // Gray
    };

    public static string GetStatusBgColor(string status) => status switch
    {
        "PAID" => "#E8F5E9",      // Light Green
        "UNPAID" => "#FFE0B2",    // Light Orange
        _ => "#F5F5F5"            // Light Gray
    };
}
