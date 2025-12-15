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
    private int _countdown = 600; // 10 minutes in seconds

    [ObservableProperty]
    private bool _isPaymentExpired = false;

    // Formatted price properties for UI binding
    public string SubtotalFormatted => $"{Subtotal:N0} ₫";
    public string ShippingFormatted => $"{Shipping:N0} ₫";
    public string TaxFormatted => $"{Tax:N0} ₫";
    public string TotalFormatted => $"{Total:N0} ₫";

    public async Task InitializeAsync()
    {
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
                // Group cart items by sales agent
                var groupedItems = result.Data
                    .Where(i => i.SalesAgentId.HasValue) // Only include items with sales agent
                    .GroupBy(i => new { SalesAgentId = i.SalesAgentId!.Value, i.SalesAgentName })
                    .Select(g => new SalesAgentGroup
                    {
                        SalesAgentId = g.Key.SalesAgentId,
                        SalesAgentName = g.Key.SalesAgentName ?? "Unknown Seller",
                        Items = new ObservableCollection<CheckoutItem>(
                            g.Select(i => new CheckoutItem
                            {
                                ProductId = i.ProductId,
                                Name = i.ProductName,
                                Quantity = i.Quantity,
                                Price = i.Price,
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
            var notes = $"Contact: {FullName}, Email: {Email}, Phone: {Phone}";

            var successCount = 0;
            var totalGroups = SalesAgentGroups.Count;

            // Checkout each sales agent group separately
            foreach (var group in SalesAgentGroups)
            {
                var result = await _cartFacade.CheckoutBySalesAgentAsync(
                    group.SalesAgentId,
                    fullAddress,
                    notes);

                if (result.IsSuccess)
                {
                    successCount++;
                }
                else
                {
                    await _toastService.ShowError($"Failed to checkout from {group.SalesAgentName}: {result.ErrorMessage}");
                }
            }

            if (successCount == totalGroups)
            {
                CurrentStep = "success";
                await _toastService.ShowSuccess($"All {successCount} orders placed successfully!");
                
                // Navigate to orders page
                await Task.Delay(1500);
                await _navigationService.NavigateInShell(typeof(MyShop.Client.Views.Shared.PurchaseOrdersPage).FullName!);
            }
            else if (successCount > 0)
            {
                await _toastService.ShowWarning($"{successCount} of {totalGroups} orders placed successfully");
                await _navigationService.NavigateInShell(typeof(MyShop.Client.Views.Shared.PurchaseOrdersPage).FullName!);
            }
            else
            {
                await _toastService.ShowError("Failed to place any orders");
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
}

public class SalesAgentGroup
{
    public Guid SalesAgentId { get; set; }
    public string SalesAgentName { get; set; } = string.Empty;
    public ObservableCollection<CheckoutItem> Items { get; set; } = new();
    public decimal Subtotal { get; set; }
    public string SubtotalFormatted => $"{Subtotal:N0} ₫";
}

public class CheckoutItem
{
    public Guid ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public decimal Total => Quantity * Price;
    public string TotalFormatted => $"{Total:N0} ₫";
}
