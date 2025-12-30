using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Client.Facades;
using MyShop.Client.ViewModels.Base;
using MyShop.Core.Interfaces.Facades;
using MyShop.Core.Interfaces.Repositories;
using MyShop.Core.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace MyShop.Client.ViewModels.SalesAgent;

public partial class SalesAgentOrdersViewModel : PagedViewModelBase<OrderViewModel>
{
    private readonly IOrderFacade _orderFacade;
    private readonly IAuthRepository _authRepository;
    private Guid? _currentSalesAgentId;
    private Func<XamlRoot?>? _xamlRootProvider;

    [ObservableProperty]
    private string _selectedStatus = "All";

    [ObservableProperty]
    private string _selectedPaymentStatus = "All";

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
            var paymentStatusFilter = SelectedPaymentStatus == "All" ? null : SelectedPaymentStatus;

            var result = await _orderFacade.LoadOrdersPagedAsync(
                page: CurrentPage,
                pageSize: PageSize,
                status: statusFilter,
                paymentStatus: paymentStatusFilter,
                searchQuery: SearchQuery,
                salesAgentId: _currentSalesAgentId);  // API uses JWT to identify current sales agent

            if (!result.IsSuccess || result.Data == null)
            {
                await _toastHelper?.ShowError(result.ErrorMessage ?? "Failed to load orders");
                Items.Clear();
                UpdatePagingInfo(0);
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
                    OriginalOrderId = o.Id,
                    OrderId = FormatOrderId(o.Id),
                    CustomerName = o.CustomerName,
                    CustomerEmail = $"{o.CustomerName.ToLower().Replace(" ", ".")}@example.com",
                    ProductDescription = productDesc,
                    OrderDate = o.OrderDate,
                    Status = o.Status,
                    PaymentStatus = o.PaymentStatus,
                    PaymentMethod = o.PaymentMethod ?? "N/A",
                    TotalAmount = o.FinalPrice,
                    CommissionAmount = o.FinalPrice * 0.10m,
                    OrderItems = orderItems ?? new()
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

    public void SetXamlRootProvider(Func<XamlRoot?> provider)
    {
        _xamlRootProvider = provider;
    }

    [RelayCommand]
    private async Task ViewOrderDetailsAsync(OrderViewModel order)
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"[SalesAgentOrdersViewModel] View order details: {order.OrderId}");

            if (order?.OrderItems == null || order.OrderItems.Count == 0)
            {
                await _toastHelper?.ShowWarning("No items in this order");
                return;
            }

            // Create and show dialog
            var dialog = new MyShop.Client.Views.Dialogs.OrderItemsDialog();
            var xamlRoot = _xamlRootProvider?.Invoke();
            if (xamlRoot != null)
            {
                dialog.XamlRoot = xamlRoot;
            }

            // Set up callbacks for action buttons
            dialog.OnMarkAsProcessingAsync = async () =>
            {
                await MarkAsProcessingAsync(order);
            };

            dialog.OnMarkAsShippedAsync = async () =>
            {
                await MarkAsShippedAsync(order);
            };

            dialog.OnMarkAsDeliveredAsync = async () =>
            {
                await MarkAsDeliveredAsync(order);
            };

