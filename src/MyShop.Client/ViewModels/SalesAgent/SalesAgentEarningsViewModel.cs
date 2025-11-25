using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Core.Interfaces.Services;
using MyShop.Client.ViewModels.Base;
using MyShop.Core.Interfaces.Repositories;
using MyShop.Core.Interfaces.Infrastructure;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace MyShop.Client.ViewModels.SalesAgent;

public partial class SalesAgentEarningsViewModel : BaseViewModel
{
    private readonly ICommissionRepository _commissionRepository;
    private readonly IAuthRepository _authRepository;
    private readonly IToastService _toastHelper;

    [ObservableProperty]
    private decimal _totalEarnings;

    [ObservableProperty]
    private decimal _pendingCommission;

    [ObservableProperty]
    private decimal _paidCommission;

    [ObservableProperty]
    private int _totalSales;

    [ObservableProperty]
    private ObservableCollection<CommissionViewModel> _commissions = new();

    [ObservableProperty]
    private string _selectedPeriod = "This Month";

    public SalesAgentEarningsViewModel(
        ICommissionRepository commissionRepository,
        IAuthRepository authRepository,
        IToastService toastHelper)
    {
        _commissionRepository = commissionRepository;
        _authRepository = authRepository;
        _toastHelper = toastHelper;
    }

    public async Task InitializeAsync()
    {
        await LoadCommissionsAsync();
    }

    private async Task LoadCommissionsAsync()
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
            
            // Load commission summary
            var summaryResult = await _commissionRepository.GetSummaryAsync(userId);
            if (summaryResult.IsSuccess && summaryResult.Data != null)
            {
                TotalEarnings = summaryResult.Data.TotalEarnings;
                PendingCommission = summaryResult.Data.PendingCommission;
                PaidCommission = summaryResult.Data.PaidCommission;
                TotalSales = summaryResult.Data.TotalOrders;
            }

            // Load commission history
            var commissionsResult = await _commissionRepository.GetBySalesAgentIdAsync(userId);
            
            Commissions.Clear();
            if (commissionsResult.IsSuccess && commissionsResult.Data != null)
            {
                foreach (var commission in commissionsResult.Data.Take(20)) // Show latest 20
                {
                    Commissions.Add(new CommissionViewModel
                    {
                        OrderId = commission.OrderNumber,
                        ProductName = $"Order {commission.OrderNumber}",
                        CommissionAmount = commission.CommissionAmount,
                        CommissionRate = (int)commission.CommissionRate,
                        Status = commission.Status,
                        StatusColor = GetStatusColor(commission.Status),
                        StatusBgColor = GetStatusBgColor(commission.Status),
                        OrderDate = commission.CreatedDate
                    });
                }
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
        if (PendingCommission <= 0)
        {
            await _toastHelper.ShowWarning("No pending commission to request payout.");
            return;
        }

        try
        {
            SetLoadingState(true);

            // In production, this would call the backend API:
            // var result = await _commissionRepository.RequestPayoutAsync(PendingCommission);
            // if (result.IsSuccess)
            // {
            //     await _toastHelper.ShowSuccess($"Payout request submitted: {PendingCommission:N0} VND");
            //     await LoadCommissionsAsync(); // Refresh data
            // }
            // else
            // {
            //     await _toastHelper.ShowError($"Payout request failed: {result.ErrorMessage}");
            // }

            // Mock implementation - simulate API delay
            await Task.Delay(1000);

            System.Diagnostics.Debug.WriteLine($"[EarningsViewModel] Payout requested: {PendingCommission:N0} VND");
            
            await _toastHelper.ShowSuccess($"Payout request submitted successfully!\nAmount: {PendingCommission:C2}\n\nYour request will be processed within 3-5 business days.");

            // Simulate moving pending to processing
            PendingCommission = 0;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[EarningsViewModel] Payout request error: {ex.Message}");
            await _toastHelper.ShowError("Failed to submit payout request. Please try again.");
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
    private decimal _commissionAmount;

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
