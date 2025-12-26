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
    private ObservableCollection<CartAgentGroup> _agentGroups = new();

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

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanProceedToCheckout))]
    private Guid? _selectedAgentId = null;

    /// <summary>
    /// Can only proceed if exactly one shop is selected
    /// </summary>
    public bool CanProceedToCheckout => SelectedAgentId.HasValue && AgentGroups.Any();

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
            AgentGroups.Clear();
            
            // Group items by sales agent
            var groupedByAgent = cartItems
                .Where(i => i.SalesAgentId.HasValue)
                .GroupBy(i => new { AgentId = i.SalesAgentId!.Value, i.SalesAgentName })
                .Select(g => new CartAgentGroup
                {
                    AgentId = g.Key.AgentId,
                    AgentName = g.Key.SalesAgentName ?? "Unknown Seller",
                    IsExpanded = true, // Default expanded
                    Items = new ObservableCollection<CartItemViewModel>(
                        g.Select(item => new CartItemViewModel
                        {
                            CartItemId = item.Id,
                            ProductId = item.ProductId,
                            Name = item.ProductName,
                            Category = item.CategoryName ?? "",
                            Price = item.Price,
                            Quantity = item.Quantity,
                            ImageUrl = item.ProductImage ?? "ms-appx:///Assets/Images/products/product-placeholder.png",
                            Stock = item.StockAvailable,
                            SalesAgentId = item.SalesAgentId,
                            SalesAgentName = item.SalesAgentName
                        })
                    )
                })
                .ToList();

            foreach (var group in groupedByAgent)
            {
                // Calculate subtotal for each group
                group.Subtotal = group.Items.Sum(i => i.Total);
                AgentGroups.Add(group);
            }

            // Auto-select shop if only one group
            if (AgentGroups.Count == 1)
            {
                SelectedAgentId = AgentGroups[0].AgentId;
                AgentGroups[0].IsSelected = true;
            }
            
            OnPropertyChanged(nameof(CanProceedToCheckout));

            // Also populate flat Items list for backward compatibility
            foreach (var item in cartItems)
            {
                Items.Add(new CartItemViewModel
                {
                    CartItemId = item.Id,
                    ProductId = item.ProductId,
                    Name = item.ProductName,
                    Category = item.CategoryName ?? "",
                    Price = item.Price,
                    Quantity = item.Quantity,
                    ImageUrl = item.ProductImage ?? "ms-appx:///Assets/Images/products/product-placeholder.png",
                    Stock = item.StockAvailable,
                    SalesAgentId = item.SalesAgentId,
                    SalesAgentName = item.SalesAgentName
                });
            }

            // Don't calculate totals yet - wait for user to select a shop
            // Totals will be calculated in SelectShop command
            Subtotal = 0;
            Tax = 0;
            Total = 0;
            ItemCount = Items.Count;

            IsEmpty = Items.Count == 0;

            System.Diagnostics.Debug.WriteLine($"[CartViewModel] Loaded {Items.Count} items, {AgentGroups.Count} shops");
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
        if (item == null) return;
        
        var newQty = Math.Min(item.Quantity + 1, item.Stock);
        if (newQty == item.Quantity)
        {
            await _toastService.ShowWarning("Maximum stock reached");
            return;
        }
        
        var result = await _cartFacade.UpdateCartItemQuantityAsync(item.CartItemId, newQty);
        
        if (result.IsSuccess)
        {
            item.Quantity = newQty;
            RecalculateGroupSubtotals();
            await RefreshTotalsAsync();
        }
    }

    [RelayCommand]
    private async Task DecreaseQuantityAsync(CartItemViewModel item)
    {
        if (item == null) return;
        
        if (item.Quantity <= 1)
        {
            await RemoveItemAsync(item);
            return;
        }

        var newQty = Math.Max(item.Quantity - 1, 1);
        var result = await _cartFacade.UpdateCartItemQuantityAsync(item.CartItemId, newQty);
        
        if (result.IsSuccess)
        {
            item.Quantity = newQty;
            RecalculateGroupSubtotals();
            await RefreshTotalsAsync();
        }
    }

    [RelayCommand]
    private async Task RemoveItemAsync(CartItemViewModel item)
    {
        if (item == null) return;
        
        var confirmed = await _dialogService.ShowConfirmationAsync(
            "Remove Item",
            $"Are you sure you want to remove '{item.Name}' from your cart?");

        if (!confirmed.IsSuccess || !confirmed.Data) return;

        var result = await _cartFacade.RemoveFromCartAsync(item.CartItemId);
        
        if (result.IsSuccess)
        {
            // Remove from flat list
            Items.Remove(item);
            
            // Remove from groups
            foreach (var group in AgentGroups)
            {
                var itemToRemove = group.Items.FirstOrDefault(i => i.CartItemId == item.CartItemId);
                if (itemToRemove != null)
                {
                    group.Items.Remove(itemToRemove);
                    break;
                }
            }
            
            // Remove empty groups
            var emptyGroups = AgentGroups.Where(g => g.Items.Count == 0).ToList();
            foreach (var emptyGroup in emptyGroups)
            {
                AgentGroups.Remove(emptyGroup);
            }
            
            RecalculateGroupSubtotals();
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
    private async Task SelectShop(Guid agentId)
    {
        SelectedAgentId = agentId;
        
        // Update IsSelected state for all groups
        foreach (var group in AgentGroups)
        {
            group.IsSelected = group.AgentId == agentId;
        }
        
        // Recalculate totals for selected shop only
        await RefreshTotalsAsync();
        
        OnPropertyChanged(nameof(CanProceedToCheckout));
        System.Diagnostics.Debug.WriteLine($"[CartViewModel] Selected shop: {agentId}, New Total: {Total:N0} VND");
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

        // Check if a shop is selected
        if (!SelectedAgentId.HasValue)
        {
            await _toastService.ShowWarning("Please select a shop to checkout");
            return;
        }

        await _navigationService.NavigateInShell(typeof(CheckoutPage).FullName!);
    }

    private async Task RefreshTotalsAsync()
    {
        // If a shop is selected, calculate totals only for that shop
        if (SelectedAgentId.HasValue)
        {
            var selectedGroup = AgentGroups.FirstOrDefault(g => g.AgentId == SelectedAgentId.Value);
            if (selectedGroup != null)
            {
                Subtotal = selectedGroup.Subtotal;
                Tax = Subtotal * 0.1m; // 10% tax
                Total = Subtotal + Tax + ShippingFee;
                ItemCount = selectedGroup.Items.Count;
                return;
            }
        }

        // Otherwise, get totals from backend (all items)
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

    private void RecalculateGroupSubtotals()
    {
        foreach (var group in AgentGroups)
        {
            group.Subtotal = group.Items.Sum(i => i.Total);
        }
    }
}

public partial class CartItemViewModel : ObservableObject
{
    [ObservableProperty]
    private Guid _cartItemId;

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

    [ObservableProperty]
    private Guid? _salesAgentId;

    [ObservableProperty]
    private string? _salesAgentName;

    public decimal Total => Price * Quantity;
}

/// <summary>
/// Group of cart items by sales agent
/// </summary>
public partial class CartAgentGroup : ObservableObject
{
    [ObservableProperty]
    private Guid _agentId;

    [ObservableProperty]
    private string _agentName = string.Empty;

    [ObservableProperty]
    private ObservableCollection<CartItemViewModel> _items = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SubtotalFormatted))]
    private decimal _subtotal;

    [ObservableProperty]
    private bool _isExpanded = true;

    [ObservableProperty]
    private bool _isSelected = false;

    public string SubtotalFormatted => $"{Subtotal:N0} ₫";
}
