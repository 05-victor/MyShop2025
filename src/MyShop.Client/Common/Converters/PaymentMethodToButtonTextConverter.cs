using Microsoft.UI.Xaml.Data;
using System;

namespace MyShop.Client.Common.Converters;

/// <summary>
/// Converts payment method name to appropriate button text
/// </summary>
public class PaymentMethodToButtonTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is string paymentMethod)
        {
            return paymentMethod switch
            {
                "Credit / Debit Card" => "Proceed to Payment",
                "QR Code / Banking App" => "I've Made the Payment",
                "Cash on Delivery (COD)" => "Confirm Order",
                _ => "Continue"
            };
        }
        return "Continue";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
