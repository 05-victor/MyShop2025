using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Core.Interfaces.Repositories;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace MyShop.Client.ViewModels.SalesAgent;

public partial class SalesAgentOrdersViewModel : ObservableObject
{
    private readonly IOrderRepository _orderRepository;

    [ObservableProperty]
    private ObservableCollection<OrderViewModel> _orders;

    [ObservableProperty]
    private string _selectedStatus = "All";

    [ObservableProperty]
    private int _totalOrders;

    [ObservableProperty]
    private int _pendingOrders;

    [ObservableProperty]
    private int _completedOrders;

    [ObservableProperty]
    private int _cancelledOrders;

    public SalesAgentOrdersViewModel(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
        Orders = new ObservableCollection<OrderViewModel>();
    }

    public async Task InitializeAsync()
    {
        await LoadOrdersAsync();
    }

    private ObservableCollection<OrderViewModel> _allOrders = new();

    private async Task LoadOrdersAsync()
    {
        try
        {
            var result = await _orderRepository.GetAllAsync();
            if (!result.IsSuccess || result.Data == null)
            {
                _allOrders = new ObservableCollection<OrderViewModel>();
                return;
            }
            
            _allOrders = new ObservableCollection<OrderViewModel>(
                result.Data.Select(o => new OrderViewModel
                {
                    OrderId = $"ORD-{o.Id.ToString().Substring(0, 8)}",
                    CustomerName = o.CustomerName,
                    CustomerEmail = $"{o.CustomerName.ToLower().Replace(" ", ".")}@example.com",
                    OrderDate = o.OrderDate,
                    Status = o.Status,
                    StatusColor = GetStatusColor(o.Status),
                    StatusBgColor = GetStatusBgColor(o.Status),
                    TotalAmount = o.FinalPrice,
                    CommissionAmount = o.FinalPrice * 0.10m
                })
            );

            ApplyFilters();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[SalesOrdersViewModel] Error loading orders: {ex.Message}");
            _allOrders = new ObservableCollection<OrderViewModel>();
            Orders = new ObservableCollection<OrderViewModel>();
            UpdateStats();
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
        TotalOrders = Orders.Count;
        PendingOrders = Orders.Count(o => o.Status == "Pending");
        CompletedOrders = Orders.Count(o => o.Status == "Completed" || o.Status == "Delivered");
        CancelledOrders = Orders.Count(o => o.Status == "Cancelled");
    }

    [RelayCommand]
    private void FilterByStatus(string status)
    {
        SelectedStatus = status;
        ApplyFilters();
    }

    private void ApplyFilters()
    {
        var filtered = _allOrders.AsEnumerable();

        if (SelectedStatus != "All")
        {
            filtered = filtered.Where(o => o.Status.Equals(SelectedStatus, StringComparison.OrdinalIgnoreCase));
        }

        Orders = new ObservableCollection<OrderViewModel>(filtered);
        UpdateStats();
    }

    [RelayCommand]
    private void ViewOrderDetails(OrderViewModel order)
    {
        System.Diagnostics.Debug.WriteLine($"[SalesOrdersViewModel] View order details: {order.OrderId}");
        // Navigation will be implemented when OrderDetailsPage is created
        // _navigationService.Navigate(typeof(OrderDetailsPage), order.OrderId);
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
