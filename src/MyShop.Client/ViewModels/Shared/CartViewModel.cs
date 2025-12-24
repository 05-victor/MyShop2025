using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Core.Interfaces.Facades;
using MyShop.Core.Interfaces.Services;
using MyShop.Client.Views.Shared;
using MyShop.Client.Facades;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace MyShop.Client.ViewModels.Shared;

public partial class CartViewModel : ObservableObject
{
    private readonly ICartFacade _cartFacade;
    private readonly INavigationService _navigationService;
    private readonly IDialogService _dialogService;
    private readonly IToastService _toastService;

    [ObservableProperty]
    private ObservableCollection<CartItemViewModel> _items = new();

    [ObservableProperty]
    private decimal _subtotal = 0m;

    [ObservableProperty]
    private decimal _shippingFee = 50000m;

    [ObservableProperty]
    private decimal _tax = 0m;

    [ObservableProperty]
    private decimal _total = 0m;

    [ObservableProperty]
    private int _itemCount = 0;

    [ObservableProperty]
    private bool _isEmpty = true;

    [ObservableProperty]
    private bool _isLoading = false;

    public CartViewModel(
        ICartFacade cartFacade,
        INavigationService navigationService,
        IDialogService dialogService,
        IToastService toastService)
    {
        _cartFacade = cartFacade;
        _navigationService = navigationService;
        _dialogService = dialogService;
        _toastService = toastService;
    }

    public async Task InitializeAsync()
    {
        await LoadCartAsync();
    }

    private async Task LoadCartAsync()
    {
        IsLoading = true;

        try
        {
            var result = await _cartFacade.LoadCartAsync();
            
            if (!result.IsSuccess || result.Data == null)
            {
                Items.Clear();
                IsEmpty = true;
                return;
            }

            var cartItems = result.Data;

            Items.Clear();
            foreach (var item in cartItems)
            {
                Items.Add(new CartItemViewModel
                {
                    ProductId = item.ProductId,
                    Name = item.ProductName,
                    Category = item.CategoryName ?? "",
                    Price = item.Price,
                    Quantity = item.Quantity,
                    ImageUrl = item.ProductImage ?? "ms-appx:///Assets/Images/products/product-placeholder.png",
                    Stock = item.StockAvailable
                });
            }

            // Get cart summary for totals
            var summaryResult = await _cartFacade.GetCartSummaryAsync();
            if (summaryResult.IsSuccess && summaryResult.Data != null)
            {
                Subtotal = summaryResult.Data.Subtotal;
                Tax = summaryResult.Data.Tax;
                ShippingFee = summaryResult.Data.ShippingFee;
                Total = summaryResult.Data.Total;
                ItemCount = summaryResult.Data.TotalItems;
            }

            IsEmpty = Items.Count == 0;

            System.Diagnostics.Debug.WriteLine($"[CartViewModel] Loaded {Items.Count} items, Total: {Total:N0} VND");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[CartViewModel] Error loading cart: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task IncreaseQuantityAsync(CartItemViewModel item)
    {
        var result = await _cartFacade.UpdateCartItemQuantityAsync(item.ProductId, item.Quantity + 1);
        
        if (result.IsSuccess)
        {
            item.Quantity++;
            await RefreshTotalsAsync();
        }
    }

    [RelayCommand]
    private async Task DecreaseQuantityAsync(CartItemViewModel item)
    {
        if (item.Quantity <= 1)
        {
            await RemoveItemAsync(item);
            return;
        }

        var result = await _cartFacade.UpdateCartItemQuantityAsync(item.ProductId, item.Quantity - 1);
        
        if (result.IsSuccess)
        {
            item.Quantity--;
            await RefreshTotalsAsync();
        }
    }

    [RelayCommand]
    private async Task RemoveItemAsync(CartItemViewModel item)
    {
        var confirmed = await _dialogService.ShowConfirmationAsync(
            "Remove Item",
            $"Are you sure you want to remove '{item.Name}' from your cart?");

        if (!confirmed.IsSuccess || !confirmed.Data) return;

        var result = await _cartFacade.RemoveFromCartAsync(item.ProductId);
        
        if (result.IsSuccess)
        {
            Items.Remove(item);
            await RefreshTotalsAsync();
            IsEmpty = Items.Count == 0;
        }
    }

    [RelayCommand]
    private async Task ClearCartAsync()
    {
        var confirmed = await _dialogService.ShowConfirmationAsync(
            "Clear Cart",
            $"Are you sure you want to remove all {Items.Count} items from your cart? This action cannot be undone.");

        if (!confirmed.IsSuccess || !confirmed.Data) return;

        var result = await _cartFacade.ClearCartAsync();
        
        if (result.IsSuccess)
        {
            Items.Clear();
            Subtotal = 0;
            Tax = 0;
            ShippingFee = 0;
            Total = 0;
            ItemCount = 0;
            IsEmpty = true;
        }
    }

    [RelayCommand]
    private async Task ContinueShoppingAsync()
    {
        await _navigationService.NavigateInShell(typeof(ProductBrowsePage).FullName!);
    }

    [RelayCommand]
    private async Task ProceedToCheckoutAsync()
    {
        // Check if cart is empty
        if (IsEmpty || Items.Count == 0)
        {
            await _toastService.ShowInfo("Please add items to your cart before checkout");
            return;
        }

        await _navigationService.NavigateInShell(typeof(CheckoutPage).FullName!);
    }

    private async Task RefreshTotalsAsync()
    {
        var result = await _cartFacade.GetCartSummaryAsync();

        if (result.IsSuccess && result.Data != null)
        {
            Subtotal = result.Data.Subtotal;
            Tax = result.Data.Tax;
            ShippingFee = result.Data.ShippingFee;
            Total = result.Data.Total;
            ItemCount = result.Data.TotalItems;
        }
    }
}

public partial class CartItemViewModel : ObservableObject
{
    [ObservableProperty]
    private Guid _productId;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _category = string.Empty;

    [ObservableProperty]
    private decimal _price;

    [ObservableProperty]
    private int _quantity;

    [ObservableProperty]
    private string _imageUrl = string.Empty;

    [ObservableProperty]
    private int _stock;

    public decimal Total => Price * Quantity;
}