            dialog.Initialize(order.OrderItems, order.Status, order.OrderId);
            await dialog.ShowAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[SalesAgentOrdersViewModel] Error viewing order details: {ex.Message}");
            await _toastHelper?.ShowError($"Error: {ex.Message}");
        }
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

    [RelayCommand]
    private async Task MarkAsProcessingAsync(OrderViewModel order)
    {
        if (order == null)
        {
            await _toastHelper?.ShowError("Order not found");
            return;
        }

        // Show confirmation dialog
        var xamlRoot = _xamlRootProvider?.Invoke();
        if (xamlRoot == null)
        {
            await _toastHelper?.ShowError("Cannot show dialog");
            return;
        }

        var dialog = new ContentDialog
        {
            Title = "Confirm Status Update",
            Content = new TextBlock
            {
                Text = $"Mark order {order.OrderId} as PROCESSING?\n\nNote: This action cannot be easily undone.",
                TextWrapping = TextWrapping.Wrap,
                Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 255, 255))
            },
            PrimaryButtonText = "Process",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = xamlRoot
        };

        var result = await dialog.ShowAsync();
        if (result != ContentDialogResult.Primary)
        {
            return;
        }

        // Proceed with update
        SetLoadingState(true);
        try
        {
            var updateResult = await _orderFacade.UpdateOrderStatusAsync(
                order.OriginalOrderId,
                "Processing");

            if (updateResult.IsSuccess && updateResult.Data != null)
            {
                // Update local order status
                order.Status = "Processing";
                await _toastHelper?.ShowSuccess("Order marked as processing");
                System.Diagnostics.Debug.WriteLine($"[SalesAgentOrdersViewModel] Order {order.OrderId} processing");

                // Refresh the list to update UI
                await LoadPageAsync();
            }
            else
            {
                await _toastHelper?.ShowError(updateResult.ErrorMessage ?? "Failed to update order");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[SalesAgentOrdersViewModel] Error marking as processing: {ex.Message}");
            await _toastHelper?.ShowError($"Error: {ex.Message}");
        }
        finally
        {
            SetLoadingState(false);
        }
    }

    [RelayCommand]
    private async Task MarkAsShippedAsync(OrderViewModel order)
    {
        if (order == null)
        {
            await _toastHelper?.ShowError("Order not found");
            return;
        }

        // Show confirmation dialog
        var xamlRoot = _xamlRootProvider?.Invoke();
        if (xamlRoot == null)
        {
            await _toastHelper?.ShowError("Cannot show dialog");
            return;
        }

        var dialog = new ContentDialog
        {
            Title = "Confirm Status Update",
            Content = new TextBlock
            {
                Text = $"Mark order {order.OrderId} as SHIPPED?\n\nNote: This action cannot be easily undone.",
                TextWrapping = TextWrapping.Wrap,
                Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 255, 255))
            },
            PrimaryButtonText = "Ship",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = xamlRoot
        };

        var result = await dialog.ShowAsync();
        if (result != ContentDialogResult.Primary)
        {
            return;
        }

        // Proceed with update
        SetLoadingState(true);
        try
        {
            var updateResult = await _orderFacade.UpdateOrderStatusAsync(
                order.OriginalOrderId,
                "Shipped");

            if (updateResult.IsSuccess && updateResult.Data != null)
            {
                // Update local order status
                order.Status = "Shipped";
                await _toastHelper?.ShowSuccess("Order marked as shipped");
                System.Diagnostics.Debug.WriteLine($"[SalesAgentOrdersViewModel] Order {order.OrderId} shipped");

                // Refresh the list to update UI
                await LoadPageAsync();
            }
            else
            {
                await _toastHelper?.ShowError(updateResult.ErrorMessage ?? "Failed to update order");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[SalesAgentOrdersViewModel] Error marking as shipped: {ex.Message}");
            await _toastHelper?.ShowError($"Error: {ex.Message}");
        }
        finally
        {
            SetLoadingState(false);
        }
    }

    [RelayCommand]
    private async Task MarkAsDeliveredAsync(OrderViewModel order)
    {
        if (order == null)
        {
            await _toastHelper?.ShowError("Order not found");
            return;
        }

        // Show confirmation dialog
        var xamlRoot = _xamlRootProvider?.Invoke();
        if (xamlRoot == null)
        {
            await _toastHelper?.ShowError("Cannot show dialog");
            return;
        }

        var dialog = new ContentDialog
        {
            Title = "Confirm Status Update",
            Content = new TextBlock
            {
                Text = $"Mark order {order.OrderId} as DELIVERED?\n\nNote: This action cannot be easily undone.",
                TextWrapping = TextWrapping.Wrap,
                Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 255, 255))
            },
            PrimaryButtonText = "Deliver",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = xamlRoot
        };

        var result = await dialog.ShowAsync();
        if (result != ContentDialogResult.Primary)
        {
            return;
        }

        // Proceed with update
        SetLoadingState(true);
        try
        {
            var updateResult = await _orderFacade.UpdateOrderStatusAsync(
                order.OriginalOrderId,
                "Delivered");

            if (updateResult.IsSuccess && updateResult.Data != null)
            {
                // Update local order status
                order.Status = "Delivered";
                await _toastHelper?.ShowSuccess("Order marked as delivered");
                System.Diagnostics.Debug.WriteLine($"[SalesAgentOrdersViewModel] Order {order.OrderId} delivered");

                // Refresh the list to update UI
                await LoadPageAsync();
            }
            else
            {
                await _toastHelper?.ShowError(updateResult.ErrorMessage ?? "Failed to update order");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[SalesAgentOrdersViewModel] Error marking as delivered: {ex.Message}");
            await _toastHelper?.ShowError($"Error: {ex.Message}");
        }
        finally
        {
            SetLoadingState(false);
        }
    }

    [RelayCommand]
    private async Task ConfirmPaymentReceivedAsync(OrderViewModel order)
    {
        if (order == null)
        {
            await _toastHelper?.ShowError("Order not found");
            return;
        }

        // Show confirmation dialog
        var xamlRoot = _xamlRootProvider?.Invoke();
        if (xamlRoot == null)
        {
            await _toastHelper?.ShowError("Cannot show dialog");
            return;
        }

        var paymentMethodLabel = order.PaymentMethod == "QR" ? "QR payment" : "cash payment";
        var dialog = new ContentDialog
        {
            Title = "Confirm Payment Received",
            Content = new TextBlock
            {
                Text = $"Confirm that you have received {paymentMethodLabel} for order {order.OrderId}?\n\nPayment Status will be updated from UNPAID to PAID.",
                TextWrapping = TextWrapping.Wrap,
                Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(255, 255, 255, 255))
            },
            PrimaryButtonText = "Confirm",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = xamlRoot
        };

        var result = await dialog.ShowAsync();
        if (result != ContentDialogResult.Primary)
        {
            return;
        }

        // Proceed with update
        SetLoadingState(true);
        try
        {
            var updateResult = await _orderFacade.UpdatePaymentStatusAsync(
                order.OriginalOrderId,
                "Paid");

            if (updateResult.IsSuccess && updateResult.Data != null)
            {
                // Update local order payment status
                order.PaymentStatus = "Paid";
                await _toastHelper?.ShowSuccess("Payment confirmed successfully");
                System.Diagnostics.Debug.WriteLine($"[SalesAgentOrdersViewModel] Order {order.OrderId} payment confirmed");

                // Refresh the list to update UI
                await LoadPageAsync();
            }
            else
            {
                await _toastHelper?.ShowError(updateResult.ErrorMessage ?? "Failed to confirm payment");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[SalesAgentOrdersViewModel] Error confirming payment: {ex.Message}");
            await _toastHelper?.ShowError($"Error: {ex.Message}");
        }
        finally
        {
            SetLoadingState(false);
        }
    }

    /// <summary>
    /// Format Order ID to show first character + last 4 characters for uniqueness
    /// Example: 9a68d343-08ca-4665-9161-7df1902c3035 → ORD-93035
    /// </summary>
    private string FormatOrderId(Guid id)
    {
        var idString = id.ToString().Replace("-", "");
        var firstChar = idString[0];
        var lastFourChars = idString.Substring(idString.Length - 8);
        return $"ORD-{firstChar}{lastFourChars}";
    }
}

public partial class OrderViewModel : ObservableObject
{
    [ObservableProperty]
    private Guid _originalOrderId;

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
    private string _paymentStatus = string.Empty;

    [ObservableProperty]
    private string _paymentMethod = string.Empty;

    [ObservableProperty]
    private decimal _totalAmount;

    [ObservableProperty]
    private decimal _commissionAmount;

    [ObservableProperty]
    private List<MyShop.Shared.Models.OrderItem> _orderItems = new();

    public string FormattedDate => OrderDate.ToString("MMM dd, yyyy");

    /// <summary>
    /// True if order is in CONFIRMED status and can be marked as SHIPPED
    /// </summary>
    public bool CanShip => Status == "Confirmed";

    /// <summary>
    /// True if order is in SHIPPED status and can be marked as DELIVERED
    /// </summary>
    public bool CanDeliver => Status == "Shipped";

    /// <summary>
    /// True if payment is unpaid and method is QR or COD (needs agent verification)
    /// </summary>
    public bool NeedsPaymentConfirmation =>
        PaymentStatus == "Unpaid" &&
        (PaymentMethod == "QR" || PaymentMethod == "COD");
}
