using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Client.Facades;
using MyShop.Client.Views.Dialogs;
using MyShop.Client.ViewModels.Base;
using MyShop.Core.Interfaces.Facades;
using MyShop.Core.Interfaces.Services;
using MyShop.Core.Interfaces.Repositories;
using MyShop.Shared.Models.Enums;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace MyShop.Client.ViewModels.Shared;

public partial class PurchaseOrdersViewModel : PagedViewModelBase<OrderViewModel>
{
    private readonly IOrderFacade _orderFacade;
    private readonly ICartFacade _cartFacade;
    private readonly IAuthRepository _authRepository;
    private readonly IProductRepository _productRepository;

    private Guid? _currentUserId;
    private Guid? _salesAgentId; // For Sales Agent role
    private bool _isSalesAgent;

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

    [ObservableProperty]
    private ObservableCollection<string> _searchSuggestions = new();

    public bool HasNoItems => Items.Count == 0 && !IsLoading;

    public PurchaseOrdersViewModel(
        IOrderFacade orderFacade,
        ICartFacade cartFacade,
        IAuthRepository authRepository,
        IProductRepository productRepository,
        IToastService toastService,
        INavigationService navigationService)
        : base(toastService, navigationService)
    {
        _orderFacade = orderFacade;
        _cartFacade = cartFacade;
        _authRepository = authRepository;
        _productRepository = productRepository;
        PageSize = Core.Common.PaginationConstants.OrdersPageSize;
    }

    public async Task InitializeAsync()
    {
        // Get current user to determine role and filter orders
        var userResult = await _authRepository.GetCurrentUserAsync();
        if (userResult.IsSuccess && userResult.Data != null)
        {
            var user = userResult.Data;
            _currentUserId = user.Id;

            // Check if user is Sales Agent
            _isSalesAgent = user.HasRole(UserRole.SalesAgent);

            // Purchase Orders = orders where current user is the CUSTOMER (buyer)
            // Even Sales Agents can be customers when they buy products
            // So we always use customerId = user.Id for this page
            // (SalesOrders page uses salesAgentId for orders they SOLD)
            _salesAgentId = null; // Don't filter by salesAgentId for purchase orders

            System.Diagnostics.Debug.WriteLine($"[PurchaseOrdersViewModel] Customer mode: {_currentUserId} (isSalesAgent: {_isSalesAgent})");
        }

        await LoadDataAsync();
    }

    public async Task UpdateSearchSuggestionsAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
        {
            SearchSuggestions.Clear();
            return;
        }

        // Generate suggestions based on order IDs and status
        var suggestions = new List<string>();

        foreach (var order in Items)
        {
            if (order.OrderId.Contains(query, StringComparison.OrdinalIgnoreCase))
            {
                suggestions.Add(order.OrderId);
            }
            if (order.TrackingNumber.Contains(query, StringComparison.OrdinalIgnoreCase))
            {
                suggestions.Add(order.TrackingNumber);
            }
        }

