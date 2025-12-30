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
    private string _selectedPaymentStatus = "All";

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

    protected override async Task LoadPageAsync()
    {
        try
        {
            SetLoadingState(true);

            var statusFilter = SelectedStatus == "All" ? null : SelectedStatus;
            var paymentStatusFilter = SelectedPaymentStatus == "All" ? null : SelectedPaymentStatus;

            var result = await _orderFacade.LoadOrdersPagedAsync(
                page: CurrentPage,
                pageSize: PageSize,
                status: statusFilter,
                paymentStatus: paymentStatusFilter,
                searchQuery: null,
                sortBy: null,
                sortDescending: false,
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

                System.Diagnostics.Debug.WriteLine($"[Order {o.OrderCode}] Has {orderItems.Count} items");

                foreach (var item in orderItems)
                {
                    var productName = item.ProductName ?? "Unknown Product";
                    var imageUrl = "ms-appx:///Assets/Images/products/product-placeholder.png";

                    System.Diagnostics.Debug.WriteLine($"[OrderItem] ProductId: {item.ProductId}, ProductName from API: '{item.ProductName}', UnitPrice: {item.UnitPrice}, Quantity: {item.Quantity}");

                    // Fetch product from database to get image
                    if (item.ProductId != Guid.Empty)
                    {
                        var productResult = await _productRepository.GetByIdAsync(item.ProductId);
                        if (productResult.IsSuccess && productResult.Data != null)
                        {
                            productName = productResult.Data.Name ?? productName;
                            imageUrl = productResult.Data.ImageUrl ?? imageUrl;
                            System.Diagnostics.Debug.WriteLine($"[OrderItem] Loaded product from DB: '{productName}', Price: {productResult.Data.SellingPrice}");
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"[OrderItem] Failed to load product {item.ProductId} from DB: {productResult.ErrorMessage}");
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"[OrderItem] ProductId is empty, using fallback");
                    }

                    // Store first product image for card display
                    if (firstProductImage == null)
                    {
                        firstProductImage = imageUrl;
                    }

                    itemViewModels.Add(new OrderItemViewModel
                    {
                        ProductId = item.ProductId,
                        ProductName = productName,
                        Quantity = item.Quantity,
                        Price = item.UnitPrice,
                        ImageUrl = imageUrl
                    });

                    System.Diagnostics.Debug.WriteLine($"[OrderItem] Created ViewModel: Name='{productName}', Price={item.UnitPrice}, Qty={item.Quantity}");
                }

                // Generate product summary (e.g., "MacBook Pro + 2 more items")
                var productSummary = GenerateProductSummary(itemViewModels);

                Items.Add(new OrderViewModel
                {
                    OrderId = o.OrderCode ?? $"ORD-{o.Id.ToString().Substring(0, 8)}",
                    OrderGuid = o.Id,
                    ProductSummary = productSummary,
                    FirstProductImage = firstProductImage ?? "ms-appx:///Assets/Images/products/product-placeholder.png",
                    CustomerName = o.CustomerName,
                    OrderDate = o.OrderDate,
                    TrackingNumber = $"TRK{o.Id.ToString().Substring(0, 9)}",
                    Status = o.Status,
                    PaymentStatus = o.PaymentStatus,
                    PaymentMethod = o.PaymentMethod,
                    DeliveredDate = o.Status == "Delivered" || o.Status == "DELIVERED" ? o.OrderDate.AddDays(3) : null,
                    Items = itemViewModels,
                    Subtotal = (decimal)o.Subtotal,
                    Shipping = (decimal)o.ShippingFee,
                    Tax = (decimal)o.Tax,
                    Total = CalculateOrderTotal(o)
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

    /// <summary>
    /// Calculate order total using CheckoutViewModel logic: Total = Subtotal + Shipping + Tax
    /// Falls back to FinalPrice if calculation yields 0
    /// </summary>
    private decimal CalculateOrderTotal(MyShop.Shared.Models.Order order)
    {
        System.Diagnostics.Debug.WriteLine($"[CalculateOrderTotal] Order {order.OrderCode}: FinalPrice={order.FinalPrice}, Subtotal={order.Subtotal}, Shipping={order.ShippingFee}, Tax={order.Tax}");

        // Try using FinalPrice first (most accurate from backend)
        if (order.FinalPrice > 0)
        {
            System.Diagnostics.Debug.WriteLine($"[CalculateOrderTotal] Using FinalPrice: {order.FinalPrice}");
            return (decimal)order.FinalPrice;
        }

        // Fallback: Calculate like CheckoutViewModel
        var subtotal = (decimal)order.Subtotal;
        var shipping = (decimal)order.ShippingFee;
        var tax = (decimal)order.Tax;
        var calculatedTotal = subtotal + shipping + tax;

        System.Diagnostics.Debug.WriteLine($"[CalculateOrderTotal] Calculated: {subtotal} + {shipping} + {tax} = {calculatedTotal}");

        // If still 0, try sum of items
        if (calculatedTotal == 0 && order.Items?.Any() == true)
        {
            calculatedTotal = order.Items.Sum(i => (decimal)i.TotalPrice);
            System.Diagnostics.Debug.WriteLine($"[CalculateOrderTotal] Fallback from items: {calculatedTotal}");
        }

        return calculatedTotal;
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
        SelectedPaymentStatus = "All";
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
            if (order?.Items == null || !order.Items.Any())
            {
                await _toastHelper?.ShowWarning("No items to add");
                return;
            }

            System.Diagnostics.Debug.WriteLine($"[PurchaseOrdersViewModel] Buy again: {order.OrderId}, {order.Items.Count} items");

            var successCount = 0;
            var failCount = 0;

            foreach (var item in order.Items)
            {
                var result = await _cartFacade.AddToCartAsync(item.ProductId, item.Quantity);
                if (result.IsSuccess)
                {
                    successCount++;
                }
                else
                {
                    failCount++;
                    System.Diagnostics.Debug.WriteLine($"[PurchaseOrdersViewModel] Failed to add {item.ProductName}: {result.ErrorMessage}");
                }
            }

            if (successCount > 0)
            {
                await _toastHelper?.ShowSuccess($"Added {successCount} item(s) to cart!");
                
                // Navigate to cart page
                _navigationService?.NavigateTo("Cart");
            }
            
            if (failCount > 0)
            {
                await _toastHelper?.ShowWarning($"{failCount} item(s) could not be added");
            }
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

            System.Diagnostics.Debug.WriteLine($"[PurchaseOrdersViewModel] PayNow for order: {order.OrderId}, PaymentMethod: {order.PaymentMethod}");

            // Check payment method
            var paymentMethod = order.PaymentMethod?.ToUpper();

            switch (paymentMethod)
            {
                case "CARD":
                    // Navigate to CardPaymentPage
                    var parameter = new Views.Shared.CardPaymentParameter
                    {
                        OrderId = order.OrderGuid,
                        OrderCode = order.OrderId,
                        TotalAmount = order.Total
                    };

                    await _navigationService.NavigateInShell(
                        typeof(Views.Shared.CardPaymentPage).FullName!,
                        parameter);
                    break;

                case "QR":
                    await _toastHelper?.ShowInfo("Your QR payment is pending confirmation from the seller. Please wait.");
                    break;

                case "COD":
                    await _toastHelper?.ShowInfo("This order uses Cash on Delivery. You will pay when receiving your order.");
                    break;

                default:
                    await _toastHelper?.ShowWarning($"Unknown payment method: {order.PaymentMethod}");
                    break;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[PurchaseOrdersViewModel] Error in PayNow: {ex.Message}");
            await _toastHelper?.ShowError($"Error: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task CancelOrderAsync(OrderViewModel order)
    {
        try
        {
            if (order?.OrderGuid == Guid.Empty)
            {
                await _toastHelper?.ShowError("Invalid order ID");
                return;
            }

            System.Diagnostics.Debug.WriteLine($"[PurchaseOrdersViewModel] Cancelling order: {order.OrderId}");

            // Call order facade to cancel order
            var result = await _orderFacade.CancelOrderAsync(order.OrderGuid, "Cancelled by customer");

            if (result.IsSuccess)
            {
                await _toastHelper?.ShowSuccess("Order cancelled successfully");

                // Reload orders to refresh list
                await LoadPageAsync();
            }
            else
            {
                await _toastHelper?.ShowError(result.ErrorMessage ?? "Failed to cancel order");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[PurchaseOrdersViewModel] Error cancelling order: {ex.Message}");
            await _toastHelper?.ShowError($"Error: {ex.Message}");
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
    private string _paymentStatus = string.Empty;

    [ObservableProperty]
    private string _paymentMethod = string.Empty;

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

    /// <summary>
    /// Can pay now if order is pending/confirmed, payment is unpaid, AND payment method is CARD
    /// </summary>
    public bool CanPayNow =>
        (Status.Equals("Pending", StringComparison.OrdinalIgnoreCase) ||
         Status.Equals("Confirmed", StringComparison.OrdinalIgnoreCase) ||
         Status.Equals("PENDING", StringComparison.OrdinalIgnoreCase)) &&
        (PaymentStatus.Equals("Unpaid", StringComparison.OrdinalIgnoreCase) ||
         PaymentStatus.Equals("UNPAID", StringComparison.OrdinalIgnoreCase)) &&
        (PaymentMethod?.Equals("CARD", StringComparison.OrdinalIgnoreCase) ?? false);

    /// <summary>
    /// Can cancel if order is pending/confirmed, not yet paid, and payment method is CARD
    /// QR and COD cannot be cancelled (QR already scanned, COD no prepayment)
    /// </summary>
    public bool CanCancel =>
        (Status.Equals("Pending", StringComparison.OrdinalIgnoreCase) ||
         Status.Equals("Confirmed", StringComparison.OrdinalIgnoreCase) ||
         Status.Equals("PENDING", StringComparison.OrdinalIgnoreCase)) &&
        (PaymentStatus.Equals("Unpaid", StringComparison.OrdinalIgnoreCase) ||
         PaymentStatus.Equals("UNPAID", StringComparison.OrdinalIgnoreCase)) &&
        (PaymentMethod?.Equals("CARD", StringComparison.OrdinalIgnoreCase) ?? false);
}

public partial class OrderItemViewModel : ObservableObject
{
    [ObservableProperty]
    private Guid _productId;

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
