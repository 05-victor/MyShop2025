using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using Windows.System;
using MyShop.Client.ViewModels.Shared;

namespace MyShop.Client.Views.Shared;

public sealed partial class CartPage : Page
{
    public CartViewModel ViewModel { get; }

    public CartPage()
    {
        InitializeComponent();

        ViewModel = App.Current.Services.GetRequiredService<CartViewModel>();
        this.DataContext = ViewModel;
        SetupKeyboardShortcuts();
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        await ViewModel.InitializeAsync();
    }

    private void SetupKeyboardShortcuts()
    {
        // Ctrl+Enter: Proceed to checkout
        var checkoutShortcut = new KeyboardAccelerator { Key = VirtualKey.Enter, Modifiers = VirtualKeyModifiers.Control };
        checkoutShortcut.Invoked += async (s, e) => { if (!ViewModel.IsEmpty) await ViewModel.ProceedToCheckoutCommand.ExecuteAsync(null); e.Handled = true; };
        KeyboardAccelerators.Add(checkoutShortcut);

        // Ctrl+Shift+Delete: Clear cart
        var clearShortcut = new KeyboardAccelerator { Key = VirtualKey.Delete, Modifiers = VirtualKeyModifiers.Control | VirtualKeyModifiers.Shift };
        clearShortcut.Invoked += async (s, e) => { if (!ViewModel.IsEmpty) await ViewModel.ClearCartCommand.ExecuteAsync(null); e.Handled = true; };
        KeyboardAccelerators.Add(clearShortcut);

        // Escape: Continue shopping
        var backShortcut = new KeyboardAccelerator { Key = VirtualKey.Escape };
        backShortcut.Invoked += async (s, e) => { await ViewModel.ContinueShoppingCommand.ExecuteAsync(null); e.Handled = true; };
        KeyboardAccelerators.Add(backShortcut);
    }
}