        SearchSuggestions.Clear();
        foreach (var s in suggestions.Distinct().Take(5))
        {
            SearchSuggestions.Add(s);
        }
    }

    partial void OnSelectedStatusChanged(string value)
    {
        System.Diagnostics.Debug.WriteLine($"[PurchaseOrdersViewModel] Status changed to: {value}");
        CurrentPage = 1;
        _ = LoadPageAsync();
    }

    partial void OnSelectedSortChanged(string value)
    {
        System.Diagnostics.Debug.WriteLine($"[PurchaseOrdersViewModel] Sort changed to: {value}");
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
                sortDescending: sortDesc,
                customerId: _currentUserId,
                salesAgentId: _salesAgentId);

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
                // Get order items from the order
                var orderItems = o.OrderItems ?? o.Items ?? new List<MyShop.Shared.Models.OrderItem>();

                // Fetch product info from database for each order item
                var itemViewModels = new ObservableCollection<OrderItemViewModel>();
                string? firstProductImage = null;

                foreach (var item in orderItems)
                {
                    var productName = item.ProductName ?? "Unknown Product";
                    var imageUrl = "ms-appx:///Assets/Images/placeholder.png";

                    // Fetch product from database to get image
                    if (item.ProductId != Guid.Empty)
                    {
                        var productResult = await _productRepository.GetByIdAsync(item.ProductId);
                        if (productResult.IsSuccess && productResult.Data != null)
                        {
                            productName = productResult.Data.Name ?? productName;
                            imageUrl = productResult.Data.ImageUrl ?? imageUrl;
                        }
                    }

                    // Store first product image for card display
                    if (firstProductImage == null)
                    {
                        firstProductImage = imageUrl;
                    }

                    itemViewModels.Add(new OrderItemViewModel
                    {
                        ProductName = productName,
                        Quantity = item.Quantity,
                        Price = item.UnitPrice,
                        ImageUrl = imageUrl
                    });
                }

                // Generate product summary (e.g., "MacBook Pro + 2 more items")
                var productSummary = GenerateProductSummary(itemViewModels);

                Items.Add(new OrderViewModel
                {
                    OrderId = o.OrderCode ?? $"ORD-{o.Id.ToString().Substring(0, 8)}",
                    OrderGuid = o.Id,
                    ProductSummary = productSummary,
                    FirstProductImage = firstProductImage ?? "ms-appx:///Assets/Images/placeholder.png",
                    CustomerName = o.CustomerName,
                    OrderDate = o.OrderDate,
                    TrackingNumber = $"TRK{o.Id.ToString().Substring(0, 9)}",
                    Status = o.Status,
                    DeliveredDate = o.Status == "Delivered" || o.Status == "DELIVERED" ? o.OrderDate.AddDays(3) : null,
                    Items = itemViewModels,
                    Subtotal = (decimal)o.Subtotal,
                    Shipping = (decimal)o.ShippingFee,
                    Tax = (decimal)o.Tax,
                    Total = (decimal)o.FinalPrice
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

    private void UpdateStats()
    {
        TotalOrders = Items.Count;
        PendingOrders = Items.Count(o => o.Status.Equals("Pending", StringComparison.OrdinalIgnoreCase) || o.Status.Equals("CREATED", StringComparison.OrdinalIgnoreCase));
        InTransitOrders = Items.Count(o => o.Status.Equals("Shipped", StringComparison.OrdinalIgnoreCase) || o.Status.Equals("Processing", StringComparison.OrdinalIgnoreCase));
        DeliveredOrders = Items.Count(o => o.Status.Equals("Delivered", StringComparison.OrdinalIgnoreCase) || o.Status.Equals("PAID", StringComparison.OrdinalIgnoreCase));
        TotalSpent = Items.Sum(o => o.Total);
    }

    private string GenerateProductSummary(ObservableCollection<OrderItemViewModel>? loadedItems)
    {
        if (loadedItems == null || loadedItems.Count == 0)
            return "No products";

        var firstProduct = loadedItems[0].ProductName ?? "Product";
        var firstQty = loadedItems[0].Quantity;

        // Truncate long product names
        if (firstProduct.Length > 35)
            firstProduct = firstProduct.Substring(0, 32) + "...";

        // If only one product type
        if (loadedItems.Count == 1)
        {
            if (firstQty > 1)
                return $"{firstProduct} x{firstQty}";
            return firstProduct;
        }

        // Multiple product types - show count of other product types (not quantities)
        var otherProductCount = loadedItems.Count - 1;
        return $"{firstProduct} + {otherProductCount} more product{(otherProductCount > 1 ? "s" : "")}";
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
    private async Task ApplyFiltersAsync()
    {
        CurrentPage = 1;
        await LoadPageAsync();
    }

    [RelayCommand]
    private async Task ResetFiltersAsync()
    {
        SearchQuery = string.Empty;
        SelectedStatus = "All";
        SelectedSort = "Newest First";
        CurrentPage = 1;
        await LoadPageAsync();
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
            System.Diagnostics.Debug.WriteLine($"[PurchaseOrdersViewModel] Buy again: {order.OrderId}");

            // Add order items back to cart (mock implementation)
            // In real implementation, would call _cartFacade.AddItemsFromOrder(orderId)
            await _toastHelper?.ShowSuccess($"Items from {order.OrderId} added to cart!");

            // Navigate to cart page
            _navigationService?.NavigateTo("Cart");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[PurchaseOrdersViewModel] Error in BuyAgain: {ex.Message}");
            await _toastHelper?.ShowError("Failed to add items to cart");
        }
    }

    [RelayCommand]
    private async Task PayNowAsync(OrderViewModel order)
    {
        try
        {
            if (order?.OrderGuid == Guid.Empty)
            {
                await _toastHelper?.ShowError("Invalid order ID");
                return;
            }

            System.Diagnostics.Debug.WriteLine($"[PurchaseOrdersViewModel] Processing payment for order: {order.OrderId}");

            // Call payment API with card details
            var result = await _orderFacade.ProcessCardPaymentAsync(
                orderId: order.OrderGuid,
                cardNumber: "4532015112830366", // Mock card for testing
                cardHolderName: "Test User",
                expiryDate: "12/25",
                cvv: "123");

            if (result.IsSuccess)
            {
                await _toastHelper?.ShowSuccess("Payment processed successfully!");

                // Reload orders to refresh status
                CurrentPage = 1;
                await LoadPageAsync();
            }
            else
            {
                await _toastHelper?.ShowError(result.ErrorMessage ?? "Payment failed");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[PurchaseOrdersViewModel] Error processing payment: {ex.Message}");
            await _toastHelper?.ShowError($"Payment error: {ex.Message}");
        }
    }
}

public partial class OrderViewModel : ObservableObject
{
    [ObservableProperty]
    private Guid _orderGuid;

    [ObservableProperty]
    private string _orderId = string.Empty;

    [ObservableProperty]
    private string _productSummary = string.Empty;

    [ObservableProperty]
    private string _firstProductImage = "ms-appx:///Assets/Images/placeholder.png";

    [ObservableProperty]
    private string _customerName = string.Empty;

    [ObservableProperty]
    private DateTime _orderDate;

    [ObservableProperty]
    private string _trackingNumber = string.Empty;

    [ObservableProperty]
    private string _status = string.Empty;

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
