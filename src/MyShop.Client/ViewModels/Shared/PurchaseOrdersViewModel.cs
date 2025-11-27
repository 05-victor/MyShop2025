using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Client.Facades;
using MyShop.Client.Views.Dialogs;
using MyShop.Client.ViewModels.Base;
using MyShop.Core.Interfaces.Facades;
using MyShop.Core.Interfaces.Services;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace MyShop.Client.ViewModels.Shared;

public partial class PurchaseOrdersViewModel : PagedViewModelBase<OrderViewModel>
{
    private readonly IOrderFacade _orderFacade;

    [ObservableProperty]
    private int _totalOrders;

    [ObservableProperty]
    private int _pendingOrders;

    [ObservableProperty]
    private int _inTransitOrders;

    [ObservableProperty]
    private int _deliveredOrders;

    [ObservableProperty]
    private decimal _totalSpent;

    [ObservableProperty]
    private string _selectedStatus = "All";

    [ObservableProperty]
    private string _selectedSort = "Newest First";

    public PurchaseOrdersViewModel(
        IOrderFacade orderFacade,
        IToastService toastService,
        INavigationService navigationService)
        : base(toastService, navigationService)
    {
        _orderFacade = orderFacade;
        PageSize = Core.Common.PaginationConstants.OrdersPageSize;
    }

    public async Task InitializeAsync()
    {
        await LoadDataAsync();
    }

    partial void OnSelectedStatusChanged(string value)
    {
        CurrentPage = 1;
        _ = LoadPageAsync();
    }

    partial void OnSelectedSortChanged(string value)
    {
        _ = LoadPageAsync();
    }

    protected override async Task LoadPageAsync()
    {
        try
        {
            SetLoadingState(true);

            var statusFilter = SelectedStatus == "All" ? null : SelectedStatus;
            var (sortBy, sortDesc) = SelectedSort switch
            {
                "Newest First" => ("orderDate", true),
                "Oldest First" => ("orderDate", false),
                "Highest Amount" => ("finalPrice", true),
                "Lowest Amount" => ("finalPrice", false),
                "Status" => ("status", false),
                _ => ("orderDate", true)
            };

            var result = await _orderFacade.LoadOrdersPagedAsync(
                page: CurrentPage,
                pageSize: PageSize,
                status: statusFilter,
                searchQuery: SearchQuery,
                sortBy: sortBy,
                sortDescending: sortDesc);

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
                Items.Add(new OrderViewModel
                {
                    OrderId = $"ORD-{o.Id.ToString().Substring(0, 8)}",
                    CustomerName = o.CustomerName,
                    OrderDate = o.OrderDate,
                    TrackingNumber = $"TRK{o.Id.ToString().Substring(0, 9)}",
                    Status = o.Status,
                    StatusColor = GetStatusColor(o.Status),
                    StatusBgColor = GetStatusBgColor(o.Status),
                    DeliveredDate = o.Status == "Delivered" ? o.OrderDate.AddDays(3) : null,
                    Items = new ObservableCollection<OrderItemViewModel>(),
                    Subtotal = o.Subtotal,
                    Shipping = 10.00m,
                    Tax = o.Subtotal * 0.08m,
                    Total = o.FinalPrice
                });
            }

            UpdatePagingInfo(result.Data.TotalCount);
            UpdateStats();

            System.Diagnostics.Debug.WriteLine($"[PurchaseOrdersViewModel] Loaded page {CurrentPage}/{TotalPages} ({Items.Count} items, {TotalItems} total)");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[PurchaseOrdersViewModel] Error loading orders: {ex.Message}");
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
        "Delivered" => "#10B981",
        "Shipped" or "Processing" => "#00AEEF",
        "Pending" => "#F59E0B",
        "Cancelled" => "#DC2626",
        _ => "#6B7280"
    };

    private string GetStatusBgColor(string status) => status switch
    {
        "Delivered" => "#D1FAE5",
        "Shipped" or "Processing" => "#DBEAFE",
        "Pending" => "#FEF3C7",
        "Cancelled" => "#FEE2E2",
        _ => "#F3F4F6"
    };

    private void UpdateStats()
    {
        TotalOrders = Items.Count;
        PendingOrders = Items.Count(o => o.Status == "Pending");
        InTransitOrders = Items.Count(o => o.Status == "Shipped" || o.Status == "Processing");
        DeliveredOrders = Items.Count(o => o.Status == "Delivered");
        TotalSpent = Items.Sum(o => o.Total);
    }

    [RelayCommand]
    private void FilterByStatus(string status)
    {
        SelectedStatus = status;
    }

    [RelayCommand]
    private void SortOrders(string sortOption)
    {
        SelectedSort = sortOption;
    }

    [RelayCommand]
    private async Task ViewOrderDetailsAsync(OrderViewModel order)
    {
        try
        {
            var dialog = new OrderDetailsDialog(order)
            {
                XamlRoot = App.MainWindow?.Content?.XamlRoot
            };
            
            await dialog.ShowAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[PurchaseOrdersViewModel] Error showing order details: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task BuyAgainAsync(OrderViewModel order)
    {
        System.Diagnostics.Debug.WriteLine($"[PurchaseOrdersViewModel] Buy again: {order.OrderId}");
        await Task.CompletedTask;
    }
}

public partial class OrderViewModel : ObservableObject
{
    [ObservableProperty]
    private string _orderId = string.Empty;

    [ObservableProperty]
    private string _customerName = string.Empty;

    [ObservableProperty]
    private DateTime _orderDate;

    [ObservableProperty]
    private string _trackingNumber = string.Empty;

    [ObservableProperty]
    private string _status = string.Empty;

    [ObservableProperty]
    private string _statusColor = string.Empty;

    [ObservableProperty]
    private string _statusBgColor = string.Empty;

    [ObservableProperty]
    private DateTime? _deliveredDate;

    [ObservableProperty]
    private ObservableCollection<OrderItemViewModel> _items = new();

    [ObservableProperty]
    private decimal _subtotal;

    [ObservableProperty]
    private decimal _shipping;

    [ObservableProperty]
    private decimal _tax;

    [ObservableProperty]
    private decimal _total;

    public string FormattedOrderDate => OrderDate.ToString("MMM dd, yyyy");
    public string FormattedDeliveredDate => DeliveredDate?.ToString("MMM dd, yyyy") ?? string.Empty;
    public string DeliveryMessage => DeliveredDate.HasValue ? $"Delivered on {FormattedDeliveredDate}" : "In transit";
}

public partial class OrderItemViewModel : ObservableObject
{
    [ObservableProperty]
    private string _productName = string.Empty;

    [ObservableProperty]
    private string _brand = string.Empty;

    [ObservableProperty]
    private int _quantity;

    [ObservableProperty]
    private decimal _price;

    [ObservableProperty]
    private string _imageUrl = string.Empty;
}
