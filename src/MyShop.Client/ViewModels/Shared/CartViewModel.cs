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
    private ObservableCollection<SalesAgentGroupViewModel> _agentGroups = new();

    [ObservableProperty]
    private decimal _grandTotal = 0m;

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
            var result = await _cartFacade.LoadGroupedCartAsync();
            
            if (!result.IsSuccess || result.Data == null)
            {
                AgentGroups.Clear();
                IsEmpty = true;
                GrandTotal = 0m;
                ItemCount = 0;
                return;
            }

            var groupedCart = result.Data;

            // Map to ViewModels
            AgentGroups.Clear();
            foreach (var group in groupedCart.SalesAgentGroups)
            {
                var groupVM = new SalesAgentGroupViewModel
                {
                    SalesAgentId = group.SalesAgentId,
                    SalesAgentFullName = group.SalesAgentFullName,
                    Subtotal = group.Subtotal,
                    Total = group.Total,
                    Items = new ObservableCollection<CartItemViewModel>(
                        group.Items.Select(item => new CartItemViewModel
                        {
                            ProductId = item.ProductId,
                            Name = item.ProductName,
                            Category = item.CategoryName ?? "",
                            Price = item.Price,
                            Quantity = item.Quantity,
                            ImageUrl = item.ProductImage ?? "ms-appx:///Assets/Images/products/product-placeholder.png",
                            Stock = item.StockAvailable
                        })
                    )
                };
                AgentGroups.Add(groupVM);
            }

            GrandTotal = groupedCart.GrandTotal;
            ItemCount = groupedCart.TotalItemCount;
            IsEmpty = AgentGroups.Count == 0;

            System.Diagnostics.Debug.WriteLine($"[CartViewModel] Loaded {AgentGroups.Count} agent groups, {ItemCount} items, Total: {GrandTotal:N0} VND");
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
            await LoadCartAsync(); // Reload grouped cart
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
            await LoadCartAsync(); // Reload grouped cart
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
            await LoadCartAsync(); // Reload grouped cart
        }
    }

    [RelayCommand]
    private async Task ClearCartAsync()
    {
        var confirmed = await _dialogService.ShowConfirmationAsync(
            "Clear Cart",
            $"Are you sure you want to remove all {ItemCount} items from your cart? This action cannot be undone.");

        if (!confirmed.IsSuccess || !confirmed.Data) return;

        var result = await _cartFacade.ClearCartAsync();
        
        if (result.IsSuccess)
        {
            AgentGroups.Clear();
            GrandTotal = 0;
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
        if (IsEmpty || ItemCount == 0)
        {
            await _toastService.ShowInfo("Please add items to your cart before checkout");
            return;
        }

        await _navigationService.NavigateInShell(typeof(CheckoutPage).FullName!);
    }
}

/// <summary>
/// ViewModel for grouped cart items by sales agent
/// </summary>
public partial class SalesAgentGroupViewModel : ObservableObject
{
    [ObservableProperty]
    private Guid _salesAgentId;

    [ObservableProperty]
    private string _salesAgentUsername = string.Empty;

    [ObservableProperty]
    private string _salesAgentFullName = string.Empty;

    [ObservableProperty]
    private ObservableCollection<CartItemViewModel> _items = new();

    [ObservableProperty]
    private decimal _subtotal;

    [ObservableProperty]
    private decimal _total;

    [ObservableProperty]
    private int _itemCount;
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
