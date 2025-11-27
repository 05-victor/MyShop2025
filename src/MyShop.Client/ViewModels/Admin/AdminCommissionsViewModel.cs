using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Client.Facades.Reports;
using MyShop.Client.ViewModels.Base;
using MyShop.Core.Interfaces.Facades;
using MyShop.Core.Interfaces.Services;
using MyShop.Shared.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MyShop.Client.ViewModels.Admin;

/// <summary>
/// ViewModel for Admin Commission management with paging
/// View and manage commissions for all sales agents
/// </summary>
public partial class AdminCommissionsViewModel : PagedViewModelBase<CommissionViewModel>
{
    private readonly ICommissionFacade _commissionFacade;
    private readonly IDialogService _dialogService;

    [ObservableProperty]
    private string? _selectedStatus;

    [ObservableProperty]
    private DateTimeOffset? _startDate;

    [ObservableProperty]
    private DateTimeOffset? _endDate;

    [ObservableProperty]
    private string _sortBy = "createdDate";

    [ObservableProperty]
    private bool _sortDescending = true;

    // Summary statistics
    [ObservableProperty]
    private decimal _totalCommissions;

    [ObservableProperty]
    private decimal _pendingCommissions;

    [ObservableProperty]
    private decimal _paidCommissions;

    [ObservableProperty]
    private int _totalSalesAgents;

    public AdminCommissionsViewModel(
        ICommissionFacade commissionFacade,
        IToastService toastService,
        INavigationService navigationService,
        IDialogService dialogService)
        : base(toastService, navigationService)
    {
        _commissionFacade = commissionFacade;
        _dialogService = dialogService;

        // Initialize with last 30 days
        _endDate = DateTimeOffset.Now;
        _startDate = DateTimeOffset.Now.AddDays(-30);
    }

    public async Task InitializeAsync()
    {
        await LoadDataAsync();
    }

    /// <summary>
    /// Reload when filters change
    /// </summary>
    partial void OnSelectedStatusChanged(string? value)
    {
        CurrentPage = 1;
        _ = LoadPageAsync();
    }

    partial void OnStartDateChanged(DateTimeOffset? value)
    {
        CurrentPage = 1;
        _ = LoadPageAsync();
    }

    partial void OnEndDateChanged(DateTimeOffset? value)
    {
        CurrentPage = 1;
        _ = LoadPageAsync();
    }

    partial void OnSortByChanged(string value)
    {
        _ = LoadPageAsync();
    }

    partial void OnSortDescendingChanged(bool value)
    {
        _ = LoadPageAsync();
    }

    /// <summary>
    /// Override LoadPageAsync to fetch commissions with paging
    /// </summary>
    protected override async Task LoadPageAsync()
    {
        SetLoadingState(true);
        try
        {
            // Load commissions summary first
            await LoadSummaryAsync();

            // Load paged commissions - currently using LoadCommissionsAsync
            // TODO: Update CommissionFacade to support admin view with paging
            var result = await _commissionFacade.LoadCommissionsAsync();

            if (!result.IsSuccess || result.Data == null)
            {
                await _toastHelper?.ShowError(result.ErrorMessage ?? "Failed to load commissions");
                Items.Clear();
                UpdatePagingInfo(0);
                return;
            }

            var commissionData = result.Data;

            // Apply filters manually (until server-side filtering is implemented)
            var filteredCommissions = commissionData.Items.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SelectedStatus))
            {
                filteredCommissions = filteredCommissions.Where(c => c.Status == SelectedStatus);
            }

            if (StartDate.HasValue)
            {
                filteredCommissions = filteredCommissions.Where(c => c.CreatedDate >= StartDate.Value.DateTime);
            }

