using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyShop.Core.Interfaces.Repositories;
using MyShop.Core.Interfaces.Services;
using MyShop.Shared.DTOs.Requests;
using System;
using System.Threading.Tasks;

namespace MyShop.Client.ViewModels.Shared;

public partial class CardPaymentViewModel : ObservableObject
{
    private readonly IOrderRepository _orderRepository;
    private readonly INavigationService _navigationService;
    private readonly IToastService _toastService;

    [ObservableProperty]
    private Guid _orderId;

    [ObservableProperty]
    private string _orderCode = string.Empty;

    [ObservableProperty]
    private decimal _totalAmount;

    [ObservableProperty]
    private string _cardNumber = string.Empty;

    [ObservableProperty]
    private string _cardHolderName = string.Empty;

    [ObservableProperty]
    private string _expiryDate = string.Empty;

    [ObservableProperty]
    private string _cvv = string.Empty;

    [ObservableProperty]
    private bool _isProcessing = false;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _hasError = false;

    public string TotalAmountFormatted => $"{TotalAmount:N0} ₫";

    public CardPaymentViewModel(
        IOrderRepository orderRepository,
        INavigationService navigationService,
        IToastService toastService)
    {
        _orderRepository = orderRepository;
        _navigationService = navigationService;
        _toastService = toastService;
    }

    public void Initialize(Guid orderId, string orderCode, decimal totalAmount)
    {
        OrderId = orderId;
        OrderCode = orderCode;
        TotalAmount = totalAmount;
    }

    [RelayCommand]
    private async Task ProcessPaymentAsync()
    {
        HasError = false;
        ErrorMessage = string.Empty;

        // Validation
        if (string.IsNullOrWhiteSpace(CardNumber))
        {
            ShowError("Card number is required");
            return;
        }

        if (string.IsNullOrWhiteSpace(CardHolderName))
        {
            ShowError("Cardholder name is required");
            return;
        }

        if (string.IsNullOrWhiteSpace(ExpiryDate))
        {
            ShowError("Expiry date is required");
            return;
        }

        if (string.IsNullOrWhiteSpace(Cvv))
        {
            ShowError("CVV is required");
            return;
        }

        // Validate card number format (basic)
        var cleanCardNumber = CardNumber.Replace(" ", "").Replace("-", "");
        if (cleanCardNumber.Length != 16 || !long.TryParse(cleanCardNumber, out _))
        {
            ShowError("Invalid card number format (must be 16 digits)");
            return;
        }

        // Validate expiry date format (MM/YY)
        if (!ExpiryDate.Contains("/") || ExpiryDate.Length != 5)
        {
            ShowError("Invalid expiry date format (MM/YY)");
            return;
        }

        // Validate CVV (3 digits)
        if (Cvv.Length != 3 || !int.TryParse(Cvv, out _))
        {
            ShowError("CVV must be 3 digits");
            return;
        }

        IsProcessing = true;

        try
        {
            var request = new ProcessCardPaymentRequest
            {
                OrderId = OrderId,
                CardNumber = CardNumber, // Keep original format with spaces for API validation
                CardHolderName = CardHolderName,
                ExpiryDate = ExpiryDate,
                Cvv = Cvv
            };

            var result = await _orderRepository.ProcessCardPaymentAsync(OrderId, request);

            if (!result.IsSuccess)
            {
                ShowError(result.ErrorMessage ?? "Payment processing failed");
                await _toastService.ShowError("Payment failed. Please try again.");
                return;
            }

            var success = result.Data;

            if (success)
            {
                await _toastService.ShowSuccess("Payment successful! Your order is confirmed.");
                
                // Navigate to orders page
                await Task.Delay(1500);
                await _navigationService.NavigateInShell(typeof(Views.Shared.PurchaseOrdersPage).FullName!);
            }
            else
            {
                ShowError("Payment was declined");
                await _toastService.ShowError("Payment was declined. Please check your card details.");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[CardPaymentViewModel] ProcessPaymentAsync failed: {ex.Message}");
            ShowError("An error occurred while processing payment");
            await _toastService.ShowError("Payment processing error. Please try again.");
        }
        finally
        {
            IsProcessing = false;
        }
    }

    [RelayCommand]
    private async Task CancelAsync()
    {
        await _navigationService.NavigateInShell(typeof(Views.Shared.PurchaseOrdersPage).FullName!);
    }

    [RelayCommand]
    private void UseTestCard()
    {
        CardNumber = "1111 1111 1111 1111";
        CardHolderName = "Test User";
        ExpiryDate = "12/25";
        Cvv = "123";
        
        HasError = false;
        ErrorMessage = string.Empty;
    }

    private void ShowError(string message)
    {
        ErrorMessage = message;
        HasError = true;
    }
}
