using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Core.Interfaces.Services;
using MyShop.Client.Views.Shared;
using MyShop.Core.Interfaces.Repositories;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace MyShop.Client.ViewModels.Shared;

public partial class CartViewModel : ObservableObject
{
    private readonly ICartRepository _cartRepository;
    private readonly IAuthRepository _authRepository;
    private readonly INavigationService _navigationService;
    private readonly IToastService _toastHelper;

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
        ICartRepository cartRepository,
        IAuthRepository authRepository,
        INavigationService navigationService,
        IToastService toastHelper)
    {
        _cartRepository = cartRepository;
        _authRepository = authRepository;
        _navigationService = navigationService;
        _toastHelper = toastHelper;
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
            var userIdResult = await _authRepository.GetCurrentUserIdAsync();
            
            if (!userIdResult.IsSuccess || userIdResult.Data == Guid.Empty)
            {
                Items.Clear();
                IsEmpty = true;
                return;
            }

            var userId = userIdResult.Data;

            // Load cart items
            var cartItemsResult = await _cartRepository.GetCartItemsAsync(userId);

            Items.Clear();
            if (cartItemsResult.IsSuccess && cartItemsResult.Data != null)
            {
                foreach (var item in cartItemsResult.Data)
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
            }

            // Load cart summary
            var summaryResult = await _cartRepository.GetCartSummaryAsync(userId);
            if (summaryResult.IsSuccess && summaryResult.Data != null)
            {
                Subtotal = summaryResult.Data.Subtotal;
                Tax = summaryResult.Data.Tax;
                ShippingFee = summaryResult.Data.ShippingFee;
                Total = summaryResult.Data.Total;
                ItemCount = summaryResult.Data.ItemCount;
            }
            IsEmpty = Items.Count == 0;

            System.Diagnostics.Debug.WriteLine($"[CartViewModel] Loaded {Items.Count} items, Total: {Total:N0} VND");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[CartViewModel] Error loading cart: {ex.Message}");
            _toastHelper.ShowError("Failed to load cart");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task IncreaseQuantityAsync(CartItemViewModel item)
    {
        if (item.Quantity >= item.Stock)
        {
            await _toastHelper.ShowWarning("Maximum stock reached");
            return;
        }

        try
        {
            var userIdResult = await _authRepository.GetCurrentUserIdAsync();
            if (!userIdResult.IsSuccess) return;

            var userId = userIdResult.Data;
            var newQuantity = item.Quantity + 1;

            var result = await _cartRepository.UpdateQuantityAsync(userId, item.ProductId, newQuantity);
            
            if (result.IsSuccess && result.Data)
            {
                item.Quantity = newQuantity;
                await RefreshTotalsAsync();
            }
            else
            {
                await _toastHelper.ShowError("Failed to update quantity");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[CartViewModel] Error increasing quantity: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task DecreaseQuantityAsync(CartItemViewModel item)
    {
        if (item.Quantity <= 1)
        {
            // Remove item if quantity becomes 0
            await RemoveItemAsync(item);
            return;
        }

        try
        {
            var userIdResult = await _authRepository.GetCurrentUserIdAsync();
            if (!userIdResult.IsSuccess) return;

            var userId = userIdResult.Data;
            var newQuantity = item.Quantity - 1;

            var result = await _cartRepository.UpdateQuantityAsync(userId, item.ProductId, newQuantity);
            
            if (result.IsSuccess && result.Data)
            {
                item.Quantity = newQuantity;
                await RefreshTotalsAsync();
            }
            else
            {
                await _toastHelper.ShowError("Failed to update quantity");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[CartViewModel] Error decreasing quantity: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task RemoveItemAsync(CartItemViewModel item)
    {
        try
        {
            var userIdResult = await _authRepository.GetCurrentUserIdAsync();
            if (!userIdResult.IsSuccess) return;

            var userId = userIdResult.Data;

            var result = await _cartRepository.RemoveFromCartAsync(userId, item.ProductId);
            
            if (result.IsSuccess && result.Data)
            {
                Items.Remove(item);
                await RefreshTotalsAsync();
                IsEmpty = Items.Count == 0;
                await _toastHelper.ShowSuccess($"Removed {item.Name} from cart");
            }
            else
            {
                await _toastHelper.ShowError("Failed to remove item");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[CartViewModel] Error removing item: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task ClearCartAsync()
    {
        try
        {
            var userIdResult = await _authRepository.GetCurrentUserIdAsync();
            if (!userIdResult.IsSuccess) return;

            var userId = userIdResult.Data;

            var result = await _cartRepository.ClearCartAsync(userId);
            
            if (result.IsSuccess && result.Data)
            {
                Items.Clear();
                Subtotal = 0;
                Tax = 0;
                ShippingFee = 0;
                Total = 0;
                ItemCount = 0;
                IsEmpty = true;
                await _toastHelper.ShowSuccess("Cart cleared");
            }
            else
            {
                await _toastHelper.ShowError("Failed to clear cart");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[CartViewModel] Error clearing cart: {ex.Message}");
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
        if (IsEmpty)
        {
            await _toastHelper.ShowWarning("Your cart is empty");
            return;
        }

        await _navigationService.NavigateInShell(typeof(CheckoutPage).FullName!);
    }

    private async Task RefreshTotalsAsync()
    {
        try
        {
            var userIdResult = await _authRepository.GetCurrentUserIdAsync();
            if (!userIdResult.IsSuccess) return;

            var userId = userIdResult.Data;
            var summaryResult = await _cartRepository.GetCartSummaryAsync(userId);

            if (summaryResult.IsSuccess && summaryResult.Data != null)
            {
                Subtotal = summaryResult.Data.Subtotal;
                Tax = summaryResult.Data.Tax;
                ShippingFee = summaryResult.Data.ShippingFee;
                Total = summaryResult.Data.Total;
                ItemCount = summaryResult.Data.ItemCount;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[CartViewModel] Error refreshing totals: {ex.Message}");
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
