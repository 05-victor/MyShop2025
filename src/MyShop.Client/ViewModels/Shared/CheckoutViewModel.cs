using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace MyShop.Client.ViewModels.Shared;

public partial class CheckoutViewModel : ObservableObject
{
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
    private string _country = "United States";

    [ObservableProperty]
    private decimal _subtotal = 0m;

    [ObservableProperty]
    private decimal _shipping = 10m;

    [ObservableProperty]
    private decimal _tax = 0m;

    [ObservableProperty]
    private decimal _total = 0m;

    [ObservableProperty]
    private ObservableCollection<CheckoutItem> _orderItems = new();

    [ObservableProperty]
    private string _currentStep = "details"; // "details", "payment", "success"

    [ObservableProperty]
    private int _countdown = 600; // 10 minutes in seconds

    [ObservableProperty]
    private bool _isPaymentExpired = false;

    public CheckoutViewModel()
    {
        // Data will be passed from CartViewModel when navigating to checkout
    }

    [RelayCommand]
    private void BackToCart()
    {
        // Navigate back to cart
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
    private void ConfirmPayment()
    {
        if (IsPaymentExpired)
        {
            return;
        }

        CurrentStep = "success";
    }

    [RelayCommand]
    private void RetryPayment()
    {
        Countdown = 600;
        IsPaymentExpired = false;
    }

    [RelayCommand]
    private void ViewOrders()
    {
        // Navigate to orders
    }

    [RelayCommand]
    private void ContinueShopping()
    {
        // Navigate to products
    }
}

public class CheckoutItem
{
    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Price { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public decimal Total => Quantity * Price;
}
