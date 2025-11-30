using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Client.Facades;
using MyShop.Client.ViewModels.Base;
using MyShop.Core.Interfaces.Facades;
using MyShop.Core.Interfaces.Repositories;
using MyShop.Core.Interfaces.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace MyShop.Client.ViewModels.SalesAgent;

public partial class SalesAgentOrdersViewModel : PagedViewModelBase<OrderViewModel>
{
    private readonly IOrderFacade _orderFacade;
    private readonly IAuthRepository _authRepository;
    private Guid? _currentSalesAgentId;

    [ObservableProperty]
    private string _selectedStatus = "All";

    [ObservableProperty]
    private string _sortBy = "date";

    [ObservableProperty]
    private bool _sortDescending = true;

    [ObservableProperty]
    private int _totalOrders;

    [ObservableProperty]
    private int _pendingOrders;

    [ObservableProperty]
    private int _completedOrders;

    [ObservableProperty]
    private int _cancelledOrders;

    [ObservableProperty]
    private ObservableCollection<string> _searchSuggestions = new();

    public SalesAgentOrdersViewModel(
        IOrderFacade orderFacade,
        IAuthRepository authRepository,
        IToastService toastService,
        INavigationService navigationService)
        : base(toastService, navigationService)
    {
        _orderFacade = orderFacade;
        _authRepository = authRepository;
        PageSize = Core.Common.PaginationConstants.OrdersPageSize;
    }

    public async Task InitializeAsync()
    {
        // Get current sales agent ID
        var userIdResult = await _authRepository.GetCurrentUserIdAsync();
        if (userIdResult.IsSuccess)
        {
            _currentSalesAgentId = userIdResult.Data;
            System.Diagnostics.Debug.WriteLine($"[SalesAgentOrdersViewModel] Current SalesAgentId: {_currentSalesAgentId}");
        }
        else
        {
            System.Diagnostics.Debug.WriteLine($"[SalesAgentOrdersViewModel] Failed to get current user ID");
        }
        
        await LoadDataAsync();
    }

    [RelayCommand]
    private async Task ApplyFiltersAsync()
    {
        CurrentPage = 1;
        await LoadPageAsync();
    }

    [RelayCommand]
    private async Task ResetFiltersAsync()
    {
        SelectedStatus = "All";
        SearchQuery = string.Empty;
        SortBy = "date";
        SortDescending = true;
        CurrentPage = 1;
        await LoadPageAsync();
    }

    protected override async Task LoadPageAsync()
    {
        try
        {
            SetLoadingState(true);

            var statusFilter = SelectedStatus == "All" ? null : SelectedStatus;

            var result = await _orderFacade.LoadOrdersPagedAsync(
                page: CurrentPage,
                pageSize: PageSize,
                status: statusFilter,
                searchQuery: SearchQuery,
                salesAgentId: _currentSalesAgentId);  // Filter by current sales agent

            if (!result.IsSuccess || result.Data == null)
            {
                await _toastHelper?.ShowError(result.ErrorMessage ?? "Failed to load orders");
                Items.Clear();
                UpdatePagingInfo(0);
                UpdateStats();
                return;
            }

            Items.Clear();
            foreach (var o in result.Data.Items)
            {
                // Use OrderItems (from JSON) or Items as fallback
                var orderItems = o.OrderItems?.Count > 0 ? o.OrderItems : o.Items;
                var firstProduct = orderItems?.FirstOrDefault()?.ProductName ?? "No products";
                var additionalCount = (orderItems?.Count ?? 0) - 1;
                var productDesc = additionalCount > 0 
                    ? $"{firstProduct} +{additionalCount} more" 
                    : firstProduct;

                Items.Add(new OrderViewModel
                {
                    OrderId = $"ORD-{o.Id.ToString().Substring(0, 8)}",
                    CustomerName = o.CustomerName,
                    CustomerEmail = $"{o.CustomerName.ToLower().Replace(" ", ".")}@example.com",
                    ProductDescription = productDesc,
                    OrderDate = o.OrderDate,
                    Status = o.Status,
                    StatusColor = GetStatusColor(o.Status),
                    StatusBgColor = GetStatusBgColor(o.Status),
                    TotalAmount = o.FinalPrice,
                    CommissionAmount = o.FinalPrice * 0.10m
                });
            }

            UpdatePagingInfo(result.Data.TotalCount);
            UpdateStats();

            System.Diagnostics.Debug.WriteLine($"[SalesAgentOrdersViewModel] Loaded page {CurrentPage}/{TotalPages} ({Items.Count} items, {TotalItems} total)");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[SalesAgentOrdersViewModel] Error loading orders: {ex.Message}");
            await _toastHelper?.ShowError($"Error loading orders: {ex.Message}");
            Items.Clear();
            UpdatePagingInfo(0);
            UpdateStats();
        }
        finally
        {
            SetLoadingState(false);
        }
    }

    private string GetStatusColor(string status) => status switch
    {
        "Completed" or "Delivered" => "#10B981",
        "Pending" => "#F59E0B",
        "Cancelled" => "#DC2626",
        _ => "#6B7280"
    };

    private string GetStatusBgColor(string status) => status switch
    {
        "Completed" or "Delivered" => "#D1FAE5",
        "Pending" => "#FEF3C7",
        "Cancelled" => "#FEE2E2",
        _ => "#F3F4F6"
    };

    private void UpdateStats()
    {
        TotalOrders = Items.Count;
        PendingOrders = Items.Count(o => o.Status == "Pending");
        CompletedOrders = Items.Count(o => o.Status == "Completed" || o.Status == "Delivered");
        CancelledOrders = Items.Count(o => o.Status == "Cancelled");
    }

    [RelayCommand]
    private void FilterByStatus(string status)
    {
        SelectedStatus = status;
    }

    [RelayCommand]
    private void ViewOrderDetails(OrderViewModel order)
    {
        System.Diagnostics.Debug.WriteLine($"[SalesOrdersViewModel] View order details: {order.OrderId}");
        // Navigation will be implemented when OrderDetailsPage is created
        // _navigationService.Navigate(typeof(OrderDetailsPage), order.OrderId);
    }

    [RelayCommand]
    private async Task ExportOrdersAsync()
    {
        SetLoadingState(true);
        try
        {
            var status = SelectedStatus == "All" ? null : SelectedStatus;
            // Export only current sales agent's orders
            await _orderFacade.ExportOrdersToCsvAsync(status: status, salesAgentId: _currentSalesAgentId);
        }
        catch (Exception ex)
        {
            await _toastHelper?.ShowError($"Export failed: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"[SalesOrdersViewModel] Export error: {ex.Message}");
        }
        finally
        {
            SetLoadingState(false);
        }
    }
}

public partial class OrderViewModel : ObservableObject
{
    [ObservableProperty]
    private string _orderId = string.Empty;

    [ObservableProperty]
    private string _customerName = string.Empty;

    [ObservableProperty]
    private string _customerEmail = string.Empty;

    [ObservableProperty]
    private string _productDescription = string.Empty;

    [ObservableProperty]
    private DateTime _orderDate;

    [ObservableProperty]
    private string _status = string.Empty;

    [ObservableProperty]
    private string _statusColor = string.Empty;

    [ObservableProperty]
    private string _statusBgColor = string.Empty;

    [ObservableProperty]
    private decimal _totalAmount;

    [ObservableProperty]
    private decimal _commissionAmount;

    public string FormattedDate => OrderDate.ToString("MMM dd, yyyy");
}
