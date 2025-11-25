using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Core.Interfaces.Repositories;
using MyShop.Client.Views.Dialogs;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace MyShop.Client.ViewModels.Shared;

public partial class PurchaseOrdersViewModel : ObservableObject
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICartRepository _cartRepository;
    private List<OrderViewModel> _allOrders = new();

    [ObservableProperty]
    private ObservableCollection<OrderViewModel> _orders;

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

    public PurchaseOrdersViewModel(IOrderRepository orderRepository, ICartRepository cartRepository)
    {
        _orderRepository = orderRepository;
        _cartRepository = cartRepository;
        Orders = new ObservableCollection<OrderViewModel>();
    }

    public async Task InitializeAsync()
    {
        await LoadOrdersAsync();
    }

    private async Task LoadOrdersAsync()
    {
        try
        {
            var result = await _orderRepository.GetAllAsync();
            if (!result.IsSuccess || result.Data == null)
            {
                _allOrders = new List<OrderViewModel>();
                return;
            }
            
            _allOrders = result.Data.Select(o => new OrderViewModel
            {
                OrderId = $"ORD-{o.Id.ToString().Substring(0, 8)}",
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
            }).ToList();

            ApplyFiltersAndSort();
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[PurchaseOrdersViewModel] Error loading orders: {ex.Message}");
            _allOrders = new List<OrderViewModel>();
            Orders = new ObservableCollection<OrderViewModel>();
            UpdateStats();
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
        TotalOrders = Orders.Count;
        PendingOrders = Orders.Count(o => o.Status == "Pending");
        InTransitOrders = Orders.Count(o => o.Status == "Shipped" || o.Status == "Processing");
        DeliveredOrders = Orders.Count(o => o.Status == "Delivered");
        TotalSpent = Orders.Sum(o => o.Total);
    }

    [RelayCommand]
    private void FilterByStatus(string status)
    {
        SelectedStatus = status;
        ApplyFiltersAndSort();
    }

    [RelayCommand]
    private void SortOrders(string sortOption)
    {
        SelectedSort = sortOption;
        ApplyFiltersAndSort();
    }

    private void ApplyFiltersAndSort()
    {
        // Apply status filter
        var filtered = _allOrders.AsEnumerable();
        
        if (SelectedStatus != "All")
        {
            filtered = filtered.Where(o => o.Status.Equals(SelectedStatus, StringComparison.OrdinalIgnoreCase));
        }

        // Apply sorting
        filtered = SelectedSort switch
        {
            "Newest First" => filtered.OrderByDescending(o => o.OrderDate),
            "Oldest First" => filtered.OrderBy(o => o.OrderDate),
            "Highest Amount" => filtered.OrderByDescending(o => o.Total),
            "Lowest Amount" => filtered.OrderBy(o => o.Total),
            "Status" => filtered.OrderBy(o => o.Status),
            _ => filtered.OrderByDescending(o => o.OrderDate)
        };

        Orders = new ObservableCollection<OrderViewModel>(filtered);
        UpdateStats();
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
        try
        {
            // Get order details with items
            var orderGuid = Guid.Parse(order.OrderId);
            var orderResult = await _orderRepository.GetByIdAsync(orderGuid);

            if (!orderResult.IsSuccess || orderResult.Data == null || !orderResult.Data.Items.Any())
            {
                System.Diagnostics.Debug.WriteLine($"[PurchaseOrdersViewModel] Order {order.OrderId} has no items");
                return;
            }

            // Add each item to cart
            foreach (var item in orderResult.Data.Items)
            {
                await _cartRepository.AddToCartAsync(Guid.Empty, item.ProductId, item.Quantity);
            }

            System.Diagnostics.Debug.WriteLine($"[PurchaseOrdersViewModel] Added {orderResult.Data.Items.Count} items to cart");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[PurchaseOrdersViewModel] Error re-ordering: {ex.Message}");
        }
    }
}

public partial class OrderViewModel : ObservableObject
{
    [ObservableProperty]
    private string _orderId = string.Empty;

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
