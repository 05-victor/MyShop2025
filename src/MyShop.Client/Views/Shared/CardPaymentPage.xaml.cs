using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MyShop.Client.ViewModels.Shared;
using System;

namespace MyShop.Client.Views.Shared;

public sealed partial class CardPaymentPage : Page
{
    public CardPaymentViewModel ViewModel { get; }

    public CardPaymentPage()
    {
        this.InitializeComponent();
        ViewModel = App.Current.Services.GetRequiredService<CardPaymentViewModel>();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if (e.Parameter is CardPaymentParameter parameter)
        {
            ViewModel.Initialize(parameter.OrderId, parameter.OrderCode, parameter.TotalAmount);
        }
    }
}

/// <summary>
/// Navigation parameter for CardPaymentPage
/// </summary>
public class CardPaymentParameter
{
    public Guid OrderId { get; set; }
    public string OrderCode { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
}
