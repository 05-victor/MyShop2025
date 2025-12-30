using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using MyShop.Client.Facades;
using MyShop.Core.Interfaces.Facades;
using MyShop.Core.Interfaces.Services;
using MyShop.Core.Interfaces.Repositories;

namespace MyShop.Client.ViewModels.Shared;

public partial class CheckoutViewModel : ObservableObject
{
    private readonly ICartFacade _cartFacade;
    private readonly INavigationService _navigationService;
    private readonly IAuthRepository _authRepository;
    private readonly IToastService _toastService;

    [ObservableProperty]
    private bool _isLoading = false;

    [ObservableProperty]
    private bool _isProcessing = false;

    public CheckoutViewModel(
        ICartFacade cartFacade, 
        INavigationService navigationService,
        IAuthRepository authRepository,
        IToastService toastService)
    {
        _cartFacade = cartFacade;
        _navigationService = navigationService;
        _authRepository = authRepository;
        _toastService = toastService;
    }
    
    [ObservableProperty]
    private string _fullName = string.Empty;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _phone = string.Empty;

    [ObservableProperty]
    private string _address = string.Empty;

    [ObservableProperty]
    private string _city = string.Empty;

    [ObservableProperty]
    private string _zipCode = string.Empty;

    [ObservableProperty]
    private string _country = "Vietnam";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SubtotalFormatted))]
    private decimal _subtotal = 0m;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShippingFormatted))]
    private decimal _shipping = 30000m; // 30,000 VND

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TaxFormatted))]
    private decimal _tax = 0m;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TotalFormatted))]
    private decimal _total = 0m;

    [ObservableProperty]
    private ObservableCollection<SalesAgentGroup> _salesAgentGroups = new();

    // Store the selected agent ID from Cart
    private Guid? _selectedAgentId;

    // Backward compatibility for XAML binding
    public ObservableCollection<CheckoutItem> OrderItems
    {
        get
        {
            var allItems = new ObservableCollection<CheckoutItem>();
            foreach (var group in SalesAgentGroups)
            {
                foreach (var item in group.Items)
                {
                    allItems.Add(item);
                }
            }
            return allItems;
        }
    }

    [ObservableProperty]
    private string _currentStep = "details"; // "details", "payment", "success"

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsCardPayment))]
    [NotifyPropertyChangedFor(nameof(IsQrPayment))]
    [NotifyPropertyChangedFor(nameof(IsCodPayment))]
    private string _selectedPaymentMethod = "QR Code / Banking App";

    [ObservableProperty]
    private string _selectedPaymentIcon = "\uE8A7"; // QR icon

    // Computed properties for UI visibility
    public bool IsCardPayment => SelectedPaymentMethod == "Credit / Debit Card";
    public bool IsQrPayment => SelectedPaymentMethod == "QR Code / Banking App";
    public bool IsCodPayment => SelectedPaymentMethod == "Cash on Delivery (COD)";

    [ObservableProperty]
    private int _countdown = 600; // 10 minutes in seconds

    [ObservableProperty]
    private bool _isPaymentExpired = false;

    // Formatted price properties for UI binding
    public string SubtotalFormatted => $"{Subtotal:N0} ₫";
    public string ShippingFormatted => $"{Shipping:N0} ₫";
    public string TaxFormatted => $"{Tax:N0} ₫";
    public string TotalFormatted => $"{Total:N0} ₫";

    public async Task InitializeAsync(Guid? selectedAgentId = null)
    {
        _selectedAgentId = selectedAgentId;
        IsLoading = true;
        try
        {
            // Load current user info to pre-fill form
            var userResult = await _authRepository.GetCurrentUserAsync();
            if (userResult.IsSuccess && userResult.Data != null)
            {
                var user = userResult.Data;
                FullName = user.Username ?? string.Empty;
                Email = user.Email ?? string.Empty;
                Phone = user.PhoneNumber ?? string.Empty;
            }

            // Load cart items grouped by sales agent
            var result = await _cartFacade.LoadCartAsync();
            SalesAgentGroups.Clear();
            
            if (result.IsSuccess && result.Data != null)
            {
                // Filter by selected agent if provided
                var itemsToCheckout = _selectedAgentId.HasValue
                    ? result.Data.Where(i => i.SalesAgentId == _selectedAgentId.Value).ToList()
                    : result.Data;

                // Group cart items by sales agent
                var groupedItems = itemsToCheckout
                    .Where(i => i.SalesAgentId.HasValue) // Only include items with sales agent
                    .GroupBy(i => new { SalesAgentId = i.SalesAgentId!.Value, i.SalesAgentName })
                    .Select(g => new SalesAgentGroup
                    {
                        SalesAgentId = g.Key.SalesAgentId,
                        SalesAgentName = g.Key.SalesAgentName ?? "Unknown Seller",
                        Items = new ObservableCollection<CheckoutItem>(
                            g.Select(i => new CheckoutItem
                            {
                                CartItemId = i.Id, // Map CartItem.Id for API calls
                                ProductId = i.ProductId,
                                Name = i.ProductName,
                                Quantity = i.Quantity,
                                Price = i.Price,
                                StockAvailable = i.StockAvailable, // For qty validation
                                ImageUrl = !string.IsNullOrEmpty(i.ProductImage) 
                                    ? i.ProductImage 
                                    : "ms-appx:///Assets/Images/products/product-placeholder.png"
                            })),
                        Subtotal = g.Sum(i => i.Price * i.Quantity)
                    })
                    .ToList();

                foreach (var group in groupedItems)
                {
                    SalesAgentGroups.Add(group);
                }
            }

            // Calculate totals
            CalculateTotals();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[CheckoutViewModel] InitializeAsync failed: {ex.Message}");
            await _toastService.ShowError("Failed to load checkout page");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void CalculateTotals()
    {
        Subtotal = SalesAgentGroups.Sum(g => g.Subtotal);
        Tax = Subtotal * 0.10m; // 10% tax
        Shipping = Subtotal >= 500000 ? 0 : 30000; // Free shipping over 500k
        Total = Subtotal + Shipping + Tax;
    }

    [RelayCommand]
    private async Task BackToCartAsync()
    {
        await _navigationService.NavigateInShell(typeof(MyShop.Client.Views.Shared.CartPage).FullName!);
    }

    /// <summary>
    /// Command will be handled by View to show PaymentMethodDialog
    /// View will update SelectedPaymentMethod and SelectedPaymentIcon
    /// </summary>
    [RelayCommand]
    private void ChoosePaymentMethod()
    {
        // Event pattern: View will handle this and show dialog
        System.Diagnostics.Debug.WriteLine("[CheckoutViewModel] ChoosePaymentMethod triggered");
    }

    /// <summary>
    /// Update payment method selection (called by View after dialog)
    /// </summary>
    public void UpdatePaymentMethod(string methodText, string iconGlyph)
    {
        SelectedPaymentMethod = methodText;
        SelectedPaymentIcon = iconGlyph;
        System.Diagnostics.Debug.WriteLine($"[CheckoutViewModel] Payment method updated: {methodText}");
        
        // Trigger UI updates
        OnPropertyChanged(nameof(IsCardPayment));
        OnPropertyChanged(nameof(IsQrPayment));
        OnPropertyChanged(nameof(IsCodPayment));
    }

    [RelayCommand]
    private void ContinueToPayment()
    {
        // Validate form
        if (string.IsNullOrWhiteSpace(FullName) || 
            string.IsNullOrWhiteSpace(Email) || 
            string.IsNullOrWhiteSpace(Phone) ||
            string.IsNullOrWhiteSpace(Address) || 
            string.IsNullOrWhiteSpace(City) || 
            string.IsNullOrWhiteSpace(ZipCode))
        {
            // Show validation error
            return;
        }

        CurrentStep = "payment";
        Countdown = 600;
        IsPaymentExpired = false;
    }

    [RelayCommand]
    private async Task ConfirmPaymentAsync()
    {
        if (IsPaymentExpired || IsProcessing)
        {
            return;
        }

        // Validate shipping form
        if (string.IsNullOrWhiteSpace(FullName) || 
            string.IsNullOrWhiteSpace(Email) || 
            string.IsNullOrWhiteSpace(Phone) ||
            string.IsNullOrWhiteSpace(Address) || 
            string.IsNullOrWhiteSpace(City) || 
            string.IsNullOrWhiteSpace(ZipCode))
        {
            await _toastService.ShowWarning("Please fill in all required shipping information");
            return;
        }

        if (SalesAgentGroups.Count == 0)
        {
            await _toastService.ShowWarning("Your cart is empty");
            return;
        }

        IsProcessing = true;
        try
        {
            // Build full address
            var fullAddress = $"{Address}, {City}, {ZipCode}, {Country}";
            var baseNotes = $"Contact: {FullName}, Email: {Email}, Phone: {Phone}";

            var successCount = 0;
            var totalGroups = SalesAgentGroups.Count;

            // Map payment method from UI to API format
            var apiPaymentMethod = SelectedPaymentMethod switch
            {
                "Credit / Debit Card" => "CARD",
                "QR Code / Banking App" => "QR",
                "Cash on Delivery (COD)" => "COD",
                _ => "COD"
            };

            // Add payment-specific notes
            var notes = apiPaymentMethod switch
            {
                "QR" => $"{baseNotes}\n[{DateTime.Now:yyyy-MM-dd HH:mm}] Customer confirmed QR payment",
                "COD" => $"{baseNotes}\nPayment Method: Cash on Delivery",
                _ => baseNotes
            };

            // Checkout each sales agent group separately
            Guid? lastOrderId = null;
            string? lastOrderCode = null;

            foreach (var group in SalesAgentGroups)
            {
                var result = await _cartFacade.CheckoutBySalesAgentAsync(
                    group.SalesAgentId,
                    fullAddress,
                    notes,
                    apiPaymentMethod);

                if (result.IsSuccess && result.Data != null)
                {
                    successCount++;
                    lastOrderId = result.Data.Id;
                    lastOrderCode = result.Data.OrderCode;
                }
                else
                {
                    await _toastService.ShowError($"Failed to checkout from {group.SalesAgentName}: {result.ErrorMessage}");
                }
            }

            if (successCount == 0)
            {
                await _toastService.ShowError("Failed to place any orders");
                return;
            }

            // Handle success based on payment method
            if (apiPaymentMethod == "CARD")
            {
                // For Card: Navigate to Card Payment page
                if (lastOrderId.HasValue)
                {
                    await _toastService.ShowSuccess($"{successCount} order(s) created. Complete your payment.");
                    
                    var parameter = new Views.Shared.CardPaymentParameter
                    {
                        OrderId = lastOrderId.Value,
                        OrderCode = lastOrderCode ?? "N/A",
                        TotalAmount = Total
                    };
                    
                    await _navigationService.NavigateInShell(
                        typeof(Views.Shared.CardPaymentPage).FullName!,
                        parameter);
                }
            }
            else
            {
                // For QR/COD: Go directly to orders page
                string successMessage;
                
                if (apiPaymentMethod == "QR")
                {
                    successMessage = successCount == totalGroups
                        ? $"All {successCount} orders placed! Seller will verify your payment soon."
                        : $"{successCount} of {totalGroups} orders placed. Awaiting payment verification.";
                }
                else // COD
                {
                    successMessage = successCount == totalGroups
                        ? $"All {successCount} orders confirmed! You'll pay when receiving your order."
                        : $"{successCount} of {totalGroups} orders placed successfully.";
                }
                
                if (successCount == totalGroups)
                {
                    CurrentStep = "success";
                    await _toastService.ShowSuccess(successMessage);
                }
                else
                {
                    await _toastService.ShowWarning(successMessage);
                }
                
                await Task.Delay(1500);
                await _navigationService.NavigateInShell(typeof(MyShop.Client.Views.Shared.PurchaseOrdersPage).FullName!);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[CheckoutViewModel] ConfirmPaymentAsync failed: {ex.Message}");
            await _toastService.ShowError("Failed to process payment. Please try again.");
        }
        finally
        {
            IsProcessing = false;
        }
    }

    [RelayCommand]
    private void RetryPayment()
    {
        Countdown = 600;
        IsPaymentExpired = false;
    }

    [RelayCommand]
    private async Task ViewOrdersAsync()
    {
        await _navigationService.NavigateInShell(typeof(MyShop.Client.Views.Shared.PurchaseOrdersPage).FullName!);
    }

    [RelayCommand]
    private async Task ContinueShoppingAsync()
    {
        await _navigationService.NavigateInShell(typeof(MyShop.Client.Views.Shared.ProductBrowsePage).FullName!);
    }

    // Task F: Cart Interaction Commands (using existing ICartFacade methods)
    [RelayCommand]
    private async Task IncreaseQuantityAsync(CheckoutItem item)
    {
        try
        {
            var newQty = Math.Min(item.Quantity + 1, item.StockAvailable);
            if (newQty == item.Quantity) 
            {
                await _toastService.ShowWarning("Maximum stock reached");
                return;
            }

            // Use existing UpdateCartItemQuantityAsync with cartItemId
            var result = await _cartFacade.UpdateCartItemQuantityAsync(item.CartItemId, newQty);
            if (result.IsSuccess)
            {
                item.Quantity = newQty;
                RecalculateGroupSubtotals();
                CalculateTotals();
            }
            else
            {
                await _toastService.ShowError($"Failed to update quantity: {result.ErrorMessage}");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[CheckoutViewModel] IncreaseQuantityAsync failed: {ex.Message}");
            await _toastService.ShowError("Failed to update quantity");
        }
    }

    [RelayCommand]
    private async Task DecreaseQuantityAsync(CheckoutItem item)
    {
        if (item.Quantity <= 1)
        {
            await RemoveItemAsync(item);
            return;
        }

        try
        {
            var newQty = Math.Max(item.Quantity - 1, 1);
            var result = await _cartFacade.UpdateCartItemQuantityAsync(item.CartItemId, newQty);
            if (result.IsSuccess)
            {
                item.Quantity = newQty;
                RecalculateGroupSubtotals();
                CalculateTotals();
            }
            else
            {
                await _toastService.ShowError($"Failed to update quantity: {result.ErrorMessage}");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[CheckoutViewModel] DecreaseQuantityAsync failed: {ex.Message}");
            await _toastService.ShowError("Failed to update quantity");
        }
    }

    [RelayCommand]
    private async Task RemoveItemAsync(CheckoutItem item)
    {
        try
        {
            var result = await _cartFacade.RemoveFromCartAsync(item.CartItemId);
            if (result.IsSuccess)
            {
                // Remove from UI
                foreach (var group in SalesAgentGroups)
                {
                    var itemToRemove = group.Items.FirstOrDefault(i => i.CartItemId == item.CartItemId);
                    if (itemToRemove != null)
                    {
                        group.Items.Remove(itemToRemove);
                        break;
                    }
                }

                // Remove empty groups
                var emptyGroups = SalesAgentGroups.Where(g => g.Items.Count == 0).ToList();
                foreach (var emptyGroup in emptyGroups)
                {
                    SalesAgentGroups.Remove(emptyGroup);
                }

                RecalculateGroupSubtotals();
                CalculateTotals();
                await _toastService.ShowSuccess("Item removed from cart");
            }
            else
            {
                await _toastService.ShowError($"Failed to remove item: {result.ErrorMessage}");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[CheckoutViewModel] RemoveItemAsync failed: {ex.Message}");
            await _toastService.ShowError("Failed to remove item");
        }
    }

    private void RecalculateGroupSubtotals()
    {
        foreach (var group in SalesAgentGroups)
        {
            group.Subtotal = group.Items.Sum(i => i.Total);
        }
    }
}

public partial class SalesAgentGroup : ObservableObject
{
    [ObservableProperty]
    private Guid _salesAgentId;

    [ObservableProperty]
    private string _salesAgentName = string.Empty;

    [ObservableProperty]
    private ObservableCollection<CheckoutItem> _items = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SubtotalFormatted))]
    private decimal _subtotal;

    public string SubtotalFormatted => $"{Subtotal:N0} ₫";
}

public partial class CheckoutItem : ObservableObject
{
    [ObservableProperty]
    private Guid _cartItemId; // CartItem.Id from backend

    [ObservableProperty]
    private Guid _productId;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Total), nameof(TotalFormatted))]
    private int _quantity;

    [ObservableProperty]
    private decimal _price;

    [ObservableProperty]
    private string _imageUrl = string.Empty;

    [ObservableProperty]
    private int _stockAvailable = int.MaxValue; // For qty validation

    public decimal Total => Quantity * Price;
    public string TotalFormatted => $"{Total:N0} ₫";
}