            if (EndDate.HasValue)
            {
                filteredCommissions = filteredCommissions.Where(c => c.CreatedDate <= EndDate.Value.DateTime);
            }

            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                var query = SearchQuery.ToLower();
                filteredCommissions = filteredCommissions.Where(c =>
                    c.SalesAgentName.ToLower().Contains(query) ||
                    c.OrderNumber.ToLower().Contains(query));
            }

            // Apply sorting
            filteredCommissions = SortBy.ToLower() switch
            {
                "amount" => SortDescending
                    ? filteredCommissions.OrderByDescending(c => c.CommissionAmount)
                    : filteredCommissions.OrderBy(c => c.CommissionAmount),
                "salesagent" => SortDescending
                    ? filteredCommissions.OrderByDescending(c => c.SalesAgentName)
                    : filteredCommissions.OrderBy(c => c.SalesAgentName),
                _ => SortDescending
                    ? filteredCommissions.OrderByDescending(c => c.CreatedDate)
                    : filteredCommissions.OrderBy(c => c.CreatedDate)
            };

            var filteredList = filteredCommissions.ToList();
            var totalCount = filteredList.Count;

            // Apply paging
            var pagedData = filteredList
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize);

            Items.Clear();
            foreach (var commission in pagedData)
            {
                Items.Add(CommissionViewModel.FromModel(commission));
            }

            UpdatePagingInfo(totalCount);

            System.Diagnostics.Debug.WriteLine($"[AdminCommissionsViewModel] Loaded page {CurrentPage}/{TotalPages} ({Items.Count} items, {TotalItems} total)");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AdminCommissionsViewModel] Error loading commissions: {ex.Message}");
            await _toastHelper?.ShowError($"Error loading commissions: {ex.Message}");
            Items.Clear();
            UpdatePagingInfo(0);
        }
        finally
        {
            SetLoadingState(false);
        }
    }

    private async Task LoadSummaryAsync()
    {
        try
        {
            // Get summary for all agents (pass Guid.Empty for all)
            var result = await _commissionFacade.GetCommissionSummaryAsync(Guid.Empty, "current");
            if (result.IsSuccess && result.Data != null)
            {
                TotalCommissions = result.Data.TotalEarnings;
                PendingCommissions = result.Data.PendingCommission;
                PaidCommissions = result.Data.PaidCommission;
                // TotalSalesAgents not available in CommissionSummary
                TotalSalesAgents = 0; // Would need separate query
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AdminCommissionsViewModel] Error loading summary: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task ApproveCommissionAsync(CommissionViewModel commission)
    {
        try
        {
            var confirmResult = await _dialogService.ShowConfirmationAsync(
                "Approve Commission",
                $"Approve commission of {commission.CommissionAmount:C} for {commission.SalesAgentName}?");

            if (!confirmResult.IsSuccess || !confirmResult.Data)
                return;

            // TODO: Implement approve commission in facade
            await _toastHelper?.ShowSuccess($"Commission for {commission.SalesAgentName} approved");
            commission.Status = "Approved";
            commission.StatusColor = "#3B82F6";
            commission.StatusBgColor = "#DBEAFE";
        }
        catch (Exception ex)
        {
            await _toastHelper?.ShowError($"Error approving commission: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task MarkAsPaidAsync(CommissionViewModel commission)
    {
        try
        {
            var confirmResult = await _dialogService.ShowConfirmationAsync(
                "Mark as Paid",
                $"Mark commission of {commission.CommissionAmount:C} as paid for {commission.SalesAgentName}?");

            if (!confirmResult.IsSuccess || !confirmResult.Data)
                return;

            // TODO: Implement mark as paid in facade
            await _toastHelper?.ShowSuccess($"Commission for {commission.SalesAgentName} marked as paid");
            commission.Status = "Paid";
            commission.StatusColor = "#10B981";
            commission.StatusBgColor = "#D1FAE5";
            commission.PaidDate = DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            await _toastHelper?.ShowError($"Error marking commission as paid: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task ViewCommissionDetailsAsync(CommissionViewModel commission)
    {
        // TODO: Navigate to commission details or order details
        await _toastHelper?.ShowInfo($"Viewing commission for order {commission.OrderNumber}");
        System.Diagnostics.Debug.WriteLine($"[AdminCommissionsViewModel] View commission details: {commission.OrderNumber}");
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
                await _toastHelper?.ShowSuccess($"Commissions exported to: {result.Data}");
            }
            else
            {
                await _toastHelper?.ShowError(result.ErrorMessage ?? "Export failed");
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
}

/// <summary>
/// ViewModel representation of Commission for UI binding
/// </summary>
public partial class CommissionViewModel : ObservableObject
{
    [ObservableProperty]
    private Guid _id;

    [ObservableProperty]
    private Guid _orderId;

    [ObservableProperty]
    private Guid _salesAgentId;

    [ObservableProperty]
    private string _salesAgentName = string.Empty;

    [ObservableProperty]
    private string _orderNumber = string.Empty;

    [ObservableProperty]
    private decimal _orderAmount;

    [ObservableProperty]
    private decimal _commissionRate;

    [ObservableProperty]
    private decimal _commissionAmount;

    [ObservableProperty]
    private string _status = string.Empty;

    [ObservableProperty]
    private string _statusColor = string.Empty;

    [ObservableProperty]
    private string _statusBgColor = string.Empty;

    [ObservableProperty]
    private DateTime _createdDate;

    [ObservableProperty]
    private DateTime? _paidDate;

    public string CreatedDateFormatted => CreatedDate.ToString("MMM dd, yyyy");
    public string PaidDateFormatted => PaidDate?.ToString("MMM dd, yyyy") ?? "-";
    public string CommissionRateFormatted => $"{CommissionRate:F1}%";

    public static CommissionViewModel FromModel(Commission commission)
    {
        var statusColor = commission.Status switch
        {
            "Paid" => "#10B981",
            "Approved" => "#3B82F6",
            "Pending" => "#F59E0B",
            _ => "#6B7280"
        };

        var statusBgColor = commission.Status switch
        {
            "Paid" => "#D1FAE5",
            "Approved" => "#DBEAFE",
            "Pending" => "#FEF3C7",
            _ => "#F3F4F6"
        };

        return new CommissionViewModel
        {
            Id = commission.Id,
            OrderId = commission.OrderId,
            SalesAgentId = commission.SalesAgentId,
            SalesAgentName = commission.SalesAgentName,
            OrderNumber = commission.OrderNumber,
            OrderAmount = commission.OrderAmount,
            CommissionRate = commission.CommissionRate,
            CommissionAmount = commission.CommissionAmount,
            Status = commission.Status,
            StatusColor = statusColor,
            StatusBgColor = statusBgColor,
            CreatedDate = commission.CreatedDate,
            PaidDate = commission.PaidDate
        };
    }

    public Commission ToModel()
    {
        return new Commission
        {
            Id = Id,
            OrderId = OrderId,
            SalesAgentId = SalesAgentId,
            SalesAgentName = SalesAgentName,
            OrderNumber = OrderNumber,
            OrderAmount = OrderAmount,
            CommissionRate = CommissionRate,
            CommissionAmount = CommissionAmount,
            Status = Status,
            CreatedDate = CreatedDate,
            PaidDate = PaidDate
        };
    }
}
