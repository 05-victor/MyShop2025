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
    private ObservableCollection<CheckoutItem> _orderItems = new();

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
                // Address fields might not be in user model, keep defaults
            }

            // Load cart items from cart facade
            var result = await _cartFacade.LoadCartAsync();
            OrderItems.Clear();
            
            if (result.IsSuccess && result.Data != null)
            {
                foreach (var item in result.Data)
                {
                    OrderItems.Add(new CheckoutItem
                    {
                        ProductId = item.ProductId,
                        Name = item.ProductName,
                        Quantity = item.Quantity,
                        Price = item.Price,
                        ImageUrl = !string.IsNullOrEmpty(item.ProductImage) 
                            ? item.ProductImage 
                            : "ms-appx:///Assets/Images/products/product-placeholder.png"
                    });
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
        Subtotal = OrderItems.Sum(i => i.Total);
        Tax = Subtotal * 0.08m; // 8% tax
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

        if (OrderItems.Count == 0)
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

            // Call checkout via facade
            var result = await _cartFacade.CheckoutAsync(fullAddress, notes);

            if (result.IsSuccess && result.Data != null)
            {
                CurrentStep = "success";
                await _toastService.ShowSuccess($"Order #{result.Data.OrderCode} placed successfully!");
                
                // Navigate to orders page within shell after user dismisses toast
                await Task.Delay(100); // Small delay to ensure toast is processed
                await _navigationService.NavigateInShell(typeof(MyShop.Client.Views.Shared.PurchaseOrdersPage).FullName!);
            }
            else
            {
                await _toastService.ShowError(result.ErrorMessage ?? "Failed to place order");
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
